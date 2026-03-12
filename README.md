# MidnightArchive

MidnightArchive is a web application for sharing and exploring user-generated stories.  
The platform allows users to create, read, and interact with stories organized by categories.

The goal of the project is to simulate a real-world content platform while practicing clean architecture, ASP.NET Core development, and modern web development patterns.

---

## Project Purpose

This project was built as a learning project to practice building a full ASP.NET Core MVC application from scratch.  
It focuses on implementing common real-world patterns such as:

- Layered architecture
- DTO-based service layer
- Entity Framework Core data access
- Clean separation of concerns
- CRUD operations
- User-generated content management

---

## Main Features

### Story Management

Users can create and manage stories in the system.

Features include:

- Create stories
- Edit stories
- Delete stories (soft delete / hard delete)
- View story details
- Browse all stories
- Filter stories by category

Each story contains:

- Title
- Content
- Author
- Category
- Created / Modified date
- Views count
- Likes count
- Anonymous posting option

---

### Categories

Stories are organized into categories which help structure the content.

Features include:

- Create categories
- Edit categories
- Delete categories
- Browse categories
- View stories inside a category

---

### Comments

Users can leave comments under stories.

Each comment includes:

- Author
- Content
- Created date

---

## Technologies Used

### Backend

- ASP.NET Core MVC
- Entity Framework Core
- ASP.NET Core Identity
- AutoMapper
- LINQ
- Dependency Injection

### Frontend

- Razor Views
- HTML
- CSS
- JavaScript

### Database

- SQL Server
- Code-First approach with Entity Framework Core

---
### Layers

**Web Layer**

Handles HTTP requests and responses through MVC controllers and Razor views.

**Core Layer**

Contains business logic, DTOs, and service interfaces.

**Infrastructure Layer**

Defines the database models and entity configuration.

**Data Layer**

Contains the database context used by Entity Framework Core.

---

## Key Concepts Practiced

This project focuses on practicing important backend concepts:

- Dependency Injection
- Separation of concerns
- Service layer architecture
- DTO mapping with AutoMapper
- Entity Framework Core queries
- Soft delete pattern
- Razor view development
- Clean controller logic

---

## Future Improvements

Possible future improvements include:

- Full authentication and authorization features
- Like system for stories
- Story search functionality
- Pagination
- Improved UI/UX
- Caching for read-heavy queries
- SEO-friendly URLs
- Advanced filtering
- Deployment

---

## Author

This project was created as part of a learning journey focused on mastering ASP.NET Core and backend development.
