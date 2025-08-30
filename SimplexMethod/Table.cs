using System.Collections;
using System.Text;
using static SimplexMethod.DoubleUtils;

namespace SimplexMethod;

public class Table : IEnumerable<double>
{
    private double[,] _table;
    public int Rows => _table.GetLength(0);
    public int ConstraintRows => Rows - 1;
    public int CostRow => Rows - 1;
    public int Columns => _table.GetLength(1);
    public int VariableColumns => Columns - 1;
    public int BCol => Columns - 1;

public Table(string table) 
        : this(GetArray(table)){}
    public Table(Func<string?> getLine) 
        : this(GetArray(getLine)) { }

    public Table(double[,] table) => _table = table ?? throw new ArgumentNullException(nameof(table));

    public Table(Table table)
    {
        _table = table._table.Clone() as double[,] ?? throw new InvalidOperationException();
        _table = new double[table.Rows, table.Columns];
        for (int i = 0; i < table.Rows; i++)
        for (int j = 0; j < table.Columns; j++)
        {
            _table[i, j] = table[i, j];
        }
    }

    private static double[,] GetArray(Func<string?> getLine)
    {
        List<List<double>> numbers = new List<List<double>>();
        int row = 0;
        string? input;
        while (!string.IsNullOrWhiteSpace(input = getLine()))
        {
            input = input
                .Replace("{", "")
                .Replace("}", "");
            string[] parts = input.Split([' ',','], StringSplitOptions.RemoveEmptyEntries);
            
            numbers.Add([]);
            foreach (string part in parts)
                if (double.TryParse(part, out var num))
                    numbers[row].Add(num);
            row++;
        }
        
        int rows = numbers.Count;
        int cols = numbers.Max(n => n.Count);
        double[,] numbersArray = new double[rows, cols];
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < numbers[i].Count; j++)
                numbersArray[i, j] = numbers[i][j];
            for (int j = numbers[i].Count; j < cols; j++)
                numbersArray[i, j] = 0;
        }

        return numbersArray;
    }

    private static double[,] GetArray(string inputStr)
    {
        var lines = inputStr.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);
        var numbers = new List<double>[lines.Length];
        for (int row = 0; row < lines.Length; row++)
        {
            var input = lines[row]
                .Replace("{", "")
                .Replace("}", "");
            string[] parts = input.Split([' ',','], StringSplitOptions.RemoveEmptyEntries);
            numbers[row] = new List<double>();
            foreach (string part in parts)
                if (double.TryParse(part, out var num))
                    numbers[row].Add(num);
        }
        
        int rows = numbers.Length;
        int cols = numbers.Max(n => n.Count);
        double[,] numbersArray = new double[rows, cols];
        
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < numbers[i].Count; j++)
                numbersArray[i, j] = numbers[i][j];
            for (int j = numbers[i].Count; j < cols; j++)
                numbersArray[i, j] = 0;
        }

        return numbersArray;
    }
    
    public double this[int row, int column]
    {
        get => _table[row, column];
        set => _table[row, column] = value;
    }
    
    public IEnumerator<double> GetEnumerator()
    {
        for (int i = 0; i < _table.GetLength(0); i++)
        {
            for (int j = 0; j < _table.GetLength(1); j++)
            {
                yield return _table[i, j];
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string? ToString()
    {
        var sb = new StringBuilder();
        int cellWidth = 10;

        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                // string formatted = _table[i, j]
                //     .ToString("0.####") // Up to 4 decimal places, no trailing zeroes
                //     .PadLeft(cellWidth);
                Rational rational = Rational.FromDouble(_table[i, j]);
                string formatted = rational.ToString().PadLeft(cellWidth);
                sb.Append(formatted);
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }


    public bool AllItemsOfCostRowAreLessOrEqualToZero()
    {
        var r = CostRow;
        for (var c = 0; c < VariableColumns; c++)
        {
            if (_table[r, c] > Tolerance)
                return false;
        }

        return true;
    }

    public Pivot GetPivot()
    {
        var exceptions = new List<int>();
        int row;
        int col;
        do
        {
            col = GetPivotCol(exceptions);
            row = GetPivotRow(col, exceptions);
        } while (row == -1 && exceptions.Count < VariableColumns);
        return new Pivot(row, col);
    }
    public int GetPivotCol(List<int> exceptions)
    {
        int maxIndex = 0;
        
        double maxValue = _table[CostRow, 0];
        foreach (var colExcept in exceptions)
            if (colExcept == 0) maxValue = double.MinValue;
        
        for (int c = 1; c < VariableColumns; c++)
        {
            bool isValid = true;
            foreach (var colExcept in exceptions)
                if (colExcept == c) isValid = false;
            if (!isValid) continue;
            if (_table[CostRow, c] > maxValue)
            {
                maxValue = _table[CostRow, c];
                maxIndex = c;
            }
        }

        if (_table[CostRow, maxIndex] <= 0)
            throw new Exception("No pivot");
        
        return maxIndex;
    }

    public int GetPivotRow(int pivotCol, List<int> exceptions)
    {
        var r = 0;
        int minIndex = 0;
        double minRatio;
        if (_table[r, pivotCol] > 0)
            minRatio = _table[r, BCol] / _table[r, pivotCol];
        else
            minRatio = double.MaxValue;
        
        for (r = 1; r < ConstraintRows; r++)
        {
            if (_table[r, pivotCol] <= 0) continue;
            if (_table[r, BCol] / _table[r, pivotCol] >= minRatio) continue;
            
            minRatio = _table[r, BCol] / _table[r, pivotCol];
            minIndex = r;
        }

        if (minRatio == double.MaxValue)
        {
            exceptions.Add(pivotCol);
            return -1;
        }
        
        return minIndex;
    }

    public void NormalizationOfThePivotRow(Pivot pivot)
    {
        var pivotValue = _table[pivot.Row, pivot.Col];
        for (int c = 0; c < Columns; c++)
        {
            _table[pivot.Row, c] /= pivotValue;
        }
    }
    public void RowReduction(Pivot pivot)
    {
        for (var row = 0; row < Rows; row++)
        {
            if (row == pivot.Row) continue;
            RowReduction(pivot.Row, pivot.Col, row);
        }
    }
    private void RowReduction(int pivotRow, int pivotCol, int row)
    {
        var multiplier = _table[row, pivotCol];
        for (var c = 0; c < Columns; c++)
        {
            _table[row, c] -= _table[pivotRow, c] * multiplier;
        }
    }
}