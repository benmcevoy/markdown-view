# MdView

A lightweight local web server for viewing markdown documents and other file types directly from your filesystem.

## Features

- Serve markdown files with syntax highlighting
- Support for code files (with syntax highlighting)
- Support for Mermaid diagrams
- Image rendering
- PDF rendering
- Local-only HTTP server (loopback)

## Installation

See releases for a standalone executable for Linux or Windows.

## Usage

```
Usage: md-view [path-to-folder] [commands]

path-to-folder:
  The path to a folder to serve as a markdown viewer site.

commands:
  -h|--help                         Display help.
  -p|--port <port>                  Specify listen port (Default: 5001), e.g. http://localhost:<port>
```

### Server Mode

Run the server and serve files from a directory:

```bash
# Serve from current directory on port 5001
./md-view .

# Serve from a specific directory on port 8080
./md-view /path/to/your/files -p 8080

# Specify a custom port
./md-view . --port 3000

```

The server will listen on `http://127.0.0.1:{port}` and serve files directly from your filesystem.

### CLI Commands

- `--port <port>`: HTTP port to listen on
- `--help`: Display help information

## Development

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

### Building

```bash
dotnet build
```

### Publish AOT

```bash
dotnet publish
```

### Running Tests

```bash
dotnet test
```

## License

MIT License
