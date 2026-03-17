# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Markdown Viewer** (.NET 10.0) that scans a directory for markdown files and creates a navigable, locally hosted website from them. It uses Kestrel web server to serve markdown content with static assets.

**Current State**: Core routing and rendering infrastructure is implemented. Scanner.cs and NavigationItem.cs were deleted in a previous commit.

## Architecture

### Core Components (Current)

1. **Program.cs** - Main entry point that:
   - Creates a Router with root directory path
   - Builds Kestrel web server listening on localhost:5001
   - Routes all requests through a Content handler that generates HTML content

2. **Router.cs** - Maps HTTP request paths to file paths in the filesystem with:
   - Path traversal protection (blocks `../`, `./`, `%` encoding, `?`, `#`)
   - Static asset routing for `/css/*` and `/js/*`
   - File existence checks to detect folders vs files

3. **Renderer.cs** - Renders content based on route type:
   - Static assets (CSS, JS) served with correct content types
   - Markdown files rendered using Markdig library
   - Folder routes resolve to `index.md`
   - Internal markdown link resolution (relative paths only)

4. **RouterTests.cs** - 14 unit tests covering:
   - Path mapping for root, index, markdown files, folders
   - Static asset routing for CSS and JS
   - Security: path traversal, query strings, URI fragments, encoding

### File Structure

```
/home/agent/hello-world/
├── sample/                    # Sample markdown content directory
│   ├── index.md
│   ├── page1.md
│   └── topic/
│       └── topic1.md
├── src/md-view/
│   ├── md-view.csproj         # .NET project file
│   ├── Program.cs             # Main entry point
│   ├── Router.cs              # HTTP path to file mapping
│   ├── Renderer.cs            # Content rendering
│   └── wwwroot/               # Static assets
│       └── js/toggle.js
└── src/tests/
    └── RouterTests.cs         # Unit tests
```

## Commands

### Build and Run

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Run with specific directory path
dotnet run -- --root /path/to/markdown/files
```

### Development

```bash
# Run with watch for hot reload
dotnet watch run

# Run tests
dotnet test

# Run specific tests
dotnet test --filter "FullyQualifiedName~RouterTests"
```

## Routing Behavior

### Static Assets (`/css/*`, `/js/*`)
- Route matches paths starting with `/css/` or `/js/`
- Resolves to `wwwroot/css/*` or `wwwroot/js/*`
- Returns `IsStaticAsset = true`
- Only `.css` and `.js` extensions are served as static assets

### Markdown Files (`/*.md`)
- Route matches paths ending in `.md`
- Resolves to `<root>/<path>.md`
- Returns `IsFolder = false`, `IsStaticAsset = false`

### Folders (`/*/`)
- Route matches paths without file extension
- Returns `IsFolder = true` (checks if file doesn't exist)
- Resolves to `<root>/<path>`

### Security Restrictions
The following are blocked and throw `NotSupportedException`:
- Path traversal: `../`, `./`, `/.`
- Query strings: `?`
- URI fragments: `#`
- URI encoding: `%`

## Configuration

### Project Settings (src/md-view/md-view.csproj)

- **Target Framework**: net10.0
- **Output Type**: Exe
- **Packages**:
  - Markdig 1.1.0 (markdown rendering)
  - Microsoft.AspNetCore.App 2.2.8
  - Microsoft.AspNetCore.Server.Kestrel 2.3.9
  - Microsoft.Extensions.Hosting 10.0.3

### Default Configuration

- **Root Directory**: `/home/agent/hello-world/sample`
- **Server**: Kestrel
- **Host**: localhost
- **Port**: 5001

## Development Patterns

### Adding New Static Assets

1. Place files in `src/md-view/wwwroot/css/` or `src/md-view/wwwroot/js/`
2. Update CSS/JS files in wwwroot
3. Assets are automatically served at `/css/<filename>` or `/js/<filename>`

### Adding New Markdown Files

1. Place `.md` files in the sample directory (or configured root)
2. The Router automatically discovers them
3. Files are rendered at `/<filename>.md`

### Adding Unit Tests

Tests are located in `src/tests/RouterTests.cs`
- Use `[Fact]` attribute for test methods
- Tests verify Router.Map() behavior
- Tests cover normal paths and security restrictions

## TODO Items

1. **Scanner.cs** - Previously deleted, needs to be recreated for:
   - Recursive directory scanning for `.md` files
   - Build nested navigation tree structure
   - Parse YAML frontmatter from markdown files
   - Generate sidebar navigation HTML

2. **NavigationItem.cs** - Previously deleted, needs to be recreated:
   - Model for markdown files/folders
   - Properties: Name, FilePath, Children, Content, RelativePath, Frontmatter

3. **Renderer Template** - Currently placeholder:
   - `src/md-view/wwwroot/html/main.html` needs to be created
   - Should include sidebar navigation template
   - Support for title, aside, and main content

4. **Full Content Generation** - Currently placeholder:
   - `Renderer.cs` has `var title = "TODO"` and `var aside = "TODO"`
   - Needs proper HTML template rendering
   - Sidebar should show navigation tree from Scanner

## Security Notes

The Router implements basic security:
- Validates all request paths before processing
- Blocks directory traversal attempts
- Blocks query strings and URI fragments
- Blocks URI-encoded characters

When implementing features:
- Continue validating and sanitizing all user requests
- Prevent directory traversal attacks
- Don't expose internal file system paths in responses
- Consider implementing rate limiting for public endpoints

## Notes

- The project is in early development stage
- Core routing and rendering infrastructure is functional
- Scanner and NavigationItem models were deleted and need to be recreated
- HTML template file needs to be created
- Markdig library is used for markdown rendering
- Unit tests cover Router functionality comprehensively

## Test Coverage

| Test | Description |
|------|-------------|
| `Map_Root_ReturnsIsFolder` | Root `/` maps to sample directory |
| `Map_IndexMd_ReturnsIndexMdAbsolutePath` | `/index.md` maps to full file path |
| `Map_StaticAssetCss_ReturnsCss` | `/css/style.css` is a static asset |
| `Map_StaticAssetJs_ReturnsJs` | `/js/toggle.js` is a static asset |
| `Map_StaticAssetJpg_IsNotSupported` | `/images/test.jpg` is NOT a static asset |
| `Map_FileMd_ReturnsFileMd` | `/page1.md` maps to markdown file path |
| `Map_StaticAsset_IsStaticAsset` | Verifies css files are static assets |
| `Map_FileMd_IsNotStaticAsset` | Verifies md files are NOT static assets |
| `Map_FileMd_IsNotFolder` | Verifies md files are NOT folders |
| `Map_Folder_IsNotStaticAsset` | `/topic/` is a folder, not static asset |
| `Map_PathTraversal_Throws` | `../` path traversal throws exception |
| `Map_QueryString_Throws` | `?` query string throws exception |
| `Map_UriFragment_Throws` | `#` fragment throws exception |
| `Map_UriEndcoded_Throws` | `%` URI encoding throws exception |
