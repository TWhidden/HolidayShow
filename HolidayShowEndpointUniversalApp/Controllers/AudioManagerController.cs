﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using HolidayShowLibUniversal.Controllers;
using HolidayShowLibUniversal.Services;

namespace HolidayShowEndpointUniversalApp.Controllers
{
    public class AudioManagerController : IAudioManagerController
    {
        private readonly IResolverService _resolverService;
        private readonly ConcurrentQueue<IAudioInstanceController> _availableAudioInstances = new ConcurrentQueue<IAudioInstanceController>();


        public AudioManagerController(List<IAudioInstanceController> availableAudioInstances, IResolverService resolverService)
        {
            _resolverService = resolverService;
            // The platform that creates this class will supply its max number of supported audio instances
            // in the abstract controller form. This is because there may be requirements for creating and pooling
            // instances for the system.
            foreach (var audioInstanceController in availableAudioInstances)
            {
                audioInstanceController.Complete += AudioInstanceController_Complete;
                _availableAudioInstances.Enqueue(audioInstanceController);
            }
        }

        private void AudioInstanceController_Complete(object sender, IAudioRequestController e)
        {
            // When complete is called, it will insert itself back into the queue of available instance controllers
            _availableAudioInstances.Enqueue(sender as IAudioInstanceController);
        }

        /// <summary>
        /// Private function which will prepare the media from the request and pull form the queue an
        /// available playback controller for the audio.
        /// </summary>
        /// <param name="desiredRequestController"></param>
        /// <returns></returns>
        private async Task<IAudioInstanceController> TryPlayAudio(IAudioRequestController desiredRequestController)
        {
            // Attempt to get the Uri for the media needed to play before dequing a media item
            var mediaUri = await desiredRequestController.FileReady();
            if (mediaUri == null)
                return null; // Could not aquire a media URI to play

            // Try to Dequeue an available audio instance controller
            IAudioInstanceController c;
            if (_availableAudioInstances.TryDequeue(out c))
            {
                // get the mediaUrl
                c.PlayMediaUri(desiredRequestController, mediaUri);
                return c;
            }

            return null; // Could not dequeue a controller. THis means that all available are in use.
        }

        public void StopAllAudio()
        {
            // Stop all the running audios
            foreach (var c in _availableAudioInstances)
            {
                c.StopPlayback();
            }
        }

        public async Task<IAudioRequestController> RequestAndPlay(string fileName)
        {
            var newReuqest = _resolverService.Resolve<IAudioRequestController>();
            newReuqest.FileName = fileName;
            var playerController = await TryPlayAudio(newReuqest);
            if (playerController != null)
            {
                newReuqest.OnStop += ((s, e) => { playerController.StopPlayback(); });
                return newReuqest;
            }
            else
            {
                return null;
            }
        }
    }
}
