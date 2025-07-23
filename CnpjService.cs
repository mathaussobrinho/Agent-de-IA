using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

public class CnpjService
{
    private readonly string _apiKey;
    public CnpjService(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<string> ConsultarCNPJ(string cnpj)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", _apiKey);

        var url = $"https://open.cnpja.com/office/{cnpj}";
        var response = await client.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return $"Erro ao consultar CNPJ. Código: {response.StatusCode}";

        var json = await response.Content.ReadAsStringAsync();
        var dados = JObject.Parse(json);

        var sb = new StringBuilder();
        sb.AppendLine($"🔍 **Consulta CNPJ**");
        sb.AppendLine($"Razão Social: {dados["company"]?["name"]}");
        sb.AppendLine($"CNPJ: {dados["taxId"]}");
        sb.AppendLine($"Situação: {dados["status"]?["text"]}");
        sb.AppendLine($"Fundação: {dados["founded"]}");
        sb.AppendLine();
        sb.AppendLine("📍 **Endereço**:");
        sb.AppendLine($"{dados["address"]?["street"]}, Nº {dados["address"]?["number"]} - {dados["address"]?["district"]}");
        sb.AppendLine($"{dados["address"]?["city"]} - {dados["address"]?["state"]}, CEP: {dados["address"]?["zip"]}");
        sb.AppendLine();
        sb.AppendLine("📞 **Telefones**:");
        if (dados["phones"] != null)
        {
            foreach (var phone in dados["phones"])
                sb.AppendLine($"{phone["type"]}: ({phone["area"]}) {phone["number"]}");
        }
        sb.AppendLine();
        sb.AppendLine("✉️ **Emails**:");
        if (dados["emails"] != null)
        {
            foreach (var email in dados["emails"])
                sb.AppendLine($"{email["ownership"]}: {email["address"]}");
        }
        sb.AppendLine();
        sb.AppendLine($"🏢 **Atividade Principal:** {dados["mainActivity"]?["text"]}");
        return sb.ToString();
    }
}
