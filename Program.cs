using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class Program
{
    private DiscordSocketClient _client;

    // 🔐 Substitua aqui pelos seus tokens reais
    private string _discordToken = "Sua Chave aqui";
    private string _openAiApiKey = "Sua Chave aqui";
    private string _cnpjaApiKey = "Sua Chave aqui";

    static async Task Main(string[] args)
    {
        var program = new Program();
        await program.MainAsync();
    }

    public async Task MainAsync()
    {
        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.AllUnprivileged |
                             GatewayIntents.MessageContent |
                             GatewayIntents.Guilds |
                             GatewayIntents.GuildMessages
        });

        _client.Log += LogAsync;
        _client.MessageReceived += MessageReceivedAsync;

        await _client.LoginAsync(TokenType.Bot, _discordToken);
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

        // Log no console com nome do autor e mensagem
        Console.WriteLine($"Mensagem recebida: {message.Content} de {message.Author.Username}");

        string msg = message.Content.Trim();

        if (msg.StartsWith("!oi"))
        {
            await message.Channel.SendMessageAsync($"Olá, {message.Author.Username}! 👋");
        }
        else if (msg.StartsWith("!ia "))
        {
            string pergunta = msg.Substring(4);
            string resposta = await ConsultarOpenAI(pergunta);
            if (resposta.Length > 2000)
                resposta = resposta.Substring(0, 1990) + "... [truncado]";
            await message.Channel.SendMessageAsync($"```{resposta}```");
        }
        else if (msg.StartsWith("!cnpj "))
        {
            string cnpj = msg.Substring(6).Trim().Replace(".", "").Replace("/", "").Replace("-", "");
            var embed = await ConsultarCNPJ(cnpj);
            if (embed == null)
                await message.Channel.SendMessageAsync("❌ Não foi possível encontrar dados para esse CNPJ.");
            else
                await message.Channel.SendMessageAsync(embed: embed);
        }
    }

    private async Task<string> ConsultarOpenAI(string prompt)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);

        var body = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] {
                new { role = "user", content = prompt }
            },
            max_tokens = 500,
            temperature = 0.7
        };

        var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
        var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);

        if (!response.IsSuccessStatusCode)
            return $"Erro: {response.StatusCode} - {response.ReasonPhrase}";

        var json = await response.Content.ReadAsStringAsync();
        var obj = JObject.Parse(json);
        return obj["choices"]?[0]?["message"]?["content"]?.ToString().Trim() ?? "❌ Resposta vazia.";
    }

    private async Task<Embed> ConsultarCNPJ(string cnpj)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", _cnpjaApiKey);

        var url = $"https://open.cnpja.com/office/{cnpj}";
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        var dados = JObject.Parse(json);

        var company = dados["company"];
        var address = dados["address"];
        var mainActivity = dados["mainActivity"];
        var sideActivities = dados["sideActivities"];
        var members = company?["members"];

        string endereco = $"{address?["street"]}, {address?["number"]}, {address?["district"]} - {address?["city"]}/{address?["state"]}, CEP: {address?["zip"]}";

        string atividadesSecundarias = sideActivities != null && sideActivities.HasValues
            ? string.Join("\n", sideActivities.Select(a => $"- {a["text"]}"))
            : "Nenhuma";

        string diretores = members != null && members.HasValues
            ? string.Join("\n", members.Select(m => $"- {m["person"]?["name"]} ({m["role"]?["text"]})"))
            : "Nenhum";

        var embedBuilder = new EmbedBuilder()
            .WithTitle(company?["name"]?.ToString() ?? "Empresa")
            .WithColor(Color.Blue)
            .AddField("CNPJ", dados["taxId"]?.ToString() ?? "N/D", true)
            .AddField("Natureza Jurídica", company?["nature"]?["text"]?.ToString() ?? "N/D", true)
            .AddField("Situação", dados["status"]?["text"]?.ToString() ?? "N/D", true)
            .AddField("Data de Fundação", DateTime.TryParse((string)dados["founded"], out var founded) ? founded.ToString("dd/MM/yyyy") : "N/D", true)
            .AddField("Atividade Principal", mainActivity?["text"]?.ToString() ?? "N/D", false)
            .AddField("Atividades Secundárias", atividadesSecundarias, false)
            .AddField("Endereço", endereco, false);

        // Email
        var email = dados["emails"]?.FirstOrDefault()?["address"]?.ToString();
        if (!string.IsNullOrEmpty(email))
            embedBuilder.AddField("E-mail", email, true);

        // Telefone
        var phone = dados["phones"]?.FirstOrDefault();
        if (phone != null)
        {
            string telefone = $"({phone["area"]}) {phone["number"]}";
            embedBuilder.AddField("Telefone", telefone, true);
        }

        embedBuilder.AddField("Diretores", diretores, false);

        return embedBuilder.Build();
    }
}
