using Microsoft.AspNetCore.Mvc;
using CocktailWebApplication.Models;

[ApiController]
[Route("api/v1")]
public class TaxonomyController : ControllerBase
{
    private readonly CocktailService _cocktailService;

    public TaxonomyController(CocktailService cocktailService)
    {
        _cocktailService = cocktailService;
    }

    private IActionResult GetActionByResult(DrinkResponse? result)
    {
        if (result == null)
        {
            return StatusCode(503, "외부 칵테일 서비스 처리 중 오류가 발생했습니다.");
        }

        return Ok(result);
    }

    [HttpGet("taxonomy/bases")]
    public async Task<IActionResult> GetBases()
    {
        var result = await _cocktailService.ListCategories();

        if (result == null)
        {
            return StatusCode(503, "외부 칵테일 서비스 처리 중 오류가 발생했습니다.");
        }

        return Ok(result);
    }

    [HttpGet("taxonomy/tastes")]
    public async Task<IActionResult> GetTastes(string name)
    {
        var result = await _cocktailService.ListCategories();

        if (result == null)
        {
            return StatusCode(503, "외부 칵테일 서비스 처리 중 오류가 발생했습니다.");
        }

        return Ok(result);
    }

    [HttpGet("taxonomy/ingredient-categories")]
    public async Task<IActionResult> GetCategories()
    {
        var result = await _cocktailService.ListIngredients();

        if (result == null)
        {
            return StatusCode(503, "외부 칵테일 서비스 처리 중 오류가 발생했습니다.");
        }

        return Ok(result);
    }


}