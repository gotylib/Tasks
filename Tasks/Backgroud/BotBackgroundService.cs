using Tasks.Services;

namespace Tasks.Backgroud;

public class BotBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public BotBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var botService = scope.ServiceProvider.GetRequiredService<BotService>();
                await botService.NotifyUsersAsync(stoppingToken);
            }
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken); // Проверяем каждый день
        }
    }
}