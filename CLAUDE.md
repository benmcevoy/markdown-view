# CLAUDE.md

  This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

  ## Build, Test, and Run Commands

  - **Build**: `dotnet build src/md-view/md-view.csproj`
  - **Run**: `dotnet run --project src/md-view/md-view.csproj -- sample`
  - **Test**: `dotnet test src/tests/tests.csproj`
  - **Run single test**: `dotnet test src/tests/tests.csproj --filter
  "FullyQualifiedName=RouterTests.Map_PathTraversal_Throws"`
  - **Format code**: `dotnet format`
  - **Publish (self-contained)**: `dotnet publish src/md-view/md-view.csproj -c Release -p:PublishSingleFile=true
  -p:SelfContained=true`

  ## Project Structure

```
  hello-world/
  ├── src/md-view/           # Main web application (.NET 10 Web API)
  │   ├── Program.cs         # Entry point, Kestrel host configuration
  │   ├── Router.cs          # Maps HTTP paths to filesystem paths
  │   ├── FileSystemInfoService.cs  # Builds folder/file tree
  │   └── Templates/         # HTML templates and CSS/JS assets
  ├── src/tests/            # Unit tests (xUnit)
  └── sample/               # Sample markdown content for testing
```

  ## Architecture

  **MdView** is a static file viewer that serves markdown documents with syntax highlighting, code coloring, and Mermaid
  diagram support. Key components:

  - **Router**: Converts HTTP request paths (e.g., `/page1.md`) to filesystem paths with security validation (blocks path
  traversal, query strings, fragments)
  - **FileSystemInfoService**: Recursively builds a tree of folders and files, filtering by extension (default: `.md`)
  - **Renderer**: Dispatches files to appropriate handlers (Markdown, Code, Image, PDF, Folder)
  - **Handlers**: Implement `IRenderingHandler` interface to render content (MarkdownFileRendererHandler uses Markdig +
  Markdown.ColorCode)

  ## Key Files

  - `src/md-view/Program.cs`: Main entry point, sets up Kestrel on localhost:5001, opens browser automatically
  - `src/md-view/Router.cs`: Path resolution and security validation (`IsValidRequest`, `ResolvePath`)
  - `src/md-view/Rendering/Renderer.cs`: Renders HTML with title, navigation, breadcrumbs, and content
  - `src/md-view/Templates/DefaultTemplate.cs`: String template engine using `{{placeholder}}` syntax
  - `src/md-view/Templates/Assets.cs`: Embedded CSS and JavaScript (Mermaid.js, Markdown.ColorCode)

  ## Development Notes

  - Uses .NET 10 with AOT publishing enabled (`PublishAot=true`)
  - Tests are xUnit-based in `src/tests/`
  - Default sample folder: `/home/agent/hello-world/sample`
  - Markdown rendering uses Markdig 1.1.1 + Markdown.ColorCode 3.0.1
  - Images in markdown should be converted to inline data:base64 for proper rendering
  - TODOs in `Program.cs` include: admin page, config file support, error handling, security middleware

  ## Security Considerations

  - Path traversal is blocked (checks for `../`, `./`, `\`)
  - Query strings and URL fragments are forbidden
  - Input validation in `Router.IsValidRequest()`
  - TODO: Add proper middleware for CORS, rate limiting, request logging
