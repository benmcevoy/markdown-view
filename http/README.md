# http

Basic HTTP client and daemon library. Intended for locally hosted use when Kestrel feels like too much of a dependency.

Adequate for basic HTTP requests, passing JSON around.

## Features

- supports `GET` **and** `POST` requests!
- supports query string and headers!
- Body stream is simply passed along to caller
- basic path-traversal protection on incoming requests (rejects `./`, `/.`, `\`)
- handler exceptions are caught and returned as a server error response

## Missing features

- only GET and POST
- no cookie support
- no certificates
- no TLS support
- no chunked encoding body
- and many other things missing

## Usage

### Daemon

Listen on a specified port.

Daemon must be given an `IPAddress` and port, typically `System.Net.IPAddress.Loopback`.

The `RequestHandler` must be provided — a function that accepts a `Request` and returns a `Response`.

`public required Func<Request, Response> RequestHandler { get; init; }`

```csharp
var daemon = new http.Daemon(ipAddress, port)
{
    RequestHandler = MyRequestReceived
};

await daemon.Start(cancellationToken);

http.Response MyRequestReceived(http.Request request) => handle the request and return a response;
```

### Client

Send a request to a specified host and port.

Client must be given an `IPAddress` and port, typically `System.Net.IPAddress.Loopback`.

```csharp
var client = new http.Client(ipAddress, port);

var response = await client.SendAsync(request);
```