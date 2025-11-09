import { Api } from '../services/api.js'
import { categoryOfIng } from '../data/sample.js'
import { el } from '../components/common.js'
import { state, persist } from '../state.js'
import { updateCounts, renderSelectedSummary, renderIngredients as renderIngredientsInRecommend } from './recommend.js'

export async function renderIngCategoryChips(rootId, isManager=false){
  const tax = await Api.taxonomy()
  const root = document.getElementById(rootId)
  if (!root) return
  root.innerHTML = ''
  ;(tax.categories||[]).forEach(cat=>{
    const active = (isManager? state.ingMgrCat : state.ingCat) === cat.id
    root.appendChild(el('span', { class:`chip ${active?'active':''}`, onclick:()=>{
      if (isManager){ state.ingMgrCat = cat.id; renderIngredientsManager(document.getElementById('ingSearch')?.value||'') }
      else { state.ingCat = cat.id; renderIngredientsInRecommend() }
    }}, cat.label))
  })
}

export async function renderIngredientGroups(rootId, items){
  const root = document.getElementById(rootId)
  if (!root) return
  root.innerHTML = ''
  const tax = await Api.taxonomy()
  const cats = (tax.categories||[]).filter(c=>c.id!=='all')
  cats.forEach(cat=>{
    const list = items.filter(n=> n && typeof n==='string' && categoryOfIng(n)===cat.id)
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
          persist.saveHave(); label.classList.toggle('active', e.target.checked); updateCounts(); renderSelectedSummary((await Api.taxonomy()).tastes)
        }}),
        el('span',{class:'box'}),
        el('span',{class:'text'}, name)
      )
      grid.appendChild(label)
    })
    root.appendChild(group)
  })
}

export async function renderIngredientsManager(filterText=''){
  const tax = await Api.taxonomy()
  const items = (await Api.ingredients({ category: state.ingMgrCat })).items.filter(n =>
    n.toLowerCase().includes((filterText||'').toLowerCase())
  )
  await renderIngredientGroups('ingredientsManager', items)
}

let bound=false
export function bindIngredientsOnce(){
  if (bound) return; bound = true
  const f = document.getElementById('ingSearch')
  f.addEventListener('input', e=> renderIngredientsManager(e.target.value))
  document.getElementById('ingSelectAll').addEventListener('click', async ()=>{
    const items = (await Api.ingredients()).items
    items.forEach(i=>state.have.add(i)); persist.saveHave(); await Api.replaceInventory([...state.have]); renderIngredientsManager(f.value||''); updateCounts()
  })
  document.getElementById('ingClearAll').addEventListener('click', async ()=>{ state.have.clear(); persist.saveHave(); await Api.replaceInventory([]); renderIngredientsManager(f.value||''); updateCounts() })
}
