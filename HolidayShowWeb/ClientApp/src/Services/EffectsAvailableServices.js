class EffectsAvailableServices {
    getAllAvailableEffects = async () => {
        let response = await fetch(`/api/EffectInstructionsAvailables`)
        return await response.json();
    }

}

export default new EffectsAvailableServices();