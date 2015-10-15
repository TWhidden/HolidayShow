using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using HolidayShowEditor.Controls;
using HolidayShowEditor.Regions;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.UnityExtensions;
using Microsoft.Practices.Unity;

namespace HolidayShowEditor
{
    class Bootstrapper : UnityBootstrapper
    {

        public static IUnityContainer ShellContainer;
        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

        }

        protected override IUnityContainer CreateContainer()
        {
            ShellContainer = base.CreateContainer();
            return ShellContainer;
        }


        protected override IModuleCatalog CreateModuleCatalog()
        {
            /* Creates a new catalog */
            var catalog = new ModuleCatalog();

            catalog.AddModule(typeof(Module));
            
            return catalog;
        }

        protected override RegionAdapterMappings ConfigureRegionAdapterMappings()
        {
            RegionAdapterMappings mappings = base.ConfigureRegionAdapterMappings();

            /* Get the default behavior factory */
            IRegionBehaviorFactory factory = base.ConfigureDefaultRegionBehaviors();

            /* Register a mapping to our SwappableContentControl to it's adapter */
            mappings.RegisterMapping(typeof(AnimatedContentControl),new AnimatedContentControlRegionAdapter(factory));
            mappings.RegisterMapping(typeof(Panel), Container.Resolve<PanelHostRegionAdapter>());

            return mappings;
        }
        protected override DependencyObject CreateShell() 
        {

            var t = typeof (ShellWindow);
            var shell = base.Container.Resolve(t, t.ToString()) as ShellWindow;

            shell.ContentRendered += shell_Loaded;
            shell.Show();

            return shell;
        }

        void shell_Loaded(object sender, EventArgs e)
        {
            Application.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
        }

        void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var ignoreTypes = new List<Type>();

#if !DEBUG
            ignoreTypes.Add(typeof(AnimationException));
            ignoreTypes.Add(typeof(InvalidOperationException));
#endif



            if (!ignoreTypes.Contains(e.Exception.GetType()))
            {

                // Only show error messages during a DEBUG Build. NO need to show it anylonger on the user interface.
                MessageBox.Show(e.Exception.Message);
            }

            e.Handled = true;

        }
    }
}
