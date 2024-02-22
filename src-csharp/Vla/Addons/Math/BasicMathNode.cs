using Vla.Addon;

namespace Vla.Addons.Math;

[Node]
[NodeCategory("Math")]
[NodeTags("Math", "Arithmetic", "Trigonometry", "Rounding", "Conversion", "Comparison")]
public class MathNode : Node
{
    public override string Name => $"Math {Mode.GetValueName()}";
    
    [NodeProperty]
    public MathMode Mode { get; set; } = MathMode.Add;

    public override Task Execute()
    {
        if (Mode == MathMode.Add)
            Add();
        else if (Mode == MathMode.Subtract)
            Subtract();
        else if (Mode == MathMode.Multiply)
            Multiply();
        else if (Mode == MathMode.Divide)
            Divide();
        else if (Mode == MathMode.Modulo)
            Modulo();
        else if (Mode == MathMode.Power)
            Power();
        else if (Mode == MathMode.Logarithm)
            Logarithm();
        else if (Mode == MathMode.SquareRoot)
            SquareRoot();
        else if (Mode == MathMode.InverseSquareRoot)
            InverseSquareRoot();
        else if (Mode == MathMode.Absolute)
            Absolute();
        else if (Mode == MathMode.Exponent)
            Exponent();
        else if (Mode == MathMode.Minimum)
            Minimum();
        else if (Mode == MathMode.Maximum)
            Maximum();
        else if (Mode == MathMode.LessThan)
            LessThan();
        else if (Mode == MathMode.GreaterThan)
            GreaterThan();
        else if (Mode == MathMode.Sign)
            Sign();
        else if (Mode == MathMode.Compare)
            Compare();
        else if (Mode == MathMode.SmoothMinimum)
            SmoothMinimum();
        else if (Mode == MathMode.SmoothMaximum)
            SmoothMaximum();
        else if (Mode == MathMode.Round)
            Round();
        else if (Mode == MathMode.Floor)
            Floor();
        else if (Mode == MathMode.Ceil)
            Ceil();
        else if (Mode == MathMode.Truncate)
            Truncate();
        else if (Mode == MathMode.Fraction)
            Fraction();
        else if (Mode == MathMode.Wrap)
            Wrap();
        else if (Mode == MathMode.Snap)
            Snap();
        else if (Mode == MathMode.PingPong)
            PingPong();
        else if (Mode == MathMode.Sine)
            Sine();
        else if (Mode == MathMode.Cosine)
            Cosine();
        else if (Mode == MathMode.Tangent)
            Tangent();
        else if (Mode == MathMode.ArcSine)
            ArcSine();
        else if (Mode == MathMode.ArcCosine)
            ArcCosine();
        else if (Mode == MathMode.ArcTangent)
            ArcTangent();
        else if (Mode == MathMode.ArcTangent2)
            ArcTangent2();
        else if (Mode == MathMode.HyperbolicSine)
            HyperbolicSine();
        else if (Mode == MathMode.HyperbolicCosine)
            HyperbolicCosine();
        else if (Mode == MathMode.HyperbolicTangent)
            HyperbolicTangent();
        else if (Mode == MathMode.ToRadians)
            ToRadians();
        else if (Mode == MathMode.ToDegrees)
            ToDegrees();
        else
            throw new ArgumentOutOfRangeException();

        return Task.CompletedTask;
    }

    private void Add()
    {
        var a = Input<double>("add.a", "Value", 0);
        var b = Input<double>("add.b", "Value", 0);
        
        var result = a + b;
        
        Output("add.result", "Value", result);
    }
    
    private void Subtract()
    {
        var a = Input<double>("subtract.a", "Value", 0);
        var b = Input<double>("subtract.b", "Value", 0);
        
        var result = a - b;
        
        Output("subtract.result", "Value", result);
    }
    
    private void Multiply()
    {
        var a = Input<double>("multiply.a", "Value", 0);
        var b = Input<double>("multiply.b", "Value", 0);
        
        var result = a * b;
        
        Output("multiply.result", "Value", result);
    }
    
    private void Divide()
    {
        var a = Input<double>("divide.a", "Value", 0);
        var b = Input<double>("divide.b", "Value", 0);
        
        var result = a / b;
        
        Output("divide.result", "Value", result);
    }
    
    private void Modulo()
    {
        var a = Input<double>("modulo.a", "Value", 0);
        var b = Input<double>("modulo.b", "Value", 0);
        
        var result = a % b;
        
        Output("modulo.result", "Value", result);
    }
    
    private void Power()
    {
        var a = Input<double>("power.base", "Base", 0);
        var b = Input<double>("power.exponent", "Exponent", 1);
        
        var result = System.Math.Pow(a, b);

        Console.WriteLine(result);
        
        Output("power.result", "Value", result);
    }
    
    private void Logarithm()
    {
        var a = Input<double>("logarithm.a", "Value", 0);
        var b = Input<double>("logarithm.b", "Base", 0);
        
        var result = System.Math.Log(a, b);
        
        Output("logarithm.result", "Value", result);
    }
    
    private void SquareRoot()
    {
        var a = Input<double>("squareRoot.a", "Value", 0);
        
        var result = System.Math.Sqrt(a);
        
        Output("squareRoot.result", "Value", result);
    }
    
    private void InverseSquareRoot()
    {
        var a = Input<double>("inverseSquareRoot.a", "Value", 0);
        
        var result = 1 / System.Math.Sqrt(a);
        
        Output("inverseSquareRoot.result", "Value", result);
    }
    
    private void Absolute()
    {
        var a = Input<double>("absolute.a", "Value", 0);
        
        var result = System.Math.Abs(a);
        
        Output("absolute.result", "Value", result);
    }
    
    private void Exponent()
    {
        var a = Input<double>("exponent.a", "Value", 0);
        
        var result = System.Math.Exp(a);
        
        Output("exponent.result", "Value", result);
    }
    
    private void Minimum()
    {
        var a = Input<double>("minimum.a", "Value", 0);
        var b = Input<double>("minimum.b", "Value", 0);
        
        var result = System.Math.Min(a, b);
        
        Output("minimum.result", "Value", result);
    }
    
    private void Maximum()
    {
        var a = Input<double>("maximum.a", "Value", 0);
        var b = Input<double>("maximum.b", "Value", 0);
        
        var result = System.Math.Max(a, b);
        
        Output("maximum.result", "Value", result);
    }
    
    private void LessThan()
    {
        var a = Input<double>("lessThan.a", "Value", 0);
        var b = Input<double>("lessThan.b", "Value", 0);
        
        var result = a < b;
        
        Output("lessThan.result", "Value", result);
    }
    
    private void GreaterThan()
    {
        var a = Input<double>("greaterThan.a", "Value", 0);
        var b = Input<double>("greaterThan.b", "Value", 0);
        
        var result = a > b;
        
        Output("greaterThan.result", "Value", result);
    }
    
    private void Sign()
    {
        var a = Input<double>("sign.a", "Value", 0);
        
        var result = System.Math.Sign(a);
        
        Output("sign.result", "Value", result);
    }
    
    private void Compare()
    {
        var a = Input<double>("compare.a", "Value", 0);
        var b = Input<double>("compare.b", "Value", 0);
        var epsilon = Input<double>("compare.epsilon", "Epsilon", 0);
        
        var result = System.Math.Abs(a - b) < epsilon;
        
        Output("compare.result", "Value", result);
    }
    
    private void SmoothMinimum()
    {
        var a = Input<double>("smoothMinimum.a", "Value", 0);
        var b = Input<double>("smoothMinimum.b", "Value", 0);
        var k = Input<double>("smoothMinimum.k", "K", 0);
        
        var result = System.Math.Min(a, b) - k * System.Math.Pow(a - b, 2);
        
        Output("smoothMinimum.result", "Value", result);
    }
    
    private void SmoothMaximum()
    {
        var a = Input<double>("smoothMaximum.a", "Value", 0);
        var b = Input<double>("smoothMaximum.b", "Value", 0);
        var k = Input<double>("smoothMaximum.k", "K", 0);
        
        var result = System.Math.Max(a, b) + k * System.Math.Pow(a - b, 2);
        
        Output("smoothMaximum.result", "Value", result);
    }
    
    private void Round()
    {
        var a = Input<double>("round.a", "Value", 0);
        
        var result = System.Math.Round(a);
        
        Output("round.result", "Value", result);
    }
    
    private void Floor()
    {
        var a = Input<double>("floor.a", "Value", 0);
        
        var result = System.Math.Floor(a);
        
        Output("floor.result", "Value", result);
    }
    
    private void Ceil()
    {
        var a = Input<double>("ceil.a", "Value", 0);
        
        var result = System.Math.Ceiling(a);
        
        Output("ceil.result", "Value", result);
    }
    
    private void Truncate()
    {
        var a = Input<double>("truncate.a", "Value", 0);
        
        var result = System.Math.Truncate(a);
        
        Output("truncate.result", "Value", result);
    }
    
    private void Fraction()
    {
        var a = Input<double>("fraction.a", "Value", 0);
        
        var result = a - System.Math.Floor(a);
        
        Output("fraction.result", "Value", result);
    }
    
    private void Wrap()
    {
        var a = Input<double>("wrap.a", "Value", 0);
        var min = Input<double>("wrap.min", "Min", 0);
        var max = Input<double>("wrap.max", "Max", 0);
   
        var result = System.Math.Min(max, System.Math.Max(min, a));
        
        Output("wrap.result", "Value", result);
    }
    
    private void Snap()
    {
        var a = Input<double>("snap.a", "Value", 0);
        var step = Input<double>("snap.step", "Step", 0);
        
        var result = System.Math.Round(a / step) * step;
        
        Output("snap.result", "Value", result);
    }
    
    private void PingPong()
    {
        var a = Input<double>("pingPong.a", "Value", 0);
        var length = Input<double>("pingPong.length", "Length", 0);
        
        var result = length - System.Math.Abs(System.Math.IEEERemainder(a, length * 2) - length);
        
        Output("pingPong.result", "Value", result);
    }
    
    private void Sine()
    {
        var a = Input<double>("sine.a", "Value", 0);
        
        var result = System.Math.Sin(a);
        
        Output("sine.result", "Value", result);
    }
    
    private void Cosine()
    {
        var a = Input<double>("cosine.a", "Value", 0);
        
        var result = System.Math.Cos(a);
        
        Output("cosine.result", "Value", result);
    }
    
    private void Tangent()
    {
        var a = Input<double>("tangent.a", "Value", 0);
        
        var result = System.Math.Tan(a);
        
        Output("tangent.result", "Value", result);
    }
    
    private void ArcSine()
    {
        var a = Input<double>("arcSine.a", "Value", 0);
        
        var result = System.Math.Asin(a);
        
        Output("arcSine.result", "Value", result);
    }
    
    private void ArcCosine()
    {
        var a = Input<double>("arcCosine.a", "Value", 0);
        
        var result = System.Math.Acos(a);
        
        Output("arcCosine.result", "Value", result);
    }
    
    private void ArcTangent()
    {
        var a = Input<double>("arcTangent.a", "Value", 0);
        
        var result = System.Math.Atan(a);
        
        Output("arcTangent.result", "Value", result);
    }
    
    private void ArcTangent2()
    {
        var a = Input<double>("arcTangent2.a", "Y", 0);
        var b = Input<double>("arcTangent2.b", "X", 0);
        
        var result = System.Math.Atan2(a, b);
        
        Output("arcTangent2.result", "Value", result);
    }
    
    private void HyperbolicSine()
    {
        var a = Input<double>("hyperbolicSine.a", "Value", 0);
        
        var result = System.Math.Sinh(a);
        
        Output("hyperbolicSine.result", "Value", result);
    }
    
    private void HyperbolicCosine()
    {
        var a = Input<double>("hyperbolicCosine.a", "Value", 0);
        
        var result = System.Math.Cosh(a);
        
        Output("hyperbolicCosine.result", "Value", result);
    }
    
    private void HyperbolicTangent()
    {
        var a = Input<double>("hyperbolicTangent.a", "Value", 0);
        
        var result = System.Math.Tanh(a);
        
        Output("hyperbolicTangent.result", "Value", result);
    }
    
    private void ToRadians()
    {
        var a = Input<double>("toRadians.a", "Degrees", 0);
        
        var result = System.Math.PI / 180 * a;
        
        Output("toRadians.result", "Radians", result);
    }
    
    private void ToDegrees()
    {
        var a = Input<double>("toDegrees.a", "Radians", 0);
        
        var result = 180 / System.Math.PI * a;
        
        Output("toDegrees.result", "Degrees", result);
    }
    
    public enum MathMode
    {
        [NodeEnumValue("Add", "Arithmetic")] Add,
        [NodeEnumValue("Subtract", "Arithmetic")] Subtract,
        [NodeEnumValue("Multiply", "Arithmetic")] Multiply,
        [NodeEnumValue("Divide", "Arithmetic")] Divide,
        [NodeEnumValue("Modulo", "Arithmetic")] Modulo,
        
        [NodeEnumValue("Power", "Functions")] Power,
        [NodeEnumValue("Logarithm", "Functions")] Logarithm,
        [NodeEnumValue("Square root", "Functions")] SquareRoot,
        [NodeEnumValue("Inverse square root", "Functions")] InverseSquareRoot,
        [NodeEnumValue("Absolute", "Functions")] Absolute,
        [NodeEnumValue("Exponent", "Functions")] Exponent,
        
        [NodeEnumValue("Minimum", "Comparison")] Minimum,
        [NodeEnumValue("Maximum", "Comparison")] Maximum,
        [NodeEnumValue("Less than", "Comparison")] LessThan,
        [NodeEnumValue("Greater than", "Comparison")] GreaterThan,
        [NodeEnumValue("Sign", "Comparison")] Sign,
        [NodeEnumValue("Compare", "Comparison")] Compare,
        [NodeEnumValue("Smooth minimum", "Comparison")] SmoothMinimum,
        [NodeEnumValue("Smooth maximum", "Comparison")] SmoothMaximum,
        
        [NodeEnumValue("Round", "Rounding")] Round,
        [NodeEnumValue("Floor", "Rounding")] Floor,
        [NodeEnumValue("Ceil", "Rounding")] Ceil,
        [NodeEnumValue("Truncate", "Rounding")] Truncate,
        
        [NodeEnumValue("Fraction", "Rounding")] Fraction,
        [NodeEnumValue("Wrap", "Rounding")] Wrap,
        [NodeEnumValue("Snap", "Rounding")] Snap,
        [NodeEnumValue("Ping-Pong", "Rounding")] PingPong,
        
        [NodeEnumValue("Sine", "Trigonometry")] Sine,
        [NodeEnumValue("Cosine", "Trigonometry")] Cosine,
        [NodeEnumValue("Tangent", "Trigonometry")] Tangent,
        [NodeEnumValue("Arcsine", "Trigonometry")] ArcSine,
        [NodeEnumValue("Arccosine", "Trigonometry")] ArcCosine,
        [NodeEnumValue("Arctangent", "Trigonometry")] ArcTangent,
        [NodeEnumValue("Arctangent 2", "Trigonometry")] ArcTangent2,
        [NodeEnumValue("Hyperbolic sine", "Trigonometry")] HyperbolicSine,
        [NodeEnumValue("Hyperbolic cosine", "Trigonometry")] HyperbolicCosine,
        [NodeEnumValue("Hyperbolic tangent", "Trigonometry")] HyperbolicTangent,
        
        [NodeEnumValue("To radians", "Conversion")] ToRadians,
        [NodeEnumValue("To degrees", "Conversion")] ToDegrees,
    }
}