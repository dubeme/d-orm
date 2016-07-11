using DamnORM.Model.Attributes;
using DamnORM.Model.Reflection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DamnORM.Helpers
{
    /// <summary>
    ///
    /// </summary>
    internal static class ReflectionHelper
    {
        /// <summary>
        /// Gets the table metadata from type T.
        /// </summary>
        /// <typeparam name="T">
        ///     A CLR class/struct reprsenting the table definition.
        ///     <para>
        ///         T must be decorated using the
        ///         <see cref="DbTableAttribute" /> and
        ///         <see cref="DbColumnAttribute" /> respectively.
        ///     </para>
        /// </typeparam>
        /// <returns>The table metadata</returns>
        public static DbTableReflectionInfo<T> GetTableMetadata<T>()
        {
            return GetTableMetadata<T>(default(T));
        }

        /// <summary>
        /// Gets the table metadata from type T.
        /// </summary>
        /// <typeparam name="T">
        ///     A CLR class/struct reprsenting the table definition.
        ///     <para>
        ///         T must be decorated using the
        ///         <see cref="DbTableAttribute" /> and
        ///         <see cref="DbColumnAttribute" /> respectively.
        ///     </para>
        /// </typeparam>
        /// <param name="obj">An object whose values are stored in the metadata.</param>
        /// <returns>The table metadata</returns>
        public static DbTableReflectionInfo<T> GetTableMetadata<T>(T obj)
        {
            var tableData = new DbTableReflectionInfo<T>
            {
                Columns = new List<DbColumnReflectionInfo>(),
                TableName = string.Empty
            };

            var _TDefault = default(T);
            var _columnDefault = default(DbColumnReflectionInfo);
            var type = typeof(T);

            var tableAttribute = type.GetCustomAttributes(typeof(DbTableAttribute), true)
                .FirstOrDefault() as DbTableAttribute;

            if (tableAttribute != null)
            {
                tableData.TableName = tableAttribute.TableName;
            }

            var properties = type.GetProperties();

            tableData.Columns = properties.Select(property =>
            {
                var columnAttribute = property
                    .GetCustomAttributes(typeof(DbColumnAttribute), true)
                    .FirstOrDefault() as DbColumnAttribute;

                if (columnAttribute != null)
                {
                    object value = null;

                    if (object.Equals(obj, _TDefault) == false)
                        value = property.GetValue(obj);

                    return new DbColumnReflectionInfo
                    {
                        Attribute = columnAttribute,
                        BackingPropertyType = property.PropertyType,
                        BackingPropertyName = property.Name,
                        BackingPropertyValue = value
                    };
                }

                return _columnDefault;
            }).Where(column => !object.Equals(_columnDefault, column));

            return tableData;
        }

        /// <summary>
        /// Creates an instance of type T from a data reader.
        /// </summary>
        /// <typeparam name="T">
        ///     A CLR class/struct reprsenting the table definition.
        ///     <para>
        ///         T must be decorated using the
        ///         <see cref="DbTableAttribute" /> and
        ///         <see cref="DbColumnAttribute" /> respectively.
        ///     </para>
        /// </typeparam>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="columns">The metadata about the columns.</param>
        /// <returns>The object</returns>
        public static T CreateInstance<T>(
            IDataReader dataReader,
            IEnumerable<DbColumnReflectionInfo> columns)
        {
            // http://stackoverflow.com/a/6280540
            // http://stackoverflow.com/a/6611446
            var type = typeof(T);
            var obj = (T)Activator.CreateInstance(type, true);

            foreach (var column in columns)
            {
                SetPropertyValue(
                    ref obj,
                    column.BackingPropertyName,
                    dataReader[column.Attribute.ColumnName]);
            }

            return obj;
        }

        /// <summary>
        /// Creates an instance of type T from column-value pairs.
        /// </summary>
        /// <typeparam name="T">
        ///     A CLR class/struct reprsenting the table definition.
        ///     <para>
        ///         T must be decorated using the
        ///         <see cref="DbTableAttribute" /> and
        ///         <see cref="DbColumnAttribute" /> respectively.
        ///     </para>
        /// </typeparam>
        /// <param name="columnValueMapping">The column-value mapping.</param>
        /// <param name="columns">The metadata about the columns.</param>
        /// <returns>The object</returns>
        public static T CreateInstance<T>(
            IDictionary<string, object> columnValueMapping,
            IEnumerable<DbColumnReflectionInfo> columns)
        {
            var type = typeof(T);
            var obj = (T)Activator.CreateInstance(type, true);

            foreach (var column in columns)
            {
                SetPropertyValue(
                    ref obj,
                    column.BackingPropertyName,
                    columnValueMapping[column.Attribute.ColumnName]);
            }

            return obj;
        }

        /// <summary>
        /// From the data in the datareader, set the properties of an object.
        /// </summary>
        /// <typeparam name="T">
        ///     A CLR class/struct reprsenting the table definition.
        ///     <para>
        ///         T must be decorated using the
        ///         <see cref="DbTableAttribute" /> and
        ///         <see cref="DbColumnAttribute" /> respectively.
        ///     </para>
        /// </typeparam>
        /// <param name="obj">The object.</param>
        /// <param name="dataReader">The data reader.</param>
        /// <param name="columns">The metadata about the columns.</param>
        /// <returns>The object</returns>
        public static T SetProperties<T>(ref T obj,
            IDataReader dataReader,
            IEnumerable<DbColumnReflectionInfo> columns)
        {
            foreach (var column in columns)
            {
                SetPropertyValue(
                    ref obj,
                    column.BackingPropertyName,
                    dataReader[column.Attribute.ColumnName]);
            }

            return obj;
        }

        /// <summary>
        /// Sets the value of a property in an object.
        /// </summary>
        /// <typeparam name="T">A CLR type</typeparam>
        /// <param name="destination">The destination.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">The value.</param>
        /// <returns>The object</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T SetPropertyValue<T>(ref T destination, string propertyName, object value)
        {
            var type = destination.GetType();
            var property = type.GetProperty(propertyName);
            var convertedVal = Convert(value, property.PropertyType);

            object boxed = destination;

            property.SetValue(boxed, convertedVal);

            destination = (T)boxed;

            return (T)boxed;
        }

        /// <summary>
        /// Gets the value of a property from an object.
        /// </summary>
        /// <typeparam name="T">A CLR type</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The properties value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object GetPropertyValue<T>(T source, string propertyName)
        {
            var type = source.GetType();
            var property = type.GetProperty(propertyName);

            return property.GetValue(source);
        }

        /// <summary>
        /// Converts the specified source.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="destinationType">Type to convert to.</param>
        /// <returns>Converted value</returns>
        /// <exception cref="ArgumentNullException">If destinationType is null</exception>
        private static object Convert(object source, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType.IsGenericType &&
                destinationType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (source == null)
                {
                    return null;
                }
                destinationType = Nullable.GetUnderlyingType(destinationType);
            }

            return System.Convert.ChangeType(source, destinationType);
        }
    }
}