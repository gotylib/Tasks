using Tasks.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = Tasks.Data.User;


namespace Tasks.Services;

public class BotService
{
    private readonly ITelegramBotClient _botClient;
    private readonly TasksDbContext _context;

    public BotService(ITelegramBotClient botClient, TasksDbContext context)
    {
        _botClient = botClient;
        _context = context;
    }

    public async Task HandleUpdateAsync(Update update, CancellationToken cancellationToken)
    {
        if (update.Message is { } message && message.Type == MessageType.Text)
        {
            if (message.Text == "/hello")
            {
                var chatId = message.Chat.Id;
                var user = new User { ChatId = chatId };

                if (!_context.Users.Any(u => u.ChatId == chatId))
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(cancellationToken);
                    await _botClient.SendTextMessageAsync(chatId, "Вы подписались на уведомления!", cancellationToken: cancellationToken);
                }
                else
                {
                    await _botClient.SendTextMessageAsync(chatId, "Вы уже подписаны на уведомления!", cancellationToken: cancellationToken);
                }
            }
        }
    }

    public async Task NotifyUsersAsync(CancellationToken cancellationToken)
    {
        var users = _context.Users.ToList();
        var tasks = _context.TaskItems.ToList();

        foreach (var task in tasks)
        {
            var timeSpan = task.TaskDateTime - DateTime.UtcNow;

            switch (timeSpan.TotalDays)
            {
                // Если задача просрочена, удаляем её
                case < 0:
                    _context.TaskItems.Remove(task);
                    continue; // Переходим к следующей задаче
                // Если задача заканчивается в ближайшие 5 дней, уведомляем пользователей
                case <= 5:
                {
                    foreach (var user in users)
                    {
                        await _botClient.SendTextMessageAsync(user.ChatId, $"Задача '{task.Name}' заканчивается через {(int)timeSpan.TotalDays} дней. Описание: {task.Description}", cancellationToken: cancellationToken);
                    }

                    break;
                }
            }
        }

        // Сохраняем изменения в базе данных (удаление просроченных задач)
        await _context.SaveChangesAsync(cancellationToken);
    }
}