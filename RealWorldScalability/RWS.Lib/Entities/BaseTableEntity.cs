using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Table.Protocol;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RWS.Lib.Entities
{
    [DataContract]
    public class BaseTableEntity : ITableEntity
    {

        public BaseTableEntity()
        {
        }

        public BaseTableEntity(string partitionKey, string rowKey)
        {
            this.PartitionKey = partitionKey;
            this.RowKey = rowKey;
        }

        #region ITableEntity Implementation

        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public string ETag { get; set; }

        public virtual void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            ReflectionRead(this, properties, operationContext);
        }

        public static void ReadUserObject(object entity, IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            ReflectionRead(entity, properties, operationContext);
        }

        private static void ReflectionRead(object entity, IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            IEnumerable<PropertyInfo> objectProperties = entity.GetType().GetProperties();
            foreach (PropertyInfo property in objectProperties)
            {
                if (ShouldSkipProperty(property, operationContext))
                {
                    continue;
                }

                // only proceed with properties that have a corresponding entry in the dictionary
                if (!properties.ContainsKey(property.Name))
                {
                    continue;
                }

                EntityProperty entityProperty = properties[property.Name];

                if (entityProperty.PropertyAsObject == null)
                {
                    property.SetValue(entity, null, null);
                }
                else
                {
                    switch (entityProperty.PropertyType)
                    {
                        case EdmType.String:
                            if (property.PropertyType != typeof(string))
                            {
                                continue;
                            }

                            property.SetValue(entity, entityProperty.StringValue, null);
                            break;
                        case EdmType.Binary:
                            if (property.PropertyType != typeof(byte[]))
                            {
                                continue;
                            }

                            property.SetValue(entity, entityProperty.BinaryValue, null);
                            break;
                        case EdmType.Boolean:
                            if (property.PropertyType != typeof(bool) && property.PropertyType != typeof(bool?))
                            {
                                continue;
                            }

                            property.SetValue(entity, entityProperty.BooleanValue, null);
                            break;
                        case EdmType.DateTime:
                            if (property.PropertyType == typeof(DateTime))
                            {
                                property.SetValue(entity, entityProperty.DateTimeOffsetValue.Value.UtcDateTime, null);
                            }
                            else if (property.PropertyType == typeof(DateTime?))
                            {
                                property.SetValue(entity, entityProperty.DateTimeOffsetValue.HasValue ? entityProperty.DateTimeOffsetValue.Value.UtcDateTime : (DateTime?)null, null);
                            }
                            else if (property.PropertyType == typeof(DateTimeOffset))
                            {
                                property.SetValue(entity, entityProperty.DateTimeOffsetValue.Value, null);
                            }
                            else if (property.PropertyType == typeof(DateTimeOffset?))
                            {
                                property.SetValue(entity, entityProperty.DateTimeOffsetValue, null);
                            }

                            break;
                        case EdmType.Double:
                            if (property.PropertyType != typeof(double) && property.PropertyType != typeof(double?))
                            {
                                continue;
                            }

                            property.SetValue(entity, entityProperty.DoubleValue, null);
                            break;
                        case EdmType.Guid:
                            if (property.PropertyType != typeof(Guid) && property.PropertyType != typeof(Guid?))
                            {
                                continue;
                            }

                            property.SetValue(entity, entityProperty.GuidValue, null);
                            break;
                        case EdmType.Int32:
                            if (property.PropertyType != typeof(int) && property.PropertyType != typeof(int?))
                            {
                                continue;
                            }

                            property.SetValue(entity, entityProperty.Int32Value, null);
                            break;
                        case EdmType.Int64:
                            if (property.PropertyType != typeof(long) && property.PropertyType != typeof(long?))
                            {
                                continue;
                            }

                            property.SetValue(entity, entityProperty.Int64Value, null);
                            break;
                    }
                }
            }
        }

        public virtual IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return ReflectionWrite(this, operationContext);
        }

        public static IDictionary<string, EntityProperty> WriteUserObject(object entity, OperationContext operationContext)
        {
            return ReflectionWrite(entity, operationContext);
        }

        private static IDictionary<string, EntityProperty> ReflectionWrite(object entity, OperationContext operationContext)
        {
            Dictionary<string, EntityProperty> retVals = new Dictionary<string, EntityProperty>();

            IEnumerable<PropertyInfo> objectProperties = entity.GetType().GetProperties();

            foreach (PropertyInfo property in objectProperties)
            {
                if (ShouldSkipProperty(property, operationContext))
                {
                    continue;
                }

                EntityProperty newProperty = CreateEntityPropertyFromObject(property.GetValue(entity, null), property.PropertyType);

                // property will be null if unknown type
                if (newProperty != null)
                {
                    retVals.Add(property.Name, newProperty);
                }
            }

            return retVals;
        }

        #endregion

        #region Static Helpers

        internal static bool ShouldSkipProperty(PropertyInfo property, OperationContext operationContext)
        {
            // reserved properties
            string propName = property.Name;
            if (propName == TableConstants.PartitionKey ||
                propName == TableConstants.RowKey ||
                propName == TableConstants.Timestamp ||
                propName == TableConstants.Etag)
            {
                return true;
            }

            MethodInfo setter = property.GetSetMethod();
            MethodInfo getter = property.GetGetMethod();

            // Enforce public getter / setter
            if (setter == null || !setter.IsPublic || getter == null || !getter.IsPublic)
            {
                return true;
            }

            // Skip static properties
            if (setter.IsStatic)
            {
                return true;
            }

            // properties with [IgnoreAttribute]
            if (Attribute.IsDefined(property, typeof(IgnorePropertyAttribute)))
            {
                return true;
            }

            return false;
        }

        internal static EntityProperty CreateEntityPropertyFromObject(object value, Type type)
        {
            if (type == typeof(string))
            {
                return new EntityProperty((string)value);
            }
            else if (type == typeof(byte[]))
            {
                return new EntityProperty((byte[])value);
            }
            else if (type == typeof(bool))
            {
                return new EntityProperty((bool)value);
            }
            else if (type == typeof(bool?))
            {
                return new EntityProperty((bool?)value);
            }
            else if (type == typeof(DateTime))
            {
                return new EntityProperty((DateTime)value);
            }
            else if (type == typeof(DateTime?))
            {
                return new EntityProperty((DateTime?)value);
            }
            else if (type == typeof(DateTimeOffset))
            {
                return new EntityProperty((DateTimeOffset)value);
            }
            else if (type == typeof(DateTimeOffset?))
            {
                return new EntityProperty((DateTimeOffset?)value);
            }
            else if (type == typeof(double))
            {
                return new EntityProperty((double)value);
            }
            else if (type == typeof(double?))
            {
                return new EntityProperty((double?)value);
            }
            else if (type == typeof(Guid?))
            {
                return new EntityProperty((Guid?)value);
            }
            else if (type == typeof(Guid))
            {
                return new EntityProperty((Guid)value);
            }
            else if (type == typeof(int))
            {
                return new EntityProperty((int)value);
            }
            else if (type == typeof(int?))
            {
                return new EntityProperty((int?)value);
            }
            else if (type == typeof(long))
            {
                return new EntityProperty((long)value);
            }
            else if (type == typeof(long?))
            {
                return new EntityProperty((long?)value);
            }
            else
            {
                return null;
            }
        }

        internal static EntityProperty CreateEntityPropertyFromObject(object value, EdmType type)
        {
            if (type == EdmType.String)
            {
                return new EntityProperty((string)value);
            }
            else if (type == EdmType.Binary)
            {
                return new EntityProperty(Convert.FromBase64String((string)value));
            }
            else if (type == EdmType.Boolean)
            {
                return new EntityProperty(bool.Parse((string)value));
            }
            else if (type == EdmType.DateTime)
            {
                return new EntityProperty(DateTimeOffset.Parse((string)value, CultureInfo.InvariantCulture));
            }
            else if (type == EdmType.Double)
            {
                return new EntityProperty(double.Parse((string)value, CultureInfo.InvariantCulture));
            }
            else if (type == EdmType.Guid)
            {
                return new EntityProperty(Guid.Parse((string)value));
            }
            else if (type == EdmType.Int32)
            {
                return new EntityProperty(int.Parse((string)value, CultureInfo.InvariantCulture));
            }
            else if (type == EdmType.Int64)
            {
                return new EntityProperty(long.Parse((string)value, CultureInfo.InvariantCulture));
            }
            else
            {
                return null;
            }
        }

        #endregion

    }
}

