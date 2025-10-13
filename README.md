# Full-Stack Finance Tracker Application (Portfolio Project)

## Project Purpose & Showcase

This is a full-stack portfolio project developed to demonstrate a comprehensive skill set in modern web development, from backend architecture and API design to frontend implementation and automated cloud deployment.

The primary goal was to build a real-world, end-to-end application that showcases proficiency in the following technologies and principles:

*   **Backend Development:** Building secure and scalable APIs with **C# & .NET 8**, **ASP.NET Core**, and **Entity Framework Core**.
*   **Frontend Development:** Creating a responsive and dynamic user interface with **Angular 19**, **TypeScript**, and **Angular Material**.
*   **Database Management:** Designing a relational schema and interacting with a **PostgreSQL** database.
*   **DevOps & Deployment:** Using **Terraform** for Infrastructure as Code (IaC), containerizing applications with **Docker**, configuring **Nginx**, and deploying to **Azure Container Apps**.
*   **Architectural Patterns:** Implementing RESTful API design, JWT authentication, and clean architecture principles.
*   **Security:** Securing the application with JWT-based authentication and API rate limiting.

---

***Note to Reviewers:*** *A live version of this application can be made available for demonstration upon request.*

---

## Overview

Finance Tracker is a full-stack web application that helps users manage their personal finances. It provides a complete solution for tracking income and expenses through a robust system of transactions and categories, powered by a secure backend API and a responsive Angular frontend.

## Core Features

*   **Secure User Authentication:** Full user registration and login system.
*   **Transaction Management:** Full CRUD (Create, Read, Update, Delete) operations for financial transactions.
*   **Category Management:** Full CRUD operations for custom spending categories.
*   **Data Integrity:** Backend logic prevents data inconsistencies (e.g., assigning an "Income" transaction to an "Expense" category).
*   **Dynamic Server-Side Sorting:** Data is sorted efficiently on the server before being sent to the client.
*   **Secure Account Deletion:** Users can permanently delete their account and all associated data.
*   **API Security:** The backend is protected with IP-based rate limiting.
*   **Interactive API Documentation:** Live Swagger/OpenAPI documentation for the backend.

## Technology Stack

### Backend

*   **Framework/Language:** ASP.NET Core 8, C# 12
*   **API:** RESTful Web API
*   **Data Access:** Entity Framework Core 8
*   **Database:** PostgreSQL
*   **Authentication:** JWT (JSON Web Tokens)
*   **Utilities:** AutoMapper, AspNetCoreRateLimit

### Frontend

*   **Framework/Language:** Angular 19, TypeScript
*   **UI Components:** Angular Material
*   **Styling:** SCSS

### DevOps & Deployment

*   **Infrastructure as Code:** Terraform
*   **Containerization:** Docker
*   **Cloud Hosting:** Azure Container Apps
*   **Database Hosting:** Azure Database for PostgreSQL
*   **Web Server:** Nginx (serving the Angular frontend and acting as a reverse proxy)

## Architecture & Design Highlights

### Infrastructure as Code (IaC) with Terraform

The entire cloud infrastructure for this project is defined and managed as code using **Terraform**. This includes the Azure Container Apps environment, the Azure Database for PostgreSQL instance, and all related networking and configuration. This approach ensures that the deployment is automated, reproducible, and version-controlled, showcasing a modern DevOps mindset.

### Production Deployment Architecture

The application is deployed using a modern, distributed architecture on Microsoft Azure.
*   The **backend (.NET API)** and **frontend (Angular)** are hosted in separate **Docker containers** within an **Azure Container Apps** environment, which is ideal for running microservices and containerized applications.
*   The Angular frontend is served by an **Nginx** web server, which also acts as a **reverse proxy** to securely forward API requests to the backend container.
*   Data persistence is handled by **Azure Database for PostgreSQL**, a fully managed, secure, and scalable cloud database service.

### Stateless JWT Authentication

The application uses **JSON Web Tokens (JWT)** for secure, stateless authentication. After a user logs in, the API issues a JWT. This token is included in the `Authorization` header of all subsequent requests from the frontend, allowing the backend to verify the user's identity without maintaining session state.

### Data Integrity and Business Logic

The system enforces critical business rules to ensure data consistency. For instance, 'Income' transactions can only be assigned to 'Income' or 'Neutral' categories. If a user attempts an action that violates this (like changing a category's type), the backend automatically re-assigns affected transactions to a default "WITHOUT CATEGORY" to prevent data corruption.
