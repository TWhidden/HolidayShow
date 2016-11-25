using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Windows.Input;
using HolidayShow.Data;
using HolidayShowEditor.BaseClasses;
using HolidayShowEditor.Interfaces;
using HolidayShowEditor.Services;
using Microsoft.Practices.Prism.Commands;

namespace HolidayShowEditor.ViewModels
{
    public class DeviceViewModel : ViewModelBase, IDeviceViewModel, IHeaderedViewModel 
    {
        private readonly IDbDataContext _dataContext;
        private Devices _deviceSelected = null;
        private bool _isDeviceSelected;
        private DevicePatterns _devicePatternSelected;
        private bool _isDevicePatternSelected;
        private DevicePatternSequences _devicePatternSequenceSelected;

        public DeviceViewModel(IDbDataContext dataContext)
        {
            _dataContext = dataContext;
            HeaderInfo = "Devices";
            Devices = new ObservableCollection<Devices>();
            base.SetTimeout(0, LoadDevices);
            CommandSave = new DelegateCommand(OnCommandSave);
            CommandAddNewPattern = new DelegateCommand(OnCommandAddNewPattern);
            CommandDeletePattern = new DelegateCommand(OnCommandDeletePattern);
            CommandAddNewCommand = new DelegateCommand(OnCommandAddNewCommand);
            CommandDeleteCommand = new DelegateCommand(OnCommandDeleteCommand);
            CommandDeviceIoPortDetect = new DelegateCommand<DeviceIoPorts>(OnCommandDeviceIoPortDetect);

        }

        public object HeaderInfo { get; set; }

        public ObservableCollection<Devices> Devices { get; set; }

        public void LoadDevices()
        {
            Devices.Clear();
            
            _dataContext.Context.Devices.ToList().ForEach(x => Devices.Add(x));
            
        }

        public Devices DeviceSelected
        {
            get { return _deviceSelected; }
            set { _deviceSelected = value;
                IsDeviceSelected = (value != null);
                OnPropertyChanged(()=>DeviceSelected);
                OnPropertyChanged(() => DevicePatterns);
                OnPropertyChanged(() => DeviceIoPorts);
                
            }
        }

        public List<DeviceIoPorts>  DeviceIoPorts
        {
            get {
                return DeviceSelected?.DeviceIoPorts.Where(x => !x.IsNotVisable).OrderBy(x => x.CommandPin).ToList();
            }
        }

        public List<DevicePatterns> DevicePatterns
        {
            get
            {
                return DeviceSelected?.DevicePatterns.OrderBy(x => x.PatternName).ToList();
            }
        }

        public List<DevicePatternSequences> DevicePatternSequences
        {
            get
            {
                return DevicePatternSelected?.DevicePatternSequences.OrderBy(x => x.OnAt).ToList();
            }
        }

        public bool IsDeviceSelected
        {
            get { return _isDeviceSelected; }
            set { _isDeviceSelected = value;
                OnPropertyChanged(()=>IsDeviceSelected);
            }
        }

        public DevicePatterns DevicePatternSelected
        {
            get { return _devicePatternSelected; }
            set { _devicePatternSelected = value;
                OnPropertyChanged(()=>DevicePatternSelected);
                IsDevicePatternSelected = (value != null);
                OnPropertyChanged(() => DevicePatternSequences);
            }
        }

        public DevicePatternSequences DevicePatternSequenceSelected
        {
            get { return _devicePatternSequenceSelected; }
            set { _devicePatternSequenceSelected = value;
                OnPropertyChanged(() => DevicePatternSequenceSelected);
            }
        }

        public bool IsDevicePatternSelected
        {
            get { return _isDevicePatternSelected; }
            set { _isDevicePatternSelected = value;
            OnPropertyChanged(() => IsDevicePatternSelected);
            }
        }

        public ICommand CommandSave { get; private set; }

        public ICommand CommandAddNewPattern { get; private set; }

        public ICommand CommandDeletePattern { get; private set; }

        public ICommand CommandAddNewCommand { get; private set; }

        public ICommand CommandDeleteCommand { get; private set; }

        public DelegateCommand<DeviceIoPorts> CommandDeviceIoPortDetect { get; private set; }

        private async void OnCommandDeviceIoPortDetect(DeviceIoPorts ioPort)
        {
            var existingSetting = await _dataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.DetectDevicePin).FirstOrDefaultAsync();
            if (existingSetting == null)
            {
                existingSetting = new Settings()
                {
                    SettingName = SettingKeys.DetectDevicePin
                };
                _dataContext.Context.Settings.Add(existingSetting);
            }
            existingSetting.ValueString = $"{ioPort.DeviceId}:{ioPort.CommandPin}";

            var option = await _dataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.SetPlaybackOption).FirstOrDefaultAsync();
            if (option == null)
            {
                option = new Settings();
                option.SettingName = SettingKeys.SetPlaybackOption;
                _dataContext.Context.Settings.Add(option);
            }
            option.ValueDouble = (double)SetPlaybackOptionEnum.DevicePinDetect;
            await _dataContext.Context.SaveChangesAsync();

            var entryInDb = new Settings
            {
                SettingName = SettingKeys.Refresh,
                ValueString = "None"
            };

            using (var dc = new EfHolidayContext())
            {
                dc.Settings.AddOrUpdate(entryInDb);
                await dc.SaveChangesAsync();
            }

        }

        public List<AudioOptions>  AudioOptions
        {
            get { return _dataContext.Context.AudioOptions.OrderBy(x => x.Name).ToList(); }
        }

        private void OnCommandAddNewCommand()
        {
            int nextCommand = 0;
            // find the top-most value
            var c = DevicePatternSequences.OrderByDescending(x => x.OnAt + Math.Max(x.Duration, x.AudioOptions?.AudioDuration ?? 0)).FirstOrDefault();;
            if (c != null)
            {
                nextCommand = (c.OnAt + Math.Max(c.Duration, c.AudioOptions?.AudioDuration ?? 0)); 
            }
            var ioPort = DeviceSelected.DeviceIoPorts.First(x => x.CommandPin == -1);
            var audio = _dataContext.Context.AudioOptions.First(x => x.Name == "NONE");

            DevicePatternSelected.DevicePatternSequences.Add(new DevicePatternSequences()
                {
                    OnAt = nextCommand, 
                    Duration = 1000, 
                    DeviceIoPorts = ioPort,
                    AudioOptions = audio
            });
            OnPropertyChanged(() => DevicePatternSequences); // reloads in the ui.

        }

        private void OnCommandDeleteCommand()
        {
            if (DevicePatternSequenceSelected != null)
            {
                //if (DevicePatternSequenceSelected.IsAttached())
                    _dataContext.Context.DevicePatternSequences.Remove(DevicePatternSequenceSelected);
                DevicePatternSelected.DevicePatternSequences.Remove(DevicePatternSequenceSelected);
                DevicePatternSequenceSelected = null;
                OnPropertyChanged(() => DevicePatternSequences);
            }
        }

        private void OnCommandDeletePattern()
        {
            if (DevicePatternSelected != null)
            {
                //if (DevicePatternSelected.IsAttached())
                    _dataContext.Context.DevicePatterns.Remove(DevicePatternSelected);
                DeviceSelected.DevicePatterns.Remove(DevicePatternSelected);
                OnPropertyChanged(() => DevicePatterns);
                DevicePatternSelected = null;
            }
            
        }

        private void OnCommandAddNewPattern()
        {
            var pattern = new DevicePatterns {PatternName = "**New Pattern"};
            DeviceSelected.DevicePatterns.Add(pattern);
            OnPropertyChanged(() => DevicePatterns);
        }

        private void OnCommandSave()
        {
             _dataContext.Context.SaveChanges();
        }
    }
}
