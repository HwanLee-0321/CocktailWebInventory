// API service that reads the bundled cocktails_en.json once
// and serves all queries directly from that snapshot.

import { COCKTAILS, GLASSES, DRINK_CATEGORIES, ALCOHOL_OPTIONS, INGREDIENT_CATEGORIES, categoryOfIng } from '../data/sample.js'

const STATIC_DATA_URL = '/cocktails_ko.json'
const USE_STATIC_JSON = String(import.meta.env.VITE_USE_STATIC_JSON ?? 'true').toLowerCase() !== 'false'
const DEFAULT_TAXONOMY = {
  glasses: GLASSES,
  alcoholic: ALCOHOL_OPTIONS,
  drinkCategories: DRINK_CATEGORIES,
  ingredientCategories: INGREDIENT_CATEGORIES
}
const CATEGORY_LABEL_MAP = Object.fromEntries([
  ...INGREDIENT_CATEGORIES.map(item => [item.id, item.label]),
  ['all', '전체'],
  ['liqueur', '리큐르'],
  ['wine', '와인/주정'],
  ['beer', '맥주/에일'],
  ['herb', '허브/잎'],
  ['spice', '향신료'],
  ['dairy', '유제품'],
  ['tea', '티/인퓨전'],
  ['coffee', '커피/카카오'],
  ['sweetener', '감미료'],
  ['garnish', '가니시'],
  ['vegetable', '야채/가니시'],
  ['grain', '곡물/시리얼'],
  ['floral', '플로럴'],
  ['nut', '견과류']
])

const INGREDIENT_CATEGORY_OVERRIDES = {
  rum: 'spirit',
  vodka: 'spirit',
  gin: 'spirit',
  whiskey: 'spirit',
  whisky: 'spirit',
  tequila: 'spirit',
  bourbon: 'spirit',
  brandy: 'spirit',
  cognac: 'spirit',
  mezcal: 'spirit',
  soju: 'spirit',
  sake: 'spirit',
  'triple sec': 'spirit',
  'sweet vermouth': 'spirit',
  'dry vermouth': 'spirit',
  campari: 'spirit',
  absinthe: 'bitter',
  bitters: 'bitter',
  'angostura bitters': 'bitter',
  'simple syrup': 'syrup',
  syrup: 'syrup',
  sugar: 'syrup',
  honey: 'syrup',
  'agave syrup': 'syrup',
  'maple syrup': 'syrup',
  grenadine: 'syrup',
  'orgeat syrup': 'syrup',
  'lime juice': 'juice',
  'lemon juice': 'juice',
  'orange juice': 'juice',
  'pineapple juice': 'juice',
  'cranberry juice': 'juice',
  'grapefruit juice': 'juice',
  'apple juice': 'juice',
  'tomato juice': 'juice',
  cola: 'mixer',
  'club soda': 'mixer',
  'soda water': 'mixer',
  'ginger ale': 'mixer',
  'ginger beer': 'mixer',
  tonic: 'mixer',
  'tonic water': 'mixer',
  'coconut milk': 'mixer',
  'coconut cream': 'mixer',
  cream: 'mixer',
  milk: 'mixer',
  salt: 'other',
  pepper: 'other'
}
const CATEGORY_KEYWORD_RULES = [
  { test: /wine|vermouth|porto|sherry/, id: 'wine' },
  { test: /liqueur|curacao|amaro|aperol|cointreau|schnapps|sambuca|galliano|benedictine/, id: 'liqueur' },
  { test: /beer|lager|ale|stout/, id: 'beer' },
  { test: /juice|nectar|puree|squash/, id: 'juice' },
  { test: /fruit|berry|banana|melon|apple|orange|lemon|lime|pineapple|grape|cherry|mango|kiwi|peach|pear|coconut/, id: 'fruit' },
  { test: /vegetable|tomato|celery|cucumber|carrot|pepper/, id: 'vegetable' },
  { test: /syrup|sweetener|honey|molasses|maple|agave|caramel/, id: 'syrup' },
  { test: /sugar/, id: 'sweetener' },
  { test: /soda|water|cola|tonic|ginger|energy|seltzer|sparkling|club|ginger ale|ginger beer|lemonade/, id: 'mixer' },
  { test: /mint|herb|basil|rosemary|thyme|lavender|sage/, id: 'herb' },
  { test: /cinnamon|clove|pepper|nutmeg|ginger|cardamom|anise|spice/, id: 'spice' },
  { test: /absinthe|bitter|bitters|amaro/, id: 'bitter' },
  { test: /cream|milk|yoghurt|butter|cheese/, id: 'dairy' },
  { test: /tea|matcha|chai/, id: 'tea' },
  { test: /coffee|espresso|cocoa|chocolate/, id: 'coffee' },
  { test: /flower|floral|rose|violet/, id: 'floral' },
  { test: /nut|almond|hazelnut|pistachio|peanut/, id: 'nut' },
  { test: /oat|barley|wheat|grain/, id: 'grain' }
]

let catalogPromise = null

export const Api = {
  async taxonomy(){
    const catalog = await ensureCatalog()
    return catalog.taxonomy
  },

  async cocktails(params = {}){
    const catalog = await ensureCatalog()
    const items = applyCocktailFilters(catalog.cocktails, params)
    return { items, total: items.length }
  },

  async cocktail(id){
    const catalog = await ensureCatalog()
    return catalog.cocktails.find(item => item.id === id) || null
  },

  async ingredients(params = {}){
    const catalog = await ensureCatalog()
    return { items: filterIngredients(catalog, params) }
  },

  async recommendations(params = {}){
    const catalog = await ensureCatalog()
    return buildRecommendations(catalog, params)
  }
}

async function ensureCatalog(){
  if (!catalogPromise){
    catalogPromise = loadStaticCatalog().catch(err => {
      console.warn('[api] failed to load cocktails_en.json, falling back to sample data:', err?.message || err)
      return buildCatalog(COCKTAILS)
    })
  }
  return catalogPromise
}

async function loadStaticCatalog(){
  if (!USE_STATIC_JSON) return buildCatalog(COCKTAILS)
  const res = await fetch(STATIC_DATA_URL, { cache: 'no-store' })
  if (!res.ok) throw new Error(`Failed to fetch ${STATIC_DATA_URL} (${res.status})`)
  const payload = await res.json()
  const drinks = Array.isArray(payload?.drinks) ? payload.drinks : []
  const normalized = drinks.map(normalizeCocktailDbDrink).filter(Boolean)
  return buildCatalog(normalized.length ? normalized : COCKTAILS)
}

function buildCatalog(list){
  const ingredientSet = new Set()
  const ingredientCategoryMap = new Map()
  const drinkCategoryCounts = new Map()
  const addIngredient = (name, categoryId) => {
    if (!name) return
    if (!ingredientSet.has(name)) ingredientSet.add(name)
    if (!ingredientCategoryMap.has(name)){
      ingredientCategoryMap.set(name, categoryId || categorizeIngredient(name))
    }
  }
  list.forEach(cocktail => {
    const catId = cocktail.categoryId || 'other'
    const catLabel = cocktail.categoryLabel || formatCategoryLabel(catId)
    if (!drinkCategoryCounts.has(catId)){
      drinkCategoryCounts.set(catId, { count: 0, label: catLabel })
    }
    drinkCategoryCounts.get(catId).count += 1
    const groups = Array.isArray(cocktail.ingredientGroups) ? cocktail.ingredientGroups : null
    if (groups?.length){
      groups.forEach(group => {
        const categoryId = group.categoryId || 'other'
        ;(group.items || []).forEach(item => addIngredient(item, categoryId))
      })
    }
    (cocktail.ingredients || []).forEach(name => addIngredient(name))
  })
  const ingredients = Array.from(ingredientSet).sort((a, b) => a.localeCompare(b))
  const ingredientCategories = deriveIngredientCategories(ingredientCategoryMap)
  const drinkCategories = deriveDrinkCategories(drinkCategoryCounts)
  return {
    cocktails: list,
    ingredients,
    ingredientCategoryMap,
    taxonomy: {
      ...DEFAULT_TAXONOMY,
      drinkCategories: drinkCategories.length ? drinkCategories : DEFAULT_TAXONOMY.drinkCategories,
      ingredientCategories
    }
  }
}

function deriveIngredientCategories(map){
  const counts = new Map()
  map.forEach(value => {
    const key = value || 'other'
    counts.set(key, (counts.get(key) || 0) + 1)
  })
  const ordered = [...counts.entries()]
    .filter(([id]) => id && id !== 'all')
    .sort((a, b) => b[1] - a[1])
    .map(([id]) => ({ id, label: formatCategoryLabel(id) }))
  return [{ id: 'all', label: formatCategoryLabel('all') }, ...ordered]
}

function deriveDrinkCategories(counts){
  if (!counts || !counts.size) return []
  return [...counts.entries()]
    .filter(([id]) => id && id !== 'all')
    .sort((a, b) => {
      if (b[1].count !== a[1].count) return b[1].count - a[1].count
      return a[0].localeCompare(b[0])
    })
    .map(([id, info]) => ({ id, label: info.label || formatCategoryLabel(id) }))
}

const toTitle = (id) => id.replace(/-/g, ' ').replace(/\b\w/g, ch => ch.toUpperCase())
const formatCategoryLabel = (id) => CATEGORY_LABEL_MAP[id] || toTitle(id || '기타')

function applyCocktailFilters(cocktails, params){
  const q = (params.q || '').toLowerCase().trim()
  const strength = (params.strength || '').toLowerCase()
  const baseSet = toLowerSet(params.base)
  const tasteSet = toLowerSet(params.taste)
  const ingredientSet = toLowerSet(params.ingredient || params.ingredients)
  const glassSet = toSet(params.glass || params.glasses)
  const categorySet = toSet(params.category || params.categories)
  const alcoholic = (params.alcoholic || '').toLowerCase()

  return cocktails.filter(c => {
    if (q){
      const hay = [c.name, c.base, c.details?.glass, c.details?.garnish, ...(c.tastes || []), ...(c.ingredients || [])].join(' ').toLowerCase()
      if (!hay.includes(q)) return false
    }
    if (strength && c.strength !== strength) return false
    if (baseSet.size && !baseSet.has(c.base)) return false
    if (tasteSet.size && !(c.tastes || []).some(t => tasteSet.has((t || '').toLowerCase()))) return false
    if (ingredientSet.size && !(c.ingredients || []).some(i => ingredientSet.has(i.toLowerCase()))) return false
    if (glassSet.size && !glassSet.has(c.glassId)) return false
    if (categorySet.size && !categorySet.has(c.categoryId)) return false
    if (alcoholic && c.alcoholicId !== alcoholic) return false
    return true
  })
}

function filterIngredients(catalog, params){
  const category = (params.category || 'all').toLowerCase()
  const q = (params.q || '').trim().toLowerCase()
  return catalog.ingredients.filter(name => {
    if (category !== 'all' && catalog.ingredientCategoryMap.get(name) !== category) return false
    if (q && !name.includes(q)) return false
    return true
  }).slice(0, 500)
}

function buildRecommendations(catalog, params){
  const glassSet = toSet(params.glass || params.glasses)
  const categorySet = toSet(params.category || params.categories)
  const haveSet = toLowerSet(params.ingredient || params.ingredients)
  const alcoholic = (params.alcoholic || '').toLowerCase()
  const q = (params.q || '').toLowerCase()

  const filtered = catalog.cocktails.filter(c => {
    if (glassSet.size && !glassSet.has(c.glassId)) return false
    if (categorySet.size && !categorySet.has(c.categoryId)) return false
    if (alcoholic && c.alcoholicId !== alcoholic) return false
    if (haveSet.size && !(c.ingredients || []).some(name => haveSet.has(name.toLowerCase()))) return false
    return true
  })
  const hasConstraints = glassSet.size || categorySet.size || haveSet.size || alcoholic || q
  const pool = filtered.length || hasConstraints ? filtered : catalog.cocktails
  const items = pool.map(c => ({
    cocktail: c,
    score: scoreCocktail(c, q, glassSet, categorySet, haveSet, alcoholic)
  })).filter(item => item.score > -3).sort((a, b) => b.score - a.score)
  return { items, total: items.length }
}

function scoreCocktail(cocktail, q, glassSet, categorySet, haveSet, alcoholic){
  let score = 0
  if (q){
    const hay = [cocktail.name, cocktail.base, cocktail.details?.glass, cocktail.details?.garnish, ...(cocktail.tastes || []), ...(cocktail.ingredients || [])].join(' ').toLowerCase()
    score += hay.includes(q) ? 2 : -5
  }
  if (glassSet.size) score += glassSet.has(cocktail.glassId) ? 3 : -3
  if (categorySet.size) score += categorySet.has(cocktail.categoryId) ? 2 : -2
  if (alcoholic) score += cocktail.alcoholicId === alcoholic ? 2 : -4
  if (haveSet.size) score += (cocktail.ingredients || []).filter(i => haveSet.has(i.toLowerCase())).length * 1.5
  return score
}

function toSet(value){
  if (!value) return new Set()
  if (value instanceof Set) return new Set(value)
  if (Array.isArray(value)) return new Set(value.filter(Boolean))
  return new Set([value].filter(Boolean))
}

function toLowerSet(value){
  const set = toSet(value)
  return new Set(Array.from(set).map(v => (v || '').toString().toLowerCase()).filter(Boolean))
}

const slugify = (value = '') => value.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '')
const slugifyLocale = (value = '') => value.toString().trim().toLowerCase()
  .replace(/[^\p{L}\p{N}]+/gu, '-')
  .replace(/^-|-$/g, '')

const matchGlassId = (value = '') => {
  const normalized = value.toString().trim().toLowerCase()
  const slug = slugify(value)
  const matches = (...terms) => terms.some(term => term && normalized.includes(term))
  if (slug.includes('highball') || slug.includes('collins') || matches('하이볼', '콜린스')) return 'highball'
  if (
    slug.includes('old-fashioned') ||
    slug.includes('rocks') ||
    matches('올드패션', '위스키', '온더락', '록스')
  ) return 'rocks'
  if (slug.includes('margarita') || matches('마가리타')) return 'margarita'
  if (
    slug.includes('martini') ||
    slug.includes('coupe') ||
    matches('마티니', '쿠페', '칵테일')
  ) return 'martini'
  return 'other-glass'
}

const matchCategoryId = (value = '') => {
  const slug = slugify(value)
  if (slug.includes('party') || slug.includes('punch') || slug.includes('tiki')) return 'party'
  if (slug.includes('classic') || slug.includes('ordinary') || slug.includes('iba')) return 'classic'
  if (slug.includes('refresh') || slug.includes('cooler') || slug.includes('long') || slug.includes('fizz')) return 'refreshing'
  if (slug.includes('dessert') || slug.includes('sweet') || slug.includes('after-dinner')) return 'dessert'
  if (slug.includes('signature') || slug.includes('contemporary')) return 'signature'
  return 'other'
}

const matchAlcoholId = (value = '') => (slugify(value).includes('non') ? 'non-alcoholic' : 'alcoholic')

function normalizeCocktailDbDrink(drink){
  if (!drink) return null
  const ingredients = []
  for (let i = 1; i <= 15; i += 1){
    const key = drink[`strIngredient${i}`]
    if (key && key.trim()) ingredients.push(key.trim().toLowerCase())
  }
  const instructions = drink.strInstructions?.trim() || ''
  const categoryLabel = drink.strCategory?.trim() || ''
  const categoryId = normalizeCategoryId(categoryLabel)
  const ingredientGroups = normalizeIngredientGroups(drink.ingredientGroups)
  return {
    id: drink.idDrink,
    name: drink.strDrink,
    base: (drink.strIngredient1 || '').toLowerCase(),
    tastes: [drink.strCategory, drink.strIBA].filter(Boolean),
    ingredients,
    instructions,
    image: drink.strDrinkThumb || '',
    strength: matchAlcoholId(drink.strAlcoholic) === 'alcoholic' ? 'medium' : 'light',
    glassId: matchGlassId(drink.strGlass),
    categoryId,
    categoryLabel: categoryLabel || formatCategoryLabel(categoryId),
    alcoholicId: matchAlcoholId(drink.strAlcoholic),
    details: {
      method: drink.strTags || '',
      glass: drink.strGlass || '',
      garnish: drink.strIBA || '',
      steps: instructions ? instructions.split('.').map(s => s.trim()).filter(Boolean) : undefined,
      enjoy: drink.strDescription || ''
    },
    ingredientGroups
  }
}

function categorizeIngredient(name){
  const normalized = name.replace(/\s*\(.*?\)\s*/g, '').trim().toLowerCase()
  if (!normalized) return 'other'
  if (INGREDIENT_CATEGORY_OVERRIDES[normalized]) return INGREDIENT_CATEGORY_OVERRIDES[normalized]
  if (normalized.includes('zest') || normalized.includes('peel')) return 'garnish'
  const sampleCategory = categoryOfIng(normalized)
  if (sampleCategory) return sampleCategory
  for (const rule of CATEGORY_KEYWORD_RULES){
    if (rule.test.test(normalized)) return rule.id
  }
  return 'other'
}

function normalizeIngredientGroups(groups){
  if (!Array.isArray(groups)) return null
  const normalized = groups.map(group => {
    if (!group) return null
    const items = Array.isArray(group.items)
      ? group.items.map(item => (item || '').toString().trim().toLowerCase()).filter(Boolean)
      : []
    if (!items.length) return null
    const categoryId = (group.categoryId || '').toLowerCase() || categorizeIngredient(items[0])
    return {
      categoryId,
      categoryLabel: group.categoryLabel || formatCategoryLabel(categoryId),
      items
    }
  }).filter(Boolean)
  return normalized.length ? normalized : null
}

function normalizeCategoryId(label = ''){
  const matched = matchCategoryId(label)
  if (matched && matched !== 'other') return matched
  const slug = slugifyLocale(label)
  return slug || 'other'
}
