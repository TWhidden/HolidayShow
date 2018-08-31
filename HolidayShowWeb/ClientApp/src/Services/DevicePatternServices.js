class DevicePatternServices {

    getDevicePatternsByDeviceId = async (id) => {
        let response = await fetch(`/api/DevicePatterns/GetDevicePatternsByDeviceId/${id}`);
        return await response.json();
    }

    deletePatternByPatternId = async (id) => {
        let options = {
            method: 'delete',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/DevicePatterns/${id}`, options)
    }

    createPattern = async (pattern) => {
        let options = {
            method: 'post',
            body: JSON.stringify(pattern),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        let response = await fetch(`/api/DevicePatterns/`, options)

        return await response.json();
    }

    updatePattern = async (pattern) => {
        let options = {
            method: 'put',
            body: JSON.stringify(pattern),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        let response = await fetch(`/api/DevicePatterns/${pattern.devicePatternId}`, options)
    }
}

export default new DevicePatternServices();