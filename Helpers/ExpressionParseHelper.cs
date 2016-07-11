using DamnORM.Model;
using DamnORM.Model.Attributes;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DamnORM.Helpers
{
    /// <summary>
    /// Helper class for parsing a <see cref="Expression"/> into a <see cref="SqlExpression{T}"/> 
    /// </summary>
    /// <typeparam name="T">The type for the table column being queried.</typeparam>
    [Serializable]
    public static class ExpressionParseHelper<T>
    {
        /// <summary>
        /// Parses the specified <see cref="Expression"/> into a <see cref="SqlExpression{T}"/>.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>A <see cref="SqlExpression{T}"/></returns>
        public static SqlExpression<T> Parse(Expression<Func<T, bool>> expr)
        {
            if (ReferenceEquals(expr, null))
            {
                return null;
            }

            if (expr.Body is BinaryExpression)
            {
                return ParseAsBinaryExpression(expr.Body as BinaryExpression);
            }
            else if (expr.Body is MethodCallExpression)
            {
                return ParseAsMethodCall(expr.Body as MethodCallExpression);
            }
            else if (expr.Body is UnaryExpression)
            {
                return ParseAsUnary(expr.Body as UnaryExpression);
            }

            return null;
        }

        /// <summary>
        /// Parses an <see cref="Expression"/> into the appropriate value/object.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>The value/object yielded from the <see cref="Expression"/></returns>
        private static object ParseExpression(Expression expr)
        {
            if (expr is ConstantExpression)
            {
                return Evaluate(expr);
            }
            else if (expr is BinaryExpression)
            {
                return ParseAsBinaryExpression(expr as BinaryExpression);
            }
            else if (expr is MethodCallExpression)
            {
                return ParseAsMethodCall(expr as MethodCallExpression);
            }
            else if (expr is UnaryExpression)
            {
                return ParseAsUnary(expr as UnaryExpression);
            }
            else if (expr is MemberExpression)
            {
                return ParseAsMemberAccsess(expr as MemberExpression);
            }

            return null;
        }

        /// <summary>
        /// Parses a <see cref="BinaryExpression"/> into a <see cref="SqlExpression{T}"/>.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>A <see cref="SqlExpression{T}"/></returns>
        private static SqlExpression<T> ParseAsBinaryExpression(BinaryExpression expr)
        {
            return new SqlExpression<T>
            {
                LeftOperand = ParseExpression(expr.Left) ?? ParseAsMemberAccsess(expr.Left as MemberExpression),
                Operator = expr.NodeType,
                RightOperand = ParseExpression(expr.Right) ?? ParseAsMemberAccsess(expr.Right as MemberExpression)
            };
        }

        /// <summary>
        /// Parses a <see cref="MemberExpression"/> into a <see cref="DbColumnAttribute"/>, or an <see cref="object"/>.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>A <see cref="DbColumnAttribute"/>, or an <see cref="object"/></returns>
        private static object ParseAsMemberAccsess(MemberExpression expr)
        {
            return ExtractColumnInfo(expr) ?? Evaluate(expr);
        }

        /// <summary>
        /// Parses a <see cref="MethodCallExpression"/> into a <see cref="SqlExpression{T}"/>.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>A <see cref="SqlExpression{T}"/></returns>
        /// <exception cref="InvalidOperationException">This isn't a supported method.</exception>
        private static SqlExpression<T> ParseAsMethodCall(MethodCallExpression method)
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
                RightOperand = parameter
            };
        }

        /// <summary>
        /// Parses a <see cref="UnaryExpression"/> into a <see cref="SqlExpression{T}"/>.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>A <see cref="SqlExpression{T}"/></returns>
        private static SqlExpression<T> ParseAsUnary(UnaryExpression expr)
        {
            return new SqlExpression<T>
            {
                LeftOperand = SqlExpression<T>.Nothing,
                IsBooleanNot = expr.Type.IsEquivalentTo(typeof(bool)),
                Operator = expr.NodeType,
                RightOperand = ParseExpression(expr.Operand)
            };
        }

        /// <summary>
        /// Extracts the <see cref="DbColumnAttribute"/> from the expression.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>The <see cref="DbColumnAttribute"/>, or null if not found.</returns>
        private static DbColumnAttribute ExtractColumnInfo(Expression expr)
        {
            if (!(expr is MemberExpression))
                return null;

            var member = expr as MemberExpression;
            var columnAttribute = member.Member.GetCustomAttributes(typeof(DbColumnAttribute), true);
            return columnAttribute.FirstOrDefault() as DbColumnAttribute;
        }

        /// <summary>
        /// Compile, Execute and Return the value of the expression.
        /// </summary>
        /// <param name="expr">The expression.</param>
        /// <returns>The value from the expression</returns>
        private static object Evaluate(Expression expr)
        {
            if (expr == null)
            {
                return null;
            }

            // http://stackoverflow.com/a/2616980
            var objectMember = Expression.Convert(expr, typeof(object));
            var getterLambda = Expression.Lambda<Func<object>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }
    }
}