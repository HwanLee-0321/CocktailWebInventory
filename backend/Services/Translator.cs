using CocktailWebApplication.Models;
using OpenAI.Chat;
using Sprache;
using System.Data;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace CocktailWebApplication.Services
{
    public class Translator
    {
        private readonly JsonDocument _jsonDoc;
        private readonly HttpClient _httpClient;
        private readonly ChatClient _client1;
        private readonly ChatClient _client2;
        public Translator(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _client1 = new(model: "gpt-4o", apiKey: Settings.OPEN_AI_API);
            _client2 = new(model: "gpt-3.5-turbo", apiKey: Settings.OPEN_AI_API);
            var jsonText = File.ReadAllText(Constants.translationFilePath);
            _jsonDoc = JsonDocument.Parse(jsonText);
        }

        public async Task<string> TranslateWithPapago(string sourceText)
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

        private string TranslateWithJson(string input)
        {
            foreach (var section in _jsonDoc.RootElement.EnumerateObject())
            {
                var category = section.Value;
                foreach (var prop in category.EnumerateObject())
                {
                    if (prop.Name.Equals(input, StringComparison.OrdinalIgnoreCase))
                    {
                        return prop.Value.GetString() ?? input;
                    }
                }
            }

            return input;
        }

        private async Task<string> Translate(string input)
        {
            string result = TranslateWithJson(input);
            if(result == input)
            {
                result = await TranslateWithPapago(result);
            }

            return result;
        }

        public async Task<DrinkResponse> TranslateResponse(DrinkResponse inputResponse)
        {
            var drinkTasks = inputResponse.drinks.Select(async drink =>
            {
                var outputDrink = new Drink(drink);

                outputDrink.strDrink = await TranslateNameWithGpt(drink.strDrink!);
                outputDrink.strCategory = await Translate(drink.strCategory!);
                outputDrink.strGlass = await Translate(drink.strGlass!);
                outputDrink.strAlcoholic = await Translate(drink.strAlcoholic!);

                for (int i = 1; i <= 15; i++)
                {
                    var propIngredient = typeof(Drink).GetProperty($"strIngredient{i}");
                    if (propIngredient?.GetValue(drink) is string valIngredient && !string.IsNullOrWhiteSpace(valIngredient))
                    {
                        propIngredient.SetValue(outputDrink, await Translate(valIngredient));
                    }

                    var propMeasure = typeof(Drink).GetProperty($"strMeasure{i}");
                    if (propMeasure?.GetValue(drink) is string valMeasure && !string.IsNullOrWhiteSpace(valMeasure))
                    {
                        propMeasure.SetValue(outputDrink, await TranslateIngridentWithGpt(valMeasure));
                    }
                }
                outputDrink.strInstructions = await Translate(drink.strInstructions!);

                outputDrink.strDescription = await ExplainCocktail(drink.strDrink!);

                return outputDrink;
            });

            var outputDrinks = await Task.WhenAll(drinkTasks);

            return new DrinkResponse
            {
                drinks = outputDrinks.ToList()
            };
        }

        public async Task<string> TranslateInstructionWithGpt(string sourceText)
        {
            string systemPrompt = "You are a professional bartender and translator. Translate the following English text into Korean recipe instructions.";
            ChatCompletion response = await _client1.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text.Replace("\n", "").Trim();
        }
        public async Task<string> TranslateNameWithGpt(string sourceText)
        {
            string systemPrompt = "Transliterate the cocktail or beverage name into natural Korean pronunciation.\r\nOutput only Korean characters (no English, no parentheses).";
            ChatCompletion response = await _client2.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text.Replace("\n", "").Trim();
        }
        public async Task<string> TranslateIngridentWithGpt(string sourceText)
        {
            string systemPrompt = "Translate the cocktail ingredient into natural Korean.\r\n- Always place the quantity and unit at the beginning (e.g., \"2샷\", \"1 1/3 온스\").\r\n- Do NOT add extra counting words like \"개\".\r\n- Translate descriptive words (like Fresh, Sweet) into natural Korean adjectives before the ingredient (e.g., \"달콤한\", \"신선한\").\r\n- Keep the full quantity, unit, and ingredient together.\r\n- Output only the translated line.";
            ChatCompletion response = await _client2.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text.Replace("\n", "").Trim();
        }

        public async Task<string> ExplainCocktail(string sourceText)
        {
            string systemPrompt = "Explain the cocktail briefly in Korean in 1 paragraph (4~5 sentences). Do not include the name, recipe, or ingredients — only describe the drink and its characteristics. Output only the description in Korean.";
            ChatCompletion response = await _client2.CompleteChatAsync($"{systemPrompt}\n\n{sourceText}");
            return response.Content[0].Text.Replace("\n", "").Trim();
        }

        public async Task GetTranslationFromJson(string writeStr, string readStr, string apiJson, string str = "drinks")
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
            JsonArray? drinks = apiData?[str]?.AsArray();

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