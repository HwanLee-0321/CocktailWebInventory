import { Api } from '../services/api.js'
import { el } from '../components/common.js'
import { ResultCard } from '../components/resultCard.js'
import { state, persist } from '../state.js'

export async function renderFavorites(){
  const favIds = (await Api.getFavorites()).items
  state.favorites = new Set(favIds)
  persist.saveFav()
  const all = (await Api.cocktails()).items
  const list = all.filter(c=> state.favorites.has(c.id))
  const root = document.getElementById('favoritesList')
  root.innerHTML = ''
  if (!list.length){ root.appendChild(el('div',{class:'empty card'},'표시할 항목이 없습니다.')); return }
  list.forEach(c => root.appendChild(ResultCard({ cocktail:c, isFavorite: true, onFavorite: async (id)=>{
    const out = await Api.toggleFavorite(id)
    state.favorites = new Set(out.items)
    persist.saveFav()
    renderFavorites()
  }})))
}

