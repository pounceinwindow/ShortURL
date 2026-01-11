<div align="center">

# 🔗 ShortURL — AWESOME CREATOR SHORT LINKS WITH ANALYZE

Minimal API на **ASP.NET (net9.0)** + **PGSQL** + статический фронт на **HTML/CSS/JS**.

<br/>

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![PGs](https://img.shields.io/badge/PGSQL-DB-4169E1?style=for-the-badge&logo=PGsql&logoColor=white)
![Minimal API](https://img.shields.io/badge/ASP.NET-Minimal%20API-0B5FFF?style=for-the-badge)
![OAuth](https://img.shields.io/badge/OAuth2-Google-DB4437?style=for-the-badge&logo=google&logoColor=white)
![QR](https://img.shields.io/badge/QR-Code-000000?style=for-the-badge)

<br/>

> “life is short, link is long” — **Some smart guy....**

</div>

---

## ✅  Features

- 🔐 **Регистрация / логин** (JWT)
- 🟦 **Вход через Google (OAuth2)** *(server-side redirect flow → выдача твоего JWT)*
- 🔗 **Создание короткой ссылки**
  - кастомный `slug` (опционально)
  - авто-генерация кода
- 📃 **Список твоих ссылок** (пагинация + поиск)
- 📊 **Статистика по ссылке**
  - всего кликов
  - последние переходы (таблица)
- 🧠 **Сбор аналитики клика**
  - устройство (desktop / mobile / tablet)
  - браузер (по User-Agent)
  - реферер
  - страна/город (GeoIP, MaxMind)
- 🧾 **QR код** для короткой ссылки
- 🚀 **Редирект**

---

## Stack

- Backend: **ASP.NET Core (Minimal API)**, **EF Core**, **JWT Bearer**
- DB: **PGSQL** 
- OAuth: **Google** 
- QR: **QRCoder**
- UA parse: **UAParser**
- Geo: **MaxMind.GeoIP2** (mmdb)

### 1) Создай БД и пропиши connection string.

`appsettings.json` (пример):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=url;Username=PGs;Password=YOUR_PASSWORD"
  }
}
```

***Не забудьте про первую миграцию!!!***
