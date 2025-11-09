# CocktailWebInventory Backend (.NET 8 Minimal API)

이 디렉터리는 C#(.NET 8) 기반의 백엔드 API 스텁입니다. 프론트엔드(`frontend`)의 데모 데이터/동작과 동일한 계약으로 구현되어 있어 바로 연동/치환이 가능합니다.

## 빠른 시작

요구사항: .NET SDK 8.0+

```
cd backend-dotnet
dotnet restore
dotnet run
```

- 기본 URL: `http://localhost:8080`
- 헬스체크: `GET /api/v1/health`
- CORS: 기본 `http://localhost:5173` 허용 (환경변수 `CORS_ORIGIN`로 수정 가능)
- Swagger UI: 개발 모드에서 `/swagger` 경로 제공

## 엔드포인트 요약

- 카탈로그: `GET /api/v1/cocktails`, `GET /api/v1/cocktails/{id}`, `GET /api/v1/ingredients`
- 분류: `GET /api/v1/taxonomy/bases | tastes | ingredient-categories`
- 추천: `GET /api/v1/recommendations` (프론트와 동일 가중치 스코어링)
- 사용자 상태(모킹): `GET/PUT/PATCH /api/v1/me/inventory`, `GET/PUT /api/v1/me/favorites`, `PATCH /api/v1/me/favorites/toggle`

사용자 식별은 요청 헤더 `x-demo-user`로 구분되며(기본 `demo`), 인메모리로 상태를 유지합니다.

## 프론트 연동 메모

- 추천 호출: `recommend()`를 `GET /api/v1/recommendations`로 대체
- 목록/검색: `GET /api/v1/cocktails`
- 재고/즐겨찾기: `/me/inventory`, `/me/favorites`
- 분류/라벨: `/taxonomy/*`

자세한 응답/요청 스키마는 `backend/openapi.yaml`(Node 스텁과 동일 계약) 또는 Swagger 문서를 참조하세요.

