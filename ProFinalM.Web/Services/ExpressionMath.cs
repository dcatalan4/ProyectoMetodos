using System.Globalization;

namespace ProFinalM.Web.Services;

public sealed class ExpressionMath
{
    public MathExpression Parse(string text)
    {
        var parser = new Parser(text);
        return new MathExpression(parser.Parse());
    }

    private sealed class Parser(string text)
    {
        private int _index;

        public ExprNode Parse()
        {
            var expression = ParseExpression();
            SkipWhiteSpace();
            if (_index != text.Length)
            {
                throw new InvalidOperationException($"No se pudo interpretar desde '{text[_index..]}'.");
            }

            return expression;
        }

        private ExprNode ParseExpression()
        {
            var node = ParseTerm();
            while (true)
            {
                SkipWhiteSpace();
                if (Match('+'))
                {
                    node = new BinaryNode("+", node, ParseTerm());
                }
                else if (Match('-'))
                {
                    node = new BinaryNode("-", node, ParseTerm());
                }
                else
                {
                    return node;
                }
            }
        }

        private ExprNode ParseTerm()
        {
            var node = ParsePower();
            while (true)
            {
                SkipWhiteSpace();
                if (Match('*'))
                {
                    node = new BinaryNode("*", node, ParsePower());
                }
                else if (Match('/'))
                {
                    node = new BinaryNode("/", node, ParsePower());
                }
                else
                {
                    return node;
                }
            }
        }

        private ExprNode ParsePower()
        {
            var node = ParseUnary();
            SkipWhiteSpace();
            if (Match('^'))
            {
                node = new BinaryNode("^", node, ParsePower());
            }

            return node;
        }

        private ExprNode ParseUnary()
        {
            SkipWhiteSpace();
            if (Match('+'))
            {
                return ParseUnary();
            }

            if (Match('-'))
            {
                return new UnaryNode("-", ParseUnary());
            }

            return ParsePrimary();
        }

        private ExprNode ParsePrimary()
        {
            SkipWhiteSpace();
            if (Match('('))
            {
                var node = ParseExpression();
                Expect(')');
                return node;
            }

            if (CurrentIsNumber())
            {
                return ParseNumber();
            }

            if (CurrentIsIdentifier())
            {
                var identifier = ParseIdentifier().ToLowerInvariant();
                if (identifier == "x")
                {
                    return new VariableNode();
                }

                if (identifier == "pi")
                {
                    return new ConstantNode(Math.PI);
                }

                if (identifier == "e")
                {
                    return new ConstantNode(Math.E);
                }

                Expect('(');
                var argument = ParseExpression();
                Expect(')');
                return new FunctionNode(identifier, argument);
            }

            throw new InvalidOperationException("La funcion contiene un simbolo no reconocido.");
        }

        private ConstantNode ParseNumber()
        {
            var start = _index;
            while (_index < text.Length && (char.IsDigit(text[_index]) || text[_index] == '.'))
            {
                _index++;
            }

            var valueText = text[start.._index];
            var value = double.Parse(valueText, CultureInfo.InvariantCulture);
            return new ConstantNode(value);
        }

        private string ParseIdentifier()
        {
            var start = _index;
            while (_index < text.Length && char.IsLetter(text[_index]))
            {
                _index++;
            }

            return text[start.._index];
        }

        private bool CurrentIsNumber() => _index < text.Length && (char.IsDigit(text[_index]) || text[_index] == '.');
        private bool CurrentIsIdentifier() => _index < text.Length && char.IsLetter(text[_index]);

        private bool Match(char expected)
        {
            SkipWhiteSpace();
            if (_index >= text.Length || text[_index] != expected)
            {
                return false;
            }

            _index++;
            return true;
        }

        private void Expect(char expected)
        {
            if (!Match(expected))
            {
                throw new InvalidOperationException($"Se esperaba '{expected}'.");
            }
        }

        private void SkipWhiteSpace()
        {
            while (_index < text.Length && char.IsWhiteSpace(text[_index]))
            {
                _index++;
            }
        }
    }
}

public sealed class MathExpression(ExprNode root)
{
    public double Evaluate(double x) => root.Evaluate(x);
    public MathExpression Derivative() => new(root.Differentiate());
    public override string ToString() => root.Format();
}

public abstract class ExprNode
{
    public abstract double Evaluate(double x);
    public abstract ExprNode Differentiate();
    public abstract string Format();
}

public sealed class ConstantNode(double value) : ExprNode
{
    public double Value { get; } = value;
    public override double Evaluate(double x) => Value;
    public override ExprNode Differentiate() => new ConstantNode(0);
    public override string Format() => Value.ToString("0.####", CultureInfo.InvariantCulture);
}

public sealed class VariableNode : ExprNode
{
    public override double Evaluate(double x) => x;
    public override ExprNode Differentiate() => new ConstantNode(1);
    public override string Format() => "x";
}

public sealed class UnaryNode(string op, ExprNode value) : ExprNode
{
    public override double Evaluate(double x) => op == "-" ? -value.Evaluate(x) : value.Evaluate(x);
    public override ExprNode Differentiate() => new UnaryNode(op, value.Differentiate());
    public override string Format() => $"-({value.Format()})";
}

public sealed class BinaryNode(string op, ExprNode left, ExprNode right) : ExprNode
{
    public override double Evaluate(double x)
    {
        var a = left.Evaluate(x);
        var b = right.Evaluate(x);
        return op switch
        {
            "+" => a + b,
            "-" => a - b,
            "*" => a * b,
            "/" => a / b,
            "^" => Math.Pow(a, b),
            _ => throw new InvalidOperationException("Operador no soportado.")
        };
    }

    public override ExprNode Differentiate()
    {
        return op switch
        {
            "+" => new BinaryNode("+", left.Differentiate(), right.Differentiate()),
            "-" => new BinaryNode("-", left.Differentiate(), right.Differentiate()),
            "*" => new BinaryNode("+", new BinaryNode("*", left.Differentiate(), right), new BinaryNode("*", left, right.Differentiate())),
            "/" => new BinaryNode("/", new BinaryNode("-", new BinaryNode("*", left.Differentiate(), right), new BinaryNode("*", left, right.Differentiate())), new BinaryNode("^", right, new ConstantNode(2))),
            "^" when right is ConstantNode c => new BinaryNode("*", new BinaryNode("*", new ConstantNode(c.Value), new BinaryNode("^", left, new ConstantNode(c.Value - 1))), left.Differentiate()),
            "^" => new BinaryNode("*", new BinaryNode("^", left, right), new BinaryNode("+", new BinaryNode("*", right.Differentiate(), new FunctionNode("ln", left)), new BinaryNode("*", right, new BinaryNode("/", left.Differentiate(), left)))),
            _ => throw new InvalidOperationException("Operador no soportado.")
        };
    }

    public override string Format() => $"({left.Format()} {op} {right.Format()})";
}

public sealed class FunctionNode(string name, ExprNode argument) : ExprNode
{
    public override double Evaluate(double x)
    {
        var value = argument.Evaluate(x);
        return name switch
        {
            "sin" => Math.Sin(value),
            "cos" => Math.Cos(value),
            "tan" => Math.Tan(value),
            "exp" => Math.Exp(value),
            "ln" or "log" => Math.Log(value),
            "sqrt" => Math.Sqrt(value),
            _ => throw new InvalidOperationException($"Funcion '{name}' no soportada.")
        };
    }

    public override ExprNode Differentiate()
    {
        var inner = argument.Differentiate();
        ExprNode outer = name switch
        {
            "sin" => new FunctionNode("cos", argument),
            "cos" => new UnaryNode("-", new FunctionNode("sin", argument)),
            "tan" => new BinaryNode("/", new ConstantNode(1), new BinaryNode("^", new FunctionNode("cos", argument), new ConstantNode(2))),
            "exp" => new FunctionNode("exp", argument),
            "ln" or "log" => new BinaryNode("/", new ConstantNode(1), argument),
            "sqrt" => new BinaryNode("/", new ConstantNode(1), new BinaryNode("*", new ConstantNode(2), new FunctionNode("sqrt", argument))),
            _ => throw new InvalidOperationException($"Funcion '{name}' no soportada.")
        };

        return new BinaryNode("*", outer, inner);
    }

    public override string Format() => $"{name}({argument.Format()})";
}
