using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HolidayShowEditor.ViewModels;
using Microsoft.Practices.Prism.Regions;

namespace HolidayShowEditor.Controllers
{
    public class MainController : IMainController
    {
        private readonly IRegionManager _regionManager;
        private readonly IDeviceViewModel _deviceViewModel;
        private readonly ISetsViewModel _setsViewModel;
        private readonly ISettingsViewModel _settingsViewModel;
        private readonly IAudioFilesViewModel _audioFilesViewModel;


        //public MainController(IRegionManager regionManager, IDeviceViewModel deviceViewModel, ISetsViewModel setsViewModel, ISettingsViewModel settingsViewModel, IAudioFilesViewModel audioFilesViewModel)
        //{
        //    _regionManager = regionManager;
        //    //_deviceViewModel = deviceViewModel;
        //    //_setsViewModel = setsViewModel;
        //    //_settingsViewModel = settingsViewModel;
        //    //_audioFilesViewModel = audioFilesViewModel;
        //}

        public IRegion MainRegion
        {
            get { return _regionManager.Regions[RegionNames.MainRegion]; }
        }

        public void Run()
        {
            //if (!MainRegion.Views.Contains(_deviceViewModel.View))
            //    MainRegion.Add(_deviceViewModel.View);
            //MainRegion.Activate(_deviceViewModel.View);

            //if (!MainRegion.Views.Contains(_setsViewModel.View))
            //    MainRegion.Add(_setsViewModel.View);

            //if (!MainRegion.Views.Contains(_settingsViewModel.View))
            //    MainRegion.Add(_settingsViewModel.View);

            //if (!MainRegion.Views.Contains(_audioFilesViewModel.View))
            //    MainRegion.Add(_audioFilesViewModel.View);

        }

        public void Stop()
        {
            //if (MainRegion.Views.Contains(_deviceViewModel.View))
            //    MainRegion.Remove(_deviceViewModel.View);

            //if (MainRegion.Views.Contains(_setsViewModel.View))
            //    MainRegion.Remove(_setsViewModel.View);

            //if (MainRegion.Views.Contains(_settingsViewModel.View))
            //    MainRegion.Remove(_settingsViewModel.View);

            //if (MainRegion.Views.Contains(_audioFilesViewModel.View))
            //    MainRegion.Remove(_audioFilesViewModel.View);
        }
    }
}
