using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Android;
using Android.App;
using Java.Util.Jar;
using static Java.Util.Jar.Attributes;
using Xamarin.Forms.PlatformConfiguration;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("EVIRFID.Android")]
[assembly: AssemblyDescription("EVI RFID")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Guia RFID")]
[assembly: AssemblyProduct("EVIRFID.Android")]
[assembly: AssemblyCopyright("Copyright ©  2022")]
[assembly: AssemblyTrademark("Guia Rfid")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Application(Icon = "@drawable/evi")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

// Add some common permissions, these can be removed if not needed
[assembly: UsesPermission(Android.Manifest.Permission.Internet)]
[assembly: UsesPermission(Android.Manifest.Permission.WriteExternalStorage)]
//[assembly: UsesPermission(Android.Manifest.Permission.ForegroundService)]
//[assembly: UsesPermission(Android.Manifest.Permission.StartForegroundServicesFromBackground)]
//[assembly: UsesPermission(Android.Manifest.Permission.InstantAppForegroundService)]
[assembly: UsesPermission(Android.Manifest.Permission.Vibrate)]