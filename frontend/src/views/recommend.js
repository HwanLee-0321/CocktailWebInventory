import { Api } from '../services/api.js'
import { el, labelBase, labelIng } from '../components/common.js'
import { ResultCard } from '../components/resultCard.js'
import { state, persist } from '../state.js'

export function updateCounts(){
  const baseRoot = document.getElementById('baseFilters')
  const tasteRoot = document.getElementById('tasteFilters')
  const haveRoot = document.getElementById('ingredientsList')
  if (baseRoot && baseRoot.previousElementSibling) baseRoot.previousElementSibling.textContent = `베이스 (${state.bases.size})`
  if (tasteRoot && tasteRoot.previousElementSibling) tasteRoot.previousElementSibling.textContent = `취향 (${state.tastes.size})`
  if (haveRoot && haveRoot.previousElementSibling) haveRoot.previousElementSibling.textContent = `보유 재료 (${state.have.size})`
}

export function renderSelectedSummary(TASTES){
  const root = document.getElementById('selectedSummary')
  if (!root) return
  root.innerHTML = ''
  ;[...state.bases].forEach(id=>{
    root.appendChild(el('span',{class:'chip removable', onclick:()=>{ state.bases.delete(id); renderFilters(TASTES); recommend(); }}, labelBase(id), el('span',{class:'x'},'×')))
  })
  ;[...state.tastes].forEach(id=>{
    const t = TASTES.find(x=>x.id===id)
    root.appendChild(el('span',{class:'chip removable', onclick:()=>{ state.tastes.delete(id); renderFilters(TASTES); recommend(); }}, t? t.label : id, el('span',{class:'x'},'×')))
  })
  ;[...state.have].forEach(name=>{
    root.appendChild(el('span',{class:'chip removable', onclick:async ()=>{ state.have.delete(name); persist.saveHave(); updateCounts(); renderSelectedSummary(TASTES); renderIngredients(); recommend(); await Api.patchInventory([], [name]) }}, labelIng(name), el('span',{class:'x'},'×')))
  })
}

export async function renderFilters(TASTES){
  const baseRoot = document.getElementById('baseFilters')
  if (!baseRoot) return
  baseRoot.innerHTML = ''
  const tax = await Api.taxonomy()
  const tastesData = TASTES || tax.tastes || []
  const makeBaseToggle = (id) => {
    const chip = el('span',{class:'chip', onclick:(e)=>{
      const isActive = state.bases.has(id)
      if (isActive) state.bases.delete(id); else state.bases.add(id)
      e.currentTarget.classList.toggle('active', !isActive)
      updateCounts(); recommend(); renderSelectedSummary(tastesData)
    }}, labelBase(id))
    chip.classList.toggle('active', state.bases.has(id))
    return chip
  }
  ;(tax.bases||[]).forEach(b=>{
    const id = b.id || b
    baseRoot.appendChild(makeBaseToggle(id))
  })
  const tasteRoot = document.getElementById('tasteFilters')
  tasteRoot.innerHTML = ''
  ;(tastesData).forEach(t=>{
    const chip = el('span',{class:'chip', onclick:(e)=>{
      const isActive = state.tastes.has(t.id)
      if (isActive) state.tastes.delete(t.id); else state.tastes.add(t.id)
      e.currentTarget.classList.toggle('active', !isActive)
      updateCounts(); recommend(); renderSelectedSummary(tastesData)
    }}, t.label)
    chip.classList.toggle('active', state.tastes.has(t.id))
    tasteRoot.appendChild(chip)
  })
  updateCounts(); renderSelectedSummary(tastesData)
}

export async function renderIngredients(){
  const root = document.getElementById('ingredientsList')
  if (!root) return
  root.innerHTML = ''
  const tax = await Api.taxonomy()
  const cats = (tax.categories||[]).filter(c=>c.id!=='all')
  const activeCats = cats.filter(cat => state.ingCat==='all' || state.ingCat===cat.id)
  const lists = await Promise.all(activeCats.map(cat => Api.ingredients({ category: cat.id })))
  activeCats.forEach((cat, idx)=>{
    const result = lists[idx]
    const list = result?.items || []
    if (!list.length) return
    const group = el('div',{class:'group'},
      el('div',{class:'group__title'}, cat.label),
      el('div',{class:'ingredients'})
    )
    const grid = group.querySelector('.ingredients')
    list.forEach(name=>{
      const checked = state.have.has(name)
      const label = el('label',{class:`ingredient ${checked?'active':''}`},
        el('input',{type:'checkbox',checked:checked?true:undefined, onchange:async e=>{
          if (e.target.checked) { state.have.add(name); await Api.patchInventory([name], []) }
          else { state.have.delete(name); await Api.patchInventory([], [name]) }
          persist.saveHave(); label.classList.toggle('active', e.target.checked); updateCounts(); renderSelectedSummary(tax.tastes)
        }}),
        el('span',{class:'box'}),
        el('span',{class:'text'}, labelIng(name))
      )
      grid.appendChild(label)
    })
    root.appendChild(group)
  })
  updateCounts(); renderSelectedSummary(tax.tastes||[])
}

export async function recommend(){
  const res = await Api.recommendations({
    q: state.search,
    bases: [...state.bases],
    tastes: [...state.tastes],
    have: [...state.have]
  })
  const root = document.getElementById('results')
  if (!root) return
  root.innerHTML = ''
  if (!res.items || !res.items.length){
    root.appendChild(el('div',{class:'empty card'},'조건에 맞는 추천이 없습니다.'))
    return
  }
  const favs = new Set(state.favorites)
  res.items.forEach(({ cocktail, score })=>{
    const stars = Math.max(1, Math.min(5, Math.round((score + 3) / 2)))
    root.appendChild(ResultCard({ cocktail, stars, isFavorite: favs.has(cocktail.id), onFavorite: async (id)=>{
      const out = await Api.toggleFavorite(id)
      state.favorites = new Set(out.items)
      persist.saveFav()
      recommend()
    }}))
  })
}

let bound = false
export function bindRecommendOnce(){
  if (bound) return; bound = true
  const search = document.getElementById('searchInput')
  search.addEventListener('input', e=>{ state.search = e.target.value; })
  document.getElementById('recommendBtn').addEventListener('click', recommend)
  document.getElementById('clearBtn').addEventListener('click', async ()=>{
    state.search=''; state.bases.clear(); state.tastes.clear(); state.have.clear();
    persist.saveHave(); await Api.replaceInventory([])
    document.getElementById('searchInput').value=''; renderFilters((await Api.taxonomy()).tastes); renderIngredients(); recommend()
  })
}
