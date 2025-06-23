# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
This is a household expense dashboard application built with Angular frontend, C# backend, and SQL database. The architecture follows n-tier design with Frontend (FE), Backend (BE), and Data Access Layer (DAL).

## Architecture Requirements
- **Frontend**: Angular with Angular Material design system
- **Backend**: C# with Entity Framework and Repository pattern
- **Database**: SQL Server 2022 with Entity Framework
- **Deployment**: Docker containers for all components (using OrbStack)
- **Design Patterns**: SOLID principles, GRASP, Repository pattern, Unit of Work pattern
- **Theming**: Dark and light theme support with instant switching

## Development Commands

### Prerequisites
- .NET 8.0 SDK installed at `/Users/alexoltean/.dotnet/dotnet`
- Node.js and npm installed
- Docker/OrbStack running for database

### Frontend (Angular)
```bash
cd frontend

# Install dependencies
npm install

# Start development server (runs on http://localhost:4200)
npm start

# Build for production
npm run build

# Generate components/services
npx ng generate component [name]
npx ng generate service [name]
```

### Backend (C#)
```bash
cd backend/ExpenseTracker.Api

# Add .NET to PATH
export PATH="$PATH:/Users/alexoltean/.dotnet"

# Restore packages
/Users/alexoltean/.dotnet/dotnet restore

# Build solution
/Users/alexoltean/.dotnet/dotnet build

# Run application (runs on http://localhost:5000)
/Users/alexoltean/.dotnet/dotnet run

# Run tests
cd ../ExpenseTracker.Api.Tests
/Users/alexoltean/.dotnet/dotnet test
```

### Database (SQL Server in Docker)
```bash
# Start SQL Server database
docker run --name sqlserver-db -e ACCEPT_EULA=Y -e SA_PASSWORD=ExpenseTracker123! -e MSSQL_PID=Express -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

# Check running containers
docker ps

# Stop database
docker stop sqlserver-db

# Remove database container
docker rm sqlserver-db
```

## Project Structure
```
/
├── frontend/                           # Angular application
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/
│   │   │   │   ├── expense-list/      # Expense listing component
│   │   │   │   └── expense-form/      # Expense creation/editing form
│   │   │   ├── services/
│   │   │   │   ├── expense.service.ts # HTTP client for API calls
│   │   │   │   └── theme.service.ts   # Dark/light theme management
│   │   │   ├── models/
│   │   │   │   └── expense.model.ts   # TypeScript interfaces
│   │   │   ├── app.component.ts       # Main app component with navbar
│   │   │   └── app.routes.ts          # Angular routing
│   │   ├── styles.scss                # Global styles with theme support
│   │   └── main.ts                    # Application bootstrap
│   ├── angular.json
│   ├── package.json
│   ├── Dockerfile
│   └── nginx.conf
├── backend/ExpenseTracker.Api/         # C# Web API
│   ├── Controllers/
│   │   └── ExpensesController.cs       # REST API endpoints
│   ├── Services/
│   │   ├── IExpenseService.cs
│   │   └── ExpenseService.cs           # Business logic layer
│   ├── Models/
│   │   └── Expense.cs                  # Domain model
│   ├── DTOs/
│   │   └── ExpenseDto.cs               # Data transfer objects
│   ├── Data/
│   │   ├── ExpenseContext.cs           # Entity Framework DbContext
│   │   ├── Repositories/               # Repository pattern implementation
│   │   └── UnitOfWork/                 # Unit of Work pattern
│   ├── Program.cs                      # Application startup
│   ├── Dockerfile
│   └── appsettings.json
├── backend/ExpenseTracker.Api.Tests/   # Unit tests
│   └── Services/
│       └── ExpenseServiceTests.cs
└── docker-compose.yml                  # Docker orchestration
```

## Key Features to Implement
- Add/edit/delete household expenses
- Expense categorization and filtering
- Monthly expense reporting and visualization
- Responsive design with Angular Material
- Theme switching (dark/light mode)
- Data persistence with SQL database

## Current Status
✅ **Completed Features:**
- Angular frontend with Material Design
- C# Web API backend with proper architecture
- SQL Server 2022 database with Entity Framework
- Repository and Unit of Work patterns implemented
- CRUD operations for expenses
- Dark/light theme switching
- Docker configuration for all components
- Unit tests for business logic
- API endpoints with proper error handling
- Responsive design with Angular Material
- Chart.js integration for interactive expense visualization

🚀 **Running the Application:**
1. Start all services: `docker-compose up -d`
2. Open http://localhost:4200 in browser
3. API available at http://localhost:5050 with Swagger UI at /swagger

## Development Workflow
1. SQL Server 2022 database runs in Docker on port 1433
2. Backend API runs on http://localhost:5050 with Swagger UI at /swagger (changed from 5000 due to macOS AirPlay conflict)
3. Frontend runs on http://localhost:4200
4. All unit tests pass (4/4 tests passing)
5. API verified working with SQL Server
6. Follow SOLID principles and design patterns implemented
7. Use meaningful naming conventions
8. Write minimal, clean code following senior developer standards

## Angular Component Architecture Rules
- **NEVER use inline templates**: Always create separate .html files for component templates
- **NEVER use inline styles**: Always create separate .css/.scss files for component styles
- Component files should follow this structure:
  - component-name.component.ts (TypeScript logic only)
  - component-name.component.html (HTML template)
  - component-name.component.css (Component styles)
- Keep TypeScript files focused on logic, data binding, and component lifecycle
- Separate concerns: HTML for structure, CSS for styling, TypeScript for behavior

## API Endpoints
- GET /api/expenses - Get all expenses
- GET /api/expenses/{id} - Get expense by ID
- POST /api/expenses - Create new expense
- PUT /api/expenses/{id} - Update expense
- DELETE /api/expenses/{id} - Delete expense
- GET /api/expenses/month/{year}/{month} - Get expenses by month
- GET /api/expenses/by-category - Get expenses grouped by category
- GET /api/expenses/total - Get total of all expenses