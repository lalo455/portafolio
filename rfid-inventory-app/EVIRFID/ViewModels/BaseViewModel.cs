using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Xamarin.Forms;

using EVIRFID.Models;
using EVIRFID.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EVIRFID.ViewModels
{
    //[INotifyPropertyChanged]
    public partial class BaseViewModel : ObservableObject
    {
        public IDataStore<Person> DataStore => DependencyService.Get<IDataStore<Person>>();
        public IRfidService servicioRFID => DependencyService.Get<IRfidService>();

        [ObservableProperty]
        bool isBusy = false;

        [ObservableProperty]
        string title = string.Empty;

        [ObservableProperty]
        int potencia = 5;

        [ObservableProperty]
        Person currentPerson;

        [ObservableProperty]
        bool autoNamePrenda = true;

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