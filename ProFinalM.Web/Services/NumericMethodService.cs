using System.Globalization;
using ProFinalM.Web.Models;

namespace ProFinalM.Web.Services;

public class NumericMethodService
{
    private readonly ExpressionMath _expressionMath = new();

    public TaylorResult CalculateTaylor(TaylorInput input)
    {
        if (input.Order < 0 || input.Order > 10)
        {
            throw new InvalidOperationException("El orden debe estar entre 0 y 10.");
        }

        var expression = _expressionMath.Parse(input.Function);
        var derivative = expression;
        var terms = new List<TaylorTerm>();

        for (var k = 0; k <= input.Order; k++)
        {
            var derivativeAtA = derivative.Evaluate(input.ExpansionPoint);
            terms.Add(new TaylorTerm
            {
                Order = k,
                Derivative = derivative.ToString(),
                DerivativeAtA = derivativeAtA,
                Coefficient = derivativeAtA / Factorial(k)
            });

            derivative = derivative.Derivative();
        }

        var evaluationPoint = input.ExpansionPoint + input.Step;
        var approximate = EvaluatePolynomial(terms, input.ExpansionPoint, evaluationPoint);
        var real = expression.Evaluate(evaluationPoint);
        var absoluteError = Math.Abs(real - approximate);
        var percentageError = Math.Abs(real) < 1e-12 ? 0 : Math.Abs(absoluteError / real) * 100;
        var realCurve = new List<ChartPoint>();
        var taylorCurve = new List<ChartPoint>();
        var min = input.ExpansionPoint - 5;
        var max = input.ExpansionPoint + 5;

        for (var i = 0; i <= 80; i++)
        {
            var x = min + ((max - min) * i / 80.0);
            var yReal = expression.Evaluate(x);
            var yTaylor = EvaluatePolynomial(terms, input.ExpansionPoint, x);
            if (double.IsFinite(yReal) && double.IsFinite(yTaylor))
            {
                realCurve.Add(new ChartPoint { X = x, Y = yReal });
                taylorCurve.Add(new ChartPoint { X = x, Y = yTaylor });
            }
        }

        return new TaylorResult
        {
            Terms = terms,
            Polynomial = BuildPolynomial(terms, input.ExpansionPoint),
            EvaluationPoint = evaluationPoint,
            RealValue = real,
            ApproximateValue = approximate,
            AbsoluteError = absoluteError,
            PercentageError = percentageError,
            RealCurve = realCurve,
            TaylorCurve = taylorCurve
        };
    }

    public LeastSquaresResult CalculateLeastSquares(LeastSquaresInput input)
    {
        var points = ParsePoints(input.Points);
        if (points.Count < 2)
        {
            throw new InvalidOperationException("Ingresa al menos dos puntos.");
        }

        var n = points.Count;
        var sumX = points.Sum(p => p.X);
        var sumY = points.Sum(p => p.Y);
        var sumXY = points.Sum(p => p.XY);
        var sumX2 = points.Sum(p => p.X2);
        var sumY2 = points.Sum(p => p.Y2);
        var denominator = (n * sumX2) - (sumX * sumX);

        if (Math.Abs(denominator) < 1e-12)
        {
            throw new InvalidOperationException("No se puede ajustar una recta si todos los valores de x son iguales.");
        }

        var slope = ((n * sumXY) - (sumX * sumY)) / denominator;
        var intercept = ((sumY * sumX2) - (sumX * sumXY)) / denominator;
        var correlationDenominator = Math.Sqrt(((n * sumX2) - (sumX * sumX)) * ((n * sumY2) - (sumY * sumY)));
        var correlation = Math.Abs(correlationDenominator) < 1e-12 ? 0 : ((n * sumXY) - (sumX * sumY)) / correlationDenominator;
        var minX = points.Min(p => p.X);
        var maxX = points.Max(p => p.X);

        return new LeastSquaresResult
        {
            Points = points,
            Slope = slope,
            Intercept = intercept,
            Correlation = correlation,
            Determination = correlation * correlation,
            SumX = sumX,
            SumY = sumY,
            SumXY = sumXY,
            SumX2 = sumX2,
            SumY2 = sumY2,
            Line =
            [
                new ChartPoint { X = minX, Y = (slope * minX) + intercept },
                new ChartPoint { X = maxX, Y = (slope * maxX) + intercept }
            ]
        };
    }

    private static List<DataPoint> ParsePoints(string text)
    {
        var points = new List<DataPoint>();
        var lines = text.Split(["\r\n", "\n", ";"], StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var parts = line.Split([',', '\t', ' '], StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new InvalidOperationException($"El dato '{line}' debe tener formato x,y.");
            }

            points.Add(new DataPoint
            {
                X = double.Parse(parts[0], CultureInfo.InvariantCulture),
                Y = double.Parse(parts[1], CultureInfo.InvariantCulture)
            });
        }

        return points;
    }

    private static double EvaluatePolynomial(IEnumerable<TaylorTerm> terms, double a, double x)
    {
        return terms.Sum(term => term.Coefficient * Math.Pow(x - a, term.Order));
    }

    private static string BuildPolynomial(IEnumerable<TaylorTerm> terms, double a)
    {
        return string.Join(" + ", terms.Select(term =>
        {
            var coefficient = term.Coefficient.ToString("0.######", CultureInfo.InvariantCulture);
            return term.Order == 0 ? coefficient : $"{coefficient}(x - {a.ToString("0.####", CultureInfo.InvariantCulture)})^{term.Order}";
        }));
    }

    private static double Factorial(int value)
    {
        var result = 1.0;
        for (var i = 2; i <= value; i++)
        {
            result *= i;
        }

        return result;
    }
}
