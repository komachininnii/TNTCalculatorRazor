# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction
- If First-chance exceptions are displayed in Visual Studio but do not affect the application, do not address them; treat them as Visual Studio behavior to be ignored.

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
- Prefer keeping `AdjustedWeight` and `CorrectedWeight` calculated and stored separately in `Index.cshtml.cs` for readability. 
- Mark `AdjustedWeightCalculator.Calculate` with `[System.Obsolete("â¬ì«ê´å¸è„ÇÃÇΩÇﬂÅAIndex.cshtml.cs Ç≈ÇÕ GetBasis Ç∆ CalculateAdjusted Çégóp", false)]` to discourage usage and prefer explicit `GetBasis` and `CalculateAdjusted` for adjusted weight and separate selection of `CorrectedWeight` based on basis. 
- Do not replace with a single `AdjustedWeightCalculator.Calculate` call; prefer explicit `AdjustedWeightCalculator.CalculateAdjusted` for adjusted weight and separate selection of `CorrectedWeight` based on basis.
- Do not persist error logs on Azure; keep the current minimal `Program.cs` (with startup try/catch and AppDomain/TaskScheduler handlers) as-is.