namespace SimplexMethod;

public struct Rational
{
    public long Numerator { get; }
    public long Denominator { get; }

    public Rational(long numerator, long denominator)
    {
        if (denominator == 0)
            throw new DivideByZeroException();

        // Normalize sign
        if (denominator < 0)
        {
            numerator = -numerator;
            denominator = -denominator;
        }

        long gcd = GCD(Math.Abs(numerator), denominator);
        Numerator = numerator / gcd;
        Denominator = denominator / gcd;
    }

    private static long GCD(long a, long b)
    {
        while (b != 0)
        {
            long t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    public override string ToString()
    {
        return Denominator == 1 ? $"{Numerator}" : $"{Numerator}/{Denominator}";
    }

    public static Rational FromDouble(double value, double tolerance = 1.0E-10, int maxIterations = 1000)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Value must be a finite number.");

        long numerator = 1;
        long denominator = 0;
        long prevNumerator = 0;
        long prevDenominator = 1;
        double frac = value;
        int iter = 0;

        while (iter++ < maxIterations)
        {
            long a = (long)Math.Floor(frac);
            long tempNumerator = a * numerator + prevNumerator;
            long tempDenominator = a * denominator + prevDenominator;

            double approx = (double)tempNumerator / tempDenominator;
            if (Math.Abs(value - approx) < tolerance)
                return new Rational(tempNumerator, tempDenominator);

            prevNumerator = numerator;
            prevDenominator = denominator;
            numerator = tempNumerator;
            denominator = tempDenominator;

            frac = 1.0 / (frac - a);
        }

        return new Rational(numerator, denominator);
    }
}