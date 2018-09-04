class DevicePatternSequenceServices {

    sequenceCreate = async (deviceId, sequence) => {
        let options = {
            method: 'post',
            body: JSON.stringify(sequence),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        let response = await fetch(`/api/DevicePatternSequences/${deviceId}`, options)

        return await response.json();
    }

    sequenceGetByPatternId = async (patternId) => {
        let response = await fetch(`/api/DevicePatternSequences/SequenceByPatternId/${patternId}`)

        return await response.json();
    }

    sequenceSave = async (sequenceId, sequence) => {
        let options = {
            method: 'put',
            body: JSON.stringify(sequence),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/DevicePatternSequences/${sequenceId}`, options)
    }

    sequenceDelete = async (sequenceId) => {
        let options = {
            method: 'delete',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/DevicePatternSequences/${sequenceId}`, options)
    }
}

export default new DevicePatternSequenceServices();