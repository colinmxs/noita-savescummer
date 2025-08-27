# GitHub Copilot Instructions for Noita Save Scummer

## Project Overview & Context

This is a .NET 9.0 console application for automatically backing up and restoring Noita game saves. The project emphasizes simplicity, reliability, and Windows optimization with automated distribution via GitHub Actions.

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
- **JSON Serialization**: Always use JsonSerializerContext for AOT compatibility (see JsonContext.cs)

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
- **Use IconProvider for all UI icons** - provides Unicode support with ASCII fallback for Windows console compatibility

### File Operations
- Always use Path.Combine() for Windows path handling
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

## Release Management & Distribution

### GitHub Actions Workflows
- **CI Workflow**: Validates builds on Windows for every push/PR
- **Release Workflow**: Creates Windows binaries when version tags are pushed
- **Dev Builds Workflow**: Generates latest Windows artifacts for main branch pushes

### Version Management
- Follow semantic versioning (vX.Y.Z)
- PATCH for bug fixes, MINOR for features, MAJOR for breaking changes
- Always test builds before creating release tags
- Update VERSION.md with detailed release notes

### Release Process
```bash
# Standard release flow
git add . && git commit -m "descriptive message"
git push origin main
git tag vX.Y.Z
git push origin vX.Y.Z
```

### Windows Considerations
- Test self-contained publishing: `dotnet publish -c Release -r win-x64 --self-contained`
- Ensure JSON serialization uses JsonSerializerContext for AOT compatibility
- Use IconProvider for console display to handle Unicode/ASCII fallback
- Verify builds work on Windows 10/11 (64-bit)

## Documentation Strategy

### Document Hierarchy
- **BUILD_STATUS.md**: Current technical status and build health
- **RELEASE_STRATEGY.md**: Operational procedures and workflows  
- **VERSION.md**: Release history and detailed changelog
- **README.md**: User-facing documentation and features
- **.github/copilot-instructions.md**: This file - AI assistant guidelines

### Documentation Updates
- Update BUILD_STATUS.md when technical configuration changes
- Update RELEASE_STRATEGY.md when operational procedures change
- Update VERSION.md for every release with detailed notes
- Update README.md when user-facing features change

## Critical Reminders

### JSON Serialization (AOT Compatibility)
```csharp
// Always use JsonSerializerContext for serialization
var config = JsonSerializer.Deserialize(json, NoitaSaveScummerJsonContext.Default.Configuration);
var json = JsonSerializer.Serialize(configuration, NoitaSaveScummerJsonContext.Default.Configuration);

// Update JsonContext.cs when adding new serializable models
[JsonSerializable(typeof(NewModel))]
```

### Console Display (Windows Icons)
```csharp
// Always use IconProvider instead of hardcoded emojis
Console.WriteLine($"{IconProvider.Success} Operation completed!");
Console.WriteLine($"{IconProvider.Error} Operation failed!");

// Automatically handles Unicode support detection and ASCII fallback
```

### Build Validation
```bash
# Always test before releasing
dotnet clean && dotnet build --configuration Release
dotnet publish -c Release -r win-x64 --self-contained -o test-build
./test-build/noita-savescummer.exe  # Test the executable
rm -rf test-build  # Clean up
```
