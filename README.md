# LinearProgramming

Simplex **tableau** calculator that shows the work, not just the answer.

At the core is `SimplexMethod` (a `net9.0` library) which takes an initial tableau, performs pivoting, and returns every intermediate tableau. On top of that are two small front-ends:

- `Simplex.Console`: quick local usage from the terminal
- `LinearProgrammingWeb`: a minimal ASP.NET Core MVC UI to share and view the steps

Live demo (may change over time): https://linearprogramming-endq.onrender.com/

## Why This Exists

This project started as a small console tool because doing simplex by hand is slow and error-prone when the tables get big.

Online solvers can produce the same optimal value, but the **step-by-step tableaus** often don't match what you are trying to follow in class or in a book. A lot of simplex material (and some tools) are built around a "maximize" canonical form, while minimization is handled by converting `min z` into `max -z` or by switching the cost-row convention. Those transformations preserve the optimum, but they can change the tableau you iterate on, which changes the pivot path and intermediate tableaus.

We needed the steps for the convention we were using, so the tool was extracted into a library and wrapped in a small web app (Docker-friendly) instead of sharing platform-specific binaries.

## Requirements

- .NET SDK `9.x`
- Optional: Docker

## Run

Web UI:

```powershell
dotnet run --project LinearProgrammingWeb
```

Console:

```powershell
dotnet run --project Simplex.Console
```

Docker (web app):

```powershell
docker build -t linearprogramming .
docker run --rm -p 8080:80 linearprogramming
```

Open `http://localhost:8080/`.

## Input Format (Tableau)

This project is tableau-first. It does not convert equations/inequalities into a tableau for you. You provide the **initial simplex tableau** directly, including any slack/surplus/artificial columns you want.

Parsing rules:

- One row per line
- Columns separated by spaces and/or commas
- Optional `{` `}` are ignored (so `{ 1, 2, 3 }` is accepted)
- Short rows are padded with trailing zeros to match the widest row
- Numbers use `double.TryParse` with the current runtime culture
  - Commas are always treated as separators, so decimal-comma like `1,5` is not supported
  - If you need decimals, prefer `.` and run under a culture that parses `.` as the decimal separator

Table shape the library assumes:

- Last column is RHS `b`
- Last row is the cost row
- Every column except `b` is treated as a variable column (`x1..xn`), including any slack/artificial columns you included

Example (same sample as the web UI placeholder):

```text
-10 1 1 1 0 0 0 0
-6  1 0 0 1 0 0 0
-7  1 0 0 0 1 0 0
 1  2 2 0 0 0 1 1
 1  2 2 0 0 0 0 1
```

Console input note: paste one row per line, then submit an empty line to finish input.

## What The Library Does

Given the tableau you provide, the solver:

- Stops when all cost-row entries (excluding `b`) are `<= 0` (tolerance `1e-10`)
- Picks the entering column as the largest cost-row value (must be `> 0`)
- Picks the leaving row via the minimum ratio test over rows with a positive pivot-column entry: `b_i / a_{i,pivotCol}`
- Normalizes the pivot row and performs row reduction on every other row

If no valid pivot exists, the result state is "Solution not found". To avoid infinite loops, there is a max-attempts guard of `100` (the current check trips after the 101st pivot).

## Library Usage

```csharp
using SimplexMethod;

var table = new Table(tableString);   // or: new Table(double[,])
var result = Simplex.Calculate(table);

// result.Tables contains the initial tableau + every intermediate tableau
// result.State indicates success or failure
```

The web UI renders values as reduced `Rational` strings for readability; calculations run on `double`.

## Limitations

- Requires a valid starting tableau/basis (no two-phase simplex / Big-M helper)
- Floating-point arithmetic (`double`)
