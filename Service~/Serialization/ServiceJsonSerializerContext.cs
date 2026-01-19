using System.Text.Json.Serialization;
using BreezeBlockGames.PackageBase.Shared.Contracts;

namespace BreezeBlockGames.PackageBase.Service.Serialization
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(HelloWorldRequest))]
    [JsonSerializable(typeof(HelloWorldResponse))]
    internal partial class ServiceJsonSerializerContext : JsonSerializerContext
    {
    }
}