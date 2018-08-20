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
    
} 

export default new DeviceIoPortServices();