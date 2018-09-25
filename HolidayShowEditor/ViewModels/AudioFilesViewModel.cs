using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HolidayShow.Data;
using HolidayShowEditor.BaseClasses;
using HolidayShowEditor.Services;
using Microsoft.Practices.Prism.Commands;
using Microsoft.WindowsAPICodePack.Shell;

namespace HolidayShowEditor.ViewModels
{
    //public class AudioFilesViewModel : ViewAttachedViewModelBase<IAudioFilesView>, IAudioFilesViewModel
    public class AudioFilesViewModel : ViewModelBase, IAudioFilesViewModel
    {
        private readonly IDbDataContext _dbDataContext;
        private AudioOptions _audioFileSelected;

        public AudioFilesViewModel(IDbDataContext dbDataContext)
        {
            _dbDataContext = dbDataContext;
            HeaderInfo = "Audio Files";
            CommandAudioAdd = new DelegateCommand(OnCommandAudioAdd);
            CommandAudioRemove = new DelegateCommand(OnCommandAudioRemove);
            CommandScanDirectory = new DelegateCommand(OnCommandScanDirectory);
        }

        public object HeaderInfo { get; set; }

        public List<AudioOptions> AudioFilesList
        {
            get { return _dbDataContext.Context.AudioOptions.Where(x => !x.IsNotVisable).OrderBy(x => x.Name).ToList(); }
        }

        public AudioOptions AudioFileSelected
        {
            get { return _audioFileSelected; }
            set { _audioFileSelected = value;
                OnPropertyChanged(()=>AudioFileSelected);
            }
        }

        public ICommand CommandAudioAdd { get; private set; }

        public ICommand CommandAudioRemove { get; private set; }

        private void OnCommandAudioAdd()
        {
            var audioFile = new AudioOptions()
                {
                    Name = "**New Audio File",
                    FileName = string.Empty,
                    AudioDuration = 0
                };
            _dbDataContext.Context.AudioOptions.Add(audioFile);
            _dbDataContext.Context.SaveChanges();
            OnPropertyChanged(() => AudioFilesList);
        }

        private void OnCommandAudioRemove()
        {
            if (AudioFileSelected == null) return;
            _dbDataContext.Context.AudioOptions.Remove(AudioFileSelected);
            _dbDataContext.Context.SaveChanges();
            AudioFileSelected = null;
            OnPropertyChanged(() => AudioFilesList);
        }

        public DelegateCommand CommandScanDirectory { get; private set; }

        private async void OnCommandScanDirectory()
        {
            try
            {
                // Get the configured base directory
                var baseDirectory =
                    await
                        _dbDataContext.Context.Settings.Where(x => x.SettingName == SettingKeys.FileBasePath)
                            .Select(x => x.ValueString)
                            .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(baseDirectory))
                {
                    MessageBox.Show("Base Directory not set in Setting section.");
                    return;
                }


                if (Directory.Exists(baseDirectory))
                {
                    // get all the audio files
                    var files = Directory.EnumerateFiles(baseDirectory, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".mp3") || s.EndsWith(".flac") || s.EndsWith(".m4a"));
                    
                    List<string> availableFIles = new List<string>();

                    foreach (var file in files)
                    {
                        var fileName = file.Replace(baseDirectory, "");
                        if (fileName.StartsWith(@"\"))
                        {
                            fileName = fileName.Substring(1, fileName.Length - 1);
                        }
                        availableFIles.Add(fileName);
                        ShellFile so = ShellFile.FromFilePath(file);
                        double nanoseconds;
                        double.TryParse(so.Properties.System.Media.Duration.Value.ToString(),
                        out nanoseconds);
                        Console.WriteLine(@"NanoSeconds: {0}", nanoseconds);
                        if (nanoseconds > 0)
                        {
                            int milliseconds = (int)Convert100NanosecondsToMilliseconds(nanoseconds) ;

                            var entry = _dbDataContext.Context.AudioOptions.FirstOrDefault(x => x.FileName == fileName);
                            if (entry == null)
                            {
                                entry = new AudioOptions();
                                entry.FileName = fileName;
                                entry.IsNotVisable = false;
                                entry.Name = Path.GetFileNameWithoutExtension(file);
                                entry.AudioDuration = milliseconds;
                                _dbDataContext.Context.AudioOptions.Add(entry);
                            }
                        }

                    }
                    
                    await _dbDataContext.Context.SaveChangesAsync();

                    // Cleanup and remove (later may disable) the audio file that are missing
                    // Ask the user if they would like to remove the missing files 
                    var onesToRemove =
                        _dbDataContext.Context.AudioOptions.Where(
                            dbFile => availableFIles.All(aviailableFile => aviailableFile != dbFile.FileName) && !dbFile.IsNotVisable).ToList();

                    var countToRemove = onesToRemove.Count;

                    if (countToRemove > 0)
                    {
                        var messageBoxConfirm = MessageBox.Show($"Remove missing {countToRemove} entries?",
                            $"Removing {countToRemove} will also modify patterns that use them",
                            MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.OK);
                        if (messageBoxConfirm == MessageBoxResult.OK)
                        {
                            _dbDataContext.Context.AudioOptions.RemoveRange(onesToRemove);
                            await _dbDataContext.Context.SaveChangesAsync();
                        }

                    }
                }

                
            }
            catch
            {
               // Ignore 
            }

            OnPropertyChanged(() => AudioFilesList);
            
        }

        public static double Convert100NanosecondsToMilliseconds(double nanoseconds)
        {
            // One million nanoseconds in 1 millisecond, 
            // but we are passing in 100ns units...
            return nanoseconds * 0.0001;
        }

    }
}
