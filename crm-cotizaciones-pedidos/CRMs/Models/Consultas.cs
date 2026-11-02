using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CRMs.Models;
using System.Data.SqlClient;
using CRMs.Utilerias;

namespace CRMs.Models
{
    public class Consultas
    {
        ConexionSQL Conexion = new ConexionSQL();
        //Metodos
        //Log incidencias = new Log();
        public ModelosConsultas.UsuarioAutenticado LoginUsuario(DatosAcceso Credenciales)
        {
            ModelosConsultas.UsuarioAutenticado ModeloUsuario = new ModelosConsultas.UsuarioAutenticado();

            try
            {
                using (SqlConnection Con = new SqlConnection(Conexion.Cadena()))
                {
                    Con.Open();
                    using (SqlCommand comando = Con.CreateCommand())
                    {
                        comando.CommandText = @"SP_Login";
                        comando.CommandType = System.Data.CommandType.StoredProcedure;

                        var paramUsuario = comando.CreateParameter();
                        paramUsuario.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramUsuario.ParameterName = "Usuario";
                        paramUsuario.Value = Credenciales.Usuario;
                        comando.Parameters.Add(paramUsuario);

                        var paramPassword = comando.CreateParameter();
                        paramPassword.SqlDbType = System.Data.SqlDbType.NVarChar;
                        paramPassword.ParameterName = "Password";
                        paramPassword.Value = Credenciales.Password;
                        comando.Parameters.Add(paramPassword);


                        using (var read = comando.ExecuteReader())
                        {

                            try
                            {
                                if (!read.HasRows)
                                {
                                    ModeloUsuario.AutenticacionCorrecta = false;
                                    ModeloUsuario.Acciones = new List<string>();
                                    ModeloUsuario.IdEmpleadoSAP = "-1";
                                    ModeloUsuario.UsuarioWeb = "";
                                    ModeloUsuario.PasswordWeb = "";
                                }

                                while (read.Read())
                                {
                                    //incidencias.Registrar(read.GetString(0), read.GetString(1), read.GetString(2), "Models.Consultas");
                                    ModeloUsuario = new ModelosConsultas.UsuarioAutenticado()
                                    {
                                        IdEmpleadoSAP = Convert.ToString(read.GetValue(3)),
                                        UsuarioWeb = Convert.ToString(read.GetValue(1)),
                                        PasswordWeb = Convert.ToString(read.GetValue(2)),
                                        NombreEmpleado = Convert.ToString(read.GetValue(4)),
                                        RolWeb = Convert.ToString(read.GetValue(5)),
                                        AutenticacionCorrecta = true
                                    };
                                }
                            }
                            catch (Exception ex)
                            {
                                //incidencias.Registrar(ex.Message, ex.Message, "Consultas.LoginUsuario", "Models.Consultas");
                                ExceptionUtility.LogException(ex, "Consultas.LoginUsuario");
                            }
                            finally
                            {
                                if (read != null)
                                {
                                    read.Close();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionUtility.LogException(ex, "Consultas.LoginUsuario");
            }

            return ModeloUsuario;
        }
    }
}