class DeviceIoPortServices{

    ioPortIdentify = async (id) =>{

        let options = {
                method: 'put',
                headers: {
                  'Accept': 'application/json',
                  'Content-Type': 'application/json'
                }
        }

        await fetch(`/api/DeviceIoPorts/PutDeviceIoPortIdentify/${id}`, options)
    }

    ioPortGetByDeviceId = async (deviceId) => {
        let response =  await fetch(`/api/DeviceIoPorts/ByDeviceId/${deviceId}`)
        return await response.json();
    }

    ioPortGetAll = async () => {
        let response =  await fetch(`/api/DeviceIoPorts`)
        return await response.json();
    }

    ioPortUpdate = async (ioPort) => {
        let options = {
            method: 'put',
            body: JSON.stringify(ioPort),
            headers: {
              'Accept': 'application/json',
              'Content-Type': 'application/json'
            }
    }

    await fetch(`/api/DeviceIoPorts/${ioPort.deviceIoPortId}`, options)
    }
    
} 

export default new DeviceIoPortServices();