using System;
using EVIRFID.Models;
namespace EVIRFID.Services
{
	public class Utils
	{
        public Person IgualarPersonas(Person personaOriginal)
        {
            Person nuevaPersona = new Person()
            {
                nombre = personaOriginal.nombre,
                cuip = personaOriginal.cuip,
                tipoSangre = personaOriginal.tipoSangre,
                adscripcion = personaOriginal.adscripcion,
                idPersona = personaOriginal.idPersona,
                itemsAsignados = personaOriginal.itemsAsignados,
                fechaCalzadoEntregado = personaOriginal.fechaCalzadoEntregado,
                fechaCinturonEntregado = personaOriginal.fechaCinturonEntregado,
                fechaKepiEntregado = personaOriginal.fechaKepiEntregado,
                fechaPantalonEntregado = personaOriginal.fechaPantalonEntregado,
                fechaImpermeableEntregado = personaOriginal.fechaImpermeableEntregado,
                calzadoEntregado = personaOriginal.calzadoEntregado,
                cinturonEntregado = personaOriginal.cinturonEntregado,
                kepiEntregado = personaOriginal.kepiEntregado,
                pantalonEntregado = personaOriginal.pantalonEntregado,
                impermeableEntregado = personaOriginal.impermeableEntregado,
                fechaUltimaAtencion = personaOriginal.fechaUltimaAtencion,
                observaciones = personaOriginal.observaciones,
                fechaGorraEntregado = personaOriginal.fechaGorraEntregado,
                gorraEntregado = personaOriginal.gorraEntregado
            };
            return nuevaPersona;
        }
    }
}

