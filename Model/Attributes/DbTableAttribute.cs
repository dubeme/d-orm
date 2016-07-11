using System;

namespace DamnORM.Model.Attributes
{
    /// <summary>
    /// Attribute for database table information
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class DbTableAttribute : Attribute
    {
        /// <summary>
        /// The _ table name
        /// </summary>
        private readonly string _TableName;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbTableAttribute"/> class.
        /// </summary>
        /// <param name="tableName">Name of the table.</param>
        public DbTableAttribute(string tableName)
        {
            this._TableName = tableName;
        }

        /// <summary>
        /// Gets the name of the table.
        /// </summary>
        public string TableName
        {
            get { return _TableName; }
        }
    }
}