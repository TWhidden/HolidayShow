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

        //let response = await fetch(`/api/Devices/${device.deviceId}`, options)
        let response = await fetch(`/api/Devices/${device.deviceId}`, options)
        return await response.json();
    }
    
} 

export default new DeviceServices();