using System;

namespace DamnORM.Model.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class DbTableAttribute : Attribute
    {
        private readonly string _TableName;

        public DbTableAttribute(string tableName)
        {
            this._TableName = tableName;
        }

        public string TableName
        {
            get { return _TableName; }
        }
    }
}