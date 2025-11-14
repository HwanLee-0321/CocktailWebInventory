import { Api } from '../services/api.js'
import { el } from '../components/common.js'
import { ResultCard } from '../components/resultCard.js'
import { renderPaginatedList } from '../components/pagination.js'
import { state, storage } from '../state.js'

let bound=false
export function bindCocktailsOnce(){
  if (bound) return; bound=true
  const input = document.getElementById('listSearch')
  input.addEventListener('input', async e=>{
    const q = e.target.value.toLowerCase()
    const res = await Api.cocktails({ q })
    renderCards(document.getElementById('cocktailList'), res.items, true)
  })
}

export function renderCards(container, list, resetPage = false){
  if (!container) return
  const pagerRoot = document.getElementById('cocktailPagination')
  renderPaginatedList({
    container,
    items: list,
    pageKey: 'cocktails',
    pageSize: 12,
    emptyMessage: '표시할 항목이 없습니다.',
    resetPage,
    pagerContainer: pagerRoot,
    renderPager: pagerRoot ? buildCocktailPager : null,
    renderItem: (cocktail)=>{
      return ResultCard({
        cocktail,
        isFavorite: state.favorites.has(cocktail.id),
        onFavorite: ()=>{
          storage.toggleFavorite(cocktail.id)
          renderCards(container, list)
        }
      })
    }
  })
}

const buildCocktailPager = ({ current, total, pageSize, totalItems, onChange })=>{
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

