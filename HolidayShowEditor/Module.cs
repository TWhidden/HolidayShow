using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HolidayShowEditor.Controllers;
using HolidayShowEditor.Services;
using HolidayShowEditor.ViewModels;
using HolidayShowEditor.Views;
using Microsoft.Practices.Prism.Modularity;
using Microsoft.Practices.Unity;

namespace HolidayShowEditor
{
    public class Module : IModule
    {
        private IMainController mainController;

        private readonly IUnityContainer _container;

        public Module(IUnityContainer container)
        {
            _container = container;
        }

        public void Initialize()
        {
            _container.RegisterInstance<IDbDataContext>(_container.Resolve<DbDataContext>());

            _container.RegisterType<IDeviceViewModel, DeviceViewModel>();
            _container.RegisterType<IDeviceView, DeviceView>();

            _container.RegisterType<ISetsViewModel, SetsViewModel>();
            _container.RegisterType<ISetsView, SetsView>();

            _container.RegisterType<ISettingsViewModel, SettingsViewModel>();
            _container.RegisterType<ISettingsView, SettingsView>();

            _container.RegisterType<IAudioFilesViewModel, AudioFilesViewModel>();
            _container.RegisterType<IAudioFilesView, AudioFilesView>();

            _container.RegisterType<IEffectsView, EffectsView>();
            _container.RegisterType<IEffectsViewModel, EffectsViewModel>();

            mainController = _container.Resolve<MainController>();
            mainController.Run();
        }
    }
}
