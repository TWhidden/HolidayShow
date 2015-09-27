using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using HolidayShowEndpointUniversalApp.Controllers;
using HolidayShowLibUniversal.Controllers;
using HolidayShowLibUniversal.Services;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace HolidayShowEndpointUniversalApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly IResolverService _resloverService;

        public MainPage()
        {
            this.InitializeComponent();
            // Not going to build up the infrastructure for just one view and view modol.
            // This will due for now.

            // For now, this is where the ResolverService will be created with the registered singleton
            _resloverService = new ResolverService();
            _resloverService.Register<IResolverService, IResolverService>(_resloverService);

            // Type used to register a request for audio playback
            _resloverService.Register<IAudioRequestController, AudioRequestController>();

            var availableInstances = new List<IAudioInstanceController>();
            // Create the audio instances
            var audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media0);
            availableInstances.Add(audioInstance);

            audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media1);
            availableInstances.Add(audioInstance);

            audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media2);
            availableInstances.Add(audioInstance);

            audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media3);
            availableInstances.Add(audioInstance);

            audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media4);
            availableInstances.Add(audioInstance);

            audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media5);
            availableInstances.Add(audioInstance);

            audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media6);
            availableInstances.Add(audioInstance);

            audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media7);
            availableInstances.Add(audioInstance);

            audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media8);
            availableInstances.Add(audioInstance);

            audioInstance = _resloverService.Resolve<AudioInstanceController>();
            audioInstance.SetMediaElement(Media9);
            availableInstances.Add(audioInstance);


            // Create the manager controller
            var audioManagerController = _resloverService.Resolve<AudioManagerController>(availableInstances);
            _resloverService.Register<IAudioManagerController, IAudioManagerController>(audioManagerController);

            var vm = _resloverService.Resolve<MainPageViewModel>(this);

            this.DataContext = vm;
        }
    }
}
