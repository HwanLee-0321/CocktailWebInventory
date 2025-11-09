// Demo data and taxonomy aligned with previous monolithic main.js

export const COCKTAILS = [
  {
    id:'mojito', name:'모히토 (Mojito)', base:'rum', tastes:['fresh','sweet'],
    ingredients:['rum','lime','mint','sugar','soda'],
    instructions:'라임과 설탕을 으깨고 민트를 넣어 가볍게 빻은 뒤 럼과 얼음을 넣고 소다로 채웁니다.',
    image:'https://images.unsplash.com/photo-1560518883-ce09059eeffa?q=80&w=320&auto=format&fit=crop', strength:'light',
    details:{ method:'Build', glass:'하이볼(Highball)', garnish:'민트(Mint), 라임 웨지',
      steps:['라임과 설탕을 글라스에서 머들','민트를 살짝 빻아 향을 내기','럼과 얼음을 넣고 저은 후','소다수로 채워 가볍게 스터'],
      enjoy:'시원하고 상쾌한 향을 즐기려면 얼음을 넉넉히, 빨대 없이 향을 바로 느껴보세요.' }
  },
  {
    id:'old-fashioned', name:'올드 패션드 (Old Fashioned)', base:'whiskey', tastes:['bitter','rich'],
    ingredients:['whiskey','sugar','angostura','orange peel'],
    instructions:'설탕과 앙고스투라를 스터하여 위스키를 넣고 얼음으로 희석한 뒤 오렌지 필로 마무리.',
    image:'https://images.unsplash.com/photo-1601084881623-cdf9a8f3a522?q=80&w=320&auto=format&fit=crop', strength:'strong',
    details:{ method:'Stir', glass:'올드패션드(ROCKS)', garnish:'오렌지 필',
      steps:['글라스에 설탕과 비터를 넣고 약간의 물로 녹임','얼음과 위스키를 넣고 스터','오렌지 필을 짜 향을 입힌 뒤 가니시'],
      enjoy:'잔을 손으로 감싸 향을 맡아가며 천천히 한 모금씩.' }
  },
  {
    id:'margarita', name:'마가리타 (Margarita)', base:'tequila', tastes:['sour','salty'],
    ingredients:['tequila','triple sec','lime','salt'],
    instructions:'테킬라, 트리플 섹, 라임 주스를 셰이크하고 솔트 림 잔에 따릅니다.',
    image:'https://images.unsplash.com/photo-1604908554027-0c94c41f9a8f?q=80&w=320&auto=format&fit=crop', strength:'medium',
    details:{ method:'Shake', glass:'마가리타', garnish:'솔트 림, 라임 휠',
      steps:['잔 림에 라임을 문지르고 소금 묻히기','셰이커에 재료와 얼음 넣고 셰이크','잔에 걸러 붓고 라임으로 가니시'],
      enjoy:'짭짤한 소금 림과 산뜻한 산미의 대비를 한 입씩 즐겨보세요.' }
  },
  {
    id:'negroni', name:'네그로니 (Negroni)', base:'gin', tastes:['bitter','aromatic'],
    ingredients:['gin','campari','sweet vermouth','orange peel'],
    instructions:'진, 캄파리, 스위트 베르무트를 1:1:1로 스터.',
    image:'https://images.unsplash.com/photo-1582582494700-66f27004e70a?q=80&w=320&auto=format&fit=crop', strength:'medium',
    details:{ method:'Stir', glass:'ROCKS', garnish:'오렌지 필',
      steps:['믹싱글라스에 재료와 얼음','차갑게 스터','얼음 위에 따르고 오렌지 필 트위스트'],
      enjoy:'쓴맛과 허브 향의 여운을 느끼며 에피타이저처럼 천천히.' }
  },
  {
    id:'cosmopolitan', name:'코스모폴리탄 (Cosmopolitan)', base:'vodka', tastes:['sweet','tart'],
    ingredients:['vodka','triple sec','cranberry','lime'],
    instructions:'보드카, 트리플 섹, 크랜베리, 라임을 셰이크.',
    image:'https://images.unsplash.com/photo-1601924582971-b99237a53f6a?q=80&w=320&auto=format&fit=crop', strength:'medium',
    details:{ method:'Shake', glass:'쿠페/마티니', garnish:'라임 휠',
      steps:['셰이커에 모든 재료와 얼음','차갑게 셰이크','잔에 더블 스트레인'],
      enjoy:'차갑게, 향이 살아있을 때 가볍게 즐기기.' }
  },
  {
    id:'whiskey-sour', name:'위스키 사워 (Whiskey Sour)', base:'whiskey', tastes:['sour','foam'],
    ingredients:['whiskey','lemon','sugar','egg white (optional)'],
    instructions:'위스키, 레몬 주스, 시럽, (선택) 흰자를 셰이크 후 더블 스트레인.',
    image:'https://images.unsplash.com/photo-1517705008128-361805f42e86?q=80&w=320&auto=format&fit=crop', strength:'medium',
    details:{ method:'Dry & Wet Shake', glass:'쿠페/ROCKS', garnish:'레몬 필/체리',
      steps:['흰자 사용 시 드라이 셰이크','얼음 넣고 다시 셰이크','잔에 더블 스트레인','가니시 올리기'],
      enjoy:'폼 위로 올라오는 향과 질감을 혀로 굴리며 천천히.' }
  }
]

export const BASES = ['vodka','gin','rum','tequila','whiskey']
export const TASTES = [
  { id:'sweet',label:'달콤함' },{ id:'sour',label:'상큼함' },{ id:'bitter',label:'쓴맛' },{ id:'fresh',label:'상쾌함' },{ id:'aromatic',label:'향미' },{ id:'tart',label:'새콤달콤' },{ id:'rich',label:'리치' },{ id:'foam',label:'거품' }
]

export const KOR_BASE = { vodka:'보드카', gin:'진', rum:'럼', tequila:'테킬라', whiskey:'위스키' }
export const KOR_ING = {
  'rum':'럼','lime':'라임','mint':'민트','sugar':'설탕','soda':'소다수','angostura':'앙고스투라',
  'orange peel':'오렌지 필','tequila':'테킬라','triple sec':'트리플 섹','salt':'소금','cranberry':'크랜베리',
  'lemon':'레몬','egg white':'달걀 흰자','sweet vermouth':'스위트 베르무트','campari':'캄파리','vodka':'보드카','whiskey':'위스키','gin':'진'
}

export const CATEGORIES = [
  { id:'all', label:'전체' },
  { id:'spirit', label:'술' },
  { id:'fruit', label:'과일' },
  { id:'juice', label:'쥬스' },
  { id:'mixer', label:'믹서' },
  { id:'syrup', label:'시럽·설탕' },
  { id:'bitter', label:'비터·허브' },
  { id:'other', label:'기타' },
]

export const ING_CAT = {
  'rum':'spirit','vodka':'spirit','gin':'spirit','whiskey':'spirit','tequila':'spirit','triple sec':'spirit','sweet vermouth':'spirit','campari':'spirit',
  'lime':'fruit','lemon':'fruit','cranberry':'juice','orange peel':'bitter','mint':'bitter',
  'soda':'mixer',
  'sugar':'syrup',
  'angostura':'bitter',
  'salt':'other','egg white':'other'
}

export const INGREDIENTS = Array.from(new Set(COCKTAILS.flatMap(c=>c.ingredients))).sort()

export const categoryOfIng = (name)=>{
  const base = name.replace(/\s*\(optional\)\s*/i,'').trim().toLowerCase()
  return ING_CAT[base] || 'other'
}
