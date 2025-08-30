using System.ComponentModel;
using System.Text;

namespace SimplexMethod;

public static class Simplex
{
    private const int MaxAttempts = 100;

    public static CalculationResult Calculate(Table table)
    {
        var result = new CalculationResult();
        int a = 0;
        result.Tables.Add(new Table(table));
        while (!table.AllItemsOfCostRowAreLessOrEqualToZero())
        {
            try
            {
                Next(table);
            }
            catch (Exception)
            {
                result.State = CalculationResult.States.SolutionNotFound;
                break;
            }
            result.Tables.Add(new Table(table));
            a++;
            
            if (a <= MaxAttempts) continue;
            result.State = CalculationResult.States.MaxAttemptsExceeded;
            break;
        }
        return result;
    }

    private static void Next(Table table)
    {
        var pivot = table.GetPivot();
        table.NormalizationOfThePivotRow(pivot);
        table.RowReduction(pivot);
    }

    public class CalculationResult(List<Table> tables, CalculationResult.States state)
    {
        public CalculationResult() : this(new List<Table>(), States.None) { }
        
        public enum States
        {
            [Description("Optimal solution found")]
            None = 0,
            [Description("Solution not found")]
            SolutionNotFound = 1,
            [Description("Max attemts exceeded")]
            MaxAttemptsExceeded = 2
        }
        public List<Table> Tables { get; set; } = tables;
        public States State { get; set; } = state;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var table in Tables)
            {
                sb.AppendLine(table.ToString());
            }
            if (State is not States.None)
                sb.AppendLine(State.GetDescription());
            return sb.ToString();
        }
    }
}