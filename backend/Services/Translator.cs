using CocktailWebApplication.Models;
using OpenAI.Chat;
using System.Data;
using System.Text;
using System.Text.Json;
namespace CocktailWebApplication.Services
{
    public class Translator
    {
        private readonly HttpClient _httpClient;
        private readonly ChatClient _client;
        public Translator(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _client = new(model: "gpt-4o-mini", apiKey: Settings.OPEN_AI_API);
        }

        public async Task<string> TranslateToKoreanWithGpt(string sourceText)
        {
            string systemPrompt = "You are a professional bartender and translator. Translate the following English text into Korean recipe instructions.";
            ChatCompletion response = await _client.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text;
        }

        public async Task<string> ExplainCocktail(string sourceText)
        {
            string systemPrompt = "You are a cocktail expert.Explain the cocktail briefly in Korean in one short paragraph (2~3 sentences).\nDo not include recipe or ingredients — only describe what kind of drink it is and its characteristics.\nOutput format:\n[cocktail name]\n[a brief description]";
            ChatCompletion response = await _client.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text;
        }

        public async Task<string> TranslateToKoreanWithPapago(string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText)) return sourceText;

            var content = new StringContent(
                $"source=en&target=ko&text={Uri.EscapeDataString(sourceText)}",
                Encoding.UTF8,
                "application/x-www-form-urlencoded"
            );
            var request = new HttpRequestMessage(HttpMethod.Post, Settings.PAPAGO_API_URL)
            {
                Content = content
            };
            request.Headers.Add("x-ncp-apigw-api-key-id", Settings.X_NCP_APIGW_API_KEY_ID);
            request.Headers.Add("x-ncp-apigw-api-key", Settings.X_NCP_APIGW_API_KEY);
            request.Headers.Add("User-Agent", "Mozilla/5.0 (compatible; DotNetApp/1.0)");

            HttpResponseMessage response = await _httpClient.SendAsync(request);

            string json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Log.Error("Translation API request fail");
                return "None";
            }

            try
            {
                using (JsonDocument document = JsonDocument.Parse(json))
                {
                    return document.RootElement
                        .GetProperty("message")
                        .GetProperty("result")
                        .GetProperty("translatedText").GetString() ?? sourceText;
                }
            }
            catch (JsonException)
            {
                Log.Error("Translation result parsing fail");
                return "None";
            }
        }

        public async Task<DrinkResponse> TranslateDrinkResponseAsync(DrinkResponse response)
        {
            if (response?.drinks == null)
            {
                return response ?? new DrinkResponse();
            }

            var translationTasks = response.drinks.Select(async drink =>
            {
                if (drink == null) return;

                if (!string.IsNullOrEmpty(drink.strInstructions))
                {
                    drink.strInstructions = await TranslateToKoreanWithGpt(drink.strInstructions);
                }

            }).ToList();

            await Task.WhenAll(translationTasks);

            return response;
        }
    }
}