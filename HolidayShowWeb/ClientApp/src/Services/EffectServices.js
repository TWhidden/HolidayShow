class EffectServices{
    getAllEffects = async () => {
        let response =  await fetch(`/api/DeviceEffects`)
        return await response.json();
    }
} 

export default new EffectServices();