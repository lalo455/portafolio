using System;
using System.Windows.Input;
using EVIRFID.Views;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.CommunityToolkit.ObjectModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using MiniExcelLibs;
using EVIRFID.Models;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;

namespace EVIRFID.ViewModels
{
	public partial class ImportViewModel : BaseViewModel
	{
		public ImportViewModel()
		{
			Title = "Importación de Datos";
		}

		public void OnAppearing()
		{

		}

		[ObservableProperty]
		public string filePath;

	}
}

