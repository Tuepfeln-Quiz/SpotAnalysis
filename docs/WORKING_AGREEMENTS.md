# SpotAnalysis – Working Agreements

## Architecture
- Keep business logic in `SpotAnalysis.Services`.
- Keep data access in `SpotAnalysis.Data`.
- Keep UI logic in `SpotAnalysisV2.Web` components only.

## Blazor Component Rules
- Prefer reusable components for repeated UI blocks.
- Keep components small and single-purpose.
- Use parameters/events instead of hard-coded dependencies.

## Naming
- Use clear, intention-revealing names.
- Use consistent suffixes (`Dto`, `Service`, `Editor`, `List`, `Player`).

## Styling
- Put global styles in `wwwroot/css`.
- Use page-level files under `wwwroot/css/pages`.
- Use component-level files under `wwwroot/css/components` for shared UI parts.

## Code Review Checklist
- Is logic in the right layer?
- Is duplicate UI extracted into a component?
- Are names clear for a new developer?
- Is behavior covered by tests when logic changed?
