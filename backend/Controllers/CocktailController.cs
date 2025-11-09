using Microsoft.AspNetCore.Mvc;
using CocktailWebApplication.Services;
using CocktailWebApplication.Models;
using System.Text.Json;
using System.Xml.Linq;
using System.Diagnostics.Metrics;
using Sprache;

//    [ApiController]
//[Route("[controller]")]
[ApiController]
[Route("api/cocktail")]
public class CocktailController : ControllerBase
{
    private readonly CocktailService _cocktailService;

    public CocktailController(CocktailService cocktailService)
    {
        _cocktailService = cocktailService;
    }

    [HttpGet("searchByName")]
    public async Task<IActionResult> SearchByName(string name)
    {
        var result = await _cocktailService.SearchByName(name);

        if (result == null)
        {
            return StatusCode(503, "외부 칵테일 서비스 처리 중 오류가 발생했습니다.");
        }

        return Ok(result);
    }
    private IActionResult GetActionByResult(DrinkResponse? result)
    {
        if (result == null)
        {
            return StatusCode(503, "외부 칵테일 서비스 처리 중 오류가 발생했습니다.");
        }

        return Ok(result);
    }

    [HttpGet("searchByFirstLetter")]
    public async Task<IActionResult> SearchByFirstLetter(char letter)
    {
        var result = await _cocktailService.SearchByFirstLetter(letter);

        return GetActionByResult(result);
    }

    [HttpGet("searchByIngredient")]
    public async Task<IActionResult> SearchByIngredient(string ingredient)
    {
        var result = await _cocktailService.SearchByIngredient(ingredient);

        return GetActionByResult(result);
    }

    [HttpGet("lookupCocktailById")]
    public async Task<IActionResult> LookupCocktailById(int id)
    {
        var result = await _cocktailService.LookupCocktailById(id);

        return GetActionByResult(result);
    }

    [HttpGet("lookupIngredientById")]
    public async Task<IActionResult> LookupIngredientById(int id)
    {
        var result = await _cocktailService.LookupIngredientById(id);

        return GetActionByResult(result);
    }

    [HttpGet("random")]
    public async Task<IActionResult> Random()
    {
        var result = await _cocktailService.Random();

        return GetActionByResult(result);
    }

    [HttpGet("randomTranslated")]
    public async Task<IActionResult> RandomTranslated()
    {
        var result = await _cocktailService.RandomTranslated();

        return GetActionByResult(result);
    }

    //필터링
    [HttpGet("filterByIngredient")]
    public async Task<IActionResult> FilterByIngredient(string ingredient)
    {
        var result = await _cocktailService.FilterByIngredient(ingredient);

        return GetActionByResult(result);
    }

    [HttpGet("filterByAlcohol")]
    public async Task<IActionResult> FilterByAlcohol(string alcohol)
    {
        var result = await _cocktailService.FilterByAlcohol(alcohol);

        return GetActionByResult(result);
    }

    [HttpGet("filterByCategory")]
    public async Task<IActionResult> FilterByCategory(string category)
    {
        var result = await _cocktailService.FilterByCategory(category);

        return GetActionByResult(result);
    }

    [HttpGet("filterByGlass")]
    public async Task<IActionResult> FilterByGlass(string glass)
    {
        var result = await _cocktailService.FilterByGlass(glass);

        return GetActionByResult(result);
    }


    //리스트 조회
    [HttpGet("listCategories")]
    public async Task<IActionResult> ListCategories()
    {
        var result = await _cocktailService.ListCategories();

        return GetActionByResult(result);
    }

    [HttpGet("listGlasses")]
    public async Task<IActionResult> ListGlasses()
    {
        var result = await _cocktailService.ListGlasses();

        return GetActionByResult(result);
    }

    [HttpGet("listIngredients")]
    public async Task<IActionResult> ListIngredients()
    {
        var result = await _cocktailService.ListIngredients();

        return GetActionByResult(result);
    }

    [HttpGet("listAlcoholic")]
    public async Task<IActionResult> ListAlcoholic()
    {
        var result = await _cocktailService.ListAlcoholic();

        return GetActionByResult(result);
    }

}