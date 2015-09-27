
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HolidayShow.Data;
using HolidayShowEditor.BaseClasses;
using HolidayShowEditor.Services;

namespace HolidayShowEditor.ViewModels
{
    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly IDbDataContext _dbDataContext;
        private System.Threading.Timer _refreshAndKeepAlive;

        private void Refresh()
        {
            OnPropertyChanged(() => CurrentPlaybackOption);
            OnPropertyChanged(() => SetList);
            OnPropertyChanged(() => DelayBetweenSets);
        }

        public SettingsViewModel(IDbDataContext dbDataContext )
        {
            _dbDataContext = dbDataContext;
            HeaderInfo = "Settings";

            _refreshAndKeepAlive = new Timer(x =>
            {
                Refresh();
            }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));
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
                    _dbDataContext.Context.Settings.Add(option);
                }
                option.ValueDouble = (double)value;
                _dbDataContext.Context.SaveChanges();

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
                    _dbDataContext.Context.Settings.Add(option);
                }
                option.ValueDouble = value;
                _dbDataContext.Context.SaveChanges();
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
                    _dbDataContext.Context.Settings.Add(option);
                }
                option.ValueDouble = value;
                _dbDataContext.Context.SaveChanges();
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
                    _dbDataContext.Context.Settings.Add(option);
                }
                option.ValueString = value;
                _dbDataContext.Context.SaveChanges();
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
                    _dbDataContext.Context.Settings.Add(option);
                }
                option.ValueString = value;
                _dbDataContext.Context.SaveChanges();
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
                    _dbDataContext.Context.Settings.Add(option);
                }
                option.ValueDouble = (value ? 1 : 0);
                _dbDataContext.Context.SaveChanges();
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
                    _dbDataContext.Context.Settings.Add(option);
                }
                option.ValueDouble = (value ? 1 : 0);
                _dbDataContext.Context.SaveChanges();
            }
        }


        public string FileBasePath
        {
            get
            {
                var option = _dbDataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.FileBasePath)
                                  .Select(x => x.ValueString)
                                  .FirstOrDefault();
                return option;
            }
            set
            {
                var option = _dbDataContext.Context.Settings.FirstOrDefault(x => x.SettingName == SettingKeys.FileBasePath);
                if (option == null)
                {
                    option = new Settings()
                    {
                        SettingName = SettingKeys.FileBasePath,
                        ValueString = string.Empty
                    };
                    _dbDataContext.Context.Settings.Add(option);
                }
                option.ValueString = value;
                _dbDataContext.Context.SaveChanges();
            }
        }


        
    }
}
