import { bindModalGlobal } from './components/modal.js'
import { bindRecommendOnce, renderFilters, renderIngredients, recommend } from './views/recommend.js'
import { bindIngredientsOnce, renderIngCategoryChips, renderIngredientsManager } from './views/ingredients.js'
import { bindCocktailsOnce, renderCards } from './views/cocktails.js'
import { renderFavorites } from './views/favorites.js'
import { Api } from './services/api.js'
import { state, syncFromServer } from './state.js'

const DEFAULT_RECOMMEND_SECTION = 'filters'
const RECOMMEND_SECTIONS = new Set(['filters','ingredients','results'])
const DEFAULT_HASH = '#/recommend/filters'
let recommendReady = false
let pressStatesBound = false
let navBound = false

// Theme toggle
document.addEventListener('click', (e) => {
  if (e.target && e.target.id === 'themeToggle') {
    document.documentElement.classList.toggle('light')
  }
})

function linkActive(hash){
  document.querySelectorAll('.nav__links .link').forEach(a=>{
    const href = a.getAttribute('href')
    const active = hash === href || hash.startsWith(`${href}/`)
    a.classList.toggle('active', active)
  })
}

function showView(id){
  document.querySelectorAll('[data-view]').forEach(v=>v.hidden=true)
  const elv = document.getElementById(id)
  if (elv) elv.hidden = false
}

async function ensureRecommendReady(){
  if (recommendReady) return
  recommendReady = true
  const taxonomy = await Api.taxonomy()
  await renderFilters(taxonomy.tastes)
  await renderIngCategoryChips('ingCatFilters')
  await renderIngredients()
  await recommend()
  bindRecommendOnce()
}

function setRecommendSection(section){
  const target = RECOMMEND_SECTIONS.has(section) ? section : DEFAULT_RECOMMEND_SECTION
  document.querySelectorAll('[data-recommend-view]').forEach(el=>{
    const isActive = el.dataset.recommendView === target
    el.hidden = !isActive
    el.dataset.recommendActive = isActive ? 'true' : 'false'
  })
  document.querySelectorAll('[data-recommend-link]').forEach(link=>{
    link.classList.toggle('active', link.dataset.recommendLink === target)
  })
  return target
}

function parseHash(hash){
  const match = (hash||'').match(/^#\/([^/]+)(?:\/([^/]+))?/)
  return {
    base: match?.[1] || 'recommend',
    section: match?.[2]
  }
}

function bindPressStates(){
  if (pressStatesBound) return
  pressStatesBound = true
  const attach = (selector)=>{
    document.addEventListener('pointerdown', e=>{
      const target = e.target.closest(selector)
      if (!target) return
      target.classList.add('is-pressing')
      const clear = ()=>{
        target.classList.remove('is-pressing')
        window.removeEventListener('pointerup', clear)
        window.removeEventListener('pointercancel', clear)
      }
      window.addEventListener('pointerup', clear, { once:true })
      window.addEventListener('pointercancel', clear, { once:true })
    })
  }
  ['.recommend-nav .link','.card.interactive','.chip'].forEach(attach)
}

function bindNavLinks(){
  if (navBound) return
  navBound = true
  document.querySelectorAll('.nav__links .link').forEach(link=>{
    link.addEventListener('click', e=>{
      const href = link.getAttribute('href')
      if (!href) return
      e.preventDefault()
      if (location.hash === href){
        route(href)
      } else {
        location.hash = href
      }
    })
  })
}

async function route(hash){
  const targetHash = hash || DEFAULT_HASH
  const { base, section } = parseHash(targetHash)
  const normalized = `#/${base}${section?`/${section}`:''}`
  linkActive(normalized)
  switch (base){
    case 'recommend':{
      await ensureRecommendReady()
      showView('view-recommend')
      const activeSection = setRecommendSection(section)
      const desiredHash = `#/recommend/${activeSection}`
      if (normalized !== desiredHash){
        location.hash = desiredHash
        return
      }
      break
    }
    case 'ingredients':
      showView('view-ingredients')
      await renderIngCategoryChips('ingMgrCatFilters', true); await renderIngredientsManager('')
      bindIngredientsOnce()
      break
    case 'cocktails':
      showView('view-cocktails')
      bindCocktailsOnce()
      renderCards(document.getElementById('cocktailList'), (await Api.cocktails()).items)
      break
    case 'favorites':
      showView('view-favorites')
      await renderFavorites()
      break
    default:
      location.hash = DEFAULT_HASH
  }
}

window.addEventListener('hashchange', ()=> route(location.hash))
document.addEventListener('DOMContentLoaded', async ()=>{
  bindModalGlobal()
  bindPressStates()
  bindNavLinks()
  await syncFromServer()
  route(location.hash||DEFAULT_HASH)
})
