using System.Numerics;
using System.Text.Json.Serialization;

namespace CocktailWebApplication.Models
{
    public class TaxonomyResponse
    {
        public List<TaxonomyItem> items { get; set; } = new List<TaxonomyItem>();
    }

    public class TaxonomyItem
    {
        public string id { get; set; } = string.Empty;
        public string labelKo { get; set; } = string.Empty;
    }

    public class CocktailResponse
    {
        public List<Cocktail> items { get; set; } = new List<Cocktail>();
        public int total { get; set; } = 0;
        public CocktailResponse() { }
        public CocktailResponse(DrinkResponse drinkResponse)
        {
            items = new List<Cocktail>();

            foreach (Drink drink in drinkResponse.drinks)
            {
                items.Add(new Cocktail(drink));
            }
            total = items.Count;
        }
    }

    public class Cocktail
    {
        public string? id { get; set; }
        public string? name { get; set; }
        public string? category { get; set; }
        public string? glass { get; set; }
        public string? alcoholic { get; set; }
        public List<string>? ingredients { get; set; }
        public List<string>? measures { get; set; }
        public string? instructions { get; set; }
        public string? image { get; set; }
        public string? strength { get; set; }
        public string? details { get; set; }

        public Cocktail() { }
        public Cocktail(Drink drink)
        {
            id = drink.idDrink;
            name = drink.strDrink;
            category = drink.strCategory;
            glass = drink.strGlass;
            alcoholic = drink.strAlcoholic;
            ingredients = drink.GetIngredients();
            measures = drink.GetMeasures();
            instructions = drink.strInstructions;
            image = drink.strDrinkThumb;
            strength = "";
            details = drink.strDescription;
        }
    }
}