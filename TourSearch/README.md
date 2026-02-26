# TourSearch - Клон G Adventures

Платформа для поиска и бронирования туров, построенная на собственном MVC-фреймворке, ORM и шаблонизаторе.

## Требования проекта

Реализовано:
- Собственный веб-сервер на HttpListener
- Собственная MVC-архитектура (без ASP.NET Core MVC)
- Собственный шаблонизатор (аналог Razor)
- Собственная ORM для PostgreSQL
- Репозитории для доступа к данным
- Полная аутентификация пользователей с email-уведомлениями
- Панель администратора для управления турами

## Технологии

- **Бэкенд**: C# / .NET 8.0
- **База данных**: PostgreSQL
- **Фронтенд**: HTML, CSS, vanilla JavaScript
- **Контейнеризация**: Docker & Docker Compose

## Быстрый старт

### Вариант 1: Docker (рекомендуется)

```bash
# Клонировать репозиторий
git clone <repository-url>
cd TourSearch

# Запустить через Docker Compose
docker-compose up --build

# Приложение будет доступно по адресу:
# http://localhost:5000

# Остановить:
docker-compose down

# Посмотреть логи:
docker-compose logs -f app
```

### Вариант 2: Ручная установка (разработка)

1. **Установите PostgreSQL** и создайте базу данных:
```sql
CREATE DATABASE toursearch;
```

2. **Выполните SQL-скрипты** по порядку:
```bash
psql -U postgres -d toursearch -f scripts/01_create_database.sql
psql -U postgres -d toursearch -f scripts/02_create_tables.sql
psql -U postgres -d toursearch -f scripts/03_seed_data.sql
```

3. **Обновите строку подключения** в `TourSearch/Data/DbConfig.cs`

4. **Соберите и запустите**:
```bash
cd TourSearch/TourSearch
dotnet restore
dotnet run
```

## Учетные данные

**Администратор:**
**Email:** `admin@toursearch.com`
**Пароль:** `Password123!`

Обычный пользователь:
**Email:** `user@example.com`
**Пароль:** `Password123!`

## Структура проекта

```
TourSearch/
├── TourSearch/
│   ├── Data/                    # Работа с базой данных
│   │   ├── Orm/                 # Собственная ORM
│   │   ├── *Repository.cs       # Классы репозиториев
│   │   └── DbConfig.cs          # Конфигурация БД
│   ├── Domain/
│   │   └── Entities/            # Сущности предметной области
│   ├── Infrastructure/          # Сквозная функциональность
│   │   ├── EmailService.cs      # Отправка писем
│   │   ├── PasswordHasher.cs    # Хеширование паролей
│   │   └── Logger.cs            # Логирование
│   ├── Mvc/                     # MVC-контроллеры
│   │   ├── AccountController.cs
│   │   ├── HomeController.cs
│   │   └── ...
│   ├── Server/                  # Компоненты веб-сервера
│   │   ├── WebServer.cs         # Обертка над HttpListener
│   │   ├── SimpleRouter.cs      # Маршрутизация
│   │   ├── *Handler.cs          # Обработчики маршрутов
│   │   └── ViewRenderer.cs      # Рендеринг представлений
│   ├── TemplateEngine/          # Собственный шаблонизатор
│   │   └── SimpleTemplateEngine.cs
│   ├── ViewModels/              # Модели представлений
│   ├── Views/                   # HTML-шаблоны
│   │   ├── Account/
│   │   ├── Admin/
│   │   ├── Home/
│   │   └── Tours/
│   ├── wwwroot/                 # Статические файлы
│   │   ├── css/
│   │   ├── images/
│   │   └── js/
│   └── Program.cs               # Точка входа
├── TourSearch.Tests/            # Модульные тесты
├── scripts/                     # SQL-скрипты
├── Dockerfile
├── docker-compose.yml
└── README.md
```

## Страницы

| Маршрут | Описание |
|---------|----------|
| `/` | Главная страница |
| `/search` | Поиск туров с фильтрами |
| `/tours/{id}` | Детальная страница тура |
| `/account/login` | Вход пользователя |
| `/account/register` | Регистрация |
| `/account/forgot-password` | Сброс пароля |
| `/admin/tours` | Панель администратора (требуется вход) |
| `/placeholder` | Страница "В разработке" |

## Возможности

### Поиск и фильтры
- Фильтр по направлению
- Фильтр по стилю путешествия
- Сортировка по цене (возрастание/убывание)
- Сортировка по длительности

### Шаблонизатор
Поддерживает:
- Подстановку переменных: `{{VariableName}}`
- Циклы: `{{#each Items}}...{{/each}}`
- Условия: `{{#if Condition}}...{{/if}}`
- Вложенные свойства: `{{Tour.Name}}`

### Собственная ORM
- CRUD-операции
- Параметризованные запросы (защита от SQL-инъекций)
- Асинхронные операции
- Маппинг сущностей

### Аутентификация
- Регистрация с валидацией
- Вход с сессионными cookie
- Сброс пароля через email
- Уведомления о входе на почту

## Сущности базы данных

1. **destinations** - Направления путешествий (10 записей)
2. **travel_styles** - Стили путешествий (6 записей)
3. **tours** - Информация о турах (10 записей)
4. **tour_photos** - Фотографии туров
5. **users** - Учетные записи пользователей
6. **bookings** - Бронирования туров
7. **password_reset_tokens** - Токены для сброса пароля

## Валидация

### На клиенте (JavaScript)
- Проверка формата email
- Индикатор сложности пароля
- Валидация полей формы

### На сервере (C#)
- Валидация email через регулярные выражения
- Требования к сложности пароля
- Санитизация входных данных

## Запуск тестов

```bash
cd TourSearch.Tests
dotnet test
```

## API endpoints

| Метод | Endpoint | Описание |
|--------|----------|-----------|
| GET | `/api/tours` | Все туры |
| GET | `/api/tours?styleId=1` | Фильтр по стилю |
| GET | `/api/tours?destinationId=1` | Фильтр по направлению |
| GET | `/health` | Проверка работоспособности |

## Переменные окружения

| Переменная | Описание | Значение по умолчанию |
|------------|----------|----------------------|
| DB_HOST | Хост БД | localhost |
| DB_PORT | Порт БД | 5432 |
| DB_NAME | Название БД | toursearch |
| DB_USER | Пользователь БД | postgres |
| DB_PASSWORD | Пароль БД | postgres |
| YANDEX_SMTP_PASSWORD | Пароль для отправки писем | - |