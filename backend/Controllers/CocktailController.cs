using Microsoft.AspNetCore.Mvc;
using CocktailWebApplication.Services;
using CocktailWebApplication.Models;
using System.Text.Json;

//    [ApiController]
//[Route("[controller]")]
[ApiController]
[Route("api/cocktail")]
public class CocktailController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly Translator _translator;

    public CocktailController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _translator = new Translator(_httpClient);
    }

    private async Task<IActionResult> GetFromApi(string url)
    {
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            return StatusCode((int)response.StatusCode, "API 요청 실패");
        }

        var json = await response.Content.ReadAsStringAsync();

        if (string.IsNullOrEmpty(json))
        {
            return Ok(new DrinkResponse());
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            DrinkResponse? result = JsonSerializer.Deserialize<DrinkResponse>(json, options);
            return Ok(result ?? new DrinkResponse());
        }
        catch (JsonException ex)
        {
            return StatusCode(500, $"역직렬화 실패: {ex.Message}");
        }
    }

    [HttpGet("searchByName")]
    public async Task<IActionResult> SearchByName(string name)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?s={name}";

        return await GetFromApi(url);
    }

    [HttpGet("searchByFirstLetter")]
    public async Task<IActionResult> SearchByFirstLetter(char letter)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?f={letter}";
        return await GetFromApi(url);
    }

    [HttpGet("searchByIngredient")]
    public async Task<IActionResult> SearchByIngredient(string ingredient)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/search.php?i={ingredient}";
        return await GetFromApi(url);
    }

    [HttpGet("lookupCocktailById")]
    public async Task<IActionResult> LookupCocktailById(int id)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/lookup.php?i={id}";

        var responseAction = await GetFromApi(url);

        if (responseAction is ObjectResult objectResult && objectResult.StatusCode == 200)
        {
            if (objectResult.Value is DrinkResponse drinkResponse)
            {
                var translatedResponse = await _translator.TranslateDrinkResponseAsync(drinkResponse);

                return Ok(translatedResponse);
            }
        }

        return await GetFromApi(url);
    }

    [HttpGet("lookupIngredientById")]
    public async Task<IActionResult> LookupIngredientById(int id)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/lookup.php?iid={id}";
        return await GetFromApi(url);
    }

    [HttpGet("random")]
    public async Task<IActionResult> Random()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/random.php";
        return await GetFromApi(url);
    }

    [HttpGet("randomTranslated")]
    public async Task<IActionResult> RandomTranslated()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/random.php";

        var responseAction = await GetFromApi(url);

        if (responseAction is ObjectResult objectResult && objectResult.StatusCode == 200)
        {
            if (objectResult.Value is DrinkResponse drinkResponse)
            {
                var translatedResponse = await _translator.TranslateDrinkResponseAsync(drinkResponse);

                return Ok(translatedResponse);
            }
        }

        return responseAction;
    }

    //필터링
    [HttpGet("filterByIngredient")]
    public async Task<IActionResult> FilterByIngredient(string ingredient)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/filter.php?i={ingredient}";
        return await GetFromApi(url);
    }

    [HttpGet("filterByAlcohol")]
    public async Task<IActionResult> FilterByAlcohol(string alcohol)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/filter.php?a={alcohol}";
        return await GetFromApi(url);
    }

    [HttpGet("filterByCategory")]
    public async Task<IActionResult> FilterByCategory(string category)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/filter.php?c={category}";
        return await GetFromApi(url);
    }

    [HttpGet("filterByGlass")]
    public async Task<IActionResult> FilterByGlass(string glass)
    {
        string url = $"https://www.thecocktaildb.com/api/json/v1/1/filter.php?g={glass}";
        return await GetFromApi(url);
    }


    //리스트 조회
    [HttpGet("listCategories")]
    public async Task<IActionResult> ListCategories()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/list.php?c=list";
        return await GetFromApi(url);
    }

    [HttpGet("listGlasses")]
    public async Task<IActionResult> ListGlasses()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/list.php?g=list";
        return await GetFromApi(url);
    }

    [HttpGet("listIngredients")]
    public async Task<IActionResult> ListIngredients()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/list.php?i=list";
        return await GetFromApi(url);
    }

    [HttpGet("listAlcoholic")]
    public async Task<IActionResult> ListAlcoholic()
    {
        string url = "https://www.thecocktaildb.com/api/json/v1/1/list.php?a=list";
        return await GetFromApi(url);
    }

}