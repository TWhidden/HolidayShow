class DevicePatternServices{

    GetDevicePatternsByDeviceId = async (id)=>{
        let response = await fetch(`/api/DevicePatterns/GetDevicePatternsByDeviceId/${id}`);
        return await response.json();
    }
} 

export default new DevicePatternServices();