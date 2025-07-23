using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class OpenAiService
{
    private readonly string _apiKey;
    public OpenAiService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> ConsultarOpenAI(string prompt)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        var body = new
        {
            model = "gpt-3.5-turbo",
            messages = new[] { new { role = "user", content = prompt } },
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
}
