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

        private const ExpressionType StartsWith = ExpressionType.LeftShift;
        private const ExpressionType EndsWith = ExpressionType.RightShift;
        private const ExpressionType Contains = ExpressionType.Unbox;
        private const ExpressionType CompareTo = ExpressionType.TypeEqual;

        private static HashSet<ExpressionType> StringLikeTypes = new HashSet<ExpressionType>
        {
            StartsWith,
            EndsWith,
            Contains
        };

        public static Dictionary<string, ExpressionType> StringMethods = new Dictionary<string, ExpressionType>
        {
            { "StartsWith", StartsWith},
            { "EndsWith", EndsWith},
            { "Contains", Contains},
            { "CompareTo", CompareTo}
        };

        /// <summary>
        /// Use this to instruct that the given operand doesn't have a string representation
        /// </summary>
        public static readonly object Nothing = new object();

        public object LeftOperand { get; set; }
        public ExpressionType Operator { get; set; }
        public object RightOperand { get; set; }
        public bool IsBooleanNot { get; set; }

        public IDictionary<string, object> ParameterValues { get; set; }

        public override string ToString()
        {
            ParameterCounter = 0;
            this.ParameterValues = new Dictionary<string, object>();

            return Convert.ToString(Stringify(this, this));
        }

        private static object Stringify(object levelSource, SqlExpression<T> topLevel)
        {
            if (!(levelSource is SqlExpression<T>))
            {
                return levelSource;
            }

            var expr = levelSource as SqlExpression<T>;
            var left = Stringify(expr.LeftOperand, topLevel);
            var right = Stringify(expr.RightOperand, topLevel);

            var fmt = (expr.Operator == ExpressionType.Add || expr.Operator == ExpressionType.Subtract) ?
                "{0} {1} {3}{2}{4}" : "({0} {1} {3}{2}{4})";

            return string.Format(fmt,
                GetExpressionString(left, topLevel),
                GetSqlOperator(expr.Operator, expr.IsBooleanNot),
                GetExpressionString(right, topLevel),
                (expr.Operator == EndsWith || expr.Operator == Contains) ? "'%' + " : "",
                (expr.Operator == StartsWith || expr.Operator == Contains) ? " + '%'" : "");
        }

        private static object GetSqlOperator(ExpressionType type, bool isBooleanNot)
        {
            if (StringLikeTypes.Contains(type))
            {
                return "LIKE";
            }

            switch (type)
            {
                case ExpressionType.Add: return "+";
                case ExpressionType.And: return "&";
                case ExpressionType.AndAlso: return "AND";
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
                case ExpressionType.Not: return isBooleanNot ? "NOT" : "~";
                case ExpressionType.NotEqual: return "<>";
                case ExpressionType.OnesComplement: return "~";
                case ExpressionType.Or: return "|";
                case ExpressionType.OrElse: return "OR";
                case ExpressionType.Subtract: return "-";
                case ExpressionType.UnaryPlus: return "+";
            }

            throw new NotSupportedException(string.Format("{0} isn't supported", type));
        }

        internal static ExpressionType GetMethodCallType(string name)
        {
            return StringMethods[name];
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
            if (ReferenceEquals(obj, Nothing))
            {
                return string.Empty;
            }

            if (obj is DbColumnAttribute)
            {
                return string.Format("[{0}]", (obj as DbColumnAttribute).ColumnName);
            }
            else if (ShouldBeParameter(obj))
            {
                return AddParameter(obj, source).Key;
            }

            return obj.ToString();
        }

        private static bool ShouldBeParameter(object obj)
        {
            return ReferenceEquals(obj, null) ||
                (
                    !NumericTypes.Contains(obj.GetType()) &&
                    !Regex.IsMatch(obj.ToString(), @"(\[|\(|@)")
                );
        }

        private static KeyValuePair<string, object> AddParameter(object val, SqlExpression<T> source)
        {
            var param = new KeyValuePair<string, object>(string.Format("@param_{0}", ParameterCounter++), val);
            source.ParameterValues.Add(param);

            return param;
        }
    }
}