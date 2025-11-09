import { el, labelBase, labelIng } from './common.js'

export function openModalFor(c){
  const root = document.getElementById('modalContent')
  if (!root) return
  root.innerHTML = ''
  const d = c.details || {}
  root.appendChild(el('div',{},
    el('div',{class:'row'},
      el('img',{src:c.image,alt:c.name,style:'width:88px;height:88px;border-radius:10px;border:1px solid var(--border);object-fit:cover'}),
      el('div',{},
        el('h3',{id:'modalTitle',style:'margin:0 0 6px 0'}, c.name),
        el('div',{class:'meta'},
          el('span',{class:'badge'}, labelBase(c.base)),
          el('span',{class:'badge'}, `도수: ${c.strength}`)
        )
      )
    ),
    el('div',{class:'kvs'},
      el('div',{class:'kv'}, el('div',{class:'k'},'재료'), el('div',{}, c.ingredients.map(labelIng).join(', '))),
      d.method ? el('div',{class:'kv'}, el('div',{class:'k'},'방법'), el('div',{}, d.method)) : null,
      d.glass ? el('div',{class:'kv'}, el('div',{class:'k'},'글라스'), el('div',{}, d.glass)) : null,
      d.garnish ? el('div',{class:'kv'}, el('div',{class:'k'},'가니시'), el('div',{}, d.garnish)) : null
    ),
    d.steps ? el('div',{class:'section'},
      el('div',{class:'section__title'},'레시피'),
      el('ol',{}, ...d.steps.map(s=> el('li',{}, s)))
    ) : null,
    d.enjoy ? el('div',{class:'section'},
      el('div',{class:'section__title'},'즐기는 방법'),
      el('p',{}, d.enjoy)
    ) : null
  ))
  const modal = document.getElementById('modal')
  if (modal) modal.hidden = false
}

export function bindModalGlobal(){
  document.addEventListener('click', (e)=>{
    if (e.target && (e.target.id==='modalClose' || e.target.hasAttribute('data-close'))) {
      const m = document.getElementById('modal'); if (m) m.hidden = true
    }
  })
  document.addEventListener('keydown', (e)=>{
    if (e.key==='Escape') { const m = document.getElementById('modal'); if (m) m.hidden = true }
  })
}

