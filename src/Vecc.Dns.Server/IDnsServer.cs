namespace Vecc.Dns.Server
{
    public interface IDnsServer
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
