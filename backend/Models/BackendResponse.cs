using System.Text.Json.Serialization;

namespace CocktailWebApplication.Models
{
    public class BackendResponse
    {
        public string? Id;
        public string? Name;
        public string? Base;
        public string? Tastes;
        public string? Ingredients;
        public string? Instructions;
        public string? Image;
        public string? Strength;
        public string? Details;
    }
    public class TaxonomyItem
    {
        public string id { get; set; } = string.Empty;
        public string labelKo { get; set; } = string.Empty;
    }
    public class TaxonomyResponse
    {
        public List<TaxonomyItem> items { get; set; } = new List<TaxonomyItem>();
    }
}