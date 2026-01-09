# 🛒 Containerized E-Commerce Backend API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat&logo=dotnet)
![Docker](https://img.shields.io/badge/Docker-Enabled-2496ED?style=flat&logo=docker)
![SQL Server](https://img.shields.io/badge/Database-SQL%20Server-CC2927?style=flat&logo=microsoft-sql-server)
![EF Core](https://img.shields.io/badge/ORM-EF%20Core-512BD4?style=flat)
![License](https://img.shields.io/badge/License-MIT-green)

A robust, containerized RESTful API for an E-Commerce platform built with **ASP.NET Core 8**.
This project demonstrates a modern backend architecture using **Docker Compose** for orchestration, **JWT** for secure authentication, and **Role-Based Access Control (RBAC)**.

---

## ✨ Key Features

* **🐳 Fully Containerized**: Seamless setup using Docker and Docker Compose. No local SQL Server installation required.
* **🔐 Secure Authentication**: Implemented **JWT (JSON Web Token)** authentication with **BCrypt** password hashing.
* **🛡️ Role-Based Authorization**: Distinct access levels for **Admin** (Full CRUD) and **User** (Read-only/Limited).
* **🗄️ Database Management**: Used **Entity Framework Core** with Code-First approach and Migrations.
* **🌱 Automated Data Seeding**: System automatically creates a default Admin account and initial data upon startup.
* **📄 API Documentation**: Integrated **Swagger UI** for interactive API testing and documentation.

---

## 🛠️ Tech Stack

* **Framework**: .NET 8 (ASP.NET Core Web API)
* **Database**: Microsoft SQL Server 2022 (Linux Container)
* **Containerization**: Docker, Docker Compose
* **ORM**: Entity Framework Core
* **Tools**: Swagger/OpenAPI, Git

---

## 🚀 Getting Started (Run in 1 Step)

Prerequisites: Ensure you have **Docker Desktop** installed and running.

### 1. Clone the Repository
```bash
git clone [https://github.com/rokie7150/ECommerceBackend.git](https://github.com/rokie7150/ECommerceBackend.git)
cd ECommerceBackend