using Microsoft.AspNetCore.Mvc;
using CocktailWebApplication.Models;

[ApiController]
[Route("api/v1/[controller]")]
public class RecommendationsController : ControllerBase
{
    private readonly CocktailService _cocktailService;

    public RecommendationsController(CocktailService cocktailService)
    {
        _cocktailService = cocktailService;
    }

    private IActionResult GetActionByResult(TaxonomyResponse? result)
    {
        if (result == null)
        {
            return StatusCode(503, "외부 칵테일 서비스 처리 중 오류가 발생했습니다.");
        }

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetRecommendations(
        [FromQuery] string? q,
        [FromQuery] string? glass,
        [FromQuery] string? category,
        [FromQuery] string? ingredient,
        [FromQuery] string? alcoholic
    )
    {
        var result = await _cocktailService.ListIngredients();
        return GetActionByResult(result);
    }

    //[HttpGet("glasses")]
    //public async Task<IActionResult> GetGlasses()
    //{
    //    var result = await _cocktailService.ListGlasses();
    //    return GetActionByResult(result);
    //}

    //[HttpGet("alcoholic")]
    //public async Task<IActionResult> GetAlcoholic()
    //{
    //    var result = await _cocktailService.ListAlcoholic();
    //    return GetActionByResult(result);
    //}

}