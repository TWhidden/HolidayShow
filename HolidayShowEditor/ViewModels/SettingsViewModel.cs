
using System.Collections.Generic;
using System.Linq;
using HolidayShow.Data;
using HolidayShowEditor.BaseClasses;
using HolidayShowEditor.Services;
using HolidayShowEditor.Views;

namespace HolidayShowEditor.ViewModels
{
    public class SettingsViewModel : ViewAttachedViewModelBase<ISettingsView>, ISettingsViewModel
    {
        private readonly IDbDataContext _dbDataContext;

        public SettingsViewModel(IDbDataContext dbDataContext )
        {
            _dbDataContext = dbDataContext;
            HeaderInfo = "Settings";
        }

        private object _headerInfo;
        public object HeaderInfo
        {
            get { return _headerInfo; }
            set { _headerInfo = value;
                OnPropertyChanged(()=>HeaderInfo);
            }
        }

        public SetPlaybackOptionEnum CurrentPlaybackOption
        {
            get
            {
                var option =
                    _dbDataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.SetPlaybackOption)
                                  .Select(x => x.ValueDouble)
                                  .FirstOrDefault();
                return (SetPlaybackOptionEnum) option;
            }
            set
            {
                var option = _dbDataContext.Context.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.SetPlaybackOption);
                if (option == null)
                {
                    option = new Settings()
                        {
                            SettingName = SettingKeys.SetPlaybackOption,
                            ValueString = string.Empty
                        };
                    _dbDataContext.Context.Settings.InsertOnSubmit(option);
                }
                option.ValueDouble = (double)value;
                _dbDataContext.Context.SubmitChanges();

                OnPropertyChanged(() => SetList); // a little hack because I dont want to use IActiveAware at the moment.
            }
        }

        public int DelayBetweenSets
        {
            get
            {
                var option =
                    _dbDataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.DelayBetweenSets)
                                  .Select(x => x.ValueDouble)
                                  .FirstOrDefault();
                return (int)option;
            }
            set
            {
                var option = _dbDataContext.Context.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.DelayBetweenSets);
                if (option == null)
                {
                    option = new Settings()
                    {
                        SettingName = SettingKeys.DelayBetweenSets,
                        ValueString = string.Empty
                    };
                    _dbDataContext.Context.Settings.InsertOnSubmit(option);
                }
                option.ValueDouble = value;
                _dbDataContext.Context.SubmitChanges();
            }
        }


        public int CurrentSet
        {
            get
            {
                var option =
                    _dbDataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.CurrentSet)
                                  .Select(x => x.ValueDouble)
                                  .FirstOrDefault();
                return (int)option;
            }
            set
            {
                var option = _dbDataContext.Context.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.CurrentSet);
                if (option == null)
                {
                    option = new Settings()
                    {
                        SettingName = SettingKeys.CurrentSet,
                        ValueString = string.Empty
                    };
                    _dbDataContext.Context.Settings.InsertOnSubmit(option);
                }
                option.ValueDouble = value;
                _dbDataContext.Context.SubmitChanges();
            }
        }

        public List<Sets> SetList
        {
            get { return _dbDataContext.Context.Sets.OrderBy(x => x.SetName).ToList(); }
        }

        public Sets SetSelected
        {
            get
            {
                // find the current set
                return _dbDataContext.Context.Sets.FirstOrDefault(x => x.SetId == CurrentSet);
            }

            set
            {
                if (value != null)
                    CurrentSet = value.SetId;

            }
        }

        public string OnAt
        {
            get
            {
                var option = _dbDataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.OnAt)
                                  .Select(x => x.ValueString)
                                  .FirstOrDefault();
                return option;
            }
            set
            {
                var option = _dbDataContext.Context.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.OnAt);
                if (option == null)
                {
                    option = new Settings()
                    {
                        SettingName = SettingKeys.OnAt,
                        ValueString = string.Empty
                    };
                    _dbDataContext.Context.Settings.InsertOnSubmit(option);
                }
                option.ValueString = value;
                _dbDataContext.Context.SubmitChanges();
            }
        }

        public string OffAt
        {
            get
            {
                var option =
                    _dbDataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.OffAt)
                                  .Select(x => x.ValueString)
                                  .FirstOrDefault();
                return option;
            }
            set
            {
                var option = _dbDataContext.Context.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.OffAt);
                if (option == null)
                {
                    option = new Settings()
                    {
                        SettingName = SettingKeys.OffAt,
                        ValueString = string.Empty
                    };
                    _dbDataContext.Context.Settings.InsertOnSubmit(option);
                }
                option.ValueString = value;
                _dbDataContext.Context.SubmitChanges();
            }
        }

        public bool IsDangerEnabled
        {
            get
            {
                var option =
                    _dbDataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.IsDanagerEnabled)
                                  .Select(x => x.ValueDouble)
                                  .FirstOrDefault();
                return ((int)option == 1);
            }
            set
            {
                var option = _dbDataContext.Context.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.IsDanagerEnabled);
                if (option == null)
                {
                    option = new Settings()
                    {
                        SettingName = SettingKeys.IsDanagerEnabled,
                        ValueString = string.Empty
                    };
                    _dbDataContext.Context.Settings.InsertOnSubmit(option);
                }
                option.ValueDouble = (value ? 1 : 0);
                _dbDataContext.Context.SubmitChanges();
            }
        }

        public bool IsAudioEnabled
        {
            get
            {
                var option =
                    _dbDataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.IsAudioEnabled)
                                  .Select(x => x.ValueDouble)
                                  .FirstOrDefault();
                return ((int)option == 1);
            }
            set
            {
                var option = _dbDataContext.Context.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.IsAudioEnabled);
                if (option == null)
                {
                    option = new Settings()
                    {
                        SettingName = SettingKeys.IsAudioEnabled,
                        ValueString = string.Empty
                    };
                    _dbDataContext.Context.Settings.InsertOnSubmit(option);
                }
                option.ValueDouble = (value ? 1 : 0);
                _dbDataContext.Context.SubmitChanges();
            }
        }
    }
}
