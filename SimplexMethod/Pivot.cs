namespace SimplexMethod;

public struct Pivot(int row, int col)
{
    public int Col { get; set; } = col;
    public int Row { get; set; } = row;
}