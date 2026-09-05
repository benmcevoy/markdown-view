# http

Basic http client and daemon library.  Intended for locally hosted use when Kestral feels like too much of a dependancy.

Adequate for basic HTTP requests, passing JSON around.

## Features

- supports `GET` **and** `POST` requests!
- supports query string and headers!
- Body stream is simply passed along to caller

## Missing features

- only GET and POST
- no cookie support
- no certificates
- no TLS support
- no chunked encoding body
- and many other things missing

## Usage

### Daemon

Listen on specified port.

Daemon must be given ipAddress and port, typically `System.Net.IPAddress.Loopback`.

The `RequestHandler` must be provided.  `RequestHandler` is a funciton that accepts a `Request` and returns a `Response`.

`public required Func<Request, Response> RequestHandler { get; init; }`

```
var daemon = new http.Daemon(ipAddress, port)
{
    RequestHandler = MyRequestReceived
};

http.Response MyRequestReceived(http.Request request) => handle the request and return a response;
```

### Client

Send message to specified port.

Client must be given ipAddress and port, typically `System.Net.IPAddress.Loopback`.


```
var client = new http.Client(ipAddress, port);

var response = await client.SendAsync(request);

```