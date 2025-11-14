import { Api } from '../services/api.js'
import { el, labelIng } from '../components/common.js'
import { ResultCard } from '../components/resultCard.js'
import { renderPaginatedList } from '../components/pagination.js'
import { state, storage } from '../state.js'

let taxonomyCache = null
const requestRecommendSection = (section)=>{
  window.dispatchEvent(new CustomEvent('recommend:set-section', { detail: section }))
}
const ensureTaxonomy = async (tax)=>{
  if (tax) taxonomyCache = tax
  if (!taxonomyCache) taxonomyCache = await Api.taxonomy()
  return taxonomyCache
}
const lookupLabel = (list=[], id)=> list.find(item=>item.id===id)?.label || id
const setFilterActive = (kind, id, isActive)=>{
  const chip = document.querySelector(`[data-filter="${kind}"][data-id="${id}"]`)
  if (chip) chip.classList.toggle('active', !!isActive)
}
const syncAlcoholChips = ()=> document.querySelectorAll('[data-filter="alcohol"]').forEach(chip=>{
  chip.classList.toggle('active', chip.dataset.id === (state.alcoholic||''))
})
const activateResultsView = (delay=false)=>{
  const target = '#/recommend/results'
  const run = ()=>{
    if (location.hash !== target){
      location.hash = target
    } else {
      requestRecommendSection('results')
    }
  }
  if (delay){
    clearTimeout(activateResultsView.timer)
    activateResultsView.timer = setTimeout(run, 300)
  } else {
    run()
  }
}

export function updateCounts(){
  const glassRoot = document.getElementById('glassFilters')
  const alcoholRoot = document.getElementById('alcoholFilters')
  const categoryRoot = document.getElementById('categoryFilters')
  const haveRoot = document.getElementById('ingredientsList')
  if (glassRoot && glassRoot.previousElementSibling) glassRoot.previousElementSibling.textContent = `잔 종류 (${state.glasses.size})`
  if (alcoholRoot && alcoholRoot.previousElementSibling) alcoholRoot.previousElementSibling.textContent = `알코올 (${state.alcoholic ? 1 : 0})`
  if (categoryRoot && categoryRoot.previousElementSibling) categoryRoot.previousElementSibling.textContent = `음료 카테고리 (${state.categories.size})`
  if (haveRoot && haveRoot.previousElementSibling) haveRoot.previousElementSibling.textContent = `보유 재료 (${state.have.size})`
}

export function renderSelectedSummary(tax){
  if (tax) taxonomyCache = tax
  const taxonomy = taxonomyCache || {}
  const root = document.getElementById('selectedSummary')
  if (!root) return
  root.innerHTML = ''
  const glasses = taxonomy.glasses || []
  const categories = taxonomy.drinkCategories || []
  const alcoholic = taxonomy.alcoholic || []

  ;[...state.glasses].forEach(id=>{
    root.appendChild(
      el('span',{class:'chip removable', onclick:()=>{
        state.glasses.delete(id)
        setFilterActive('glass', id, false)
        updateCounts()
        renderSelectedSummary()
        recommend()
      }}, lookupLabel(glasses, id), el('span',{class:'x'},'×'))
    )
  })
  if (state.alcoholic){
    root.appendChild(
      el('span',{class:'chip removable', onclick:()=>{
        state.alcoholic = null
        syncAlcoholChips()
        updateCounts()
        renderSelectedSummary()
        recommend()
      }}, lookupLabel(alcoholic, state.alcoholic), el('span',{class:'x'},'×'))
    )
  }
  ;[...state.categories].forEach(id=>{
    root.appendChild(
      el('span',{class:'chip removable', onclick:()=>{
        state.categories.delete(id)
        setFilterActive('category', id, false)
        updateCounts()
        renderSelectedSummary()
        recommend()
      }}, lookupLabel(categories, id), el('span',{class:'x'},'×'))
    )
  })
  ;[...state.have].forEach(name=>{
    root.appendChild(
      el('span',{class:'chip removable', onclick:()=>{
        storage.toggleInventory(name, false)
        updateCounts()
        renderSelectedSummary()
        renderIngredients()
        recommend()
      }}, labelIng(name), el('span',{class:'x'},'×'))
    )
  })
}

export async function renderFilters(taxonomy){
  const tax = await ensureTaxonomy(taxonomy)
  const glassRoot = document.getElementById('glassFilters')
  if (!glassRoot) return
  glassRoot.innerHTML = ''
  ;(tax.glasses||[]).forEach(glass=>{
    const chip = el('span',{
      class:`chip ${state.glasses.has(glass.id)?'active':''}`,
      'data-filter':'glass',
      'data-id':glass.id,
      onclick:()=>{
        if (state.glasses.has(glass.id)) state.glasses.delete(glass.id)
        else state.glasses.add(glass.id)
        setFilterActive('glass', glass.id, state.glasses.has(glass.id))
        updateCounts()
        renderSelectedSummary()
        recommend()
      }
    }, glass.label || lookupLabel(tax.glasses, glass.id))
    glassRoot.appendChild(chip)
  })

  const alcoholRoot = document.getElementById('alcoholFilters')
  if (alcoholRoot){
    alcoholRoot.innerHTML = ''
    ;(tax.alcoholic||[]).forEach(opt=>{
      const chip = el('span',{
        class:`chip ${state.alcoholic===opt.id?'active':''}`,
        'data-filter':'alcohol',
        'data-id':opt.id,
        onclick:()=>{
          state.alcoholic = state.alcoholic===opt.id ? null : opt.id
          syncAlcoholChips()
          updateCounts()
          renderSelectedSummary()
          activateResultsView(true)
          recommend()
        }
      }, opt.label)
      alcoholRoot.appendChild(chip)
    })
  }

  const categoryRoot = document.getElementById('categoryFilters')
  if (categoryRoot){
    categoryRoot.innerHTML = ''
    ;(tax.drinkCategories||[]).filter(c=>c.id!=='all').forEach(cat=>{
      const chip = el('span',{
        class:`chip ${state.categories.has(cat.id)?'active':''}`,
        'data-filter':'category',
        'data-id':cat.id,
        onclick:()=>{
          if (state.categories.has(cat.id)) state.categories.delete(cat.id)
          else state.categories.add(cat.id)
        setFilterActive('category', cat.id, state.categories.has(cat.id))
        updateCounts()
        renderSelectedSummary()
        activateResultsView(true)
        recommend()
      }
      }, cat.label)
      categoryRoot.appendChild(chip)
    })
  }

  updateCounts()
  renderSelectedSummary(tax)
}

export async function renderIngredients(taxonomy){
  const root = document.getElementById('ingredientsList')
  if (!root) return
  root.innerHTML = ''
  const tax = taxonomy || taxonomyCache || await Api.taxonomy()
  if (!taxonomyCache) taxonomyCache = tax
  const ingredientCategories = tax.ingredientCategories || []
  const cats = ingredientCategories.filter(c=>c.id!=='all')
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
        el('input',{type:'checkbox',checked:checked?true:undefined, onchange:e=>{
          storage.toggleInventory(name, e.target.checked)
          label.classList.toggle('active', e.target.checked)
          updateCounts()
          renderSelectedSummary()
          recommend()
        }}),
        el('span',{class:'box'}),
        el('span',{class:'text'}, labelIng(name))
      )
      grid.appendChild(label)
    })
    root.appendChild(group)
  })
  updateCounts()
  renderSelectedSummary()
}

export async function recommend(){
  const res = await Api.recommendations({
    q: state.search,
    glasses: [...state.glasses],
    categories: [...state.categories],
    ingredients: [...state.have],
    alcoholic: state.alcoholic
  })
  renderRecommendationResults(res.items || [], true)
}

const buildRecommendPager = ({ current, total, pageSize, totalItems, onChange })=>{
  const start = (current - 1) * pageSize + 1
  const end = Math.min(totalItems, current * pageSize)
  const chunkLabel = `${pageSize}개`
  const hasPrev = start > 1
  const hasNext = end < totalItems
  return el('div',{class:'results-pagination__inner'},
    el('button',{
      type:'button',
      class:'results-pagination__btn',
      disabled: hasPrev ? undefined : true,
      onclick:()=> hasPrev && onChange(current - 1)
    }, `이전 ${chunkLabel}`),
    el('div',{class:'results-pagination__summary'}, `${start}-${end}개 / 총 ${totalItems}개 · 페이지 ${current}/${total}`),
    el('button',{
      type:'button',
      class:'results-pagination__btn',
      disabled: hasNext ? undefined : true,
      onclick:()=> hasNext && onChange(current + 1)
    }, `다음 ${chunkLabel}`)
  )
}

function renderRecommendationResults(items, resetPage = false){
  const root = document.getElementById('results')
  const pagerRoot = document.getElementById('resultsPagination')
  if (!root) return
  renderPaginatedList({
    container: root,
    items,
    pageKey: 'recommend',
    pageSize: 9,
    emptyMessage: '조건에 맞는 추천이 없습니다.',
    resetPage,
    pagerContainer: pagerRoot,
    renderPager: pagerRoot ? buildRecommendPager : null,
    renderItem: ({ cocktail, score })=>{
      const stars = Math.max(1, Math.min(5, Math.round((score + 3) / 2)))
      return ResultCard({
        cocktail,
        stars,
        isFavorite: state.favorites.has(cocktail.id),
        onFavorite: (id)=>{
          storage.toggleFavorite(id)
          renderRecommendationResults(items)
        }
      })
    }
  })
}

let bound = false
export function bindRecommendOnce(){
  if (bound) return; bound = true
  const search = document.getElementById('searchInput')
  search.addEventListener('input', e=>{ state.search = e.target.value; })
  search.addEventListener('keydown', e=>{
    if (e.key !== 'Enter') return
    e.preventDefault()
    state.search = e.target.value
    activateResultsView()
    recommend()
  })
  document.getElementById('recommendBtn').addEventListener('click', recommend)
  document.getElementById('clearBtn').addEventListener('click', async ()=>{
    state.search='' 
    state.glasses.clear()
    state.categories.clear()
    state.alcoholic = null
    storage.clearInventory()
    document.getElementById('searchInput').value=''
    await renderFilters()
    await renderIngredients()
    await recommend()
  })
}
