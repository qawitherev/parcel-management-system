# 📦 Parcel Management System

A robust parcel management web application built with **ASP.NET Core** and **Entity Framework Core** to showcase modern .NET backend development and architectural best practices.

---

## 📚 Table of Contents

- [📝 Overview](#overview)
- [🛠️ Tech Stack](#tech-stack)
- [🏗️ Architecture](#architecture)
- [✅ Best Practices](#best-practices)
- [✨ Features](#features)
- [🚀 How to Run](#how-to-run)
- [🤝 Contributing](#contributing)

---

## 📝 Overview

This project is designed to demonstrate advanced .NET skills through the development of a scalable, secure, and maintainable parcel management API. It follows Clean Architecture principles and leverages patterns such as Repository and Specification to promote separation of concerns, testability, and flexibility.

---

## 🛠️ Tech Stack

- **Language:** C# (.NET 8+) ⚙️
- **Framework:** ASP.NET Core Web API 🌐
- **ORM:** Entity Framework Core (MySQL) 🗄️
- **Authentication:** JWT Bearer Token 🔐
- **Testing:** xUnit (integration-ready) 🧪
- **API Documentation:** Swagger/OpenAPI 📖

---

## 🏗️ Architecture

### 🧹 Clean Architecture

```
src/
├── ParcelManagement.Api          # Presentation Layer (Controllers, DTOs, Middleware)
├── ParcelManagement.Core         # Domain Layer (Entities, Specifications, Services, Interfaces)
├── ParcelManagement.Infrastructure # Infrastructure Layer (EF Core DbContext, Migrations, Repositories)
```

- **API Layer:** Handles HTTP requests, routing, and authentication/authorization 🌍
- **Core Layer:** Contains business/domain logic, service interfaces, entities, and specifications 🧠
- **Infrastructure Layer:** Implements repositories, database context, and migrations 🏢

### 🗝️ Key Patterns

- **Repository Pattern:** Abstracts data access and enables easy mocking for tests 🗃️
- **Specification Pattern:** Encapsulates query logic for complex filtering and business rules 📋
- **DTO Pattern:** Decouples internal models from API contracts 🔄
- **Dependency Injection:** Ensures loose coupling and testability 🧩
- **Middleware:** Centralized error handling and API exception management 🚦

---

## ✅ Best Practices Implemented

- **Separation of Concerns:** Layers for API, domain, and data access 🧱
- **SOLID Principles:** Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion 🏆
- **Async/Await:** All I/O operations are asynchronous for scalability ⚡
- **Unit of Work (via EF Core):** Ensures transactional integrity 🔄
- **Automated Migrations:** Database schema managed via code 🛠️
- **Strong Validation:** Data annotations and specification-based checks ✔️
- **Security:** JWT authentication, password hashing, role-based access 🔒
- **Environment-Based Configuration:** Secure secrets and connection strings 🌱

---

## ✨ Features

- 👤 User registration and authentication (secure password hashing)
- 🛡️ JWT-based authorization
- 🏢 Resident unit and parcel CRUD operations
- 🚚 Parcel tracking and status management
- 📑 API documentation with Swagger
- 🛑 Centralized error handling
- 🧪 Ready for integration and unit testing

---

## 🚀 How to Run

1. **Clone the repo**
    ```sh
    git clone https://github.com/qawitherev/parcel-management-system.git
    ```

2. **Set up the database**
    - Default is MySQL; configure your connection string in `appsettings.json`.

3. **Apply migrations**
    ```sh
    dotnet ef database update
    ```

4. **Run the API**
    ```sh
    dotnet run --project src/ParcelManagement.Api
    ```

5. **Access Swagger UI**
    - Navigate to `http://localhost:5000/swagger` (default port may vary).

---

## 🤝 Contributing

PRs are welcome! This project is intended as a learning showcase—feel free to fork and adapt for your own .NET studies or portfolio.

---

## 💡 Why This Project?

This repository is built to **showcase proficiency in modern .NET development**, including:
- Scalable architecture 📈
- Enterprise design patterns 🏢
- Security best practices 🔒
- Testability and maintainability 🧪
- Real-world business logic and API design 🌍

---

## 📄 License

MIT
