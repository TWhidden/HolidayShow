using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
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
using Microsoft.WindowsAPICodePack.Shell;

namespace HolidayShowEditor.ViewModels
{
    //public class AudioFilesViewModel : ViewAttachedViewModelBase<IAudioFilesView>, IAudioFilesViewModel
    public class AudioFilesViewModel : ViewModelBase, IAudioFilesViewModel
    {
        private readonly IDbDataContext _dbDataContext;
        private AudioOptions _audioFileSelected;
        private string _audioFilesPath;

        public AudioFilesViewModel(IDbDataContext dbDataContext)
        {
            _dbDataContext = dbDataContext;
            HeaderInfo = "Audio Files";
            CommandAudioAdd = new DelegateCommand(OnCommandAudioAdd);
            CommandAudioRemove = new DelegateCommand(OnCommandAudioRemove);
            CommandScanDirectory = new DelegateCommand(OnCommandScanDirectory);
            AudioFilesPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "AudioFiles");
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
                    FileName = String.Empty,
                    AudioDuration = 0
                };
            _dbDataContext.Context.AudioOptions.InsertOnSubmit(audioFile);
            _dbDataContext.Context.SubmitChanges();
            OnPropertyChanged(() => AudioFilesList);
        }

        private void OnCommandAudioRemove()
        {
            if (AudioFileSelected == null) return;
            _dbDataContext.Context.AudioOptions.DeleteOnSubmit(AudioFileSelected);
            _dbDataContext.Context.SubmitChanges();
            AudioFileSelected = null;
            OnPropertyChanged(() => AudioFilesList);
        }

        public string AudioFilesPath
        {
            get { return _audioFilesPath; }
            set { _audioFilesPath = value;
                OnPropertyChanged(()=>AudioFilesPath);
            }
        }

        public DelegateCommand CommandScanDirectory { get; private set; }

        private void OnCommandScanDirectory()
        {
            try
            {
                if (Directory.Exists(AudioFilesPath))
                {
                    // get all the audio files
                    var files = Directory.EnumerateFiles(AudioFilesPath, "*.*", SearchOption.TopDirectoryOnly).Where(s => s.EndsWith(".mp3") || s.EndsWith(".flac"));
                    

                    foreach (var file in files)
                    {
                        var fileName = Path.GetFileName(file);
                        ShellFile so = ShellFile.FromFilePath(file);
                        double nanoseconds;
                        double.TryParse(so.Properties.System.Media.Duration.Value.ToString(),
                        out nanoseconds);
                        Console.WriteLine("NanaoSeconds: {0}", nanoseconds);
                        if (nanoseconds > 0)
                        {
                            int milliseconds = (int)Convert100NanosecondsToMilliseconds(nanoseconds) ;

                            var entry = _dbDataContext.Context.AudioOptions.FirstOrDefault(x => x.FileName == fileName);
                            if (entry == null)
                            {
                                entry = new AudioOptions();
                                entry.FileName = fileName;
                                
                                entry.AudioDuration = milliseconds;
                                _dbDataContext.Context.AudioOptions.InsertOnSubmit(entry);
                            }
                        }

                    }

                    _dbDataContext.Context.SubmitChanges();
                }

                
            }
            catch (Exception ex)
            {
                
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
