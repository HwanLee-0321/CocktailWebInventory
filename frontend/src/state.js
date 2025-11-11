import { Api } from './services/api.js'

export const state = {
  search:'',
  glasses:new Set(),
  categories:new Set(),
  alcoholic:null,
  have:new Set(JSON.parse(localStorage.getItem('have')||'[]')),
  favorites:new Set(JSON.parse(localStorage.getItem('favorites')||'[]')),
  ingCat:'all',
  ingMgrCat:'all',
}

export const persist = {
  saveHave(){ localStorage.setItem('have', JSON.stringify([...state.have])) },
  saveFav(){ localStorage.setItem('favorites', JSON.stringify([...state.favorites])) },
}

export async function syncFromServer(){
  const inv = await Api.getInventory()
  if (inv && Array.isArray(inv.items)) state.have = new Set(inv.items)
  const fav = await Api.getFavorites()
  if (fav && Array.isArray(fav.items)) state.favorites = new Set(fav.items)
}
