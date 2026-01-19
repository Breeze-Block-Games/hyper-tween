using System.Net;

namespace BreezeBlockGames.PackageBase.Shared.Services
{
    public class HelloWorldService
    {
        public string GetMessage(string name, IPAddress ipAddress) =>
            $"Hi, {name}. You're visiting from {ipAddress}!";
    }
}