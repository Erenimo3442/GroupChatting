# Claude Code Context & Preferences

## Project Overview
- **Type**: Group Chatting Application (C# .NET)
- **Architecture**: Clean Architecture with separate layers (Core, Application, Infrastructure, WebAPI)
- **Database**: Entity Framework Core + MongoDB for messages
- **Features**: Real-time chat with SignalR, file uploads, group management, JWT authentication

## Key Instructions
- Do what has been asked; nothing more, nothing less
- NEVER create files unless absolutely necessary for achieving goals
- ALWAYS prefer editing existing files to creating new ones
- NEVER proactively create documentation files (*.md) or README files unless explicitly requested
- Follow existing code conventions and patterns in the codebase
- Use TodoWrite tool for task planning and tracking

## Testing & Quality
- Run lint and typecheck commands after code changes
- Verify solutions with tests when possible
- Check README or search codebase to determine testing approach

## Project Structure
```
backend/                    - Backend C# application (moved from root)
├── Application/           - Application layer (DTOs, Services, Hub)
├── Core/                  - Domain entities (User, Group, Message)
├── Infrastructure/        - Data access and external services
├── WebAPI/               - Web API controllers and configuration
├── Infrastructure.Tests/  - Infrastructure layer tests
├── WebAPI.Tests/         - API layer tests
└── GroupChatting.sln     - Solution file
frontend/                  - Frontend application
.github/workflows/         - CI/CD configuration files
```

Note: Backend application has been moved from root to `backend/` directory. CI/CD configuration may need updates to reflect new structure.