# LinearProgramming

A step-by-step Simplex **tableau** calculator, built around the cost-row/tableau convention we needed for minimization-style problems.

This repository contains:

- `SimplexMethod`: a small .NET library that pivots an initial tableau and keeps every intermediate tableau.
- `Simplex.Console`: a console app for quick local calculations.
- `LinearProgrammingWeb`: a simple ASP.NET Core MVC UI for sharing and viewing steps in the browser.

**Live demo** (may change over time): `https://linearprogramming-endq.onrender.com/`

## Why This Exists

The very first version was written in a few hours as a personal tool: manual simplex calculations get long, and small arithmetic mistakes are easy to make.

You might ask: why not use an online simplex calculator? The issue was not the final optimum, it was the **steps**. A lot of simplex material (and some online solvers) are written around the standard/canonical maximization form, and minimization is often handled by converting `min z` into `max -z` or by using a different cost-row convention. When you change conventions, the optimum is preserved, but the **tableau you iterate on changes**, so the pivot sequence and intermediate tableaus can look different.

This project was built to reproduce the step-by-step process for the minimization/tableau convention we were using, and later turned into a shareable web app (Docker-friendly) instead of passing around binaries.

## Requirements

- .NET SDK `9.x`
- Optional: Docker (for containerized hosting)

## Run It

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

## Input: Tableau (Not Equations)

This project is tableau-first. It does not turn constraints like `Ax <= b` into a tableau for you. You provide the **initial simplex tableau** directly (including any slack/surplus/artificial columns you want to use).

How parsing works:

- Newline separates rows.
- Space and/or comma separates columns.
- `{` and `}` are ignored, so `{ 1, 2, 3 }` is accepted.
- Short rows are padded with trailing zeros to match the widest row.
- Numbers are parsed using the current runtime culture (`double.TryParse`). Commas are always treated as separators, so decimal-comma input like `1,5` is not supported. If you need decimals, prefer `.` and run under a culture that parses `.` as a decimal separator.

Expected shape (as implemented today):

- Last column is RHS `b`.
- Last row is the cost row (objective row used to decide whether to keep pivoting).
- All columns except `b` are treated as variable columns (`x1..xn`), including any slack/artificial columns you included.

Example (same sample as the web UI placeholder):

```text
-10 1 1 1 0 0 0 0
-6  1 0 0 1 0 0 0
-7  1 0 0 0 1 0 0
 1  2 2 0 0 0 1 1
 1  2 2 0 0 0 0 1
```

Console input detail: paste one row per line and submit an empty line to finish.

## How The Iterations Work

The library runs simplex iterations on the tableau you provide and stores each table:

- Stop when all entries in the cost row (excluding `b`) are `<= 0` (tolerance `1e-10`).
- Choose the entering column as the variable column with the largest cost-row value (must be `> 0`).
- Choose the leaving row using the minimum ratio test over rows with a positive pivot column entry: `b_i / a_{i,pivotCol}`.
- Normalize the pivot row and apply row reduction to every other row.

If no valid pivot exists, the result is reported as "Solution not found". A hard cap of `100` attempts is applied to avoid infinite loops (the current check trips after the 101st pivot).

## Using The Library

```csharp
using SimplexMethod;

var table = new Table(tableString);   // or: new Table(double[,])
var result = Simplex.Calculate(table);

// result.Tables contains the initial tableau + every intermediate tableau
// result.State indicates success or failure
```

The web UI renders values as reduced `Rational` strings for readability, while the algorithm runs on `double` internally.

## Limitations

- Requires a valid initial tableau (including any slack/surplus/artificial columns you need).
- Does not implement two-phase simplex / Big-M to automatically find an initial feasible basis.
- Uses floating-point arithmetic (`double`) for calculations.
