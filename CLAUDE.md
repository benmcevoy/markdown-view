# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a **Markdown Viewer** (.NET 10.0) that scans a directory for markdown files and creates a navigable, locally hosted website from them. It uses Kestrel web server to serve markdown content with a sidebar navigation.

## Architecture

### Core Components

1. **Scanner.cs** - Recursively scans directories for `.md` files and builds a nested navigation structure
2. **NavigationItem.cs** - Model representing a markdown file/folder with properties: Name, FilePath, Children, Content, RelativePath
3. **Program.cs** - Main entry point that:
   - Creates a Scanner with root directory path
   - Builds Kestrel web server listening on localhost:5000
   - Routes all requests through a single handler that generates HTML content
4. **Markdig** library - Used for rendering markdown to HTML

### Request Flow

```
Request → Program.Content() → Generate HTML with:
  - Sidebar navigation (from Scanner navigation tree)
  - Main content area (rendered markdown)
  - Handle 404 for invalid paths
```

### File Structure

```
/home/agent/hello-world/
├── sample/                    # Sample markdown content directory
│   ├── index.md
│   ├── page1.md
│   └── topic/
│       └── topic1.md
└── src/
    ├── md-view.csproj         # .NET project file
    ├── Program.cs             # Main entry point
    ├── Scanner.cs             # Directory scanner
    └── Models/
        └── NavigationItem.cs  # Navigation model
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

# Run a single test (if tests exist)
dotnet test --filter "FullyQualifiedName~TestName"

# Build for specific framework
dotnet build -f net10.0
```

### Debug

```bash
# Set breakpoint and run
dotnet run --debugger

# Attach debugger
dotnet debug
```

## Development Patterns

### Adding New Markdown Files

1. Place `.md` files in the sample directory (or configured root)
2. The Scanner automatically discovers them recursively
3. No code changes needed - navigation updates automatically

### Modifying the Scanner

The Scanner builds a tree structure:
- Files: `NavigationItem` with `Name`, `FilePath`, `Content`, `RelativePath`, `Frontmatter`
- Directories: `NavigationItem` with `Children` list
- Recursive scan for subdirectories
- YAML frontmatter is parsed and stored in `Frontmatter` property
- Content property contains markdown content without frontmatter

### Content Generation in Program.cs

The `Content()` method handles request routing:
- Maps request path to file in navigation tree
- Renders markdown using Markdig
- Generates sidebar HTML
- Handles index/default file logic
- Returns 404 for invalid paths

## Configuration

### Project Settings (src/md-view.csproj)

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
- **Port**: 5000

## TODO Items (from TODO.md)

High-priority tasks:
1. Scanner implementation (parse .md files, build navigation, frontmatter support) - **COMPLETE**
2. Web server routes (/, /folder/file.md, static assets, 404 handling)
5. Internal markdown links (resolve relative links, update navigation)
6. Security (validate requests, prevent directory traversal, rate limiting)

Lower-priority:
3. Sidebar navigation enhancements (highlight current, search/filter)
4. Raw/render toggle (raw markdown vs rendered HTML, syntax highlighting)

## Security Notes

When implementing features:
- Validate and sanitize all user requests
- Prevent directory traversal attacks (check paths against root)
- Implement rate limiting for public endpoints
- Don't expose internal file system paths in responses

## Notes

- The project is in early development stage
- Current implementation has placeholder content generation
- Markdig library is used for markdown rendering
- Scanner loads file content into NavigationItem.Content and strips YAML frontmatter
- NavigationItem now includes Frontmatter property (Dictionary<string, string>) for YAML metadata

## YAML Frontmatter Pattern

- Frontmatter is enclosed between `---` markers
- Parse key: value pairs (skip empty lines and comments starting with `#`)
- Strip quotes from values if they start and end with `"` or `'`
- Frontmatter is optional; if not present, entire file content is treated as markdown
- Use `TryParseFrontmatter()` helper method for safe parsing that returns false if no frontmatter found
