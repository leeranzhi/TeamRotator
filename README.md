# TeamRotator

TeamRotator is an automated task rotation management system designed to streamline team member duty assignments.

## Features

- Multiple rotation rules (daily, weekly, bi-weekly)
- Automatic holiday skipping
- Slack integration for notifications
- RESTful API endpoints
- PostgreSQL database
- Automated scheduling with Quartz.NET

## Project Structure

The project follows a three-layer architecture:

```
src/
├── TeamRotator.Core/           # Core layer: Entities and interfaces
├── TeamRotator.Infrastructure/ # Infrastructure layer: Data access and services
└── TeamRotator.Api/           # API layer: Controllers and configuration
```

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL 16
- Docker (optional, for running PostgreSQL)
- IDE (recommended: Visual Studio 2022 or JetBrains Rider)

## Local Development Setup

### 1. Database Setup

#### Option 1: Using Docker (Recommended)
```bash
# Start PostgreSQL container
docker run --name teamrotator-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_USER=postgres \
  -e POSTGRES_DB=teamrotator \
  -p 5433:5432 \
  -d postgres:16

# To stop the container
docker stop teamrotator-postgres

# To start an existing container
docker start teamrotator-postgres
```

#### Option 2: Local PostgreSQL Installation
- Install PostgreSQL 16 from https://www.postgresql.org/download/
- Create a new database named 'teamrotator'
- Update connection string in appsettings.json accordingly

### 2. Application Configuration

1. Clone the repository:
```bash
git clone https://github.com/leeranzhi/TeamRotator.git
cd TeamRotator
```

2. Configure appsettings.json:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Username=postgres;Password=postgres;Database=teamrotator"
  },
  "Slack": {
    "WebhookUrl": "your-slack-webhook-url"
  },
  "HolidayApiSettings": {
    "Url": "https://raw.githubusercontent.com/NateScarlet/holiday-cn/master"
  }
}
```

3. Apply database migrations:
```bash
cd src/TeamRotator.Api
dotnet ef database update
```

4. Build the solution:
```bash
dotnet build
```

5. Run the application:
```bash
dotnet run
```

The application will start at http://localhost:5000

### 3. Development Tools

#### Visual Studio Code Extensions
- C# Dev Kit
- .NET Core Test Explorer
- REST Client

#### API Testing
- Swagger UI: http://localhost:5000/swagger
- OpenAPI specification: `src/TeamRotator.Api/swagger.yaml`

## Database Schema

Key tables:
- `RotationTasks`: Defines tasks that need rotation
- `Members`: Team members information
- `TaskAssignments`: Task assignment records

## Scheduled Jobs

The system includes two main scheduled jobs:

1. AssignmentUpdateJob
   - Runs at: 00:00 daily
   - Purpose: Updates task assignments based on rotation rules

2. SendToSlackJob
   - Runs at: 08:00 daily
   - Purpose: Sends notifications about current assignments

## API Endpoints

### Tasks and Assignments
- GET `/api/assignments` - Get current task assignments
- PUT `/api/assignments/{id}` - Modify task assignment
- POST `/api/assignments/update` - Trigger manual update

## Troubleshooting

Common issues and solutions:

1. Database Connection Issues
```bash
# Check PostgreSQL container status
docker ps -a | grep teamrotator-postgres

# View container logs
docker logs teamrotator-postgres
```

2. Migration Issues
```bash
# Remove existing migrations
dotnet ef migrations remove

# Add new migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

3. Application Logs
- Location: `logs/teamrotator-{date}.txt`
- Level: Information (configurable in appsettings.json)

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

MIT License

## Support

For issues and feature requests, please create an issue in the GitHub repository. 