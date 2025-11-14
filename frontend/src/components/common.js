import { KOR_BASE, KOR_ING } from '../data/sample.js'

const shouldSkip = (value)=> value === null || value === undefined || value === false

export const el = (tag, attrs={}, ...children) => {
  const $el = document.createElement(tag)
  for (const [k,v] of Object.entries(attrs)) {
    if (k === 'class') $el.className = v
    else if (k.startsWith('on') && typeof v === 'function') $el.addEventListener(k.slice(2), v)
    else if (v !== undefined) $el.setAttribute(k, v)
  }
  children.flat().forEach(ch => {
    if (shouldSkip(ch)) return
    $el.append(ch instanceof Node ? ch : document.createTextNode(String(ch)))
  })
  return $el
}

export const labelBase = (id = '')=>{
  const key = id.toLowerCase()
  const ko = KOR_BASE[key]
  if (ko) return ko
  if (!id) return ''
  return id.charAt(0).toUpperCase()+id.slice(1)
}

const normalizeText = (value='') => value.toString().trim()
const sameText = (a='', b='') => normalizeText(a).localeCompare(normalizeText(b), undefined, { sensitivity:'base' }) === 0

export const labelIng = (name)=>{
  const optional = name.includes('(optional)')
  const base = normalizeText(name.replace(/\s*\(optional\)\s*/i,''))
  const ko = normalizeText(KOR_ING[base.toLowerCase()] || base)
  const en = normalizeText(base)
  const core = (!ko || sameText(ko, en)) ? ko || en : `${ko} (${en})`
  return optional ? `${core} (optional)` : core
}

