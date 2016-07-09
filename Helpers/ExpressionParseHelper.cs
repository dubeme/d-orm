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
                var expr = expression.Body as MemberExpression;
            }
            else if (expression.Body is MethodCallExpression)
            {
                var expr = ParseMethodCallExpression(expression.Body as MethodCallExpression);
            }
            else if (expression.Body is LambdaExpression)
            {
                var expr = expression.Body as LambdaExpression;
            }

            return null;
        }

        private static object ParseExpression(Expression expr)
        {
            if (expr is BinaryExpression)
            {
                var binExpr = expr as BinaryExpression;
                return ParseBinaryExpression(binExpr.Left, binExpr.NodeType, binExpr.Right);
            }
            else if (expr is MethodCallExpression)
            {
                return ParseMethodCallExpression(expr as MethodCallExpression);
            }
            else if (expr is ConstantExpression)
            {
                return Evaluate(expr);
            }
            else if (expr is LambdaExpression)
            {
                // var expr = expression.Body as LambdaExpression;
            }
            else if (expr is MemberExpression)
            {
                // var expr = expression.Body as MemberExpression;
            }

            return null;
        }

        private static SqlExpression<T> ParseBinaryExpression(Expression left, ExpressionType type, Expression right)
        {
            return new SqlExpression<T>
            {
                LeftOperand = ParseExpression(left) ?? ExtractColumnInfo(left) ?? Evaluate(left),
                Operator = type,
                RightOperand = ParseExpression(right) ?? ExtractColumnInfo(right) ?? Evaluate(right)
            };
        }

        private static SqlExpression<T> ParseMethodCallExpression(MethodCallExpression method)
        {
            if (ReferenceEquals(method.Object, null))
            {
                throw new InvalidOperationException("This isn't a supported method.");
            }

            var parameter = ParseExpression(method.Arguments[0]);

            return new SqlExpression<T>
            {
                LeftOperand = ExtractColumnInfo(method.Object),
                Operator = SqlExpression<T>.GetMethodCallType(method.Method.Name),
                RightOperand = SqlExpression<T>.FormatForLike(method.Method.Name, parameter)
            };
        }

        private static DbColumnAttribute ExtractColumnInfo(Expression expr)
        {
            if (!(expr is MemberExpression))
                return null;

            var member = expr as MemberExpression;
            var columnAttribute = member.Member.GetCustomAttributes(typeof(DbColumnAttribute), true);
            return columnAttribute.FirstOrDefault() as DbColumnAttribute;
        }

        private static object Evaluate(Expression expr)
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

        private static bool IsNumericType(Type type)
        {
            return NumericTypes.Contains(type) ||
                   NumericTypes.Contains(Nullable.GetUnderlyingType(type));
        }
    }
}