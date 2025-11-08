using System.Collections.Concurrent;
using System.Text.Json;
using CocktailWebApplication.Models;

namespace CocktailWebApplication.Services
{
    public class CocktailCacheManager
    {
        private readonly string _filePath;
        private readonly ConcurrentDictionary<string, Drink> _cache;

        public CocktailCacheManager(string filePath = "cocktails.json")
        {
            _filePath = filePath;
            _cache = new ConcurrentDictionary<string, Drink>();
            LoadFromFile();
        }

        public bool TryGetDrink(string id, out Drink? drink)
        {
            return _cache.TryGetValue(id, out drink);
        }

        public void AddDrink(Drink drink)
        {
            if (string.IsNullOrEmpty(drink.idDrink))  return;

            _cache[drink.idDrink] = drink;
            SaveToFile();
        }

        private void LoadFromFile()
        {
            if (!File.Exists(_filePath))    return;

            try
            {
                string json = File.ReadAllText(_filePath);
                var drinksWrapper = JsonSerializer.Deserialize<DrinksWrapper>(json);

                if (drinksWrapper?.drinks != null)
                {
                    foreach (var drink in drinksWrapper.drinks)
                    {
                        if (!string.IsNullOrWhiteSpace(drink.strDrink))
                            _cache[drink.strDrink] = drink;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private void SaveToFile()
        {
            try
            {
                var wrapper = new DrinksWrapper { drinks = _cache.Values.ToList() };
                string json = JsonSerializer.Serialize(wrapper, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }
        }

        private class DrinksWrapper
        {
            public List<Drink>? drinks { get; set; }
        }
    }
}
