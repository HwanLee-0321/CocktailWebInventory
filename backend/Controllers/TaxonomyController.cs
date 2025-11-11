using Microsoft.AspNetCore.Mvc;
using CocktailWebApplication.Models;

[ApiController]
[Route("api/v1/[controller]")]
public class TaxonomyController : ControllerBase
{
    private readonly CocktailService _cocktailService;

    public TaxonomyController(CocktailService cocktailService)
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

    [HttpGet("categories")]
    public async Task<IActionResult> ListCategories()
    {
        var result = await _cocktailService.ListCategories();
        return GetActionByResult(result);
    }

    [HttpGet("glasses")]
    public async Task<IActionResult> GetGlasses()
    {
        var result = await _cocktailService.ListGlasses();
        return GetActionByResult(result);
    }

    [HttpGet("alcoholic")]
    public async Task<IActionResult> GetAlcoholic()
    {
        var result = await _cocktailService.ListAlcoholic();
        return GetActionByResult(result);
    }

    [HttpGet("ingredient-categories")]
    public async Task<IActionResult> GetIngredient()
    {
        var result = await _cocktailService.ListIngredients();
        return GetActionByResult(result);
    }

}