using System;
using System.Data;
using System.Data.Common;

namespace Rastreo.DataAccess
{
    public static class AdoExtensions
    {

        public static DbParameter AddParameter(this DbCommand comm, DbProviderFactory factory, string parameterName, DbType type, object value)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException("parameterName");
            }
            if (parameterName.StartsWith("@") || parameterName.StartsWith(":") || parameterName.StartsWith("?"))
            {
                throw new ArgumentException("Do not include prefix in parameter name.");
            }
            DbParameter param = factory.CreateParameter();
            param.DbType = type;
            
            param.ParameterName = new DataAccessElement().ParameterPrefix + parameterName;
            if (value == null)
            {
                param.Value = DBNull.Value;
            }
            else if (value is string && Convert.ToString(value) == "")
            {
                param.Value = DBNull.Value;
            }
            else
            {
                param.Value = value;
            }

            comm.Parameters.Add(param);
            return param;
        }

        /// Agregar un parametro al DbCommand y mapear el tipo de objeto a DbType
        public static DbParameter AddParameter<T>(this DbCommand comm, DbProviderFactory factory, string parameterName, T value)
        {
            Type type = typeof(T);
            DbType dbType;
            object parameterValue = value;
            switch (type.FullName)
            {
                case "System.String":
                    dbType = DbType.String;
                    break;
                case "System.Int32":
                    dbType = DbType.Int32;
                    break;
                case "System.DateTime":
                    dbType = DbType.DateTime;
                    break;
                case "System.Int64":
                    dbType = DbType.Int64;
                    break;
                case "System.Int16":
                    dbType = DbType.Int16;
                    break;
                case "System.Decimal":
                    dbType = DbType.Decimal;
                    break;
                case "System.Double":
                    dbType = DbType.Double;
                    break;
                case "System.Boolean":
                    dbType = DbType.Boolean;
                    break;
                case "System.Byte":
                    dbType = DbType.Byte;
                    break;
                case "System.Guid":
                    dbType = DbType.Guid;
                    break;
                case "System.Char[]":
                    dbType = DbType.String;
                    break;
                case "System.Single":
                    dbType = DbType.Single;
                    break;
                default:
                    throw new System.Data.DataException("Tipo no soportado para mapeo implicito DbType.");
            }
            return AddParameter(comm, factory, parameterName, dbType, parameterValue);
        }

        public static string GetNullableString(this DbDataReader reader, string columnName)
        {
            object value = reader[columnName];
            if (value == DBNull.Value)
            {
                return null;
            }
            return value.ToString();
        }

       
        public static string GetString(this DbDataReader reader, string columnName)
        {
               object value = reader[columnName];
               return value.ToString();
        }

        public static T GetNullable<T>(this DbDataReader reader, string columnName)
        {
            object value = reader[columnName];
            if (value == DBNull.Value)
            {
                return default(T);
            }
            return Get<T>(reader, columnName);
        }

        //valida que el nombre de los reader se corresponda con el nombre de las tablas regresada por los stored
        public static T Get<T>(this DbDataReader reader, string columnName)
        {
            try
            {
                return (T)reader[columnName];
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException("La conversión especificada no es válida, Campo: " + columnName, ex);
            }
        }

        public static string GetNullableString(this DataRow dr, string columnName)
        {
            object value = dr[columnName];
            if (value == DBNull.Value)
            {
                return null;
            }
            return value.ToString();
        }

        public static T? GetNullableStruct<T>(this DataRow dr, string columnName) where T : struct, IConvertible
        {
            var value = dr[columnName];
            if (value == DBNull.Value)
            {
                return (T?)null;
            }
            return (T)value;
        }

        public static string GetString(this DataRow dr, string columnName)
        {
            return dr.GetNullable<string>(columnName);
        }

        public static T GetNullable<T>(this DataRow dr, string columnName)
        {
            object value = dr[columnName];
            if (value == DBNull.Value)
            {
                return default(T);
            }
            return Get<T>(dr, columnName);
        }

        public static DateTime GetDate(this DataRow dr, string columnName)
        {
            DateTime date = (DateTime)dr[columnName];
            return DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        // Metodo que devuelve el contenido de los campos en las tablas
        public static T Get<T>(this DataRow dr, string columnName)
        {
            try
            {
                // si el campo no existe o es nulo en la tabla genera excepcion
                if (dr[columnName] == DBNull.Value)
                {
                    throw new NoNullAllowedException("Columna " + columnName + " tiene un valor nulo.");
                }
                Type type = typeof(T);
                if (type == typeof(Int32) )
                {
                    return (T)(object)Int32.Parse(dr[columnName].ToString());
                }
                else if (type == typeof(Int16))
                {
                    return (T)(object)Int16.Parse(dr[columnName].ToString());
                }
                else if (type == typeof(DateTime))
                {
                    throw new ArgumentException("Date-time no soportado.");
                }
                else if (type == typeof(Guid))
                {
                    return (T)(object)new Guid(dr[columnName].ToString());
                }
                else if (type.IsEnum)
                {
                    return (T)Enum.Parse(type, dr[columnName].ToString());
                }
           
            
                //regresa el contenido del campo en su respectivo tipo de dato
                return (T)dr[columnName];
            }
            catch (InvalidCastException ex)
            {
                throw new InvalidCastException("La conversión especificada no es válida, Campo:: " + columnName + ", Tipo: " + typeof(T).FullName, ex);
            }
        }

        /// <summary>
        /// Abrir, ejecutar y cerrar la conexion de forma segura.
        /// </summary>
        /// <param name="comm"></param>
        /// <returns>Numero de registros afectados</returns>
        public static int SafeExecuteNonQuery(this DbCommand comm)
        {
            int rowsAffected = 0;
            try
            {
                comm.Connection.Open();
                rowsAffected = comm.ExecuteNonQuery();
            }
            finally
            {
                comm.Connection.Close();
            }
            return rowsAffected;
        }
    }
}
