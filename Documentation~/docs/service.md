# Service

An example [Kestrel](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/servers/kestrel?view=aspnetcore-9.0)
web server, configured with Swagger/ReDoc support. Running the server will automatically open the docs page. The project
depends on Shared~/Shared.csproj such that contracts (request and response DTOs) can be shared between this service and
the Unity client/server.

The package base service has one endpoint to serve as an example, with the following types:

* `HelloWorldEndpoint` - an example endpoint handler
* `HelloWorldService` - an example shared service to encapsulate the logic of formatting the response message. It
can be useful to share such logic between the service and clients in order to perform client-side prediction.
* `EndpointExtensions` - an example endpoint mapper
* `ServiceExtensions` - an example DI installer
* `ServiceJsonSerializerContext` - an example `JsonSerializerContext`
* `Service.http` - an example file that defines endpoints that can be used to manually test endpoints