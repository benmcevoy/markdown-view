# wikd

A lightweight local web server for viewing markdown documents and other file types directly from your filesystem.

(Pretty much) All my projects and profeessional work rely on markdown for documentation and knowledge bases.

I want a nice way to view the documentation that is 100% local and does not require pushing to source control...

[Copy Party](https://github.com/9001/copyparty) does a great job with it's built in markdown viewer, but all I wanted is a clean, dark markdown viewer, with Mermaid support.

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
Usage: wikd [path-to-folder] [commands]

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
./wikd .

# Serve from a specific directory on port 8080
./wikd /path/to/your/files -p 8080

# Specify a custom port
./wikd . --port 3000

```

The server will listen on `http://127.0.0.1:{port}` and serve files directly from your filesystem.

### CLI Commands

- `--port <port>`: HTTP port to listen on
- `--help`: Display help information

```
██╗    ██╗██╗██╗  ██╗██████╗ 
██║    ██║██║██║ ██╔╝██╔══██╗
██║ █╗ ██║██║█████╔╝ ██║  ██║
██║███╗██║██║██╔═██╗ ██║  ██║
╚███╔███╔╝██║██║  ██╗██████╔╝
 ╚══╝╚══╝ ╚═╝╚═╝  ╚═╝╚═════╝ 
                             
Base path: '/markdown-view/sample'
Starting server.
Listening on: http://127.0.0.1:5001
```


## Development

Requires [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

### Building

```bash
dotnet build
```

### Regenerate Assets

Update embedded assets and templates.

```
cd src/wikd/Templates/
dotnet t4 Assets.tt
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
