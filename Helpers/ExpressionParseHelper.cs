using DamnORM.Model.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace DamnORM.Model
{
    [Serializable]
    public static class ExpressionParseHelper<T>
    {
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

        public static SqlExpression<T> Parse(Expression<Func<T, bool>> expression)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.Body is BinaryExpression)
            {
                var expr = expression.Body as BinaryExpression;

                return ParseBinaryExpression(expr.Left, expr.NodeType, expr.Right);
            }
            else if (expression.Body is MemberExpression)
            {
            }

            return null;
        }

        private static bool IsNumericType(Type type)
        {
            return NumericTypes.Contains(type) ||
                   NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }

        private static SqlExpression<T> ParseBinaryExpression(Expression left, ExpressionType type, Expression right)
        {
            object _left = null;
            object _right = null;

            if (left is BinaryExpression)
            {
                var expr = left as BinaryExpression;
                _left = ParseBinaryExpression(expr.Left, expr.NodeType, expr.Right);
            }

            if (right is BinaryExpression)
            {
                var expr = right as BinaryExpression;
                _right = ParseBinaryExpression(expr.Left, expr.NodeType, expr.Right);
            }

            return new SqlExpression<T>
            {
                LeftOperand = _left ?? GetValue(left),
                Operator = type,
                RightOperand = _right ?? GetValue(right)
            };
        }

        private static DbColumnAttribute GetAsColumnAttribute(Expression expr)
        {
            if (!(expr is MemberExpression))
                return null;

            var member = expr as MemberExpression;
            var columnAttribute = member.Member.GetCustomAttributes(typeof(DbColumnAttribute), true);
            return columnAttribute.FirstOrDefault() as DbColumnAttribute;
        }

        private static object GetAsLiteralValue(Expression expr)
        {
            // http://stackoverflow.com/a/2616980
            var objectMember = Expression.Convert(expr, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();

            if (IsNumericType(expr.Type))
            {
                return getter();
            }

            return string.Format("'{0}'", getter());
        }

        private static object GetAsMethodCall(Expression expr)
        {
            if (!(expr is MethodCallExpression))
            {
                return null;
            }

            var methodExpr = expr as MethodCallExpression;
            return methodExpr;
        }

        private static object GetValue(Expression expr)
        {
            return GetAsColumnAttribute(expr) ??
                GetAsMethodCall(expr) ??
                GetAsLiteralValue(expr);
        }
    }
}