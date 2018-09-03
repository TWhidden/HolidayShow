class AudioServices{
    getAllAudioOptions = async () => {
        let response =  await fetch(`/api/AudioOptions`)
        return await response.json();
    }
} 

export default new AudioServices();