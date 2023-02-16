using Discord.Commands;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Alyx;

class Program
{
    // The Main method is the entry point of the program.
    static void Main(string[] args) =>
        new Program().RunBotAsync().GetAwaiter().GetResult();

    // DiscordSocketClient is the main class of the Discord.Net library.
    private DiscordSocketClient _client;
    // CommandService is the main class of the Discord.Net.Commands library.
    private CommandService _commands;
    // IServiceProvider is a built-in .NET Core class that allows us to use dependency injection.
    private IServiceProvider _services;

    // The RunBotAsync method is the entry point of the Bot.
    public async Task RunBotAsync()
    {
        // We build the configuration here to access the appsettings.json.
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        // Here we initialize the DiscordSocketClient and set the LogSeverity to Debug.
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.DirectMessages | GatewayIntents.MessageContent
        });

        _commands = new CommandService();
        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .BuildServiceProvider();

        // The token is a unique string that allows your bot to connect to Discord.
        string token = config.GetSection("DiscordToken").Value;       

        // Here we subscribe the logging handler to both the client and the CommandService.
        _client.Log += _client_Log;

        // Here we subscribe the async event handlers to their respective events.
        await RegisterCommandsAsync();

        // Here we connect to Discord.
        await _client.LoginAsync(TokenType.Bot, token);

        // Here we start the listening process.
        await _client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);

    }

    // The LogAsync method is used to log any errors that may occur.
    private Task _client_Log(LogMessage arg)
    {
        Console.WriteLine(arg);
        return Task.CompletedTask;
    }

    // The RegisterCommandsAsync method is where we will connect to Discord and initialize our Command Handler.
    public async Task RegisterCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    // HandleCommandAsync recieves a socket message and inforces a prefix before an async command execution.
    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Message is a SocketUserMessage, which is a SocketMessage with more information.
        var message = messageParam as SocketUserMessage;

        // Context is an object that contains information about the message, such as the author, channel, etc.
        var context = new SocketCommandContext(_client, message);

        // We don't want the bot to respond to itself or other bots.
        if (message.Author.IsBot) return;

        int argPos = 0;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands.
        if (message.HasStringPrefix("!", ref argPos))
        {
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);
        }

    }




}
