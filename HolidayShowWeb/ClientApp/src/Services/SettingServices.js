class SettingServices {
    executionRestart = async () => {

        let options = {
            method: 'put',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/Settings/RestartExecution`, options)
    }

    executionOff = async () => {

        let options = {
            method: 'put',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/Settings/PlaybackOption/0`, options)
    }

    executionCurrentOnly = async () => {

        let options = {
            method: 'put',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }


        await fetch(`/api/Settings/PlaybackOption/2`, options)
    }

    getAllSettings = async () =>
    {
        let response = await await fetch(`/api/Settings/`);
        return response.json();
    }

    createSetting = async (setting) =>
    {
        let options = {
            method: 'post',
            body: JSON.stringify(setting),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/Settings`, options)
    }

    saveSetting = async (setting) =>
    {
        let options = {
            method: 'put',
            body: JSON.stringify(setting),
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/Settings/${setting.settingName}`, options)
    }
}

export default new SettingServices();