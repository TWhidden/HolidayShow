class DeviceServices{

    getAllDevices = async ()=>{
        let response = await fetch("/api/Devices");
        return await response.json();
    }

    saveDevice = async (device) =>{

        let options = {
                method: 'put',
                body: JSON.stringify(device),
                headers: {
                  'Accept': 'application/json',
                  'Content-Type': 'application/json'
                }
        }

        await fetch(`/api/Devices/${device.deviceId}`, options)
    }
    
} 

export default new DeviceServices();