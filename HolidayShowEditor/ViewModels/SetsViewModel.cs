using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HolidayShow.Data;
using HolidayShowEditor.BaseClasses;
using HolidayShowEditor.Interfaces;
using HolidayShowEditor.Services;
using HolidayShowEditor.Views;
using Microsoft.Practices.Prism.Commands;

namespace HolidayShowEditor.ViewModels
{
    public class SetsViewModel : ViewAttachedViewModelBase<ISetsView>, ISetsViewModel
    {
        private readonly IDbDataContext _dataContext;
        private Sets _setSelected;
        private bool _isSetSelected;
        private SetSequences _setSequenceSelected;
        private int _patternExecuteIn;
        private int _patternMutiplier;

        public SetsViewModel(IDbDataContext dataContext )
        {
            _dataContext = dataContext;
            HeaderInfo = "Sets";
            CommandAddNewSet = new DelegateCommand(OnCommandAddNewSet);
            CommandRemoveSet = new DelegateCommand(OnCommandRemoveSet);
            CommandRemovePattern = new DelegateCommand(OnCommandRemovePattern);
            CommandAddPattern = new DelegateCommand(OnCommandAddPattern);
            CommandSave = new DelegateCommand(OnCommandSave);
            CommandDuplicatePattern = new DelegateCommand(OnCommandDuplicatePattern);

        }

        public object HeaderInfo { get; set; }

        public List<Sets> Sets{get { return _dataContext.Context.Sets.OrderBy(x => x.SetName).ToList(); }}

        public Sets SetSelected
        {
            get { return _setSelected; }
            set { _setSelected = value;
                OnPropertyChanged(()=>SetSelected);
                IsSetSelected = (value != null);
                OnPropertyChanged(() => SetSequences);
            }
        }

        public bool IsSetSelected
        {
            get { return _isSetSelected; }
            set { _isSetSelected = value;
                OnPropertyChanged(()=>IsSetSelected);
            }
        }

        public List<SetSequences> SetSequences { get
        {
            if (SetSelected == null) return null;
            return SetSelected.SetSequencesList.OrderBy(x => x.OnAt).ToList();
        } }

        public SetSequences SetSequenceSelected
        {
            get { return _setSequenceSelected; }
            set { _setSequenceSelected = value;OnPropertyChanged(()=>SetSequenceSelected);}
        }

        public ICommand CommandAddNewSet { get; private set; }

        public ICommand CommandRemoveSet { get; private set; }

        public ICommand CommandAddPattern { get; private set; }

        public ICommand CommandRemovePattern { get; private set; }

        public ICommand CommandSave { get; private set; }

        private void OnCommandSave()
        {
            _dataContext.Context.SubmitChanges();
        }

        public List<DevicePatterns> DevicePatternsList { get
        {
            var list =  _dataContext.Context.DevicePatterns.ToList().OrderBy(x => x.PatternDescription).ToList();
            return list;
        } }

        private void OnCommandAddPattern()
        {
            if (SetSelected == null) return;
            if (!_dataContext.Context.DevicePatterns.Any()) return;


            // create at the end, one second after everyone.
            var topNumber = SetSequences.OrderBy(x => x.OnAt).Select(x => x.OnAt).LastOrDefault();

            var newSetSequence = new SetSequences()
                {
                    DevicePatterns = _dataContext.Context.DevicePatterns.First(),
                    OnAt = topNumber + 1000,
                };

            SetSelected.SetSequencesList.Add(newSetSequence);
            _dataContext.Context.SubmitChanges();
            OnPropertyChanged(()=>SetSequences);
        }

        private void OnCommandRemovePattern()
        {
            if (SetSequenceSelected == null) return;

            _dataContext.Context.SetSequences.DeleteOnSubmit(SetSequenceSelected);
            SetSelected.SetSequencesList.Remove(SetSequenceSelected);
            _dataContext.Context.SubmitChanges();
            OnPropertyChanged(()=>SetSequences);
        }

        private void OnCommandAddNewSet()
        {
            var set = new Sets()
                {
                    IsDisabled = true,
                    SetName = "**New Set",
                };

            _dataContext.Context.Sets.InsertOnSubmit(set);
            _dataContext.Context.SubmitChanges();
            OnPropertyChanged(() => Sets);
        }

        private void OnCommandRemoveSet()
        {
            if (SetSelected != null)
            {
                _dataContext.Context.Sets.DeleteOnSubmit(SetSelected);
                _dataContext.Context.SubmitChanges();
                OnPropertyChanged(() => Sets);
                SetSelected = null;
            }
        }

        public int PatternExecuteIn
        {
            get { return _patternExecuteIn; }
            set { _patternExecuteIn = value;
            OnPropertyChanged(()=>PatternExecuteIn);
            }
        }

        public int PatternMutiplier
        {
            get { return _patternMutiplier; }
            set { _patternMutiplier = value;
            OnPropertyChanged(()=>PatternMutiplier);
            }
        }

        public DelegateCommand CommandDuplicatePattern { get; private set; }

        private void OnCommandDuplicatePattern()
        {

            if (SetSequenceSelected == null) return;
            // create at the end, one second after everyone.
            
           
            var topNumber = SetSequenceSelected.OnAt ;

            for (var i = 1; i <= PatternMutiplier; i++)
            {
                var newSetSequence = new SetSequences()
                {
                    DevicePatterns = SetSequenceSelected.DevicePatterns,
                    OnAt = topNumber + (PatternExecuteIn * i),
                };

                SetSelected.SetSequencesList.Add(newSetSequence);
            
            }

            _dataContext.Context.SubmitChanges();

            OnPropertyChanged(()=>SetSequences);
        }
    }
}
