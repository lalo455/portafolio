using System;
using System.Configuration;

namespace CRMs.DataAccess
{
    public class DataAccessElement : ConfigurationElement
    {
        protected override void PostDeserialize()
        {
        }

        public DataAccessElement()
        {
            #region Default ParameterPrefix
            switch (this.ConnectionString.ProviderName.ToUpper())
            {
                case "SYSTEM.DATA.SQLCLIENT":
                    this.ParameterPrefix = "@";
                    break;
                case "MYSQL.DATA.MYSQLCLIENT":
                    this.ParameterPrefix = "param_";
                    break;
            }
            #endregion
        }

        [ConfigurationProperty("parameterPrefix", IsRequired = false)]
        public string ParameterPrefix
        {
            get
            {
                return (string)this["parameterPrefix"];
            }
            set
            {
                this["parameterPrefix"] = value;
            }
        }

        private ConnectionStringSettings _connectionString;
        public ConnectionStringSettings ConnectionString
        {
            get
            {
                if (_connectionString == null)
                {
                    //Llave usada en el diccionario de cadenas de conexion
                    const string _connectionKey = "dbCRM";
                    //Default provider
                    var providerName = "System.Data.SqlClient";
                    ConnectionStringSettings conn = ConfigurationManager.ConnectionStrings[_connectionKey];

                    if (conn == null)
                    {
                        throw new ConfigurationErrorsException("Se debe especificar la cadena de conexion en el archivo de configuracion, con la llave " + _connectionKey + ".");
                    }

                    if (!String.IsNullOrEmpty(conn.ProviderName))
                    {
                        providerName = conn.ProviderName;
                    }
                    _connectionString = new ConnectionStringSettings(_connectionKey, conn.ConnectionString, providerName);
                }
                return _connectionString;
            }
        }
    }
}
