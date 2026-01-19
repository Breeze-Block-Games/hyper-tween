namespace BreezeBlockGames.PackageBase.Shared.Contracts
{
    public record HelloWorldResponse(string Message)
    {
        public string Message { get; } = Message;
    }
}