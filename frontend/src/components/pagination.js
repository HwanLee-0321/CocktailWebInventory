import { el } from './common.js'
import { state } from '../state.js'

export function renderPaginatedList({
  container,
  items = [],
  pageKey,
  pageSize = 12,
  renderItem,
  emptyMessage = '표시할 항목이 없습니다.',
  resetPage = false,
  pagerContainer = null,
  renderPager = null
}){
  if (!container) return
  if (!state.pages) state.pages = {}
  if (resetPage || !state.pages[pageKey]) state.pages[pageKey] = 1

  const hidePager = () => {
    if (!pagerContainer) return
    pagerContainer.innerHTML = ''
    pagerContainer.hidden = true
  }
  const showPager = () => {
    if (!pagerContainer) return
    if (pagerContainer.dataset?.recommendActive === 'false') return
    pagerContainer.hidden = false
  }

  const renderPage = (pageOverride) => {
    const totalPages = Math.max(1, Math.ceil(items.length / pageSize))
    const current = clamp(pageOverride ?? state.pages[pageKey], 1, totalPages)
    state.pages[pageKey] = current
    container.innerHTML = ''
    if (!items.length){
      container.appendChild(el('div',{class:'empty card'}, emptyMessage))
      hidePager()
      return
    }
    const slice = items.slice((current - 1) * pageSize, current * pageSize)
    const frag = document.createDocumentFragment()
    slice.forEach(item => {
      const node = renderItem(item)
      if (node) frag.appendChild(node)
    })
    container.appendChild(frag)
    if (totalPages > 1){
      const onChange = (next)=> renderPage(next)
      const pagerNode = typeof renderPager === 'function'
        ? renderPager({ current, total: totalPages, pageSize, totalItems: items.length, onChange })
        : buildPager(current, totalPages, onChange)
      if (!pagerNode){
        hidePager()
        return
      }
      if (pagerContainer){
        pagerContainer.innerHTML = ''
        pagerContainer.appendChild(pagerNode)
        showPager()
      } else {
        container.appendChild(pagerNode)
      }
    } else {
      hidePager()
    }
  }

  renderPage(resetPage ? 1 : state.pages[pageKey])
}

function buildPager(current, total, onChange){
  const nav = el('div',{class:'pager'})
  for (let page = 1; page <= total; page += 1){
    nav.appendChild(
      el('button',{
        type:'button',
        class:`pager__btn ${page===current?'active':''}`,
        onclick:()=> onChange(page)
      }, `페이지 ${page}`)
    )
  }
  return nav
}

const clamp = (value, min, max) => Math.min(Math.max(value, min), max)
