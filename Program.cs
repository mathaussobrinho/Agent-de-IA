using Discord;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

class Program
{
    private DiscordSocketClient _client;
    private OpenAiService _openAiService;
    private CnpjService _cnpjService;

    static async Task Main(string[] args)
    {
        var program = new Program();
        await program.MainAsync();
    }

    public async Task MainAsync()
    {
        _openAiService = new OpenAiService("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");
        _cnpjService = new CnpjService("cf69d5b1-cce2-4aff-9c99-cfd14bf7bcf3-c37627ab-a344-4b33-9084-8c632b8637d2");

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged |
                             GatewayIntents.MessageContent |
                             GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages
        });

        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceivedAsync;

        await _client.LoginAsync(TokenType.Bot, "MTM5NzU4MzQ2ODkwODUxMTI4Mg.Gg4HSE.dwn6NlYqCq9_4HiaoL0hNXBrVbpoPcJhGEl5ik");
        await _client.StartAsync();

        await Task.Delay(-1);
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }

    private async Task MessageReceivedAsync(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        Console.WriteLine($"Mensagem recebida: {message.Content} de {message.Author.Username}");

        var msg = message.Content.Trim();

        if (msg.StartsWith("!oi"))
        {
            await message.Channel.SendMessageAsync($"Olá, {message.Author.Username}! 👋");
        }
        else if (msg.StartsWith("!ia "))
        {
            var pergunta = msg.Substring(4);
            var resposta = await _openAiService.ConsultarOpenAI(pergunta);
            if (resposta.Length > 2000)
                resposta = resposta.Substring(0, 1990) + "... [truncado]";
            await message.Channel.SendMessageAsync($"```{resposta}```");
        }
        else if (msg.StartsWith("!cnpj "))
        {
            var cnpj = msg.Substring(6).Trim().Replace(".", "").Replace("/", "").Replace("-", "");
            var resultado = await _cnpjService.ConsultarCNPJ(cnpj);
            if (resultado.Length > 2000)
                resultado = resultado.Substring(0, 1990) + "... [truncado]";
            await message.Channel.SendMessageAsync($"```{resultado}```");
        }
    }
}
