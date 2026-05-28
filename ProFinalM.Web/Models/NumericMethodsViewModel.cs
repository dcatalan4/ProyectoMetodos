using System.ComponentModel.DataAnnotations;

namespace ProFinalM.Web.Models;

public class NumericMethodsViewModel
{
    public TaylorInput Taylor { get; set; } = new();
    public TaylorResult? TaylorResult { get; set; }

    public LeastSquaresInput LeastSquares { get; set; } = new();
    public LeastSquaresResult? LeastSquaresResult { get; set; }

    public string? ErrorMessage { get; set; }
}

public class TaylorInput
{
    [Display(Name = "Funcion f(x)")]
    public string Function { get; set; } = "sin(x)";

    [Display(Name = "Punto de expansion a")]
    public double ExpansionPoint { get; set; } = 0;

    [Display(Name = "Orden n")]
    public int Order { get; set; } = 5;

    [Display(Name = "Paso h")]
    public double Step { get; set; } = 0.5;
}

public class TaylorResult
{
    public string Polynomial { get; set; } = string.Empty;
    public double EvaluationPoint { get; set; }
    public double RealValue { get; set; }
    public double ApproximateValue { get; set; }
    public double AbsoluteError { get; set; }
    public double PercentageError { get; set; }
    public List<TaylorTerm> Terms { get; set; } = [];
    public List<ChartPoint> RealCurve { get; set; } = [];
    public List<ChartPoint> TaylorCurve { get; set; } = [];
}

public class TaylorTerm
{
    public int Order { get; set; }
    public string Derivative { get; set; } = string.Empty;
    public double DerivativeAtA { get; set; }
    public double Coefficient { get; set; }
}

public class LeastSquaresInput
{
    [Display(Name = "Datos x,y")]
    public string Points { get; set; } = "1,60\n2,65\n3,70\n4,80";
}

public class LeastSquaresResult
{
    public double Slope { get; set; }
    public double Intercept { get; set; }
    public double Correlation { get; set; }
    public double Determination { get; set; }
    public double SumX { get; set; }
    public double SumY { get; set; }
    public double SumXY { get; set; }
    public double SumX2 { get; set; }
    public double SumY2 { get; set; }
    public List<DataPoint> Points { get; set; } = [];
    public List<ChartPoint> Line { get; set; } = [];
}

public class DataPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public double XY => X * Y;
    public double X2 => X * X;
    public double Y2 => Y * Y;
}

public class ChartPoint
{
    public double X { get; set; }
    public double Y { get; set; }
}
