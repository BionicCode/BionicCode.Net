# Code style guidance (Clean Code)

When proposing code changes, prefer applying Clean Code principles while adhering to existing repository standards.

## Priority order

1. Follow the repository's `.editorconfig` and established local conventions.
2. Prefer Clean Code principles where they improve clarity without causing unnecessary churn.

## Clean Code preferences (when appropriate)

- Favor clarity and maintainability over cleverness.
- Keep methods small and focused.
- Use meaningful names and consistent terminology.
- Minimize side effects and hidden behavior.
- Avoid unnecessary complexity and premature abstraction.

## General code style

- Use consistent indentation (4 spaces).
- Place `using` directives at the top of the file.
- Keep related code together and organize files logically.
- Use regions to group related code blocks.
- Follow naming conventions (e.g., PascalCase for public members, camelCase for private members).
- Write XML documentation comments for public APIs.
- Prefer expression-bodied members for simple properties and methods.
- Use string interpolation over concatenation.
- Use region name also for the closing endregion directive

## Refactoring scope

- Do not perform broad renames or large structural refactors unless explicitly requested.
- If a large refactor would help, propose it as an option and explain the tradeoffs.