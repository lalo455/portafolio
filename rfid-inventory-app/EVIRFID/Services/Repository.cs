using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using EVIRFID.Models;
using EVIRFID.Services;
namespace EVIRFID.Services
{
    public class Repository
    {
        static SQLiteAsyncConnection db;

        static async Task Init()
        {
            if (db != null)
                return;
            db = new SQLiteAsyncConnection(App.DbPath);

            await db.CreateTableAsync<User>();
        }

        public static async Task<IEnumerable<User>> GetUsers( bool includeAvatar)
        {
            await Init();
            if (!includeAvatar)
            {
                var query = db.Table<User>();
                return await query.ToListAsync();
            } else
            {
                var query = db.QueryAsync<User>("SELECT * from usuario  INNER JOIN avatar ON usuario.idAvatar=avatar.idAvatar");
                return await query;
            }
        }

        //SELECT persona.nombre AS NOMBRE, item.prenda AS DESCRIPCION, persona.cuip AS CUIP, persona.tipoSangre AS [TIPO DE SANGRE], persona.adscripcion AS ADSCRIPCION, item.epc as CHIP, item.fechaAsignacion AS FECHA FROM persona INNER JOIN item ON persona.idPersona = item.idPersona 

        public static async Task<IEnumerable<Reporte>> GetReport()
        {
            await Init();
            var query = db.Table<Reporte>();
            return await query.ToListAsync();
        }


        public static async Task<int> GetTotalItems()
        {
            await Init();
            var query = db.Table<Item>().CountAsync();
            return query.Result;
        }

        public static async Task<int> GetTodayItems()
        {
            await Init();
            string hoy = DateTime.Now.ToString("dd/MM/yyyy");
            string consulta = string.Format("SELECT COUNT(idItem) FROM item WHERE fechaAsignacion LIKE '%{0}%'", hoy);
            var query = db.ExecuteScalarAsync<int>(consulta);
            return query.Result;
        }

        public static async Task<int> GetAssignedItemsCount()
        {
            await Init();
            var query = db.ExecuteScalarAsync<int>("SELECT COUNT(idItem) FROM item WHERE fechaAsignacion = '' OR fechaAsignacion IS NOT NULL");
            return query.Result;
        }

        public static async Task<int> GetTotalPersons()
        {
            await Init();
            var query = db.Table<Person>().CountAsync();
            return query.Result;
        }

        public static async Task<int> GetTodayPersons()
        {
            await Init();
            string hoy = DateTime.Now.ToString("dd/MM/yyyy");
            var query = db.ExecuteScalarAsync<int>(string.Format("SELECT COUNT(idPersona) FROM persona WHERE fechaUltimaAtencion LIKE '%{0}%'", hoy));
            return query.Result;
        }

        public static async Task<int> GetAttendedPersons()
        {
            await Init();
            var query = db.ExecuteScalarAsync<int>("SELECT COUNT(idPersona) FROM persona WHERE fechaUltimaAtencion = '' OR fechaUltimaAtencion IS NOT NULL");
            return query.Result;
        }

        public static async Task<IEnumerable<Person>> GetSelectedPersonAsync(int idPersona)
        {
            await Init();
            var query = db.Table<Person>().Where(i => i.idPersona.Equals(idPersona));
            return await query.ToListAsync();
        }

        public static async Task<int> UpdateItemOwner(Item item)
        {
            try
            {
                await Init();
                var query = await db.UpdateAsync(item);
                return query;
            } catch (Exception exception)
            {
                Debug.WriteLine("Error al actualizar:{0}", exception.ToString());
                return 0;
            }
        }

        public static async Task<List<Person>> GetPersons()
        {
            await Init();
            var query = db.Table<Person>().OrderBy(x => x.idPersona);
            return await query.ToListAsync();
        }

        public static async Task<IEnumerable<Person>> GetPersonsAsync(bool forceRefresh = false)
        {
            await Init();
            var query = db.Table<Person>().OrderBy(x => x.idPersona);
            return await query.ToListAsync();
        }

        public static async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            await Init();
            var query = db.Table<Item>().OrderByDescending(x => "");
            return await query.ToListAsync();
        }

        public static async Task<IEnumerable<Evento>> GetEventosAsync()
        {
            await Init();
            var query = db.Table<Evento>().OrderBy(x => x.idEvento);
            return await query.ToListAsync();
        }



        public static async Task<Item> GetItemDetailsByEpcAsync(string epc)
        {
            await Init();
            //var query = db.Table<Item>().FirstAsync(x => x.epc == epc);
            var query = db.Table<Item>().Where(x => x.epc == epc).FirstOrDefaultAsync();
            return await query;
        }

   

        public static async Task<Person> GetPersonById(int idPerson)
        {
            await Init();
            var query = db.Table<Person>().Where(x => x.idPersona == idPerson).FirstOrDefaultAsync();
            return await query;
        }

        public static async Task AddItemAsync(Item item)
        {
            await Init();
            await db.InsertAsync(item);
            return;
        }

        public static async Task AddPersonAsync(Person persona)
        {
            await Init();
            await db.InsertAsync(persona);
            return;
        }

        public static async Task<IEnumerable<Person>> GetFilteredPersonsAsync(string filtro, bool forceRefresh = false)
        {
            await Init();
            var query = db.Table<Person>().Where(i => i.nombre.Contains(filtro) ||
                                                      i.cuip.Contains(filtro) ||
                                                      i.adscripcion.Contains(filtro));
            return await query.ToListAsync();
        }

     

        public static async Task<int> UpdatePersonAsync(Person person)
        {
            try
            {
                await Init();
                var query = await db.UpdateAsync(person);
                return query;
            }
            catch (Exception exception)
            {
                Debug.WriteLine("Error al actualizar:{0}", exception.ToString());
                return 0;
            }
        }

        public Task<bool> DeletePersonAsync(int id)
        {
            throw new NotImplementedException();
        }

        public static async Task<int> InsertEventAsync(Evento evento)
        {
            await Init();
            var query = await db.InsertAsync(evento);
            return query;
        }

        public static async Task<IEnumerable<string>> GetUniquePrendasAsync()
        {
            await Init();
            var items = await db.Table<Item>().ToListAsync();
            var uniquePrendas = items.Select(i => "").Distinct();
            return uniquePrendas;
        }

        public static async Task<IEnumerable<string>> GetUniqueClaveCorporacionsAsync()
        {
            await Init();
            var items = await db.Table<Item>().ToListAsync();
            var uniqueClaveCorporacions = items.Select(i => "").Distinct();
            return uniqueClaveCorporacions;
        }

        public static async Task<IEnumerable<string>> GetUniqueTallasAsync()
        {
            await Init();
            var items = await db.Table<Item>().ToListAsync();
            var uniqueTallas = items.Select(i => "").Distinct();
            return uniqueTallas;
        }

        internal static async Task<IEnumerable<Item>> GetAssignedItemsAsync(int idPersona)
        {
            throw new NotImplementedException();
        }
    }
}