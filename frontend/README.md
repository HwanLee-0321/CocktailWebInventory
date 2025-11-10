# Cocktail Web Inventory – Frontend

칵테일 재고/추천 페이지의 React + Vite 프런트엔드입니다.  
UI는 한국어를 표시하지만 **백엔드와 주고받는 모든 파라미터와 값은 영어 ID** (예: `base=rum`, `tastes=fresh`) 로 전송합니다.

## Getting Started

```bash
cd frontend
npm install
npm run dev     # http://localhost:5173
npm run build   # production bundle
```

환경 변수

| env key | 설명 | 기본값 |
| ------- | ---- | ------ |
| `VITE_API_BASE` | 백엔드 API 루트 URL | `http://localhost:8080/api/v1` |
| `VITE_API_DISABLED` | `true` 이면 로컬 샘플 데이터만 사용 | `false` |

## API 통신 개요

프런트는 `src/services/api.js` 를 통해 백엔드와 통신합니다.  
캐칭(TTL 5분)과 간단한 오프라인 fallback 이 적용되어 있으며, 아래 엔드포인트를 사용합니다.

### 1. Taxonomy

| HTTP | 경로 | 설명 |
| ---- | ---- | ---- |
| GET | `/taxonomy/bases` | 기본 주류 목록 |
| GET | `/taxonomy/tastes` | 취향/맛 목록 |
| GET | `/taxonomy/ingredient-categories` | 재료 카테고리 |

Request 예시:

```
GET /api/v1/taxonomy/bases
```

Response 예시:

```json
{
  "items": [
    { "id": "vodka", "labelKo": "보드카" },
    { "id": "gin", "labelKo": "진" }
  ]
}
```

프런트는 세 엔드포인트를 모두 호출한 뒤 다음과 같이 합쳐 캐시에 저장합니다.

```json
{
  "bases": [{ "id": "vodka", "labelKo": "보드카" }, ...],
  "tastes": [{ "id": "sweet", "label": "달콤함" }, ...],
  "categories": [{ "id": "spirit", "label": "술" }, ...]
}
```

### 2. Ingredients

| HTTP | 경로 | 쿼리 | 설명 |
| ---- | ---- | ---- | ---- |
| GET | `/ingredients` | `category=spirit`, `q=rum` 등 | 카테고리별 재료 목록 |

Request 예시:

```
GET /api/v1/ingredients?category=spirit
```

Response 예시:

```json
{
  "items": ["angostura", "campari", "gin", "rum", "tequila", "vodka"]
}
```

UI에서 한국어 라벨을 보여주지만, 실제 체크/저장은 위와 같은 영어 식별자를 사용합니다.

### 3. Cocktails

| HTTP | 경로 | 쿼리 | 설명 |
| ---- | ---- | ---- | ---- |
| GET | `/cocktails` | `q`, `base`, `taste`, `ingredient`, `strength` | 칵테일 검색 |
| GET | `/cocktails/{id}` | - | 단일 칵테일 상세 |

Request 예시:

```
GET /api/v1/cocktails?q=mojito&base=rum&taste=fresh&ingredient=lime&strength=light
```

Response 예시:

```json
{
  "items": [
    {
      "id": "mojito",
      "name": "모히토 (Mojito)",
      "base": "rum",
      "tastes": ["fresh", "sweet"],
      "ingredients": ["rum", "lime", "mint", "sugar", "soda"],
      "strength": "light",
      "image": "https://.../mojito.jpg"
    }
  ],
  "total": 1
}
```

### 4. Recommendations

| HTTP | 경로 | 쿼리 | 설명 |
| ---- | ---- | ---- | ---- |
| GET | `/recommendations` | `q`, `bases`, `tastes`, `have` | 조건 기반 추천 |

Request 예시:

```
GET /api/v1/recommendations?bases=rum&tastes=fresh&have=rum&have=lime&have=mint
```

Response 예시:

```json
{
  "items": [
    {
      "cocktail": {
        "id": "mojito",
        "name": "모히토 (Mojito)",
        "base": "rum",
        "tastes": ["fresh", "sweet"],
        "ingredients": ["rum", "lime", "mint", "sugar", "soda"]
      },
      "score": 6.5
    }
  ],
  "total": 1
}
```

프런트는 응답 점수를 이용해 별점과 정렬을 표시합니다.

### 5. Inventory (보유 재료)

| HTTP | 경로 | 바디 | 설명 |
| ---- | ---- | ---- | ---- |
| GET | `/me/inventory` | - | 현재 보유 재료 조회 |
| PUT | `/me/inventory` | `{ "items": ["rum","lime"] }` | 전체 재작성 |
| PATCH | `/me/inventory` | `{ "add": [...], "remove": [...] }` | 증감 갱신 |

Request 예시:

```http
PATCH /api/v1/me/inventory
Content-Type: application/json

{
  "add": ["mint"],
  "remove": ["tequila"]
}
```

Response 예시:

```json
{
  "items": ["rum", "lime", "mint"]
}
```

### 6. Favorites

| HTTP | 경로 | 바디 | 설명 |
| ---- | ---- | ---- | ---- |
| GET | `/me/favorites` | - | 즐겨찾기 목록 |
| PUT | `/me/favorites` | `{ "items": ["mojito","negroni"] }` | 전체 저장 |
| PATCH | `/me/favorites/toggle` | `{ "id": "mojito" }` | 즐겨찾기 토글 |

Response 예시 (`GET /me/favorites`):

```json
{
  "items": ["mojito", "negroni"]
}
```

### 요청 언어 규칙

- 모든 요청 경로와 파라미터는 영어 snake/camel case 키(`base`, `tastes`, `have`, `ingredients`)를 사용합니다.
- 값 역시 영문 ID (`rum`, `fresh`, `mint`)로 전송되며, 한국어는 UI 표현에만 사용합니다.
- 사용자가 검색창에 한국어를 입력하더라도 서버에는 그대로 문자열이 전달되지만, 필터/재료/즐겨찾기 등의 값은 항상 영어 식별자입니다.

### 오프라인/캐시 동작

- API 실패 시 60초 동안 오프라인 모드로 전환하여 콘솔 경고만 남기고 로컬 샘플 데이터를 사용합니다.
- Taxonomy 응답은 5분, Ingredients 응답은 1분 TTL로 메모리 캐시에 저장해 불필요한 호출을 줄입니다.
- 재고/즐겨찾기 API가 실패하면 LocalStorage 로 동일한 데이터를 유지합니다.

## Frontend Structure

- `src/services/api.js` : 모든 API 래퍼 및 캐시/오프라인 처리
- `src/views/` : 추천, 재료 관리, 칵테일 목록, 즐겨찾기 화면 로직
- `src/components/` : 카드, 모달, 공통 UI 컴포넌트
- `src/state.js` : 클라이언트 상태 및 LocalStorage 동기화

필요 시 README를 참고하여 백엔드 목업을 구성하면 프런트 앱을 독립적으로 테스트할 수 있습니다.
