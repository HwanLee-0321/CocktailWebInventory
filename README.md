# CocktailWebInventory

보유 재료와 취향을 기반으로 칵테일 레시피를 추천하고 관리하는 웹 앱입니다. 초기에는 데모 데이터(프런트 내 하드코딩)로 동작하며, 동일 스키마의 백엔드 API 스텁이 포함되어 있어 쉽게 실제 서비스로 확장할 수 있습니다.

## 주요 기능

- 추천: 검색/베이스/취향/보유 재료를 바탕으로 가중치 점수로 정렬된 레시피 추천
- 재료 관리: 카테고리/검색/전체 선택·해제로 보유 재료 관리
- 칵테일 목록: 전체 목록과 검색 필터링, 카드 클릭 시 상세 모달
- 즐겨찾기: 카드/모달에서 즐겨찾기 토글, 전용 목록 제공
- 상태 저장: 보유 재료(`have`), 즐겨찾기(`favorites`)를 로컬스토리지에 저장·복원
- 라벨: 베이스/재료명 한국어 라벨 + 영문 병기

## 기술 스택

- Frontend: Vite, 바닐라 JS(+CSS) 기반 SPA (해시 라우팅)
- Backend(스텁): Node.js + Express, OpenAPI 스펙 제공, 인메모리 상태

## 폴더 구조

```
CocktailWebInventory/
├─ frontend/               # 정적 SPA (Vite)
│  ├─ index.html           # 진입점 (main.js 로드)
│  └─ src/
│     ├─ main.js          # 앱 로직(뷰/필터/스코어링/모달)
│     └─ styles.css       # 스타일
└─ backend/                # API 스텁 (Express)
   ├─ src/server.js        # 실행 가능한 목업 API
   ├─ openapi.yaml         # OpenAPI 3 스펙
   └─ README.md            # 백엔드 연결 가이드
```

## 빠른 시작

프런트엔드와 백엔드는 각각 독립적으로 실행합니다. Node.js 18+ 권장(20+).

### 1) Frontend

```
cd frontend
npm i
npm run dev
```

- 기본 포트: `http://localhost:5173`
- `index.html`에서 `/src/main.js`를 모듈로 로드합니다.

### 2) Backend (API 스텁)

```
cd backend
cp .env.example .env   # Windows: copy .env.example .env
npm i
npm run dev
```

- 기본 포트: `http://localhost:8080`
- 헬스 체크: `GET /api/v1/health`
- OpenAPI 문서: `backend/openapi.yaml`
- CORS 기본 허용: `http://localhost:5173`

## 동작 개요

- 라우팅: `#/recommend`, `#/ingredients`, `#/cocktails`, `#/favorites` 해시 기반 화면 전환
- 추천 스코어링(요약):
  - 검색어 포함 가점/미포함 감점, 베이스/취향 매칭 가점, 보유 재료 교집합 가점, 일부 상충(예: strong vs sweet) 소폭 감점
  - 최종 점수 기준으로 정렬 및 하한선 미만 제외
- 저장소: 로컬스토리지 키 `have`, `favorites`

## 백엔드 연동 계획(요약)

현재는 데모 데이터로 동작하지만, 아래 포인트만 API 호출로 치환하면 실서버 연동이 가능합니다.

- 추천: `GET /api/v1/recommendations`로 대체
- 칵테일 목록/검색: `GET /api/v1/cocktails`
- 재고/즐겨찾기: `GET/PUT/PATCH /api/v1/me/inventory`, `GET/PUT/PATCH /api/v1/me/favorites`
- 분류/라벨: `GET /api/v1/taxonomy/*`

세부 계약은 `backend/openapi.yaml` 및 `backend/README.md`를 참고하세요.

## 개발 메모

- 이미지 리소스는 Unsplash 외부 URL을 사용합니다(오프라인 환경에서는 표시되지 않을 수 있음).
- 라이트 테마 토글 버튼이 있으나 CSS 변수 정의는 다크 기준으로 작성되어 있습니다(확장 여지).
- React 템플릿 파일이 포함되어 있지만 실제 구동은 `index.html` + `src/main.js`(바닐라 JS)로 동작합니다.

## 스크립트 모음

- Frontend
  - `npm run dev` — 개발 서버(Vite)
  - `npm run build` — 프로덕션 빌드
  - `npm run preview` — 빌드 미리보기
- Backend
  - `npm run dev` — nodemon 개발 실행
  - `npm start` — 프로덕션 실행

## 향후 확장 아이디어

- 사용자 인증(세션/JWT), 서버 영속화(DB)
- 사용자 정의 칵테일/재료 CRUD, 이미지 업로드/프록시
- 추천 가중치 파라미터화 및 고급 검색(태그, 난이도, 글라스 등)
