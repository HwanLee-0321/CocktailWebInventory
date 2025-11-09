using Microsoft.AspNetCore.Mvc;
using CocktailWebApplication.Services;
using CocktailWebApplication.Models;
using System.Text.Json;

public class CocktailService : ControllerBase
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

    [HttpGet("searchByName")]
    public async Task<DrinkResponse?> SearchByName(string name)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?s={name}";

        return await GetFromApi(url);
    }

    [HttpGet("searchByFirstLetter")]
    public async Task<DrinkResponse?> SearchByFirstLetter(char letter)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?f={letter}";
        return await GetFromApi(url);
    }

    [HttpGet("searchByIngredient")]
    public async Task<DrinkResponse?> SearchByIngredient(string ingredient)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?i={ingredient}";
        return await GetFromApi(url);
    }

    [HttpGet("lookupCocktailById")]
    public async Task<DrinkResponse?> LookupCocktailById(int id)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/lookup.php?i={id}";

        var responseAction = await GetFromApi(url);
        return await GetFromApi(url);
    }

    [HttpGet("lookupIngredientById")]
    public async Task<DrinkResponse?> LookupIngredientById(int id)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/lookup.php?iid={id}";
        return await GetFromApi(url);
    }

    [HttpGet("random")]
    public async Task<DrinkResponse?> Random()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/random.php";
        return await GetFromApi(url);
    }

    [HttpGet("randomTranslated")]
    public async Task<DrinkResponse?> RandomTranslated()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/random.php";
        return await GetFromApi(url);
    }

    //필터링
    [HttpGet("filterByIngredient")]
    public async Task<DrinkResponse?> FilterByIngredient(string ingredient)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/filter.php?i={ingredient}";
        return await GetFromApi(url);
    }

    [HttpGet("filterByAlcohol")]
    public async Task<DrinkResponse?> FilterByAlcohol(string alcohol)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/filter.php?a={alcohol}";
        return await GetFromApi(url);
    }

    [HttpGet("filterByCategory")]
    public async Task<DrinkResponse?> FilterByCategory(string category)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/filter.php?c={category}";
        return await GetFromApi(url);
    }

    [HttpGet("filterByGlass")]
    public async Task<DrinkResponse?> FilterByGlass(string glass)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/filter.php?g={glass}";
        return await GetFromApi(url);
    }


    //리스트 조회
    [HttpGet("listCategories")]
    public async Task<DrinkResponse?> ListCategories()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/list.php?c=list";
        return await GetFromApi(url);
    }

    [HttpGet("listGlasses")]
    public async Task<DrinkResponse?> ListGlasses()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/list.php?g=list";
        return await GetFromApi(url);
    }

    [HttpGet("listIngredients")]
    public async Task<DrinkResponse?> ListIngredients()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/list.php?i=list";
        return await GetFromApi(url);
    }

    [HttpGet("listAlcoholic")]
    public async Task<DrinkResponse?> ListAlcoholic()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/list.php?a=list";
        return await GetFromApi(url);
    }

    private DrinkResponse? GetCocktailOnCache(DrinkResponse? drinkResponse)
    {
        if (drinkResponse == null) return null;

        if (drinkResponse.drinks.Any())
        {
            Drink drink = drinkResponse.drinks.First();
            DrinkResponse? cachedDrink = _koCacheManager.GetCocktailOnCache(drink.idDrink);
            if (cachedDrink != null) return cachedDrink;
        }

        return null;
    }

    private async Task<DrinkResponse?> GetFromApi(string url)
    {
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
                PropertyNameCaseInsensitive = true
            };

            DrinkResponse? result = JsonSerializer.Deserialize<DrinkResponse>(json, options);

            DrinkResponse? cachedResult = GetCocktailOnCache(result);

            if(cachedResult != null)
            {
                return cachedResult;
            }

            if (result != null)
            {
                DrinkResponse drinkResponse = await TranslateToKorean(result);
                _koCacheManager.AddDrink(drinkResponse.drinks.First());
                _enCacheManager.AddDrink(result.drinks.First());
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

        Task<string> drinkTask = _translator.TranslateToKoreanWithPapago(translateDrink.strDrink!);
        Task<string> categoryTask = _translator.TranslateToKoreanWithPapago(translateDrink.strCategory!);
        Task<string> alcoholicTask = _translator.TranslateToKoreanWithPapago(translateDrink.strAlcoholic!);
        Task<string> glassTask = _translator.TranslateToKoreanWithPapago(translateDrink.strGlass!);
        Task<string> instructionsTask = _translator.TranslateToKoreanWithGpt(translateDrink.strInstructions!);
        Task<string> descriptionTask = _translator.ExplainCocktail(translateDrink.strDrink!);

        var ingredientTasks = new List<Task<string>>
        {
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient1!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient2!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient3!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient4!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient5!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient6!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient7!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient8!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient9!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient10!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient11!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient12!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient13!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient14!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strIngredient15!)
        };

        var measureTasks = new List<Task<string>>
        {
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure1!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure2!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure3!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure4!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure5!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure6!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure7!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure8!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure9!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure10!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure11!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure12!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure13!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure14!),
            _translator.TranslateToKoreanWithPapago(drinkResponse.drinks.First().strMeasure15!)
        };

        await Task.WhenAll(
            drinkTask, categoryTask, alcoholicTask, glassTask, instructionsTask, descriptionTask,
            Task.WhenAll(ingredientTasks),
            Task.WhenAll(measureTasks)
        );


        translatedDrink.strDrink = drinkTask.Result;
        translatedDrink.strCategory = categoryTask.Result;
        translatedDrink.strAlcoholic = alcoholicTask.Result;
        translatedDrink.strGlass = glassTask.Result;
        translatedDrink.strInstructions = instructionsTask.Result;
        translatedDrink.strDescription = descriptionTask.Result;

        for (int i = 0; i < 15; i++)
        {
            var ingredientProp = typeof(Drink).GetProperty($"strIngredient{i + 1}");
            var measureProp = typeof(Drink).GetProperty($"strMeasure{i + 1}");

            ingredientProp?.SetValue(translatedDrink, ingredientTasks[i].Result);
            measureProp?.SetValue(translatedDrink, measureTasks[i].Result);
        }

        return translatedResponse;
    }
}