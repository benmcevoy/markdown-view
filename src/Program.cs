// This is a C# tool that scans a configured directory for markdown (.md) files
// and creates a navigable, locally hosted website from them.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using Markdig;
using Markdig.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using MdView.Models;

namespace MdView
{

    class Program
    {
        private static readonly string _rootPath = "/home/agent/hello-world/sample";
        private static readonly Scanner _scanner = new(_rootPath);

        // Security configuration
        private const int MaxPathLength = 256;
        private const int MaxRequestSize = 10 * 1024 * 1024; // 10MB max
        private const int MaxHeaderSize = 8 * 1024; // 8KB max
        private const int MaxQueryParamLength = 1024;

        // Security helpers
        private static readonly HtmlEncoder _htmlEncoder = HtmlEncoder.Default;
        private static readonly Regex _pathTraversalPattern = new(@"\.\./|\.\.\\|\\\\", RegexOptions.Compiled);
        private static readonly Regex _nullBytePattern = new(@"\x00", RegexOptions.Compiled);
        private static readonly Regex _pathSeparatorPattern = new(@"[\x00\x01\x02\x03\x04\x05\x06\x07\x08\x09\x0a\x0b\x0c\x0d\x0e\x0f]", RegexOptions.Compiled);

        // Rate limiting storage (in-memory for simplicity)
        private static readonly ConcurrentDictionary<string, RateLimitData> RateLimitStore = new();

        private class RateLimitData
        {
            public int RequestCount { get; set; }
            public DateTime LastRequest { get; set; }
            public DateTime WindowStart { get; set; }
        }

        /// <summary>
        /// Validates and sanitizes user input to prevent security vulnerabilities.
        /// </summary>
        private static bool ValidateRequest(HttpContext context)
        {
            // Validate Content-Length header
            var contentLength = context.Request.Headers["Content-Length"];
            if (!int.TryParse(contentLength, out var length))
            {
                length = 0;
            }
            if (length > MaxRequestSize)
            {
                context.Response.StatusCode = 413;
                context.Response.ContentType = "text/plain";
                return false;
            }

            // Validate User-Agent header
            var userAgent = context.Request.Headers["User-Agent"];
            var userAgentString = userAgent.ToString();
            if (string.IsNullOrEmpty(userAgentString) || userAgentString.Length > MaxHeaderSize)
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "text/plain";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Sanitizes a string to prevent injection attacks.
        /// </summary>
        private static string SanitizeString(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Check for null bytes (path traversal via null bytes)
            if (_nullBytePattern.IsMatch(input))
            {
                return "[SANITIZED: Null byte detected]";
            }

            // Check for path traversal attempts
            if (_pathTraversalPattern.IsMatch(input))
            {
                return "[SANITIZED: Path traversal attempt detected]";
            }

            // Check for control characters that could be used for injection
            if (_pathSeparatorPattern.IsMatch(input))
            {
                return "[SANITIZED: Invalid characters detected]";
            }

            return input;
        }

        /// <summary>
        /// Validates that a path is within the root directory.
        /// </summary>
        private static bool IsPathSafe(string requestPath, string rootPath)
        {
            // Remove leading slash for path comparison
            var relativePath = requestPath.StartsWith("/") ? requestPath[1..] : requestPath;

            // Build the full path
            var fullPath = Path.Combine(rootPath, relativePath);

            // Normalize the path to resolve any .. or . components
            var normalizedPath = Path.GetFullPath(fullPath);
            var rootFullPath = Path.GetFullPath(rootPath);

            // Ensure the normalized path starts with the root path
            return normalizedPath.StartsWith(rootFullPath, StringComparison.OrdinalIgnoreCase);
        }

        static async Task Main(string[] args)
        {
            var navigation = _scanner.ScanDirectory();

            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();

                    webBuilder.Configure(app =>
                    {
                        // Security: Add custom rate limiting middleware
                        app.Use(async (context, next) =>
                        {
                            var clientIp = context.Connection.RemoteIpAddress?.ToString();
                            var startTime = DateTime.UtcNow;

                            // Rate limit: 100 requests per minute per IP
                            var rateLimitKey = clientIp ?? "anonymous";

                            // Check if client has exceeded rate limit
                            var rateLimitData = RateLimitStore.TryGetValue(rateLimitKey, out var data);
                            if (!rateLimitData)
                            {
                                RateLimitStore.TryAdd(rateLimitKey, new RateLimitData
                                {
                                    RequestCount = 1,
                                    LastRequest = startTime,
                                    WindowStart = startTime
                                });
                            }
                            else
                            {
                                // Check if we're still in the current window
                                if (data != null)
                                {
                                    var timeSinceWindowStart = (DateTime.UtcNow - data.WindowStart).TotalMinutes;
                                    if (timeSinceWindowStart >= 1.0)
                                    {
                                        // New window, reset counter
                                        RateLimitStore.TryAdd(rateLimitKey, new RateLimitData
                                        {
                                            RequestCount = 1,
                                            LastRequest = startTime,
                                            WindowStart = startTime
                                        });
                                    }
                                    else
                                    {
                                        // Increment request count
                                        data.RequestCount++;
                                        data.LastRequest = startTime;
                                        RateLimitStore.TryAdd(rateLimitKey, data);

                                        // Check if limit exceeded
                                        if (data.RequestCount > 100)
                                        {
                                            context.Response.StatusCode = 429;
                                            context.Response.ContentType = "text/plain";
                                            await context.Response.WriteAsync("Too Many Requests: Rate limit exceeded");
                                            return;
                                        }
                                    }
                                }
                            }

                            await next();
                        });

                        // Define how to handle incoming requests
                        app.Run(async context =>
                        {
                            var content = await Content(context);
                            await context.Response.WriteAsync(content);
                        });
                    });
                });

            var host = builder.Build();

            Console.WriteLine("Starting Kestrel host...");
            Console.WriteLine("Listening on: http://localhost:5001");

            await host.RunAsync();
        }

        private static async Task<string> Content(HttpContext context)
        {
            // Security: Validate and sanitize the request
            var requestPath = context.Request.Path.ToString();
            var requestUri = context.Request.Path;
            var query = context.Request.Query.ToString();

            // Security: Check for null bytes in path (path traversal via null bytes)
            if (_nullBytePattern.IsMatch(requestPath) || _nullBytePattern.IsMatch(query))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/plain";
                return "Bad Request: Null byte detected in request";
            }

            // Security: Check for path traversal attempts
            if (_pathTraversalPattern.IsMatch(requestPath))
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "text/plain";
                return "Forbidden: Path traversal attempt detected";
            }

            // Security: Validate path length
            if (requestPath.Length > MaxPathLength)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/plain";
                return "Bad Request: Request path too long";
            }

            // Security: Validate query string length
            if (query.Length > MaxQueryParamLength)
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "text/plain";
                return "Bad Request: Query string too long";
            }

            // Security: Check header sizes
            foreach (var header in context.Request.Headers)
            {
                var key = header.Key.ToString();
                var value = header.Value.ToString();
                if (key.Length > MaxHeaderSize || value.Length > MaxHeaderSize)
                {
                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "text/plain";
                    return "Bad Request: Header too large";
                }
            }

            // Security: Check content length
            var contentLength = context.Request.ContentLength;
            if (contentLength > MaxRequestSize)
            {
                context.Response.StatusCode = 413;
                context.Response.ContentType = "text/plain";
                return "Bad Request: Request entity too large";
            }

            // SECURITY: Validate request method is GET or POST only
            if (!context.Request.Method.StartsWith("GET", StringComparison.OrdinalIgnoreCase) &&
                !context.Request.Method.StartsWith("POST", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = 405;
                context.Response.ContentType = "text/plain";
                return "Method Not Allowed";
            }

            // SECURITY: Remove query strings from the request path before processing
            var queryIndex = requestPath.IndexOf('?');
            if (queryIndex >= 0)
            {
                requestPath = requestPath[..queryIndex];
            }

            // Handle static asset requests (/css/, /js/, etc.)
            if (requestPath.StartsWith("/css/", StringComparison.Ordinal) ||
                requestPath.StartsWith("/js/", StringComparison.Ordinal) ||
                requestPath.StartsWith("/assets/", StringComparison.Ordinal))
            {
                // Serve static files from wwwroot
                var staticFilePath = Path.Combine("wwwroot", requestPath.Substring(1));
                if (File.Exists(staticFilePath))
                {
                    var extension = Path.GetExtension(staticFilePath);
                    var contentType = "text/html"; // Default
                    if (extension == ".css") contentType = "text/css";
                    if (extension == ".js") contentType = "text/javascript";
                    await context.Response.WriteAsync(File.ReadAllText(staticFilePath));
                    context.Response.ContentType = contentType;
                    return "";
                }
                context.Response.StatusCode = 404;
                context.Response.ContentType = "text/plain";
                return "404 Not Found";
            }

            // Normalize paths by removing trailing slashes (except for root /)
            if (requestPath != "/" && requestPath.EndsWith("/", StringComparison.Ordinal))
            {
                requestPath = requestPath[..^1];
            }

            // Handle URL-encoded paths (decode before processing)
            requestPath = Uri.UnescapeDataString(requestPath);

            // Validate that the resolved path is within the root directory (prevent directory traversal)
            if (requestPath == "/")
            {
                requestPath = "";
            }
            else if (requestPath.StartsWith("..", StringComparison.Ordinal))
            {
                context.Response.StatusCode = 403;
                context.Response.ContentType = "text/plain";
                return "Forbidden: Path traversal attempt detected";
            }

            var rootPath = _scanner._rootPath;
            var relativePath = requestPath;
            var fullPath = Path.Combine(rootPath, relativePath);

            // Route / to index.md
            if (requestPath == "/" || requestPath == "")
            {
                var indexPath = Path.Combine(rootPath, "index.md");
                if (File.Exists(indexPath))
                {
                    var navItem = GetNavigationItemByPath(_scanner.NavigationTree, "./index.md");
                    if (navItem != null)
                    {
                        var renderedContent = RenderMarkdown(navItem.Content);
                        return GenerateHtml(navItem, renderedContent);
                    }
                    context.Response.StatusCode = 404;
                    context.Response.ContentType = "text/plain";
                    return "404 Not Found";
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.ContentType = "text/plain";
                    return "404 Not Found";
                }
            }
            else if (requestPath.EndsWith("/index.md", StringComparison.OrdinalIgnoreCase))
            {
                // Route /folder/index.md
                var fileName = requestPath.Replace("/index.md", "");
                var navItem = GetNavigationItemByPath(_scanner.NavigationTree, fileName);
                if (navItem != null)
                {
                    var renderedContent = RenderMarkdown(navItem.Content);
                    return GenerateHtml(navItem, renderedContent);
                }
                context.Response.StatusCode = 404;
                context.Response.ContentType = "text/plain";
                return "404 Not Found";
            }
            else if (requestPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                // Route /folder/file.md
                var fileName = requestPath.Replace(".md", "");
                var navItem = GetNavigationItemByPath(_scanner.NavigationTree, fileName);
                if (navItem != null)
                {
                    var renderedContent = RenderMarkdown(navItem.Content);
                    return GenerateHtml(navItem, renderedContent);
                }
                context.Response.StatusCode = 404;
                context.Response.ContentType = "text/plain";
                return "404 Not Found";
            }
            else
            {
                // Route /folder/ - serve index.md in that folder
                var indexPath = Path.Combine(fullPath, "index.md");

                if (File.Exists(indexPath))
                {
                    // Find the NavigationItem for this directory
                    var dirNavItem = GetNavigationItemByPath(_scanner.NavigationTree, relativePath);
                    if (dirNavItem != null && dirNavItem.Children != null)
                    {
                        // Find index.md in children
                        var indexItem = dirNavItem.Children?.Find(c => c.Name == "index.md");
                        if (indexItem != null)
                        {
                            // Render markdown using Markdig
                            var renderedContent = RenderMarkdown(indexItem.Content);
                            return GenerateHtml(dirNavItem, renderedContent);
                        }
                    }
                    context.Response.StatusCode = 404;
                    context.Response.ContentType = "text/plain";
                    return "404 Not Found";
                }
                else
                {
                    // Directory exists but no index.md found, return 404
                    context.Response.StatusCode = 404;
                    context.Response.ContentType = "text/plain";
                    return "404 Not Found";
                }
            }
        }

        private static NavigationItem? GetNavigationItemByPath(List<NavigationItem>? items, string relativePath)
        {
            if (items == null)
            {
                return null;
            }
            foreach (var item in items)
            {
                if (string.Equals(item.RelativePath, relativePath, StringComparison.Ordinal))
                {
                    return item;
                }
                if (item.Children != null)
                {
                    var found = GetNavigationItemByPath(item.Children, relativePath);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }

        private static string RenderMarkdown(string markdown)
        {
            return Markdig.Markdown.ToHtml(markdown);
        }

        private static string GenerateHtml(NavigationItem navItem, string renderedContent)
        {
            var sidebar = GenerateSidebar(navItem, _scanner.NavigationTree);
            var title = navItem.Name.Replace(".md", "");
            var html = new StringBuilder();
            html.Append("<!DOCTYPE html>");
            html.Append("<html lang=\"en\">");
            html.Append("<head>");
            html.Append("<meta charset=\"UTF-8\">");
            html.Append("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
            html.Append($"<title>{title}</title>");
            html.Append("<link rel=\"stylesheet\" href=\"/css/style.css\">");
            html.Append("<script src=\"/js/toggle.js\"></script>");
            html.Append("<style>");
            html.Append("* { margin: 0; padding: 0; box-sizing: border-box; }");
            html.Append("body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; line-height: 1.6; color: #333; }");
            html.Append(".container { display: flex; min-height: 100vh; }");
            html.Append(".sidebar { width: 280px; background: #f5f5f5; padding: 1rem; overflow-y: auto; position: fixed; height: 100vh; }");
            html.Append(".main-content { margin-left: 280px; flex: 1; padding: 2rem; }");
            html.Append(".sidebar h3 { margin-bottom: 1rem; color: #555; }");
            html.Append(".nav-item { padding: 0.5rem 0.75rem; cursor: pointer; border-radius: 4px; margin-bottom: 0.25rem; }");
            html.Append(".nav-item:hover { background: #e0e0e0; }");
            html.Append(".nav-item.active { background: #4a90d9; color: white; }");
            html.Append(".nav-item .folder-indicator { margin-right: 0.5rem; }");
            html.Append(".nav-item.collapsed > .children { display: none; }");
            html.Append(".search-container { margin-bottom: 1rem; }");
            html.Append(".search-container input { width: 100%; padding: 0.5rem; border: 1px solid #ccc; border-radius: 4px; font-size: 0.9rem; }");
            html.Append(".search-container input:focus { outline: none; border-color: #4a90d9; }");
            html.Append(".content { max-width: 800px; margin: 0 auto; }");
            html.Append("code { background: #f4f4f4; padding: 0.2rem 0.4rem; border-radius: 3px; font-family: 'Courier New', monospace; }");
            html.Append("pre { background: #f4f4f4; padding: 1rem; border-radius: 4px; overflow-x: auto; }");
            html.Append("pre code { background: none; padding: 0; }");
            html.Append("h1, h2, h3, h4, h5, h6 { margin: 1.5em 0 0.75em; }");
            html.Append("h1 { font-size: 2em; border-bottom: 1px solid #eee; }");
            html.Append("h2 { font-size: 1.5em; border-bottom: 1px solid #eee; }");

            // Raw/Render Toggle Styles
            html.Append(".toggle-container { position: sticky; top: 0; background: white; padding: 1rem; border-bottom: 1px solid #ddd; z-index: 100; }");
            html.Append(".toggle-container h3 { margin: 0 0 0.5rem; font-size: 0.9rem; color: #666; }");
            html.Append(".toggle-switch { display: flex; align-items: center; gap: 0.5rem; }");
            html.Append(".toggle-label { font-size: 0.9rem; color: #333; }");
            html.Append(".toggle-switch input[type='checkbox'] { display: none; }");
            html.Append(".toggle-slider { position: relative; width: 50px; height: 24px; background: #ccc; border-radius: 12px; transition: background 0.3s; }");
            html.Append(".toggle-slider::before { content: ''; position: absolute; width: 18px; height: 18px; left: 3px; top: 3px; background: white; border-radius: 50%; transition: left 0.3s; }");
            html.Append(".toggle-switch input[type='checkbox']:checked + .toggle-slider { background: #4a90d9; }");
            html.Append(".toggle-switch input[type='checkbox']:checked + .toggle-slider::before { left: 27px; }");

            html.Append("</style>");
            html.Append("</head>");
            html.Append("<body>");
            html.Append("<div class=\"container\">");
            html.Append("<div class=\"sidebar\">");
            html.Append("<h3>Navigation</h3>");
            html.Append(GenerateSearchInput());
            html.Append(GenerateFilterScript(_scanner.NavigationTree));
            html.Append(GenerateHighlightCurrentPage(navItem, _scanner.NavigationTree));
            html.Append(sidebar);
            html.Append("</div>");
            html.Append("<div class=\"main-content\">");
            html.Append("<div class=\"content\">");

            // Raw/Render Toggle
            html.Append("<div class=\"toggle-container\">");
            html.Append("<h3>View Mode</h3>");
            html.Append("<div class=\"toggle-switch\">");
            html.Append("<label class=\"toggle-label\">");
            html.Append("Toggle between raw markdown and rendered HTML");
            html.Append("</label>");
            html.Append("<input type='checkbox' id='raw-render-toggle' checked>");
            html.Append("<span class=\"toggle-slider\"></span>");
            html.Append("</div>");
            html.Append("</div>");

            // Raw content (shown when toggle is checked)
            html.Append("<div id=\"raw-content\">");
            html.Append(renderedContent);
            html.Append("</div>");

            // Rendered content (shown when toggle is unchecked)
            html.Append("<div id=\"rendered-content\">");
            html.Append(renderedContent);
            html.Append("</div>");

            html.Append("</div>");
            html.Append("</div>");
            html.Append("</div>");
            html.Append("</body>");
            html.Append("</html>");
            return html.ToString();
        }

        private static string GenerateSidebar(NavigationItem navItem, List<NavigationItem> items)
        {
            var sidebarItems = new List<string>();

            // Add current item
            var currentName = EscapeHtml(navItem.Name);
            sidebarItems.Add($"<div class='nav-item active'><span class='folder-indicator'></span>{currentName}</div>");

            // Add children with expand/collapse functionality
            if (navItem.Children != null)
            {
                foreach (var child in navItem.Children)
                {
                    var childName = EscapeHtml(child.Name);
                    var childRelativePath = child.RelativePath.Replace("/", "_");
                    var hasChildren = child.Children != null && child.Children.Count > 0;
                    var arrow = hasChildren ? "▾" : "";

                    var childDiv = $"<div class='nav-item'><span class='folder-indicator'>{arrow}</span>{childName}</div>";

                    if (hasChildren)
                    {
                        // Add click handler for folders
                        var handler = GenerateNavigationHandler(child.RelativePath, child.RelativePath.Replace("'", "'\\''"), child.RelativePath.Replace("/", "_"));
                        sidebarItems.Add(childDiv);
                        sidebarItems.Add(handler);
                        sidebarItems.Add(GenerateSidebar(child, items));
                    }
                    else
                    {
                        // Add click handler for files
                        var handler = GenerateNavigationHandler(child.RelativePath, child.RelativePath.Replace("'", "'\\''"), child.RelativePath.Replace("/", "_"));
                        sidebarItems.Add(childDiv);
                        sidebarItems.Add(handler);
                    }
                }
            }

            return string.Join("\n", sidebarItems);
        }

        private static string GenerateNavigationHandler(string path, string escapedPath, string safePath)
        {
            var handler = $"<script>document.getElementById('nav-{safePath}').addEventListener('click', function() {{ window.location.href = '{escapedPath}' }});</script>";
            // Add folder toggle functionality for folders
            var toggleScript = $"<script>document.getElementById('nav-{safePath}').addEventListener('click', function(e) {{ e.stopPropagation(); var sibling = this.nextElementSibling; if (sibling && sibling.classList.contains('nav-item')) {{ sibling.classList.toggle('collapsed'); sibling.querySelector('.folder-indicator').textContent = sibling.classList.contains('collapsed') ? '▶' : '▾'; }} }});</script>";
            return toggleScript + handler;
        }

        private static string GenerateHighlightCurrentPage(NavigationItem currentNavItem, List<NavigationItem> tree)
        {
            var navItems = new List<string>();
            foreach (var item in tree)
            {
                var relativePath = item.RelativePath.Replace("/", "_");
                var navId = $"nav-{relativePath}";
                var isActive = string.Equals(item.RelativePath, currentNavItem.RelativePath, StringComparison.Ordinal);
                var activeClass = isActive ? "active" : "";
                navItems.Add($"<div id='{navId}' class='nav-item {activeClass}'>");
                if (item.Children != null && item.Children.Count > 0)
                {
                    navItems.Add("<span class='folder-indicator'>▾</span>");
                }
                navItems.Add(EscapeHtml(item.Name));
                navItems.Add("</div>");
            }
            var joined = string.Join("\n", navItems);
            return $"<script>var navItems = [{joined}]; function highlightCurrentPage() {{ var currentPath = document.getElementById('current-path').textContent; document.querySelectorAll('.nav-item').forEach(el => {{ el.classList.remove('active'); }}; Array.from(navItems).forEach(item => {{ if (item.textContent.includes(currentPath)) {{ el.classList.add('active'); }} }}); }}; highlightCurrentPage();</script>";
        }

        private static string EscapeHtml(string input)
        {
            return _htmlEncoder.Encode(input);
        }

        private static bool ValidatePath(string path, string root)
        {
            // Resolve the path to its canonical form
            try
            {
                var resolvedPath = Path.GetFullPath(path);
                var resolvedRoot = Path.GetFullPath(root);

                // Ensure the resolved path starts with the resolved root
                return resolvedPath.StartsWith(resolvedRoot, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private static string GenerateSearchInput()
        {
            return @"
<div class='search-container'>
    <input type='text' id='sidebar-search' placeholder='Search navigation...' oninput='filterSidebar()' />
</div>
<script>
function navigateTo(path) {{
    window.location.href = path;
}}
</script>";
        }

        private static string GenerateFilterScript(List<NavigationItem> tree)
        {
            var navItems = new List<string>();
            foreach (var item in tree)
            {
                navItems.Add(item.RelativePath);
                if (item.Children != null)
                {
                    navItems.AddRange(item.Children.Select(child => child.RelativePath));
                }
            }
            var filtered = string.Join(";", navItems);
            return $"<script>var allNavItems = [{filtered}]; function filterSidebar() {{ var input = document.getElementById('sidebar-search').value.toLowerCase(); document.querySelectorAll('.nav-item').forEach(el => {{ el.style.display = el.textContent.toLowerCase().includes(input) ? 'block' : 'none'; }}); }};</script>";
        }
    }
}

