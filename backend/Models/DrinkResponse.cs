using System.Text.Json.Serialization;

namespace CocktailWebApplication.Models
{
    public class DrinkResponse
    {
        public List<Drink> drinks { get; set; }
        public DrinkResponse()
        {
            drinks = new List<Drink>();
        }
    }

    public class Drink
    {
        public string? idDrink { get; set; }
        public string? strDrink { get; set; }
        public string? strDrinkAlternate { get; set; }
        public string? strTags { get; set; }
        public string? strVideo { get; set; }
        public string? strCategory { get; set; }
        public string? strIBA { get; set; }
        public string? strAlcoholic { get; set; }
        public string? strGlass { get; set; }
        public string? strInstructions { get; set; }
        public string? strDrinkThumb { get; set; }

        public string? strIngredient1 { get; set; }
        public string? strIngredient2 { get; set; }
        public string? strIngredient3 { get; set; }
        public string? strIngredient4 { get; set; }
        public string? strIngredient5 { get; set; }
        public string? strIngredient6 { get; set; }
        public string? strIngredient7 { get; set; }
        public string? strIngredient8 { get; set; }
        public string? strIngredient9 { get; set; }
        public string? strIngredient10 { get; set; }
        public string? strIngredient11 { get; set; }
        public string? strIngredient12 { get; set; }
        public string? strIngredient13 { get; set; }
        public string? strIngredient14 { get; set; }
        public string? strIngredient15 { get; set; }

        public string? strMeasure1 { get; set; }
        public string? strMeasure2 { get; set; }
        public string? strMeasure3 { get; set; }
        public string? strMeasure4 { get; set; }
        public string? strMeasure5 { get; set; }
        public string? strMeasure6 { get; set; }
        public string? strMeasure7 { get; set; }
        public string? strMeasure8 { get; set; }
        public string? strMeasure9 { get; set; }
        public string? strMeasure10 { get; set; }
        public string? strMeasure11 { get; set; }
        public string? strMeasure12 { get; set; }
        public string? strMeasure13 { get; set; }
        public string? strMeasure14 { get; set; }
        public string? strMeasure15 { get; set; }

        public string? strImageSource { get; set; }
        public string? strImageAttribution { get; set; }
        public string? strCreativeCommonsConfirmed { get; set; }
        public string? dateModified { get; set; }

        public string? strDescription { get; set; }

        public List<string> GetIngredients()
        {
            var ingredients = new List<string>();

            foreach (var prop in GetType().GetProperties())
            {
                if (prop.Name.StartsWith("strIngredient"))
                {
                    var value = prop.GetValue(this) as string;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        ingredients.Add(value.Trim());
                    }
                }
            }

            return ingredients;
        }
        public List<string> GetMeasures()
        {
            var measures = new List<string>();

            foreach (var prop in GetType().GetProperties())
            {
                if (prop.Name.StartsWith("strMeasure"))
                {
                    var value = prop.GetValue(this) as string;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        measures.Add(value.Trim());
                    }
                }
            }

            return measures;
        }
        public Drink() { }

        public Drink(Drink original)
        {
            idDrink = original.idDrink;
            strDrink = original.strDrink;
            strDrinkAlternate = original.strDrinkAlternate;
            strTags = original.strTags;
            strVideo = original.strVideo;
            strCategory = original.strCategory;
            strIBA = original.strIBA;
            strAlcoholic = original.strAlcoholic;
            strGlass = original.strGlass;
            strInstructions = original.strInstructions;
            strDrinkThumb = original.strDrinkThumb;

            strIngredient1 = original.strIngredient1;
            strIngredient2 = original.strIngredient2;
            strIngredient3 = original.strIngredient3;
            strIngredient4 = original.strIngredient4;
            strIngredient5 = original.strIngredient5;
            strIngredient6 = original.strIngredient6;
            strIngredient7 = original.strIngredient7;
            strIngredient8 = original.strIngredient8;
            strIngredient9 = original.strIngredient9;
            strIngredient10 = original.strIngredient10;
            strIngredient11 = original.strIngredient11;
            strIngredient12 = original.strIngredient12;
            strIngredient13 = original.strIngredient13;
            strIngredient14 = original.strIngredient14;
            strIngredient15 = original.strIngredient15;

            strMeasure1 = original.strMeasure1;
            strMeasure2 = original.strMeasure2;
            strMeasure3 = original.strMeasure3;
            strMeasure4 = original.strMeasure4;
            strMeasure5 = original.strMeasure5;
            strMeasure6 = original.strMeasure6;
            strMeasure7 = original.strMeasure7;
            strMeasure8 = original.strMeasure8;
            strMeasure9 = original.strMeasure9;
            strMeasure10 = original.strMeasure10;
            strMeasure11 = original.strMeasure11;
            strMeasure12 = original.strMeasure12;
            strMeasure13 = original.strMeasure13;
            strMeasure14 = original.strMeasure14;
            strMeasure15 = original.strMeasure15;

            strImageSource = original.strImageSource;
            strImageAttribution = original.strImageAttribution;
            strCreativeCommonsConfirmed = original.strCreativeCommonsConfirmed;
            dateModified = original.dateModified;

            strDescription = original.strDescription;
        }
    }
}