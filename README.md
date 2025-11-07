# SteamApi Backend
Новый Пет проект!
Backend-сервис для календаря релизов и аналитики по играм Steam с реальной интеграцией.

## 🚀 Запуск

```bash
docker-compose up --build
```

- **API**: `http://localhost:8080`
- **Swagger**: `http://localhost:8080/swagger`
- **PostgreSQL**: `localhost:5436`
- **ClickHouse**: `localhost:8123`

## 📊 Как протестировать



### 💻 СПОСОБ 1: Через PowerShell

**Откройте PowerShell и скопируйте эти команды:**

```powershell
# 1. Получить токен
$body = '{"username": "admin", "password": "password"}'
$response = Invoke-WebRequest -Uri "http://localhost:8080/api/v1/auth/login" -Method POST -Body $body -ContentType "application/json"
$token = ($response.Content | ConvertFrom-Json).token
Write-Host "Токен: $token"

# 2. Синхронизировать данные
$headers = @{"Authorization" = "Bearer $token"}
$sync = Invoke-WebRequest -Uri "http://localhost:8080/api/v1/games/sync?month=2025-10" -Method POST -Headers $headers
Write-Host "Синхронизация: $($sync.Content)"

# 3. Получить список игр
$games = Invoke-WebRequest -Uri "http://localhost:8080/api/v1/games?month=2025-10"
Write-Host "Игры: $($games.Content.Length) символов"

# 4. Получить календарь
$calendar = Invoke-WebRequest -Uri "http://localhost:8080/api/v1/games/calendar?month=2025-10"
Write-Host "Календарь: $($calendar.Content)"

# 5. Получить аналитику
$analytics = Invoke-WebRequest -Uri "http://localhost:8080/api/v1/analytics/genres?month=2025-10&top=5"
Write-Host "Аналитика: $($analytics.Content)"
```
### 🎯 СПОСОБ 2: Через браузер 

1. **Откройте браузер**
2. **Перейдите на:** `http://localhost:8080/swagger`
3. **Вы увидите страницу с документацией API**
4. **Нажмите на любой эндпоинт** (например, `GET /api/v1/games`)
5. **Нажмите "Try it out"**
6. **Введите параметры** (например, `month=2025-10`)
7. **Нажмите "Execute"**
8. **Смотрите результат!**
### 🌐 ССЫЛКИ ДЛЯ ТЕСТИРОВАНИЯ:

- **Swagger UI:** `http://localhost:8080/swagger`
- **API:** `http://localhost:8080/api/v1/`

**САМЫЙ ПРОСТОЙ СПОСОБ - через браузер на `http://localhost:8080/swagger`!** 🚀

## 🎯 Основные эндпоинты

| Эндпоинт | Описание | Данные |
|----------|----------|---------|
| `GET /api/v1/games` | Список игр с фильтрацией | **Реальные данные из Steam** |
| `GET /api/v1/games/calendar` | Календарь релизов по дням | **Реальные данные из Steam** |
| `GET /api/v1/analytics/genres` | Топ-5 жанров + средние фолловеры | **Реальные данные из ClickHouse** |
| `GET /api/v1/analytics/dynamics` | Динамика изменений по месяцам | **Реальные данные из ClickHouse** |
| `POST /api/v1/games/sync` | Синхронизация данных из Steam | **Парсинг Steam Coming Soon + appdetails** |

## 🔧 Технологии

- **ASP.NET Core 8** - Web API
- **Entity Framework Core** - ORM для PostgreSQL
- **PostgreSQL** - Основная база данных
- **ClickHouse** - Аналитическая база данных
- **HTML Agility Pack** - Парсинг Steam страниц
- **System.Text.Json** - JSON обработка
- **JWT Authentication** - Безопасность API
- **Docker Compose** - Контейнеризация

## 🏗️ Архитектура

- **`Domain`** — сущности (Game, Tag, GameTag)
- **`Infrastructure`** — AppDbContext, ClickHouseWriter, Middleware
- **`Application/Services`** — бизнес-логика (GameService, AnalyticsService, SteamSyncService)
- **`Controllers`** — REST API (GamesController, AnalyticsController, AuthController)
- **`Application/DTOs`** — Data Transfer Objects
- **`Application/Common`** — Result<T> pattern

## 📈 Реальные данные

### Steam API интеграция:
- ✅ **Парсинг Coming Soon** - Получение списка предстоящих игр
- ✅ **AppDetails API** - Детальная информация об играх
- ✅ **Теги и жанры** - Реальные теги из Steam
- ✅ **Платформы** - Windows, Mac, Linux поддержка
- ✅ **Фолловеры** - Количество подписчиков
- ✅ **Изображения** - Постеры игр

### ClickHouse аналитика:
- ✅ **Исторические срезы** - Данные с привязкой к дате
- ✅ **Агрегация по жанрам** - Топ-5 жанров
- ✅ **Средние фолловеры** - Статистика по жанрам
- ✅ **Динамика изменений** - Данные по месяцам

## 🎮 Примеры ответов

### Аналитика жанров:
```json
[
  {"genre":"Indie","games":8,"avgFollowers":26138},
  {"genre":"Simulation","games":5,"avgFollowers":25431},
  {"genre":"Casual","games":4,"avgFollowers":24821}
]
```

### Календарь релизов:
```json
{
  "month":"2025-10",
  "days":[
    {"date":"2025-10-17","count":8},
    {"date":"2025-10-18","count":2}
  ]
}
```

## ✅ Готово к использованию

- **Все эндпоинты работают** с реальными данными
- **ClickHouse интегрирован** для аналитики
- **Steam API парсинг** настроен и работает
- **JWT аутентификация** реализована
- **Docker Compose** готов к запуску
- **Swagger документация** доступна
