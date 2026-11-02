
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;


namespace Rastreo.DataAccess
{
    /// <summary>
    /// Metodos para obtener datos desde la base y convertirlos a entidades
    /// </summary>
    public abstract class BaseDataAccess
    {
 
        public BaseDataAccess()
        {
            //Debe quedar vacio
        }

        /// <summary>
        /// Obtener una conexión
        /// </summary>
        /// <returns></returns>
        public virtual DbConnection GetConnection()
        {
            DbConnection conn = this.Factory.CreateConnection();
            conn.ConnectionString = new DataAccessElement().ConnectionString.ConnectionString;
            return conn;
        }

        private DbProviderFactory _factory;
        /// <summary>
        /// Fabrica del proveedor de la base de datos para crear la conexión y comandos
        /// </summary>
        public virtual DbProviderFactory Factory
        {
            get
            {
                if (_factory == null)
                {
                    _factory = DbProviderFactories.GetFactory(new DataAccessElement().ConnectionString.ProviderName);
                }
                return _factory;
            }
            set
            {
                _factory = value;
            }
        }

        //Obtener un nuevo comando para el prodedimiento almacenado
        protected DbCommand GetCommand(string procedureName)
        {
            DbCommand comm = this.Factory.CreateCommand();
            comm.CommandText = procedureName;
            comm.CommandType = CommandType.StoredProcedure;
            return comm;
        }

        protected DbCommand GetCommand2(string procedureName)
        {
            DbCommand comm = this.Factory.CreateCommand();
            comm.Connection = GetConnection();
            comm.CommandText = procedureName;
            comm.CommandType = CommandType.StoredProcedure;
            return comm;
        }


        /// <summary>
        /// Obtiene un datatable lleno con los resultados de ejecutar el comando
        /// </summary>
        protected DataTable GetTable(DbCommand command)
        {
            var dt = new DataTable();
            var da = this.Factory.CreateDataAdapter();
            da.SelectCommand = command;
            //Se cierra la conexion, por que al hacer da.fill(dT) se crea otro reader y no es posible ejecutar dos reader
            command.Connection.Close();
            //copia lo del command al datatable
             da.Fill(dt);
            return dt;
        }

        /// <summary>
        /// Obtiene un datatable lleno con el primer resultado de ejecutar un comando.
        /// </summary>
        protected DataRow GetFirstRow(DbCommand command)
        {
            DataRow dr = null;
            DataTable dt = GetTable(command);
            if (dt.Rows.Count > 0)
            {
                dr = dt.Rows[0];
            }
            return dr;
        }

        /// <summary>
        /// Desechar el lector
        /// </summary>
        /// <param name="reader"></param>
        protected void SafeDispose(DbDataReader reader)
        {
            if (reader != null)
            {
                reader.Dispose();
            }
        }

        /// <summary>
        /// Abrir, ejecutar y cerrar la conexion de forma segura.
        /// </summary>
        /// <param name="comm"></param>
        /// <returns></returns>
        protected int SafeExecuteNonQuery(DbCommand comm)
        {
            return comm.SafeExecuteNonQuery();
        }

        protected DbCommand GetConsultaSimple(string consulta)
        {

            DbCommand comm = this.Factory.CreateCommand();
            SqlCommand cmd = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            comm.CommandText = consulta;
            cmd.CommandType = CommandType.Text;
            return comm;
        }

        protected int segura(DbCommand db)
        {
            try
            {
                db.Connection.Open();
                db.ExecuteReader();
                return Convert.ToInt32(db.Parameters["@salida"].Value);
            }
            catch (DbException ex)
            {
                var t = ex.Message;
                return -2;
            }
        }

        public DbDataReader seguraRegistro(DbCommand db)
        {
            try
            {
                db.Connection.Open();
                return  db.ExecuteReader();
            }
            catch (DbException)
            {
                
                return null;
            }
        }
    }
}
