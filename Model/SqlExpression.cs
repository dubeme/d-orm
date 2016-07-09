using DamnORM.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace DamnORM.Model
{
    [Serializable]
    public class SqlExpression<T>
    {
        private static int ParameterCounter = 0;

        private static HashSet<string> LikeMethods = new HashSet<string>
        {
            "StartsWith",
            "EndsWith",
            "Contains"
        };

        private static HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(bool),
            typeof(byte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long),
            typeof(sbyte),
            typeof(short),
            typeof(uint),
            typeof(ulong),
            typeof(ushort)
        };

        public static Dictionary<string, ExpressionType> StringMethods = new Dictionary<string, ExpressionType>
        {
            { "StartsWith", ExpressionType.LeftShift },
            { "EndsWith", ExpressionType.RightShift },
            { "Contains", ExpressionType.Unbox },
            { "CompareTo", ExpressionType.TypeEqual }
        };

        public object LeftOperand { get; set; }
        public ExpressionType Operator { get; set; }
        public object RightOperand { get; set; }

        public IDictionary<string, object> ParameterValues { get; set; }

        public override string ToString()
        {
            this.ParameterValues = new Dictionary<string, object>();
            return Stringify(this.LeftOperand, this.Operator, this.RightOperand, this);
        }

        private static string Stringify(object left, ExpressionType type, object right, SqlExpression<T> source)
        {
            if (left is SqlExpression<T>)
            {
                var expr = left as SqlExpression<T>;
                left = Stringify(expr.LeftOperand, expr.Operator, expr.RightOperand, source);
            }

            if (right is SqlExpression<T>)
            {
                var expr = right as SqlExpression<T>;
                right = Stringify(expr.LeftOperand, expr.Operator, expr.RightOperand, source);
            }

            return string.Format("({0} {1} {2})",
                GetExpressionString(left, source),
                GetSqlOperator(type),
                GetExpressionString(right, source));
        }

        private static object GetSqlOperator(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.Add: return "+";
                case ExpressionType.And: return "&";
                case ExpressionType.AndAlso: return "and";
                case ExpressionType.Decrement: return "-";
                case ExpressionType.Divide: return "/";
                case ExpressionType.Equal: return "=";
                case ExpressionType.ExclusiveOr: return "^";
                case ExpressionType.GreaterThan: return ">";
                case ExpressionType.GreaterThanOrEqual: return ">=";
                case ExpressionType.Increment: return "+";
                case ExpressionType.LessThan: return "<";
                case ExpressionType.LessThanOrEqual: return "<=";
                case ExpressionType.Modulo: return "%";
                case ExpressionType.Multiply: return "*";
                case ExpressionType.Negate: return "-";
                case ExpressionType.Not: return "~";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.OnesComplement: return "~";
                case ExpressionType.Or: return "|";
                case ExpressionType.OrElse: return "or";
                case ExpressionType.Subtract: return "-";
                case ExpressionType.UnaryPlus: return "+";
                case ExpressionType.Unbox: return "LIKE";
            }

            throw new System.NotSupportedException(string.Format("{0} isn't supported", type));
        }

        internal static ExpressionType GetMethodCallType(string name)
        {
            return LikeMethods.Contains(name) ? ExpressionType.Unbox : ExpressionType.Try;
        }

        internal static object FormatForLike(string name, object val)
        {
            if (name == "StartsWith")
            {
                return string.Format("{0}%", val);
            }
            if (name == "EndsWith")
            {
                return string.Format("%{0}", val);
            }
            if (name == "Contains")
            {
                return string.Format("%{0}%", val);
            }
            if (name == "CompareTo")
            {
            }

            return val;
        }

        private static string GetExpressionString(object obj, SqlExpression<T> source)
        {
            if (obj is DbColumnAttribute)
            {
                return string.Format("[{0}]", (obj as DbColumnAttribute).ColumnName);
            }
            else if (Regex.IsMatch(obj.ToString(), @"(\[|\()") == false)
            {
                var param = new KeyValuePair<string, object>(string.Format("@param_{0}", ParameterCounter++), obj);
                source.ParameterValues.Add(param);

                return param.Key;
            }

            return obj.ToString();
        }
    }
}