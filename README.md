
# Discord Bot CNPJ + OpenAI

Bot para Discord escrito em C# que permite:

- Consultar dados de CNPJs usando a API pública do [CNPJá](https://open.cnpja.com/)
- Conversar com o bot usando comandos que interagem com a API da OpenAI (GPT-3.5 Turbo)
- Comandos simples como `!oi` para saudação

---

## Funcionalidades

| Comando    | Descrição                          |
|------------|----------------------------------|
| `!cnpj <cnpj>` | Consulta informações de um CNPJ específico |
| `!ia <pergunta>` | Faz uma pergunta ao modelo GPT-3.5 Turbo e retorna resposta |
| `!oi`      | Saudação simples do bot           |

---

## Pré-requisitos

- [.NET 7.0 SDK](https://dotnet.microsoft.com/download)
- Conta no Discord e token do bot configurado
- Chaves de API para:
  - [CNPJá](https://open.cnpja.com/)
  - [OpenAI](https://platform.openai.com/)

---

## Como usar

1. Clone o repositório:

   ```bash
   git clone https://github.com/seu-usuario/seu-repo.git
   cd seu-repo
   ```

2. Abra o arquivo `Program.cs` e configure suas chaves API:

   ```csharp
   private string _discordToken = "SEU_TOKEN_DISCORD";
   private string _openAiApiKey = "SUA_CHAVE_OPENAI";
   private string _cnpjaApiKey = "SUA_CHAVE_CNPJA";
   ```

3. Compile e execute o bot:

   ```bash
   dotnet run
   ```

4. No Discord, envie mensagens no canal onde o bot está para testar:

   - `!oi`
   - `!cnpj 00000000000191`
   - `!ia Quem foi Albert Einstein?`

---

## Estrutura do código

- `MainAsync()` — Inicializa o bot, configura eventos
- `MessageReceivedAsync()` — Lida com as mensagens recebidas, dispara comandos
- `ConsultarOpenAI()` — Chama API da OpenAI para gerar respostas
- `ConsultarCNPJ()` — Chama API do CNPJá para consultar dados do CNPJ

---

## Testes

Este projeto possui testes unitários usando [xUnit](https://xunit.net/) (opcional).

Para rodar os testes:

```bash
dotnet test BotTests/
```

---

