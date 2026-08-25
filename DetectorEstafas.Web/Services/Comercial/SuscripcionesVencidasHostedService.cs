namespace DetectorEstafas.Web.Services.Comercial;

public sealed class SuscripcionesVencidasHostedService :
    BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public SuscripcionesVencidasHostedService(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        using PeriodicTimer timer = new(
            TimeSpan.FromHours(1));

        while (!stoppingToken.IsCancellationRequested)
        {
            await ProcesarAsync(stoppingToken);

            try
            {
                if (!await timer.WaitForNextTickAsync(
                        stoppingToken))
                {
                    break;
                }
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task ProcesarAsync(
        CancellationToken cancellationToken)
    {
        using IServiceScope scope =
            _scopeFactory.CreateScope();

        IComercializacionApiService service =
            scope.ServiceProvider
                .GetRequiredService<IComercializacionApiService>();

        await service.AplicarSuspensionesVencidasAsync(
            cancellationToken);
    }
}
