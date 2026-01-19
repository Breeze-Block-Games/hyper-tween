using BreezeBlockGames.PackageBase.Shared.Contracts;
using BreezeBlockGames.PackageBase.Shared.Services;

namespace BreezeBlockGames.PackageBase.Service.Endpoints
{
    public class HelloWorldEndpoint(HelloWorldService helloWorldService)
    {
        public IResult Handle(HttpContext context, HelloWorldRequest request)
        {
            var ipAddress = context.Connection.RemoteIpAddress;
        
            if (ipAddress == null)
            {
                return Results.BadRequest(new { error = "Could not determine client IP address." });
            }

            var message = helloWorldService.GetMessage(request.Name, ipAddress);
            var response = new HelloWorldResponse(message);
        
            return Results.Ok(response);
        }
    }
}