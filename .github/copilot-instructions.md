# GitHub Copilot Instructions for Noita Save Scummer

## Code Standards & Guidelines

### General Principles
- Use latest C# language features and .NET 9.0 capabilities
- Follow Microsoft's official C# coding conventions
- Prioritize readability and maintainability over cleverness
- Use meaningful names for variables, methods, and classes
- Keep methods focused and single-purpose

### Dependencies & Libraries
- **NO third-party NuGet packages** - use only built-in .NET libraries
- Leverage System.IO, System.Text.Json, System.Threading, and other standard namespaces
- Prefer modern async/await patterns where appropriate
- Use built-in dependency injection and configuration patterns

### Architecture & Design
- Maintain clean separation of concerns
- Use proper error handling with try-catch blocks
- Implement proper resource disposal with using statements
- Follow SOLID principles where applicable
- Keep the application simple but robust

### User Experience
- **Maintain constant and stable visual presence** in the console
- Provide clear, actionable feedback to users
- Use consistent formatting and layout
- Show progress and status information clearly
- Handle edge cases gracefully with helpful error messages

### File Operations
- Always use Path.Combine() for cross-platform path handling
- Implement proper file locking and error recovery
- Use async file operations for better responsiveness
- Validate paths and permissions before operations

### Console Application Best Practices
- Use Console.Clear() judiciously to maintain stable UI
- Implement proper keyboard input handling
- Use Console.SetCursorPosition() for dynamic updates
- Provide clear exit strategies and cleanup
- Handle Console.CancelKeyPress for graceful shutdown

### Code Organization
- Keep Program.cs focused on application startup
- Create separate classes for distinct responsibilities
- Use meaningful namespaces and file organization
- Document complex logic with clear comments
- Use regions sparingly and only for logical grouping
