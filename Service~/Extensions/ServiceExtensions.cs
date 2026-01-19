using BreezeBlockGames.PackageBase.Service.Endpoints;
using BreezeBlockGames.PackageBase.Shared.Services;

namespace BreezeBlockGames.PackageBase.Service.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddHelloWorldServices(this IServiceCollection services)
        {
            return services
                .AddSingleton<HelloWorldService>()
                .AddSingleton<HelloWorldEndpoint>();
        }
    }
}