using System;
using Microsoft.Maui.Controls.Xaml;
using WmsMovil.Resources.Strings; // Apunta a tu carpeta

namespace WmsMovil
{
    [ContentProperty(nameof(Name))]
    public class TranslateExtension : IMarkupExtension
    {
        public string Name { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (string.IsNullOrWhiteSpace(Name))
                return string.Empty;

            // MIRA AQUÍ: Ahora usamos AppStrings en lugar de Resource
            var translation = AppStrings.ResourceManager.GetString(Name);

            return translation ?? $"#{Name}#";
        }
    }
}