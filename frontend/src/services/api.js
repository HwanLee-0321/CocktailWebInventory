// API service with graceful fallback to local sample data
// Configure base URL via Vite env: VITE_API_BASE

import { COCKTAILS, INGREDIENTS, GLASSES, DRINK_CATEGORIES, ALCOHOL_OPTIONS, INGREDIENT_CATEGORIES, categoryOfIng } from '../data/sample.js'

const API_BASE = import.meta.env.VITE_API_BASE || 'http://localhost:8080/api/v1'
const API_DISABLED = String(import.meta.env.VITE_API_DISABLED || '').toLowerCase() === 'true'
const DEFAULT_HEADERS = { 'Content-Type': 'application/json' }
const CACHE_TTL = 5 * 60 * 1000
const INGREDIENT_CACHE_TTL = 60 * 1000

const cacheStore = new Map()
const hasStructuredClone = typeof structuredClone === 'function'
const safeClone = (value) => {
  try {
    return hasStructuredClone ? structuredClone(value) : JSON.parse(JSON.stringify(value))
  } catch {
    return value
  }
}
function cacheRead(key){
  const hit = cacheStore.get(key)
  if (!hit) return
  if (Date.now() > hit.expire){
    cacheStore.delete(key)
    return
  }
  return safeClone(hit.value)
}
function cacheWrite(key, value, ttl = CACHE_TTL){
  cacheStore.set(key, { value: safeClone(value), expire: Date.now() + ttl })
}
let apiDeadUntil = 0
const isOffline = () => API_DISABLED || Date.now() < apiDeadUntil

async function fetchJSON(path, opts = {}){
  if (isOffline()) return { __error: true, error: new Error('API disabled/offline') }
  const url = `${API_BASE}${path}`
  try {
    const res = await fetch(url, { ...opts, headers: { ...DEFAULT_HEADERS, ...(opts.headers||{}) } })
    if (!res.ok) throw new Error(`HTTP ${res.status}`)
    return await res.json()
  } catch (e) {
    // Mark offline for 60s to avoid console spam when backend is down
    apiDeadUntil = Date.now() + 60_000
    console.warn('[api] offline, using sample data:', e?.message || e)
    return { __error: true, error: e }
  }
}

const slugify = (value='')=> value.toLowerCase().replace(/[^a-z0-9]+/g,'-').replace(/^-|-$/g,'')
const matchGlassId = (value='')=>{
  const slug = slugify(value)
  if (slug.includes('highball')) return 'highball'
  if (slug.includes('old-fashioned') || slug.includes('rocks')) return 'rocks'
  if (slug.includes('margarita')) return 'margarita'
  if (slug.includes('martini') || slug.includes('coupe')) return 'martini'
  return 'other-glass'
}
const matchCategoryId = (value='')=>{
  const slug = slugify(value)
  if (slug.includes('party') || slug.includes('punch')) return 'party'
  if (slug.includes('classic') || slug.includes('ordinary') || slug.includes('iba')) return 'classic'
  if (slug.includes('refresh') || slug.includes('cooler') || slug.includes('long')) return 'refreshing'
  if (slug.includes('dessert') || slug.includes('sweet')) return 'dessert'
  if (slug.includes('signature') || slug.includes('contemporary')) return 'signature'
  return 'other'
}
const matchAlcoholId = (value='')=>{
  const slug = slugify(value)
  if (!slug) return 'alcoholic'
  return slug.includes('non') ? 'non-alcoholic' : 'alcoholic'
}
function normalizeCocktailDbDrink(drink){
  if (!drink) return null
  const ingredients = []
  for (let i=1;i<=15;i+=1){
    const key = drink[`strIngredient${i}`]
    if (key && key.trim()) ingredients.push(key.trim().toLowerCase())
  }
  const instructions = drink.strInstructions?.trim() || ''
  return {
    id: drink.idDrink,
    name: drink.strDrink,
    base: (drink.strIngredient1 || '').toLowerCase(),
    tastes: [drink.strCategory, drink.strIBA].filter(Boolean),
    ingredients,
    instructions,
    image: drink.strDrinkThumb || '',
    strength: matchAlcoholId(drink.strAlcoholic)==='alcoholic' ? 'medium' : 'light',
    glassId: matchGlassId(drink.strGlass),
    categoryId: matchCategoryId(drink.strCategory),
    categoryLabel: drink.strCategory || '',
    alcoholicId: matchAlcoholId(drink.strAlcoholic),
    details:{
      method: drink.strTags || '',
      glass: drink.strGlass || '',
      garnish: drink.strIBA || '',
      steps: instructions ? instructions.split('.').map(s=>s.trim()).filter(Boolean) : undefined,
      enjoy: ''
    }
  }
}
async function fetchCocktailDbPopular(){
  const cacheKey = 'cocktaildb:popular'
  const cached = cacheRead(cacheKey)
  if (cached) return cached
  const externalUrl = import.meta.env.VITE_COCKTAIL_DB_URL || 'https://www.thecocktaildb.com/api/json/v2/1/popular.php'
  const res = await fetch(externalUrl)
  if (!res.ok) throw new Error(`HTTP ${res.status}`)
  const data = await res.json()
  const mapped = (data?.drinks || []).map(normalizeCocktailDbDrink).filter(Boolean)
  cacheWrite(cacheKey, mapped, CACHE_TTL)
  return mapped
}
async function getFallbackCocktails(){
  try {
    const external = await fetchCocktailDbPopular()
    if (external.length) return external
  } catch (err){
    console.warn('[api] fallback cocktaildb error:', err?.message || err)
  }
  return COCKTAILS
}

export const Api = {
  async taxonomy(){
    const cacheKey = 'taxonomy'
    const cached = cacheRead(cacheKey)
    if (cached) return cached
    const [glasses, alcoholic, drinkCats, ingredientCats] = await Promise.all([
      fetchJSON('/taxonomy/glasses'),
      fetchJSON('/taxonomy/alcoholic'),
      fetchJSON('/taxonomy/drink-categories'),
      fetchJSON('/taxonomy/ingredient-categories'),
    ])
    if (glasses.__error || alcoholic.__error || drinkCats.__error || ingredientCats.__error){
      const fallback = {
        glasses: GLASSES,
        alcoholic: ALCOHOL_OPTIONS,
        drinkCategories: DRINK_CATEGORIES,
        ingredientCategories: INGREDIENT_CATEGORIES
      }
      cacheWrite(cacheKey, fallback)
      return fallback
    }
    const payload = {
      glasses: glasses.items,
      alcoholic: alcoholic.items,
      drinkCategories: drinkCats.items,
      ingredientCategories: ingredientCats.items
    }
    cacheWrite(cacheKey, payload)
    return payload
  },
  async cocktails(params={}){
    const query = new URLSearchParams()
    if (params.q) query.set('q', params.q)
    ;(params.base||[]).forEach(b=> query.append('base', b))
    ;(params.taste||[]).forEach(t=> query.append('taste', t))
    ;(params.ingredient||[]).forEach(i=> query.append('ingredient', i))
    ;(params.glasses||[]).forEach(g=> query.append('glass', g))
    ;(params.categories||[]).forEach(c=> query.append('category', c))
    if (params.alcoholic) query.set('alcoholic', params.alcoholic)
    if (params.strength) query.set('strength', params.strength)
    const res = await fetchJSON(`/cocktails${query.toString()?`?${query}`:''}`)
    if (res.__error) return { items: COCKTAILS, total: COCKTAILS.length }
    return res
  },
  async cocktail(id){
    const res = await fetchJSON(`/cocktails/${encodeURIComponent(id)}`)
    if (res.__error) return COCKTAILS.find(x=>x.id===id)
    return res
  },
  async ingredients(params={}){
    const cacheKey = `ingredients:${params.category||'all'}:${params.q||''}`
    const cached = cacheRead(cacheKey)
    if (cached) return cached
    const query = new URLSearchParams()
    if (params.q) query.set('q', params.q)
    if (params.category) query.set('category', params.category)
    const res = await fetchJSON(`/ingredients${query.toString()?`?${query}`:''}`)
    if (res.__error) {
      const list = (params.category && params.category!=='all')
        ? INGREDIENTS.filter(n=> categoryOfIng(n)===params.category)
        : INGREDIENTS
      const filtered = { items: list.filter(n=> params.q? n.toLowerCase().includes(params.q.toLowerCase()): true) }
      cacheWrite(cacheKey, filtered, INGREDIENT_CACHE_TTL)
      return filtered
    }
    cacheWrite(cacheKey, res, INGREDIENT_CACHE_TTL)
    return res
  },
  async recommendations(params={}){
    const query = new URLSearchParams()
    if (params.q) query.set('q', params.q)
    ;(params.glasses||[]).forEach(g=> query.append('glass', g))
    ;(params.categories||[]).forEach(c=> query.append('category', c))
    ;(params.ingredients||[]).forEach(i=> query.append('ingredient', i))
    if (params.alcoholic) query.set('alcoholic', params.alcoholic)
    const res = await fetchJSON(`/recommendations${query.toString()?`?${query}`:''}`)
    if (res.__error){
      const glassSet = new Set(params.glasses||[])
      const categorySet = new Set(params.categories||[])
      const haveSet = new Set((params.ingredients||[]).map(i=>i.toLowerCase()))
      const alcoholic = params.alcoholic || null
      const q = (params.q||'').toLowerCase()
      const dataset = await getFallbackCocktails()
      const filtered = dataset.filter(c=>{
        if (glassSet.size && !glassSet.has(c.glassId)) return false
        if (categorySet.size && !categorySet.has(c.categoryId)) return false
        if (alcoholic && c.alcoholicId !== alcoholic) return false
        if (haveSet.size){
          const intersects = c.ingredients.some(name=> haveSet.has(name.toLowerCase()))
          if (!intersects) return false
        }
        return true
      })
      const hasConstraints = glassSet.size>0 || categorySet.size>0 || haveSet.size>0 || Boolean(alcoholic) || q.length>0
      const pool = filtered.length || hasConstraints ? filtered : dataset
      const scored = pool.map(c=>{
        let score = 0
        if (q){
          const hay = [c.name,c.base,c.details?.glass,c.details?.garnish,...(c.tastes||[]),...(c.ingredients||[])].join(' ').toLowerCase()
          score += hay.includes(q) ? 2 : -5
        }
        if (glassSet.size) score += glassSet.has(c.glassId) ? 3 : -3
        if (categorySet.size) score += categorySet.has(c.categoryId) ? 2 : -2
        if (alcoholic) score += c.alcoholicId === alcoholic ? 2 : -4
        score += c.ingredients.filter(i=> haveSet.has(i.toLowerCase())).length * 1.5
        return { cocktail:c, score }
      }).filter(x=>x.score>-3).sort((a,b)=>b.score-a.score)
      return { items: scored, total: scored.length }
    }
    return res
  },
  async getInventory(){
    const res = await fetchJSON('/me/inventory')
    if (res.__error) return { items: JSON.parse(localStorage.getItem('have')||'[]') }
    return res
  },
  async replaceInventory(items){
    const res = await fetchJSON('/me/inventory', { method:'PUT', body: JSON.stringify({ items }) })
    if (res.__error){ localStorage.setItem('have', JSON.stringify(items)); return { items } }
    return res
  },
  async patchInventory(add=[], remove=[]){
    const res = await fetchJSON('/me/inventory', { method:'PATCH', body: JSON.stringify({ add, remove }) })
    if (res.__error){
      const cur = new Set(JSON.parse(localStorage.getItem('have')||'[]'))
      add.forEach(i=>cur.add(i)); remove.forEach(i=>cur.delete(i))
      const items = [...cur]
      localStorage.setItem('have', JSON.stringify(items))
      return { items }
    }
    return res
  },
  async getFavorites(){
    const res = await fetchJSON('/me/favorites')
    if (res.__error) return { items: JSON.parse(localStorage.getItem('favorites')||'[]') }
    return res
  },
  async replaceFavorites(items){
    const res = await fetchJSON('/me/favorites', { method:'PUT', body: JSON.stringify({ items }) })
    if (res.__error){ localStorage.setItem('favorites', JSON.stringify(items)); return { items } }
    return res
  },
  async toggleFavorite(id){
    const res = await fetchJSON('/me/favorites/toggle', { method:'PATCH', body: JSON.stringify({ id }) })
    if (res.__error){
      const cur = new Set(JSON.parse(localStorage.getItem('favorites')||'[]'))
      if (cur.has(id)) cur.delete(id); else cur.add(id)
      const items = [...cur]
      localStorage.setItem('favorites', JSON.stringify(items))
      return { items }
    }
    return res
  }
}
