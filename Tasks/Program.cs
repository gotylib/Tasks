using Microsoft.EntityFrameworkCore;
using System.Configuration;
using Tasks.Backgroud;
using Tasks.Data;
using Tasks.Services;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ITelegramBotClient>(provider => new TelegramBotClient("7519232754:AAGty7tFDD3HZAv1yQ9WlxyUg6tY5lEw_u0"));
builder.Services.AddScoped<BotService>();
builder.Services.AddHostedService<BotBackgroundService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// builder.Services.AddHostedService<BotBackground>();

builder.Services.AddDbContext<TasksDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        builder => builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowSpecificOrigins");
app.UseHttpsRedirection();

app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/")
    {
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.SendFileAsync("wwwroot/index.html");
    }
    else
    {
        await next(); 
    }
});
app.MapControllers();   

var scope = app.Services.CreateScope();
var botClient = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();
var botService = scope.ServiceProvider.GetRequiredService<BotService>();

var receiverOptions = new ReceiverOptions
{
    AllowedUpdates = Array.Empty<UpdateType>()
};

botClient.StartReceiving(
    updateHandler: async (botClient, update, cancellationToken) =>
    {
        await botService.HandleUpdateAsync(update, cancellationToken);
    },
    errorHandler: async (botClient, exception, cancellationToken) =>
    {
        Console.WriteLine($"Error: {exception.Message}");
    },
    receiverOptions: receiverOptions,
    cancellationToken: CancellationToken.None
);

app.Run();
app.Lifetime.ApplicationStopped.Register(() =>
{
    scope.Dispose();
});