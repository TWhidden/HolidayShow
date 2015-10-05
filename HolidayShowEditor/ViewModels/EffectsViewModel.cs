using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using HolidayShow.Data;
using HolidayShowEditor.BaseClasses;
using HolidayShowEditor.Services;
using Microsoft.Practices.Prism.Commands;

namespace HolidayShowEditor.ViewModels
{
    public class EffectsViewModel : ViewModelBase, IEffectsViewModel
    {
        private readonly IDbDataContext _dataContext;
        private DeviceEffects _effectSelected;

        public EffectsViewModel(IDbDataContext dataContext)
        {
            _dataContext = dataContext;
            HeaderInfo = "Effects";
        }

        public List<DeviceEffects> Effects => _dataContext.Context.DeviceEffects.ToList();

        public DeviceEffects EffectSelected
        {
            get { return _effectSelected; }
            set
            {
                if (SetProperty(ref _effectSelected, value))
                {

                }
            }
        }

        public List<EffectInstructionsAvailable> EffectInstructionsAvailable
            => _dataContext.Context.EffectInstructionsAvailable.ToList();

        public object HeaderInfo { get; set; }

        public DelegateCommand CommandSave => new DelegateCommand(OnCommandSave);

        public async void OnCommandSave()
        {
            await _dataContext.Context.SaveChangesAsync();
        }

        public DelegateCommand CommandAddPattern => new DelegateCommand(OnCommandAddPattern);

        public async void OnCommandAddPattern()
        {
            if (!_dataContext.Context.EffectInstructionsAvailable.Any()) return;

            var newEffect = new DeviceEffects
            {
                EffectName = "**New Effect",
                Duration = 0,
                InstructionMetaData = "DEVPINS=1:1,1:2;DUR=50",
                EffectInstructionId = _dataContext.Context.EffectInstructionsAvailable.First().EffectInstructionId
            };

            _dataContext.Context.DeviceEffects.Add(newEffect);

            await _dataContext.Context.SaveChangesAsync();

            EffectSelected = newEffect;

            OnPropertyChanged(()=>Effects);
        }

        public DelegateCommand CommandRemovePattern => new DelegateCommand(OnCommandRemovePattern);

        public async void OnCommandRemovePattern()
        {
            if (EffectSelected != null)
            {
                _dataContext.Context.DeviceEffects.Remove(EffectSelected);
                await _dataContext.Context.SaveChangesAsync();
            }

            EffectSelected = await _dataContext.Context.DeviceEffects.FirstOrDefaultAsync();

            OnPropertyChanged(()=>Effects);
        }

    }
}
