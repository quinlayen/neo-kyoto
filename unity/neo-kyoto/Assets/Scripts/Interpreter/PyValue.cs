using System.Globalization;

namespace NeoKyoto.Interpreter
{
    /// <summary>
    /// Value semantics for the interpreter. Integers are long, floats are double,
    /// text is string, booleans are bool, and None is null — matching what the
    /// Python prototype produced so contract output reads identically.
    /// </summary>
    public static class PyValue
    {
        public static string Str(object v)
        {
            if (v == null) return "None";
            if (v is bool) return ((bool)v) ? "True" : "False";
            if (v is double)
            {
                double d = (double)v;
                if (d == (long)d) return ((long)d).ToString(CultureInfo.InvariantCulture) + ".0";
                return d.ToString("0.################", CultureInfo.InvariantCulture);
            }
            if (v is long) return ((long)v).ToString(CultureInfo.InvariantCulture);
            return v.ToString();
        }

        public static bool Truthy(object v)
        {
            if (v == null) return false;
            if (v is bool) return (bool)v;
            if (v is long) return (long)v != 0;
            if (v is double) return (double)v != 0.0;
            if (v is string) return ((string)v).Length > 0;
            return true;
        }

        public static bool IsNumber(object v) { return v is long || v is double; }

        public static double ToDouble(object v)
        {
            if (v is long) return (long)v;
            if (v is double) return (double)v;
            if (v is bool) return ((bool)v) ? 1 : 0;
            return 0;
        }

        public static bool AreEqual(object a, object b)
        {
            if (a == null || b == null) return a == null && b == null;
            if (a is string || b is string)
            {
                if (!(a is string) || !(b is string)) return false;
                return (string)a == (string)b;
            }
            if (a is bool && b is bool) return (bool)a == (bool)b;
            if (IsNumber(a) && IsNumber(b)) return ToDouble(a) == ToDouble(b);
            if (a is bool || b is bool) return ToDouble(a) == ToDouble(b);
            return a.Equals(b);
        }

        public static string TypeName(object v)
        {
            if (v == null) return "None";
            if (v is bool) return "bool";
            if (v is long) return "int";
            if (v is double) return "float";
            if (v is string) return "str";
            return v.GetType().Name;
        }
    }
}
