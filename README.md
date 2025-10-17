# SteamApi Backend

Backend-сервис для календаря релизов и аналитики по играм Steam.

## Запуск

```bash
docker-compose up --build
```

- API: `http://localhost:8080`
- Swagger: `http://localhost:8080/swagger`

## Основные эндпоинты
- GET `api/v1/games?month=2025-11&platform=windows&tag=Action`
- GET `api/v1/games/calendar?month=2025-11`
- GET `api/v1/analytics/genres?month=2025-11&top=5`
- GET `api/v1/analytics/dynamics`
- POST `api/v1/games/sync?month=2025-11`

## Технологии
- ASP.NET Core 8
- EF Core + PostgreSQL
- ClickHouse (аналитика/история — пока заглушка)
- Swagger / OpenAPI

## Архитектура
- `Domain` — сущности
- `Infrastructure` — `AppDbContext`
- `Application/Services` — доменные сервисы
- `Controllers` — REST API

## Дальнейшие шаги
- Реальная интеграция со Steam API/парсинг
- Исторические срезы в ClickHouse и запросы динамики
- Планировщик фоновой синхронизации
- JWT-аутентификация (опционально)
