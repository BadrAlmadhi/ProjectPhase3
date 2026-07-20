# Learning Management System (LMS)

A Learning Management System (LMS) web application built with **ASP.NET Core MVC**, **Entity Framework Core**, and **PostgreSQL**. This project was developed as part of the University of Utah CS 6016 Database Systems course.

---

## Features

- User management
- Student management
- Professor management
- Course management
- Class management
- Enrollment management
- Assignment categories
- Assignments
- Submission tracking
- PostgreSQL database integration
- Entity Framework Core ORM

---

## Technologies

- ASP.NET Core MVC (.NET 10)
- C#
- Entity Framework Core
- PostgreSQL
- Npgsql
- Razor Views
- HTML
- CSS
- Git
- GitHub

---

## Project Structure

```
ProjectPhase3/
│
├── Controllers/
├── Data/
│   └── LmsContext.cs
├── Models/
├── Views/
├── wwwroot/
├── Program.cs
└── ProjectPhase3.csproj
```

---

## Database

The project uses an existing PostgreSQL database.

Entity Framework Core Database-First scaffolding was used to generate:

- DbContext
- Entity models
- Relationships

Connection strings are stored securely using **ASP.NET Core User Secrets**.

---

## Getting Started

### Clone the repository

```bash
git clone <repository-url>
cd ProjectPhase3
```

### Restore packages

```bash
dotnet restore
```

### Configure User Secrets

Initialize User Secrets:

```bash
dotnet user-secrets init
```

Add your PostgreSQL connection string:

```bash
dotnet user-secrets set "LMS:ConnectionString" "Host=YOUR_HOST;Username=YOUR_USER;Password=YOUR_PASSWORD;Database=YOUR_DATABASE"
```

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run
```

The application will start locally and connect to the configured PostgreSQL database.

---

## Current Progress

- ✔ ASP.NET Core MVC project created
- ✔ PostgreSQL connected
- ✔ Entity Framework Core configured
- ✔ Database scaffolded
- ✔ Dependency Injection configured
- ✔ User Secrets configured
- ⏳ CRUD pages
- ⏳ Authentication
- ⏳ Additional project features

---

## Author

**Badr Almadhi** and **Dylan Weiner**

Master of Software Development  
University of Utah

---

## License

This project was created for educational purposes.
