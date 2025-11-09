using System.Collections.Concurrent;
using System.Reflection.Metadata;
using System.Text.Json;
using CocktailWebApplication.Models;
namespace CocktailWebApplication.Services
{
    public class CacheManager
    {
        private string _filePath;
        private readonly ConcurrentDictionary<string, Drink> _cache;

        public CacheManager(string filePath = Constants.filePath)
        {
            _filePath = filePath;
            _cache = new ConcurrentDictionary<string, Drink>();
            LoadFromFile();
        }

        public DrinkResponse? GetCocktailOnCache(string? id)
        {
            if (id != null && TryGetDrink(id, out Drink? cachedDrink))
            {
                if (cachedDrink != null)
                {
                    return new DrinkResponse { drinks = new List<Drink> { cachedDrink } };
                }
            }

            return null;
        }

        public bool TryGetDrink(string id, out Drink? drink)
        {
            foreach (var key in _cache.Keys)
            {
                Log.Error(key);
            }
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
                        if (!string.IsNullOrWhiteSpace(drink.idDrink))
                            _cache[drink.idDrink] = drink;
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

    public class KoCacheManager : CacheManager
    {
        public KoCacheManager(string filePath) : base(filePath) { }
    }

    public class EnCacheManager : CacheManager
    {
        public EnCacheManager(string filePath) : base(filePath) { }
    }
}
