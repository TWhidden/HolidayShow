using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HolidayShow.Data;
using HolidayShowEditor.BaseClasses;
using HolidayShowEditor.Interfaces;
using HolidayShowEditor.Services;
using HolidayShowEditor.Views;
using Microsoft.Practices.Prism.Commands;

namespace HolidayShowEditor.ViewModels
{
    public class DeviceViewModel : ViewAttachedViewModelBase<IDeviceView>, IDeviceViewModel, IHeaderedViewModel 
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
        }

        public object HeaderInfo { get; set; }

        public ObservableCollection<HolidayShow.Data.Devices> Devices { get; set; }

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
                OnPropertyChanged(() => DeviceIoPortsList);
                
            }
        }

        public List<DeviceIoPorts>  DeviceIoPortsList
        {
            get { if (DeviceSelected == null) return null;

                return DeviceSelected.DeviceIoPortsList.Where(x => !x.IsNotVisable).OrderBy(x => x.CommandPin).ToList();
            }
        }

        public List<DevicePatterns> DevicePatterns
        {
            get
            {
                if (DeviceSelected != null)
                {
                    return DeviceSelected.DevicePatternsList.OrderBy(x => x.PatternName).ToList();
                }
                return null;
            }
        }

        public List<DevicePatternSequences> DevicePatternSequences
        {
            get
            {
                if (DevicePatternSelected != null)
                {
                    return DevicePatternSelected.DevicePatternSequencesList.OrderBy(x => x.OnAt).ToList();
                }
                return null;
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

        public List<AudioOptions>  AudioOptionsList
        {
            get { return _dataContext.Context.AudioOptions.OrderBy(x => x.Name).ToList(); }
        }

        private void OnCommandAddNewCommand()
        {
            int nextCommand = 0;
            // find the top-most value
            var c = DevicePatternSequences.OrderByDescending(x => x.OnAt).FirstOrDefault();;
            if (c != null)
            {
                nextCommand = (c.OnAt); 
                // detect the duration of this last command.

                nextCommand = c.AudioOptions.AudioDuration > c.Duration ? +c.AudioOptions.AudioDuration : +c.Duration;
            }

            DevicePatternSelected.DevicePatternSequencesList.Add(new DevicePatternSequences()
                {
                    OnAt = nextCommand, 
                    Duration = 1000, 
                    DeviceIoPorts = DeviceSelected.DeviceIoPortsList.First(x => x.CommandPin == -1),
                    AudioOptions = _dataContext.Context.AudioOptions.First(x => x.Name  == "NONE")
                });
            OnPropertyChanged(() => DevicePatternSequences); // reloads in the ui.

        }

        private void OnCommandDeleteCommand()
        {
            if (DevicePatternSequenceSelected != null)
            {
                if (DevicePatternSequenceSelected.IsAttached())
                    _dataContext.Context.DevicePatternSequences.DeleteOnSubmit(DevicePatternSequenceSelected);
                DevicePatternSelected.DevicePatternSequencesList.Remove(DevicePatternSequenceSelected);
                DevicePatternSequenceSelected = null;
                OnPropertyChanged(() => DevicePatternSequences);
            }
        }

        private void OnCommandDeletePattern()
        {
            if (DevicePatternSelected != null)
            {
                if (DevicePatternSelected.IsAttached())
                    _dataContext.Context.DevicePatterns.DeleteOnSubmit(DevicePatternSelected);
                DeviceSelected.DevicePatternsList.Remove(DevicePatternSelected);
                OnPropertyChanged(() => DevicePatterns);
                DevicePatternSelected = null;
            }
            
        }

        private void OnCommandAddNewPattern()
        {
            var pattern = new DevicePatterns {PatternName = "**New Pattern"};
            DeviceSelected.DevicePatternsList.Add(pattern);
            OnPropertyChanged(() => DevicePatterns);
        }

        private void OnCommandSave()
        {
             _dataContext.Context.SubmitChanges();
        }
    }
}
