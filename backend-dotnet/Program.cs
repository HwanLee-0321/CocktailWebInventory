using System.Collections.Concurrent;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// CORS
var origins = Environment.GetEnvironmentVariable("CORS_ORIGIN")?.Split(',')
             ?? new [] { "http://localhost:5173" };
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(origins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Swagger (dev only)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/v1/health", () => Results.Json(new { ok = true }));

// ----- Demo Data (aligned with frontend) -----
var COCKTAILS = new List<Cocktail>
{
    new(
        Id: "mojito",
        Name: "모히토 (Mojito)",
        Base: "rum",
        Tastes: new() { "fresh", "sweet" },
        Ingredients: new() { "rum", "lime", "mint", "sugar", "soda" },
        Instructions: "라임과 설탕을 으깨고 민트를 넣어 가볍게 빻은 뒤 럼과 얼음을 넣고 소다로 채웁니다.",
        Image: "https://images.unsplash.com/photo-1560518883-ce09059eeffa?q=80&w=320&auto=format&fit=crop",
        Strength: "light",
        Details: new("Build", "하이볼(Highball)", "민트(Mint), 라임 웨지",
            new() { "라임과 설탕을 글라스에서 머들", "민트를 살짝 빻아 향을 내기", "럼과 얼음을 넣고 저은 후", "소다수로 채워 가볍게 스터" },
            "시원하고 상쾌한 향을 즐기려면 얼음을 넉넉히, 빨대 없이 향을 바로 느껴보세요.")
    ),
    new(
        Id: "old-fashioned",
        Name: "올드 패션드 (Old Fashioned)",
        Base: "whiskey",
        Tastes: new() { "bitter", "rich" },
        Ingredients: new() { "whiskey", "sugar", "angostura", "orange peel" },
        Instructions: "설탕과 앙고스투라를 스터하여 위스키를 넣고 얼음으로 희석한 뒤 오렌지 필로 마무리.",
        Image: "https://images.unsplash.com/photo-1601084881623-cdf9a8f3a522?q=80&w=320&auto=format&fit=crop",
        Strength: "strong",
        Details: new("Stir", "올드패션드(ROCKS)", "오렌지 필",
            new() { "글라스에 설탕과 비터를 넣고 약간의 물로 녹임", "얼음과 위스키를 넣고 스터", "오렌지 필을 짜 향을 입힌 뒤 가니시" },
            "잔을 손으로 감싸 향을 맡아가며 천천히 한 모금씩.")
    ),
    new(
        Id: "margarita",
        Name: "마가리타 (Margarita)",
        Base: "tequila",
        Tastes: new() { "sour", "salty" },
        Ingredients: new() { "tequila", "triple sec", "lime", "salt" },
        Instructions: "테킬라, 트리플 섹, 라임 주스를 셰이크하고 솔트 림 잔에 따릅니다.",
        Image: "https://images.unsplash.com/photo-1604908554027-0c94c41f9a8f?q=80&w=320&auto=format&fit=crop",
        Strength: "medium",
        Details: new("Shake", "마가리타", "솔트 림, 라임 휠",
            new() { "잔 림에 라임을 문지르고 소금 묻히기", "셰이커에 재료와 얼음 넣고 셰이크", "잔에 걸러 붓고 라임으로 가니시" },
            "짭짤한 소금 림과 산뜻한 산미의 대비를 한 입씩 즐겨보세요.")
    ),
    new(
        Id: "negroni",
        Name: "네그로니 (Negroni)",
        Base: "gin",
        Tastes: new() { "bitter", "aromatic" },
        Ingredients: new() { "gin", "campari", "sweet vermouth", "orange peel" },
        Instructions: "진, 캄파리, 스위트 베르무트를 1:1:1로 스터.",
        Image: "https://images.unsplash.com/photo-1582582494700-66f27004e70a?q=80&w=320&auto=format&fit=crop",
        Strength: "medium",
        Details: new("Stir", "ROCKS", "오렌지 필",
            new() { "믹싱글라스에 재료와 얼음", "차갑게 스터", "얼음 위에 따르고 오렌지 필 트위스트" },
            "쓴맛과 허브 향의 여운을 느끼며 에피타이저처럼 천천히.")
    ),
    new(
        Id: "cosmopolitan",
        Name: "코스모폴리탄 (Cosmopolitan)",
        Base: "vodka",
        Tastes: new() { "sweet", "tart" },
        Ingredients: new() { "vodka", "triple sec", "cranberry", "lime" },
        Instructions: "보드카, 트리플 섹, 크랜베리, 라임을 셰이크.",
        Image: "https://images.unsplash.com/photo-1601924582971-b99237a53f6a?q=80&w=320&auto=format&fit=crop",
        Strength: "medium",
        Details: new("Shake", "쿠페/마티니", "라임 휠",
            new() { "셰이커에 모든 재료와 얼음", "차갑게 셰이크", "잔에 더블 스트레인" },
            "차갑게, 향이 살아있을 때 가볍게 즐기기.")
    ),
    new(
        Id: "whiskey-sour",
        Name: "위스키 사워 (Whiskey Sour)",
        Base: "whiskey",
        Tastes: new() { "sour", "foam" },
        Ingredients: new() { "whiskey", "lemon", "sugar", "egg white (optional)" },
        Instructions: "위스키, 레몬 주스, 시럽, (선택) 흰자를 셰이크 후 더블 스트레인.",
        Image: "https://images.unsplash.com/photo-1517705008128-361805f42e86?q=80&w=320&auto=format&fit=crop",
        Strength: "medium",
        Details: new("Dry & Wet Shake", "쿠페/ROCKS", "레몬 필/체리",
            new() { "흰자 사용 시 드라이 셰이크", "얼음 넣고 다시 셰이크", "잔에 더블 스트레인", "가니시 올리기" },
            "폼 위로 올라오는 향과 질감을 혀로 굴리며 천천히.")
    )
};

var BASES = new[] { "vodka", "gin", "rum", "tequila", "whiskey" };
var TASTES = new List<Taste>
{
    new("sweet","달콤함"), new("sour","상큼함"), new("bitter","쓴맛"), new("fresh","상쾌함"),
    new("aromatic","향미"), new("tart","새콤달콤"), new("rich","리치"), new("foam","거품")
};
var CATEGORIES = new List<Category>
{
    new("all","전체"), new("spirit","술"), new("fruit","과일"), new("juice","쥬스"),
    new("mixer","믹서"), new("syrup","시럽·설탕"), new("bitter","비터·허브"), new("other","기타")
};
var ING_CAT = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase)
{
    ["rum"]="spirit", ["vodka"]="spirit", ["gin"]="spirit", ["whiskey"]="spirit",
    ["tequila"]="spirit", ["triple sec"]="spirit", ["sweet vermouth"]="spirit", ["campari"]="spirit",
    ["lime"]="fruit", ["lemon"]="fruit", ["cranberry"]="juice", ["orange peel"]="bitter", ["mint"]="bitter",
    ["soda"]="mixer", ["sugar"]="syrup", ["angostura"]="bitter", ["salt"]="other", ["egg white"]="other",
};

var INGREDIENTS = COCKTAILS.SelectMany(c => c.Ingredients).Distinct().OrderBy(x => x).ToList();
string CategoryOfIng(string name)
{
    var baseName = System.Text.RegularExpressions.Regex.Replace(name, "\\s*\\(optional\\)\\s*", "", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Trim();
    return ING_CAT.TryGetValue(baseName, out var cat) ? cat : "other";
}

// In-memory user state (per x-demo-user)
var users = new ConcurrentDictionary<string, UserState>();
UserState GetUser(HttpContext ctx)
{
    var id = ctx.Request.Headers.TryGetValue("x-demo-user", out var hv) ? hv.FirstOrDefault() ?? "demo" : "demo";
    return users.GetOrAdd(id, _ => new UserState());
}

double ScoreCocktail(Cocktail c, string? q, IEnumerable<string> bases, IEnumerable<string> tastes, IEnumerable<string> have)
{
    double score = 0;
    if (!string.IsNullOrWhiteSpace(q))
    {
        var s = q.ToLowerInvariant();
        var hay = string.Join(' ', new[] { c.Name, c.Base }.Concat(c.Tastes).Concat(c.Ingredients)).ToLowerInvariant();
        score += hay.Contains(s) ? 2 : -5;
    }
    var baseSet = bases?.ToHashSet() ?? new HashSet<string>();
    score += (baseSet.Count == 0 || baseSet.Contains(c.Base)) ? 2 : -3;
    var tasteSet = tastes?.ToHashSet() ?? new HashSet<string>();
    foreach (var t in c.Tastes) if (tasteSet.Contains(t)) score += 1;
    var haveSet = have?.ToHashSet() ?? new HashSet<string>();
    score += c.Ingredients.Count(i => haveSet.Contains(i)) * 1.5;
    if (c.Strength == "strong" && tasteSet.Contains("sweet")) score -= 0.5;
    return score;
}

var api = app.MapGroup("/api/v1");

// Taxonomy
api.MapGet("/taxonomy/bases", () => Results.Json(new
{
    items = BASES.Select(id => new { id, labelKo = id switch { "vodka" => "보드카", "gin" => "진", "rum" => "럼", "tequila" => "테킬라", "whiskey" => "위스키", _ => id } })
}));
api.MapGet("/taxonomy/tastes", () => Results.Json(new { items = TASTES }));
api.MapGet("/taxonomy/ingredient-categories", () => Results.Json(new { items = CATEGORIES }));

// Ingredients
api.MapGet("/ingredients", (HttpRequest req) =>
{
    var q = req.Query["q"].ToString();
    var category = req.Query["category"].ToString();
    IEnumerable<string> items = INGREDIENTS;
    if (!string.IsNullOrEmpty(category) && category != "all") items = items.Where(n => CategoryOfIng(n) == category);
    if (!string.IsNullOrEmpty(q)) items = items.Where(n => n.Contains(q, StringComparison.OrdinalIgnoreCase));
    return Results.Json(new { items });
});

// Cocktails list
api.MapGet("/cocktails", (HttpRequest req) =>
{
    var q = req.Query["q"].ToString();
    var strength = req.Query["strength"].ToString();
    var bases = req.Query["base"].ToList();
    var tastes = req.Query["taste"].ToList();
    var ings = req.Query["ingredient"].ToList();
    IEnumerable<Cocktail> list = COCKTAILS;
    if (!string.IsNullOrEmpty(q))
    {
        var s = q.ToLowerInvariant();
        list = list.Where(c => string.Join(' ', new[] { c.Name, c.Base }.Concat(c.Tastes).Concat(c.Ingredients)).ToLowerInvariant().Contains(s));
    }
    if (!string.IsNullOrEmpty(strength)) list = list.Where(c => c.Strength == strength);
    if (bases.Count > 0) list = list.Where(c => bases.Contains(c.Base));
    if (tastes.Count > 0) list = list.Where(c => c.Tastes.Any(t => tastes.Contains(t)));
    if (ings.Count > 0) list = list.Where(c => c.Ingredients.Any(i => ings.Contains(i)));
    var result = list.ToList();
    return Results.Json(new { items = result, total = result.Count });
});

// Cocktail by id
api.MapGet("/cocktails/{id}", (string id) =>
{
    var c = COCKTAILS.FirstOrDefault(x => x.Id == id);
    return c is null ? Results.NotFound(new { code = "NOT_FOUND", message = "cocktail not found" }) : Results.Json(c);
});

// Recommendations
api.MapGet("/recommendations", (HttpRequest req) =>
{
    var q = req.Query["q"].ToString();
    var bases = req.Query["bases"].ToList();
    var tastes = req.Query["tastes"].ToList();
    var have = req.Query["have"].ToList();
    var scored = COCKTAILS
        .Select(c => new { cocktail = c, score = ScoreCocktail(c, q, bases, tastes, have) })
        .Where(x => x.score > -3)
        .OrderByDescending(x => x.score)
        .ToList();
    return Results.Json(new { items = scored, total = scored.Count });
});

// User state (mock): inventory
api.MapGet("/me/inventory", (HttpContext ctx) => Results.Json(new { items = GetUser(ctx).Have.ToList() }));
api.MapPut("/me/inventory", async (HttpContext ctx) =>
{
    var user = GetUser(ctx);
    var payload = await ctx.Request.ReadFromJsonAsync<ItemsPayload>() ?? new ItemsPayload(new());
    user.Have = new HashSet<string>(payload.Items);
    return Results.Json(new { items = user.Have.ToList() });
});
api.MapPatch("/me/inventory", async (HttpContext ctx) =>
{
    var user = GetUser(ctx);
    var payload = await ctx.Request.ReadFromJsonAsync<PatchPayload>() ?? new PatchPayload(new(), new());
    foreach (var i in payload.Add) user.Have.Add(i);
    foreach (var i in payload.Remove) user.Have.Remove(i);
    return Results.Json(new { items = user.Have.ToList() });
});

// User state (mock): favorites
api.MapGet("/me/favorites", (HttpContext ctx) => Results.Json(new { items = GetUser(ctx).Favorites.ToList() }));
api.MapPut("/me/favorites", async (HttpContext ctx) =>
{
    var user = GetUser(ctx);
    var payload = await ctx.Request.ReadFromJsonAsync<ItemsPayload>() ?? new ItemsPayload(new());
    user.Favorites = new HashSet<string>(payload.Items);
    return Results.Json(new { items = user.Favorites.ToList() });
});
api.MapPatch("/me/favorites/toggle", async (HttpContext ctx) =>
{
    var user = GetUser(ctx);
    var payload = await ctx.Request.ReadFromJsonAsync<TogglePayload>() ?? new TogglePayload("");
    if (string.IsNullOrWhiteSpace(payload.Id)) return Results.BadRequest(new { code="BAD_REQUEST", message="id required" });
    if (user.Favorites.Contains(payload.Id)) user.Favorites.Remove(payload.Id); else user.Favorites.Add(payload.Id);
    return Results.Json(new { items = user.Favorites.ToList() });
});

app.Run();

// ----- Models -----
record Taste(string Id, string Label);
record Category(string Id, string Label);

record Details(
    string? Method,
    string? Glass,
    string? Garnish,
    List<string>? Steps,
    string? Enjoy
);

record Cocktail(
    string Id,
    string Name,
    [property: JsonPropertyName("base")] string Base,
    List<string> Tastes,
    List<string> Ingredients,
    string Instructions,
    string Image,
    string Strength,
    Details? Details
);

class UserState
{
    public HashSet<string> Have { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> Favorites { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

record ItemsPayload(List<string> Items);
record PatchPayload(List<string> Add, List<string> Remove);
record TogglePayload(string Id);

