using SimplexMethod;

namespace LinearProgrammingWeb.Models;

public class SimplexMethodModel
{
    public string TableString;
    public Simplex.CalculationResult CalculationResult;

    public SimplexMethodModel(Simplex.CalculationResult? calculationResult = null, string? tableString = null)
    {
        CalculationResult = calculationResult!;
        TableString = tableString!;
    }
}