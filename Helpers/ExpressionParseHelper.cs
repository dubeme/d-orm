using DamnORM.Model;
using DamnORM.Model.Attributes;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace DamnORM.Helpers
{
    [Serializable]
    public static class ExpressionParseHelper<T>
    {
        public static SqlExpression<T> Parse(Expression<Func<T, bool>> expression)
        {
            var tpp = expression.Body.GetType();

            if (expression.Body is BinaryExpression)
            {
                return ParseAsBinaryExpression(expression.Body as BinaryExpression);
            }
            else if (expression.Body is MethodCallExpression)
            {
                return ParseAsMethodCall(expression.Body as MethodCallExpression);
            }
            else if (expression.Body is UnaryExpression)
            {
                return ParseAsUnary(expression.Body as UnaryExpression);
            }
            else if (expression.Body is MemberExpression)
            {
                var expr = expression.Body as MemberExpression;
                throw new NotImplementedException();
            }
            else if (expression.Body is LambdaExpression)
            {
                var expr = expression.Body as LambdaExpression;
                throw new NotImplementedException();
            }
            // else if (expression.Body is LogicalBinaryExpression)
            {
            }

            return null;
        }

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
            else if (expr is LambdaExpression)
            {
                // var expr = expression.Body as LambdaExpression;
            }
            else if (expr is MemberExpression)
            {
                return ParseAsMemberAcsess(expr as MemberExpression);
            }

            return null;
        }

        private static SqlExpression<T> ParseAsBinaryExpression(BinaryExpression expr)
        {
            return new SqlExpression<T>
            {
                LeftOperand = ParseExpression(expr.Left) ?? ParseAsMemberAcsess(expr.Left as MemberExpression),
                Operator = expr.NodeType,
                RightOperand = ParseExpression(expr.Right) ?? ParseAsMemberAcsess(expr.Right as MemberExpression)
            };
        }

        private static object ParseAsMemberAcsess(MemberExpression expr)
        {
            return ExtractColumnInfo(expr) ?? Evaluate(expr);
        }

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

        private static SqlExpression<T> ParseAsUnary(UnaryExpression expr)
        {
            return new SqlExpression<T>
            {
                IsBooleanNot = expr.Type.IsEquivalentTo(typeof(bool)),
                Operator = expr.NodeType,
                RightOperand = ParseExpression(expr.Operand)
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
            return getter();
        }
    }
}