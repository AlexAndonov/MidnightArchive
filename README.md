# MidnightArchive 👻

> ASP.NET Core MVC web platform for sharing and exploring horror stories with moderation and event system.

MidnightArchive is an ASP.NET Core MVC web platform where users can share scary stories, explore stories by category, interact through comments and likes, and participate in community events.
The platform also includes an admin moderation area for managing categories and resolving reported stories.

---

## 🚀 Features

### 👤 Public/User Features

* Browse stories by category
* Search stories
* Pagination for story listings
* Create, edit, and delete stories
* View count system 👁️
* Like system ❤️
* Comment on stories 💬
* Edit and delete own comments
* Browse and join events 📅
* Leave joined events
* Create, edit, and delete own events
* Random story feature 🎲

### 🛠️ Admin Features

* Manage categories
* Access admin area
* Review reported stories 🚨
* Resolve reports
* Delete inappropriate stories through moderation flow

### ⚖️ Moderation / Reporting

* Users can report stories
* Each report includes:

  * reason
  * optional description
* Admin can review reported stories and take action

---

## 🧰 Tech Stack

* ASP.NET Core MVC
* Entity Framework Core (Code First)
* SQL Server
* ASP.NET Core Identity
* AutoMapper
* In-memory caching
* xUnit for unit testing

---

## 🏗️ Architecture

The project follows a layered architecture:

* **MidnightArchive** – MVC web application
* **MidnightArchive.Core** – business logic, services, DTOs, contracts
* **MidnightArchive.Infra** – data access, EF Core, entity models, migrations
* **MidnightArchive.Tests** – unit tests

### 🔍 Architectural Notes

* Controllers are thin
* Business logic is handled in services
* DTO + Service pattern is used
* Entity Framework Core is used with Code First approach
* ASP.NET Core Identity is configured with a custom `ApplicationUser`
* Role-based authorization is implemented
* Soft delete is used in selected parts of the system

---

## 🔑 Demo Accounts

### 👑 Admin

* Email: `admin@midnight.com`
* Password: `Admin123!`

### 👤 User

* Email: `user@midnight.com`
* Password: `User123!`

### ✍️ Writer

* Email: `writer@midnight.com`
* Password: `Writer123!`

> The admin account is used for moderation and category management.
> Demo content such as stories, events, and comments is created by the seeded user accounts.

---

## 🗄️ Database Setup

The project is configured by default to use SQL Server LocalDB:

```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MidnightArchiveDb;Trusted_Connection=True;MultipleActiveResultSets=true"
```

### ⚠️ Important

If you are using a different SQL Server instance, update the connection string in `appsettings.json`.

---

## ▶️ How to Run

### 1. Clone the repository

```bash
git clone https://github.com/AlexAndonov/MidnightArchive
```

---

### 🧪 Run using Package Manager Console (Visual Studio)

1. Set **Startup Project** → `MidnightArchive`

2. Open **Package Manager Console**

3. Set:

   * Default Project → `MidnightArchive.Infra`

4. Run:

```powershell
Update-Database
```

5. Start the project ▶️

---

### 💻 Run using CLI

```bash
dotnet ef database update --project .\MidnightArchive.Infra\MidnightArchive.Infra.csproj --startup-project .\MidnightArchive\MidnightArchive.csproj
dotnet run --project .\MidnightArchive\MidnightArchive.csproj
```

---

## 📝 Notes

* The database is automatically seeded on first run
* Categories, stories, events, and comments are preloaded
* One category contains multiple stories to demonstrate pagination 📄
* Admin account does not create content (used only for moderation)
* Two user accounts are used for realistic demo interactions

---

## 🎯 Project Purpose

This project demonstrates:

* Clean layered architecture
* Separation of concerns
* Real-world ASP.NET Core MVC patterns
* Entity Framework Core usage with Code First
* Identity integration and role-based authorization
* Feature implementation end-to-end (CRUD, filtering, pagination, moderation)

---
