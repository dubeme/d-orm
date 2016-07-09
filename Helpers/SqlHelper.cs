using DamnORM.Model.Attributes;
using System;
using System.Linq.Expressions;

namespace DamnORM.Helpers
{
    /// <summary>
    /// Utility class to help with Sql operations
    /// </summary>
    public class SqlHelper
    {
        /// <summary>
        /// Mocks an object.
        /// </summary>
        /// <typeparam name="T">Object Type</typeparam>
        /// <returns>An instance of type T</returns>
        internal static T MockObject<T>() where T : new()
        {
            return new T();
        }

        /// <summary>
        /// Retrieves the column name of a property.
        /// </summary>
        /// <example>
        /// This sample shows how to call the <see cref="GetColumnName"/> method.
        /// <code>
        ///     SqlHelper.GetColumnName&lt;Person&gt;(p =&gt; p.FirstName);
        /// </code>
        /// </example>
        /// <typeparam name="T">
        ///     A CLR class/struct reprsenting the table definition.
        ///     <para>
        ///         T must be decorated using <see cref="DamnORM.Model.Attributes.DbColumnAttribute" />.
        ///     </para>
        /// </typeparam>
        /// <param name="memberExpression">An expression indicating which property to use</param>
        /// <returns>The column name of the property</returns>
        public static string GetColumnName<T>(Expression<Func<T, object>> memberExpression)
        {
            // http://stackoverflow.com/a/12975480
            var member = memberExpression.Body as MemberExpression;
            var unary = memberExpression.Body as UnaryExpression;
            var expressionBody = member ?? (unary != null ? unary.Operand as MemberExpression : null);

            var columnAttribute = expressionBody.Member.GetCustomAttributes(true)[0] as DbColumnAttribute;

            return columnAttribute.ColumnName;
        }

        /// <summary>
        /// Generates a name to be used as a SQL parameter.
        /// </summary>
        /// <param name="columnName">Name of the column.</param>
        /// <returns>Generated string</returns>
        internal static string GenerateParameterName(string columnName)
        {
            return string.Format("@{0}_{1:N}", columnName, Guid.NewGuid());
        }
    }
}