using DamnORM.Model.Attributes;
using System;

namespace DamnORM.Model.Reflection
{
    internal class DbColumnReflectionInfo
    {
        public DbColumnAttribute Attribute { get; set; }
        public Type BackingPropertyType { get; set; }
        public string BackingPropertyName { get; set; }
        public object BackingPropertyValue { get; set; }
    }
}