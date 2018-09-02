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
}

export default new DevicePatternSequenceServices();