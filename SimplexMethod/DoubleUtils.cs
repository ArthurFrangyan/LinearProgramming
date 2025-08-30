namespace SimplexMethod;

public static class DoubleUtils
{
    public static double Tolerance { get; set; } = 1e-10;
    public static bool AreEqual(double a, double b, double epsilon = 1e-10)
    {
        return Math.Abs(a - b) < epsilon;
    }
}
