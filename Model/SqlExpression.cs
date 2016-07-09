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
            typeof(ushort),
            typeof(UInt16),
            typeof(UInt32),
            typeof(UInt64),
            typeof(Int16),
            typeof(Int32),
            typeof(Int64),
            typeof(Single)
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
            }

            throw new System.NotSupportedException(string.Format("{0} isn't supported", type));
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