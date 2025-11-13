using Microsoft.AspNetCore.Mvc;
using CocktailWebApplication.Services;
using CocktailWebApplication.Models;
using System.Text.Json;
using System;
using System.Text.Json.Nodes;
using System.Reflection.Metadata.Ecma335;

public class CocktailService
{
    private readonly HttpClient _httpClient;
    private readonly Translator _translator;
    private readonly Cache _koCacheManager;
    private readonly Cache _enCacheManager;

    public CocktailService(IHttpClientFactory httpClientFactory, Translator translator, KoCacheManager koCacheManager, EnCacheManager enCacheManager)
    {
        _httpClient = httpClientFactory.CreateClient("TheCocktailDbClient");
        _translator = translator;
        _koCacheManager = koCacheManager;
        _enCacheManager = enCacheManager;
    }

    public async Task<DrinkResponse?> Random()
    {
        string url = "https://www.thecocktaildb.com/api/json/v2/1/random.php";
        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
    }

    public async Task<DrinkResponse?> LookupCocktailById(string id)
    {
        Drink? drink = _koCacheManager.GetCocktailOnCache(id);
        if (drink != null)
        {
            return new DrinkResponse() { drinks = new List<Drink>() { drink } };
        }
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/lookup.php?i={id}";
        return await GetDrinkFromApi(url, true);
    }

    public async Task<TaxonomyResponse?> ListCategories()
    {
        string url = "https://www.thecocktaildb.com/api/json/v2/1/list.php?c=list";
        string? json = await GetFromAPI(url);
        if (json == null)
        {
            return new TaxonomyResponse();
        }

        await _translator.GetTranslationFromJson("categories", "strCategory", json);
        List<TaxonomyItem> items = await GetTraslateResponse("categories", (id, label) => new TaxonomyItem
        {
            id = id,
            labelKo = label
        });
        return new TaxonomyResponse { items = items };

    }

    public async Task<TaxonomyResponse?> ListGlasses()
    {
        string url = "https://www.thecocktaildb.com/api/json/v2/1/list.php?g=list";
        string? json = await GetFromAPI(url);
        if (json == null)
        {
            return new TaxonomyResponse();
        }

        await _translator.GetTranslationFromJson("glass", "strGlass", json);
        List<TaxonomyItem> items = await GetTraslateResponse("glass", (id, label) => new TaxonomyItem
        {
            id = id,
            labelKo = label
        });
        return new TaxonomyResponse { items = items };
    }

    public async Task<TaxonomyResponse?> ListIngredients()
    {
        string url = "https://www.thecocktaildb.com/api/json/v2/1/list.php?i=list";
        string? json = await GetFromAPI(url);
        if (json == null)
        {
            return new TaxonomyResponse();
        }

        await _translator.GetTranslationFromJson("ingredient", "strIngredient1", json);
        List<TaxonomyItem> items = await GetTraslateResponse("ingredient", (id, label) => new TaxonomyItem
        {
            id = id,
            labelKo = label
        });
        return new TaxonomyResponse { items = items };
    }

    public async Task<TaxonomyResponse?> ListAlcoholic()
    {
        string url = "https://www.thecocktaildb.com/api/json/v2/1/list.php?a=list";
        string? json = await GetFromAPI(url);
        if (json == null)
        {
            return new TaxonomyResponse();
        }

        await _translator.GetTranslationFromJson("alcoholic", "strAlcoholic", json);
        List<TaxonomyItem> items = await GetTraslateResponse("alcoholic", (id, label) => new TaxonomyItem
        {
            id = id,
            labelKo = label
        });
        return new TaxonomyResponse { items = items };
    }

    public async Task<DrinkResponse> AllSearchByQuery(List<string>? q)
    {
        string path = ".\\data\\cocktails_en.json";
        string jsonString = File.ReadAllText(path);
        List<Drink> matchingDrinks = new List<Drink>();
        List<string> searchList = new List<string>() { "strDrink", "strCategory", "strAlcoholic"};
        for(int i = 1; i <= 15; i++)
        {
            searchList.Add("strIngredient" + i);
        }

        if (jsonString == null) return new DrinkResponse();
        JsonObject jsonObject = JsonNode.Parse(jsonString).AsObject();

        if (jsonObject["drinks"]!.AsArray() is JsonArray drinksArray)
        {
            foreach (var node in drinksArray)
            {
                bool isMatchFound = false;
                if (node is JsonObject drinkObject)
                {
                    foreach(string list in searchList)
                    {
                        JsonNode propertyNode = drinkObject[list]!;
                        if (propertyNode is JsonValue drinkValue && drinkValue.GetValue<string>() is string drinkValueString)
                        {
                            foreach (string searchQ in q)
                            {
                                if (drinkValueString.Equals(searchQ, StringComparison.OrdinalIgnoreCase))
                                {
                                    string str = drinkObject.ToJsonString();
                                    Drink drinkModel = JsonSerializer.Deserialize<Drink>(str);

                                    if (drinkModel != null)
                                    {
                                        // List<Drink>에 Drink 모델 객체를 추가
                                        matchingDrinks.Add(drinkModel);
                                        isMatchFound = true;
                                        break; // 검색 성공 시 즉시 탈출
                                    }
                                }
                            }
                            if (isMatchFound)
                            {
                                break;
                            }

                        }

                    }
                }
            }
        }
        return new DrinkResponse { drinks = matchingDrinks };

    }

    public async Task<DrinkResponse> Filter(List<string>? q, string? alcoholic, string? category, string? glass, List<string>? ingredient, string? strength)
    {
        List<Task<DrinkResponse>> tasks = new List<Task<DrinkResponse>>();

        if (q != null)
        {
            tasks.Add(AllSearchByQuery(q));
        }
        if (alcoholic != null)
        {
            tasks.Add(FilterByAlcohol(alcoholic));
        }
        if (category != null)
        {
            tasks.Add(FilterByCategory(category));
        }
        if (glass != null)
        {
            tasks.Add(FilterByGlass(glass));
        }
        if (ingredient != null)
        {
            tasks.Add(FilterByIngredients(ingredient));
        }

        DrinkResponse[] responses = await Task.WhenAll(tasks);

        responses = responses.Where(r => r?.drinks != null).ToArray();

        return await Intersect(responses.ToList());
    }

    private async Task<DrinkResponse?> SearchByName(string name)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/search.php?s={name}";

        return await GetDrinkFromApi(url);
    }

    private async Task<DrinkResponse?> SearchByFirstLetter(char letter)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/search.php?f={letter}";
        return await GetDrinkFromApi(url);
    }

    private async Task<string?> SearchIngredientByName(string name)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?i={name}";
        return await GetFromAPI(url);
    }

    private async Task<DrinkResponse?> LookupIngredientById(int id)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/lookup.php?iid={id}";
        return await GetDrinkFromApi(url);
    }

    private async Task<DrinkResponse> FilterByIngredient(string ingredient)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/filter.php?i={ingredient}";
        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
    }

    private async Task<DrinkResponse> FilterByIngredients(List<string> ingredients)
    {
        string str = string.Join(",", ingredients);
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/filter.php?i={str}";

        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
    }

    private async Task<DrinkResponse> Intersect(List<DrinkResponse> drinkResponses)
    {
        if (drinkResponses == null || drinkResponses.Count == 0)
        {
            return new DrinkResponse { drinks = new List<Drink>() };
        }

        var sets = new List<HashSet<string>>();

        foreach (var response in drinkResponses)
        {
            var drinkIdSet = new HashSet<string>();

            if (response.drinks != null)
            {
                foreach (var drink in response.drinks)
                {
                    if (!string.IsNullOrEmpty(drink.idDrink))
                    {
                        drinkIdSet.Add(drink.idDrink);
                    }
                }
            }

            if (drinkIdSet.Count > 0)
            {
                sets.Add(drinkIdSet);
            }
        }

        if (!sets.Any())
        {
            return new DrinkResponse { drinks = new List<Drink>() };
        }

        var intersection = new HashSet<string>(sets.First());
        foreach (var set in sets.Skip(1))
        {
            intersection.IntersectWith(set);
        }

        if (intersection.Count == 0)
        {
            return new DrinkResponse { drinks = new List<Drink>() };
        }

        var detailTasks = intersection.Select(id => LookupCocktailById(id));
        var detailResponses = await Task.WhenAll(detailTasks);

        var drinks = new List<Drink>();
        foreach (var response in detailResponses)
        {
            if (response?.drinks != null)
            {
                drinks.AddRange(response.drinks);
            }
        }

        return new DrinkResponse { drinks = drinks };
    }

    private async Task<DrinkResponse> FilterByAlcohol(string alcohol)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/filter.php?a={alcohol}";
        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
    }

    private async Task<DrinkResponse> FilterByCategory(string category)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/filter.php?c={category}";
        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
    }

    private async Task<DrinkResponse> FilterByGlass(string glass)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/filter.php?g={glass}";
        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
    }


    private DrinkResponse GetCocktailOnCache(DrinkResponse drinkResponse)
    {
        if (drinkResponse.drinks.Any())
        {
            DrinkResponse cachedDrinkResponse = new DrinkResponse();
            foreach (var drink in drinkResponse.drinks)
            {
                Drink? temp = _koCacheManager.GetCocktailOnCache(drink.idDrink);
                if(temp != null)
                {
                    cachedDrinkResponse.drinks.Add(temp);
                }
            }
            if (!cachedDrinkResponse.drinks.Any()) return cachedDrinkResponse;
        }
        return new DrinkResponse();
    }

    private async Task<string?> GetFromAPI(string url)
    {
        await Task.Delay(100);
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            Log.Error("API request fail");
            return null;
        }

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<DrinkResponse?> GetDrinkFromApi(string url, bool willSaveCache = false)
    {
        await Task.Delay(100);
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            Log.Error("API request fail");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(json))
        {
            return new DrinkResponse();
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            DrinkResponse? result = JsonSerializer.Deserialize<DrinkResponse>(json, options);
            if (result == null) return new DrinkResponse();
            DrinkResponse cachedResult = GetCocktailOnCache(result);

            if(cachedResult.drinks.Any())
            {
                return cachedResult;
            }

            if (willSaveCache && result != null)
            {
                DrinkResponse drinkResponse = await _translator.TranslateResponse(result);
                foreach (var d in drinkResponse.drinks)
                {
                    _koCacheManager.AddDrink(d);
                }

                foreach (var d in result.drinks)
                {
                    _enCacheManager.AddDrink(d);
                }
                return drinkResponse;
            }
            return result ?? new DrinkResponse();
        }
        catch (JsonException ex)
        {
            Log.Error($"Deserialization fail: {ex.Message}");
            return null;
        }
    }

    private async Task<DrinkResponse> DetailInfoCocktail(DrinkResponse? drinkResponse)
    {
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        var fetchTasks = new List<Task<DrinkResponse?>>();

        foreach (Drink drink in drinkResponse.drinks)
        {
            if (!string.IsNullOrEmpty(drink.idDrink))
            {
                fetchTasks.Add(LookupCocktailById(drink.idDrink));
            }
        }
        DrinkResponse?[] fullResponses = await Task.WhenAll(fetchTasks);
        var finalDrinksList = new List<Drink>();
        foreach (var response in fullResponses)
        {
            if (response?.drinks != null)
            {
                finalDrinksList.AddRange(response.drinks);
            }
        }
        return new DrinkResponse { drinks = finalDrinksList };
    }

    private async Task<List<T>> GetTraslateResponse<T>(string sectionKey, Func<string, string, T> factory)
    {
        string path = ".\\data\\translation_ko.json";

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"{path} 파일을 찾을 수 없습니다.");
        }

        string jsonText = await File.ReadAllTextAsync(path);
        JsonNode? jsonData = JsonNode.Parse(jsonText);

        JsonObject? section = jsonData?[sectionKey]?.AsObject();
        if (section is null)
        {
            throw new Exception($"'{sectionKey}' 섹션을 찾을 수 없습니다.");
        }

        var result = new List<T>();

        foreach (var kvp in section)
        {
            string id = kvp.Key;
            string labelKo = kvp.Value?.ToString() ?? string.Empty;
            result.Add(factory(id, labelKo));
        }

        return result;
    }

    private async Task<List<string>> LoadJsonTranslation()
    {
        string path = ".\\data\\translation_ko.json";
        var englishKeys = new List<string>();
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

        if (jsonData is JsonObject jsonObject)
        {
            if (jsonObject.TryGetPropertyValue("ingredient", out var ingredientNode) && ingredientNode is JsonObject ingredientObject)
            {
                foreach (var property in ingredientObject)
                {
                    englishKeys.Add(property.Key);
                }
            }
        }

        return englishKeys;
    }

    private async Task<TaxonomyResponse> ListIngredientsCategories()
    {
        var collectedTypes = new List<string>();
        var fetchTasks = new List<Task<string?>>();

        var t = await LoadJsonTranslation();

        foreach (var name in t)
        {
            if (string.IsNullOrWhiteSpace(name)) continue;

            fetchTasks.Add(SearchIngredientByName(name));
        }

        string?[] allJsons = await Task.WhenAll(fetchTasks);

        foreach (var json in allJsons)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                continue;
            }

            try
            {
                var response = JsonSerializer.Deserialize<IngredientResponse>(json);

                string? type = response?.ingredients?
                                       .FirstOrDefault(i => !string.IsNullOrWhiteSpace(i.strType))
                                       ?.strType;

                if (!string.IsNullOrWhiteSpace(type))
                {
                    string trimmedType = type.Trim();
                    if (!collectedTypes.Contains(trimmedType))
                    {
                        collectedTypes.Add(trimmedType);
                    }
                }
            }
            catch (JsonException ex)
            {
                Log.Error(ex.Message);
            }
        }

        var taxonomyItems = new List<TaxonomyItem>();

        foreach (var englishType in collectedTypes)
        {
            taxonomyItems.Add(new TaxonomyItem
            {
                id = englishType,
                labelKo = englishType
            });
        }
        var resultWrapper = new
        {
            ingredients = taxonomyItems
        };

        string json1 = JsonSerializer.Serialize(resultWrapper, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await _translator.GetTranslationFromJson("ingredient-categories", "id", json1, "ingredients");
        List<TaxonomyItem> items = await GetTraslateResponse("ingredient-categories", (id, label) => new TaxonomyItem
        {
            id = id,
            labelKo = label
        });
        return new TaxonomyResponse { items = items };
    }

    //private async Task<CocktailResponse> FilterByIngredientCategories()
    //{

    //}
}