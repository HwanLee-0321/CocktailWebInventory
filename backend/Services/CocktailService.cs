using Microsoft.AspNetCore.Mvc;
using CocktailWebApplication.Services;
using CocktailWebApplication.Models;
using System.Text.Json;
using System;
using System.Text.Json.Nodes;

public class CocktailService
{
    private readonly HttpClient _httpClient;
    private readonly TranslatorService _translator;
    private readonly CacheManager _koCacheManager;
    private readonly CacheManager _enCacheManager;

    public CocktailService(IHttpClientFactory httpClientFactory, TranslatorService translator, KoCacheManager koCacheManager, EnCacheManager enCacheManager)
    {
        _httpClient = httpClientFactory.CreateClient("TheCocktailDbClient");
        _translator = translator;
        _koCacheManager = koCacheManager;
        _enCacheManager = enCacheManager;
    }

    public async Task<DrinkResponse?> SearchByName(string name)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/search.php?s={name}";

        return await GetDrinkFromApi(url);
    }

    public async Task<DrinkResponse?> SearchByFirstLetter(char letter)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/search.php?f={letter}";
        return await GetDrinkFromApi(url);
    }

    public async Task<DrinkResponse?> Random()
    {
        string url = "https://www.thecocktaildb.com/api/json/v2/1/random.php";
        return await GetDrinkFromApi(url, true);
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

    public async Task<DrinkResponse?> LookupIngredientById(int id)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/lookup.php?iid={id}";
        return await GetDrinkFromApi(url);
    }

    public async Task<DrinkResponse> FilterByIngredient(string ingredient)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/filter.php?i={ingredient}";
        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
    }

    public async Task<DrinkResponse> FilterByIngredients(List<string> ingredients)
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

    public async Task<DrinkResponse> Intersect(List<DrinkResponse> drinkResponses)
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
                sets.Add(drinkIdSet); // ← 여기 추가해야 교집합 가능
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

    public async Task<DrinkResponse> FilterByAlcohol(string alcohol)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/filter.php?a={alcohol}";
        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
    }

    public async Task<DrinkResponse> FilterByCategory(string category)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/filter.php?c={category}";
        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
    }

    public async Task<DrinkResponse> FilterByGlass(string glass)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v2/1/filter.php?g={glass}";
        DrinkResponse? drinkResponse = await GetDrinkFromApi(url);
        if (drinkResponse == null || drinkResponse.drinks == null)
        {
            return new DrinkResponse();
        }

        return await DetailInfoCocktail(drinkResponse);
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
            label = label
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
            label = label
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
            label = label
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
            label = label
        });
        return new TaxonomyResponse { items = items };
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
                DrinkResponse drinkResponse = await TranslateToKorean(result);
                //DrinkResponse drinkResponse = result;
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

    private async Task<DrinkResponse> TranslateToKorean(DrinkResponse drinkResponse)
    {
        Drink translateDrink = drinkResponse.drinks.First();
        Drink translatedDrink = new Drink(translateDrink);
        DrinkResponse translatedResponse = new DrinkResponse { drinks = new List<Drink> { translatedDrink } };

        //foreach(Drink drink in drinkResponse.drinks)
        //{
        //    translateDrink.strDrink = _translator.Translate(drink.strDrink!);
        //    translateDrink.strCategory = _translator.Translate(drink.strCategory!);
        //    translateDrink.strAlcoholic = _translator.Translate(drink.strAlcoholic!);
        //    translateDrink.strGlass = _translator.Translate(drink.strGlass!);

        //    translateDrink.strIngredient1 = _translator.Translate(drink.strIngredient1!);
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient2!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient3!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient4!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient5!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient6!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient7!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient8!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient9!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient10!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient11!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient12!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient13!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient14!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient15!);
        //    for (int i = 0; i < 15; i++)
        //    {
        //        var ingredientProp = typeof(Drink).GetProperty($"strIngredient{i + 1}");
        //        if (ingredientProp != null)
        //        {
        //            ingredientProp.SetValue(translatedDrink, _translator.Translate(ingredients[i]));

        //        }
        //    }

        //    for (int i = 0; i < 15; i++)
        //    {
        //        var measureProp = typeof(Drink).GetProperty($"strMeasure{i + 1}");
        //        if (measureProp != null)
        //            measureProp.SetValue(translatedDrink, _translator.Translate(measures[i]));
        //    }


        //}
        //Task<string> instructionsTask = _translator.ToKoreanSentenceWithGpt(translateDrink.strInstructions!);
        Task<string> descriptionTask = _translator.ExplainCocktail(translateDrink.strDrink!);

        //var ingredientTasks = new List<Task<string>>
        //{
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient1!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient2!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient3!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient4!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient5!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient6!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient7!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient8!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient9!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient10!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient11!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient12!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient13!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient14!),
        //    _translator.Translate(drinkResponse.drinks.First().strIngredient15!)
        //};

        //var measureTasks = new List<Task<string>>
        //{
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure1!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure2!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure3!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure4!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure5!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure6!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure7!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure8!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure9!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure10!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure11!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure12!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure13!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure14!),
        //    _translator.Translate(drinkResponse.drinks.First().strMeasure15!)
        //};


        //await Task.WhenAll(
        //    drinkTask, categoryTask, alcoholicTask, glassTask, instructionsTask, descriptionTask,// ingredientTask, measureTask
        //    Task.WhenAll(ingredientTasks),
        //    Task.WhenAll(measureTasks)
        //);


        //translatedDrink.strDrink = drinkTask.Result;
        //translatedDrink.strCategory = categoryTask.Result;
        //translatedDrink.strAlcoholic = alcoholicTask.Result;
        //translatedDrink.strGlass = glassTask.Result;
        //translatedDrink.strInstructions = instructionsTask.Result;
        translatedDrink.strDescription = descriptionTask.Result;

        //for (int i = 0; i < 15; i++)
        //{
        //    var ingredientProp = typeof(Drink).GetProperty($"strIngredient{i + 1}");
        //    var measureProp = typeof(Drink).GetProperty($"strMeasure{i + 1}");

        //    ingredientProp?.SetValue(translatedDrink, ingredientTasks[i].Result);
        //    measureProp?.SetValue(translatedDrink, measureTasks[i].Result);
        //}

        return translatedResponse;
    }

    public async Task<List<string>> TranslateIngredientsInBatch(Drink originalDrink, string str)
    {
        var ingredients = new List<string>();

        for (int i = 1; i <= 15; i++)
        {
            var propertyName = $"{str}{i}";

            var propertyInfo = typeof(Drink).GetProperty(propertyName);
            string? ingredient = propertyInfo?.GetValue(originalDrink) as string;

            if (!string.IsNullOrWhiteSpace(ingredient) && !ingredient.Equals("None", StringComparison.OrdinalIgnoreCase))
            {
                ingredients.Add(ingredient);
            }
        }

        string ingredientsJson = System.Text.Json.JsonSerializer.Serialize(ingredients);

        var translatedJson = await _translator.ToKoreanWordsWithGpt(ingredientsJson);
        try
        {
            return JsonSerializer.Deserialize<List<string>>(translatedJson) ?? new List<string>();
        }
        catch (JsonException)
        {
            Log.Error("Not json format");
            return new List<string>();
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

    public async Task<List<T>> GetTraslateResponse<T>(string sectionKey, Func<string, string, T> factory)
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

    public async Task<DrinkResponse> Filter(string? alcoholic, string? category, string? glass, List<string>? ingredient, string? strength)
    {
        List<Task<DrinkResponse>> tasks = new List<Task<DrinkResponse>>();

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
        DrinkResponse result = await Intersect(responses.ToList());

        return result;
    }
}