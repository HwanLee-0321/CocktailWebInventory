// API service with graceful fallback to local sample data
// Configure base URL via Vite env: VITE_API_BASE

import { COCKTAILS, INGREDIENTS, BASES, TASTES, CATEGORIES, categoryOfIng } from '../data/sample.js'

const API_BASE = import.meta.env.VITE_API_BASE || 'http://localhost:8080/api/v1'
const API_DISABLED = String(import.meta.env.VITE_API_DISABLED || '').toLowerCase() === 'true'
const DEFAULT_HEADERS = { 'Content-Type': 'application/json' }

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

export const Api = {
  async taxonomy(){
    const bases = await fetchJSON('/taxonomy/bases')
    const tastes = await fetchJSON('/taxonomy/tastes')
    const cats = await fetchJSON('/taxonomy/ingredient-categories')
    if (bases.__error || tastes.__error || cats.__error){
      return { bases: BASES.map(id=>({ id, labelKo: id })), tastes: TASTES, categories: CATEGORIES }
    }
    return { bases: bases.items, tastes: tastes.items, categories: cats.items }
  },
  async cocktails(params={}){
    const query = new URLSearchParams()
    if (params.q) query.set('q', params.q)
    ;(params.base||[]).forEach(b=> query.append('base', b))
    ;(params.taste||[]).forEach(t=> query.append('taste', t))
    ;(params.ingredient||[]).forEach(i=> query.append('ingredient', i))
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
    const query = new URLSearchParams()
    if (params.q) query.set('q', params.q)
    if (params.category) query.set('category', params.category)
    const res = await fetchJSON(`/ingredients${query.toString()?`?${query}`:''}`)
    if (res.__error) {
      const list = (params.category && params.category!=='all')
        ? INGREDIENTS.filter(n=> categoryOfIng(n)===params.category)
        : INGREDIENTS
      return { items: list.filter(n=> params.q? n.toLowerCase().includes(params.q.toLowerCase()): true) }
    }
    return res
  },
  async recommendations(params={}){
    const query = new URLSearchParams()
    if (params.q) query.set('q', params.q)
    ;(params.bases||[]).forEach(b=> query.append('bases', b))
    ;(params.tastes||[]).forEach(t=> query.append('tastes', t))
    ;(params.have||[]).forEach(i=> query.append('have', i))
    const res = await fetchJSON(`/recommendations${query.toString()?`?${query}`:''}`)
    if (res.__error){
      // simple local scoring fallback
      const tasteSet = new Set(params.tastes||[])
      const baseSet = new Set(params.bases||[])
      const haveSet = new Set(params.have||[])
      const q = (params.q||'').toLowerCase()
      const scored = COCKTAILS.map(c=>{
        let score = 0
        if (q){
          const hay = [c.name,c.base,...c.tastes,...c.ingredients].join(' ').toLowerCase()
          score += hay.includes(q) ? 2 : -5
        }
        score += (baseSet.size===0 || baseSet.has(c.base)) ? 2 : -3
        c.tastes.forEach(t=>{ if (tasteSet.has(t)) score += 1 })
        score += c.ingredients.filter(i=>haveSet.has(i)).length * 1.5
        if (c.strength==='strong' && tasteSet.has('sweet')) score -= .5
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
