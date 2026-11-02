using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EVIRFID.Models;

namespace EVIRFID.Services
{
    public class MockDataStore : IDataStore<Person>
    {
        readonly Person currentElement;

        public MockDataStore()
        {
            currentElement = new Person() { nombre="Seleccione a",adscripcion="un elemento.", idPersona=0};
        }

        public void SetCurrentPerson(Person persona)
        {
            currentElement.nombre = persona.nombre;
            currentElement.cuip = persona.cuip;
            currentElement.adscripcion = persona.adscripcion;
            currentElement.idPersona = persona.idPersona;
            currentElement.fechaCalzadoEntregado = persona.fechaCalzadoEntregado;
            currentElement.fechaCinturonEntregado = persona.fechaCinturonEntregado;
            currentElement.fechaKepiEntregado = persona.fechaKepiEntregado;
            currentElement.fechaPantalonEntregado = persona.fechaPantalonEntregado;
            currentElement.fechaImpermeableEntregado = persona.fechaImpermeableEntregado;
            currentElement.tipoSangre = persona.tipoSangre;
            currentElement.calzadoEntregado = persona.calzadoEntregado;
            currentElement.cinturonEntregado = persona.cinturonEntregado;
            currentElement.kepiEntregado = persona.kepiEntregado;
            currentElement.pantalonEntregado = persona.pantalonEntregado;
            currentElement.impermeableEntregado = persona.impermeableEntregado;
            currentElement.itemsAsignados = persona.itemsAsignados;
            currentElement.fechaUltimaAtencion = persona.fechaUltimaAtencion;
            currentElement.observaciones = persona.observaciones;
            currentElement.fechaGorraEntregado = persona.fechaGorraEntregado;
            currentElement.gorraEntregado = persona.gorraEntregado;
        }

        public Person GetCurrentPerson()
        {
            return currentElement;
        }

        public void ReleaseCurrentPerson()
        {
            currentElement.idPersona = 0;
            currentElement.nombre = "Seleccione a";
            currentElement.adscripcion = "un elemento.";
        }
    }
}
