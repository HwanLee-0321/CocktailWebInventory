import { KOR_BASE, KOR_ING } from '../data/sample.js'

export const el = (tag, attrs={}, ...children) => {
  const $el = document.createElement(tag)
  for (const [k,v] of Object.entries(attrs)) {
    if (k === 'class') $el.className = v
    else if (k.startsWith('on') && typeof v === 'function') $el.addEventListener(k.slice(2), v)
    else if (v !== undefined) $el.setAttribute(k, v)
  }
  children.flat().forEach(ch => $el.append(ch instanceof Node ? ch : document.createTextNode(String(ch))))
  return $el
}

export const labelBase = (id)=>{
  const ko = KOR_BASE[id] || id
  const en = id.charAt(0).toUpperCase()+id.slice(1)
  return `${ko} (${en})`
}

export const labelIng = (name)=>{
  const optional = name.includes('(optional)')
  const base = name.replace(/\s*\(optional\)\s*/i,'').trim()
  const ko = KOR_ING[base.toLowerCase()] || base
  const en = base
  const core = (ko && ko!==en) ? `${ko} (${en})` : en
  return optional ? `${core} (optional)` : core
}

