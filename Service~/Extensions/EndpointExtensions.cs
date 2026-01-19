using BreezeBlockGames.PackageBase.Service.Endpoints;
using BreezeBlockGames.PackageBase.Shared.Contracts;

namespace BreezeBlockGames.PackageBase.Service.Extensions
{
    public static class EndpointExtensions
    {
        public static void MapHelloWorldEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/v1/hello", (HttpContext context, HelloWorldEndpoint endpoint, [AsParameters] HelloWorldRequest request)
                    => endpoint.Handle(context, request))
                .Produces<HelloWorldResponse>();
        }
    }
}