import { Api } from '../services/api.js'
import { el } from '../components/common.js'
import { ResultCard } from '../components/resultCard.js'
import { state, persist } from '../state.js'

let bound=false
export function bindCocktailsOnce(){
  if (bound) return; bound=true
  const input = document.getElementById('listSearch')
  input.addEventListener('input', async e=>{
    const q = e.target.value.toLowerCase()
    const res = await Api.cocktails({ q })
    renderCards(document.getElementById('cocktailList'), res.items)
  })
}

export function renderCards(container, list){
  container.innerHTML = ''
  if (!list.length){
    container.appendChild(el('div',{class:'empty card'},'표시할 항목이 없습니다.'))
    return
  }
  const favs = new Set(state.favorites)
  list.forEach(c => {
    container.appendChild(ResultCard({ cocktail:c, isFavorite: favs.has(c.id), onFavorite: async (id)=>{
      const out = await Api.toggleFavorite(id)
      state.favorites = new Set(out.items)
      persist.saveFav()
      const q = document.getElementById('listSearch').value
      const res = await Api.cocktails({ q })
      renderCards(container, res.items)
    }}))
  })
}

