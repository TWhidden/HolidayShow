class EffectServices {
    getAllEffects = async () => {
        let response = await fetch(`/api/DeviceEffects`)
        return await response.json();
    }

    createEffect = async (effect) => {
        let options = {
            method: 'post',
            body: JSON.stringify(effect),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        let response = await fetch(`/api/DeviceEffects`, options)
        return await response.json();
    }

    saveEffect = async (effect) => {
        let options = {
            method: 'put',
            body: JSON.stringify(effect),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/DeviceEffects/${effect.effectId}`, options)
    }

    deleteEffect = async (effectId) => {
        let options = {
            method: 'delete',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/DeviceEffects/${effectId}`, options)
    }
}

export default new EffectServices();