import { bindModalGlobal } from './components/modal.js'
import { bindRecommendOnce, renderFilters, renderIngredients, recommend } from './views/recommend.js'
import { bindIngredientsOnce, renderIngCategoryChips, renderIngredientsManager } from './views/ingredients.js'
import { bindCocktailsOnce, renderCards } from './views/cocktails.js'
import { renderFavorites } from './views/favorites.js'
import { Api } from './services/api.js'
import { state, syncFromServer } from './state.js'

// Theme toggle
document.addEventListener('click', (e) => {
  if (e.target && e.target.id === 'themeToggle') {
    document.documentElement.classList.toggle('light')
  }
})

function linkActive(hash){
  document.querySelectorAll('.nav__links .link').forEach(a=>{
    a.classList.toggle('active', a.getAttribute('href')===hash)
  })
}

function showView(id){
  document.querySelectorAll('[data-view]').forEach(v=>v.hidden=true)
  const elv = document.getElementById(id)
  if (elv) elv.hidden = false
}

async function route(hash){
  const h = hash || '#/recommend'
  linkActive(h)
  switch (h){
    case '#/recommend':
      showView('view-recommend')
      await renderFilters((await Api.taxonomy()).tastes); await renderIngCategoryChips('ingCatFilters'); await renderIngredients(); await recommend()
      bindRecommendOnce()
      break
    case '#/ingredients':
      showView('view-ingredients')
      await renderIngCategoryChips('ingMgrCatFilters', true); await renderIngredientsManager('')
      bindIngredientsOnce()
      break
    case '#/cocktails':
      showView('view-cocktails')
      bindCocktailsOnce()
      renderCards(document.getElementById('cocktailList'), (await Api.cocktails()).items)
      break
    case '#/favorites':
      showView('view-favorites')
      await renderFavorites()
      break
    default:
      location.hash = '#/recommend'
  }
}

window.addEventListener('hashchange', ()=> route(location.hash))
document.addEventListener('DOMContentLoaded', async ()=>{
  bindModalGlobal()
  await syncFromServer()
  route(location.hash||'#/recommend')
})
