using Microsoft.AspNetCore.Mvc;
using CocktailWebApplication.Models;
using Sprache;

[ApiController]
[Route("api/[controller]")]
public class CocktailController : ControllerBase
{
    private readonly CocktailService _cocktailService;

    public CocktailController(CocktailService cocktailService)
    {
        _cocktailService = cocktailService;
    }

    private IActionResult GetActionByResult(DrinkResponse? result)
    {
        if (result == null)
        {
            return StatusCode(503, "외부 칵테일 서비스 처리 중 오류가 발생했습니다.");
        }

        return Ok(new CocktailResponse(result));
    }

    [HttpGet]
    public async Task<IActionResult> Fillter([FromQuery]List<string>? q, [FromQuery] string? alcoholic, [FromQuery] string? category,
    [FromQuery] string? glass, [FromQuery] List<string>? ingredient, [FromQuery] string? strength)
    {
        var result = await _cocktailService.Filter(q, alcoholic, category, glass, ingredient, strength);
        return GetActionByResult(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> LookupCocktailById(string id)
    {
        var result = await _cocktailService.LookupCocktailById(id);
        return GetActionByResult(result);
    }

    //[HttpGet("SearchByName")]
    //public async Task<IActionResult> SearchByName(string name)
    //{
    //    var result = await _cocktailService.SearchByName(name);
    //    return GetActionByResult(result);
    //}

    //[HttpGet("searchByFirstLetter")]
    //public async Task<IActionResult> SearchByFirstLetter(char letter)
    //{
    //    var result = await _cocktailService.SearchByFirstLetter(letter);
    //    return GetActionByResult(result);
    //}

    [HttpGet("random")]
    public async Task<IActionResult> Random()
    {
        var result = await _cocktailService.Random();
        return GetActionByResult(result);
    }

    //[HttpGet("filterByIngredient")]
    //public async Task<IActionResult> SearchByIngredient(string ingredient)
    //{
    //    var result = await _cocktailService.FilterByIngredient(ingredient);
    //    return GetActionByResult(result);
    //}

    //[HttpGet("filterByIngredients")]
    //public async Task<IActionResult> FilterByIngredients([FromQuery] List<string> ingredient)
    //{
    //    var result = await _cocktailService.FilterByIngredients(ingredient);
    //    return GetActionByResult(result);
    //}

}