using System.Text;

namespace Tack.Utils;
internal sealed class StringOperator
{
    private readonly StringBuilder _sb = new();

    public char this[int index] { get { return _sb[index]; }  set { _sb[index] = value; } }
    public char this[Index index] { get { return _sb[index]; } set { _sb[index] = value; } }

    public override string ToString()
    {
        return _sb.ToString();
    }

    private static StringOperator Append(StringOperator op, string str)
    {
        _ = op._sb.Append(str);
        return op;
    }
    private static StringOperator Append(StringOperator op, char ch)
    {
        _ = op._sb.Append(ch);
        return op;
    }

    public static StringOperator operator %(StringOperator op, string str) => Append(op, str);
    public static StringOperator operator %(StringOperator op, StringOperator op2) => Append(op, op2.ToString());
    public static StringOperator operator %(StringOperator op, char ch) => Append(op, ch);
    public static StringOperator operator ^(bool cond, StringOperator op) => cond ? op : string.Empty;
    public static implicit operator string(StringOperator op) => op.ToString();
    public static implicit operator StringOperator(string str)
    {
        var op = new StringOperator();
        _ = op._sb.Append(str);
        return op;
    }
}
