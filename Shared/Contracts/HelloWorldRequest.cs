namespace BreezeBlockGames.PackageBase.Shared.Contracts
{
    public record HelloWorldRequest(string Name)
    {
        public string Name { get; } = Name;
    }
}