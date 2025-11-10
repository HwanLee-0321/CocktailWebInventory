using CocktailWebApplication.Models;
using OpenAI.Chat;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace CocktailWebApplication.Services
{
    public class TranslatorService
    {
        private readonly HttpClient _httpClient;
        private readonly ChatClient _client1;
        private readonly ChatClient _client2;
        public TranslatorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _client1 = new(model: "gpt-4o", apiKey: Settings.OPEN_AI_API);
            _client2 = new(model: "gpt-3.5-turbo", apiKey: Settings.OPEN_AI_API);
        }

        public async Task<string> ToKoreanSentenceWithGpt(string sourceText)
        {
            string systemPrompt = "You are a professional bartender and translator. Translate the following English text into Korean recipe instructions.";
            ChatCompletion response = await _client1.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text;
        }
        public async Task<string> ToKoreanNameWithGpt(string sourceText)
        {
            string systemPrompt = "You are an expert translator specializing in converting cocktail and foreign beverage names into standard Korean transliteration (Eoreorae Pyo-gi beop). Your ONLY task is to provide the most natural and commonly accepted Korean reading/sound for the name provided by the user. Output the translated name ONLY.";
            ChatCompletion response = await _client2.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text;
        }
        public async Task<string> ToKoreanWordsWithGpt(string sourceText)
        {
            if (string.IsNullOrEmpty(sourceText)) return "";
            string systemPrompt =@"
You are a professional bartender and translator. 
Your task is to translate English cocktail ingredients and their measurement units into natural Korean, preserving meaning and readability. 

Guidelines:
1. Translate ingredient names naturally into Korean. Use commonly accepted terminology or transliteration if there is no standard Korean name.
2. Convert measurement units into Korean-friendly units when possible (e.g., ""oz"" → ""온스"", ""tsp"" → ""티스푼"", ""ml"" → ""밀리리터"", ""dash"" → ""대시"").
3. Maintain the numerical values exactly as given.
4. Keep the output concise and readable.
5. Preserve the JSON array structure exactly as it is.
6. Return only the translated JSON array. Do not add explanations, comments, or extra text.
7. Example:
   Input: [""1 oz Lime Juice"", ""2 tsp Sugar"", ""Malibu rum""]
   Output: [""라임 주스 1온스"", ""설탕 2티스푼"", ""말리부 럼""]

Input:
{input_text_here}

Output:
";
            ChatCompletion response = await _client2.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text;
        }

        public async Task<string> ExplainCocktail(string sourceText)
        {
            string systemPrompt = "You are a cocktail expert.Explain the cocktail briefly in Korean in one short paragraph (2~3 sentences).\nDo not include recipe or ingredients — only describe what kind of drink it is and its characteristics.\nOutput format:\n[cocktail name]\n[a brief description]";
            ChatCompletion response = await _client1.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text;
        }

        public async Task<string> ToKoreanWordsWithPapago(string sourceText)
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

        public async Task GetTranslationFromJson(string writeStr, string readStr, string apiJson)
        {
            string path = ".\\data\\translation_ko.json";

            JsonNode jsonData;
            if (File.Exists(path))
            {
                string existingJson = await File.ReadAllTextAsync(path);
                jsonData = JsonNode.Parse(existingJson) ?? new JsonObject();
            }
            else
            {
                jsonData = new JsonObject();
            }

            if (jsonData[writeStr] == null)
            {
                jsonData[writeStr] = new JsonObject();
            }

            JsonObject categoryObj = jsonData[writeStr]!.AsObject();

            JsonNode? apiData = JsonNode.Parse(apiJson);
            JsonArray? drinks = apiData?["drinks"]?.AsArray();

            if (drinks == null)
            {
                return;
            }
            
            foreach (JsonNode? drink in drinks)
            {
                string? category = drink?[readStr]?.ToString();
                if (!string.IsNullOrEmpty(category))
                {
                    if (!categoryObj.ContainsKey(category))
                    {
                        categoryObj[category] = category;
                    }
                }
            }

            await File.WriteAllTextAsync(
                path,
                JsonSerializer.Serialize(jsonData, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                })
            );
        }
    }
}