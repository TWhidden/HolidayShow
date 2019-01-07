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

    exectionSet = async (value) => {
        let options = {
            method: 'put',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            }
        }

        await fetch(`/api/Settings/PlaybackOption/${value}`, options)
    }

    executionOff = async () => {
        await this.exectionSet(0);
    }

    executionCurrentOnly = async () => {
        await this.exectionSet(2);
    }

    executionRandom = async () => {
        await this.exectionSet(1);
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