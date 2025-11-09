import { el, labelBase, labelIng } from './common.js'
import { openModalFor } from './modal.js'

export function ResultCard({ cocktail, stars = 0, onFavorite, isFavorite }){
  const c = cocktail
  return el('article',{class:'card result', onclick:()=> openModalFor(c)},
    el('div',{class:'header'},
      el('img',{src:c.image,alt:c.name}),
      el('div',{},
        el('div',{class:'row'},
          el('h3',{style:'margin:0'},c.name),
          el('span',{class:'badge'},labelBase(c.base)),
          stars ? el('span',{class:'stars'},'★'.repeat(stars)) : null
        ),
        el('div',{class:'muted small'},c.tastes.join(' • '))
      )
    ),
    el('div',{class:'kvs'},
      el('div',{class:'kv'}, el('div',{class:'k'},'재료'), el('div',{}, c.ingredients.map(labelIng).join(', '))),
      c.instructions ? el('div',{class:'kv'}, el('div',{class:'k'},'레시피'), el('div',{}, c.instructions)) : null
    ),
    el('div',{class:'row',style:'margin-top:8px'},
      el('button',{class:`icon-btn ${isFavorite? 'active':''}`, onclick:(e)=>{ e.stopPropagation(); onFavorite?.(c.id) }}, `${isFavorite?'★':'☆'} 즐겨찾기`)
    )
  )
}

