import {
    observable,
    action,
    configure,
    runInAction,
    computed
} from 'mobx';
import SetServices from '../Services/SetServices';
import SettingServices from '../Services/SettingServices';
import * as Enumerable from "linq-es5";
import DeviceServices from '../Services/DeviceServices'
import AudioServices from '../Services/AudioServices';
import EffectServices from '../Services/EffectServices';
import EffectsAvailableServices from '../Services/EffectsAvailableServices';
import DeviceIoPortServices from '../Services/DeviceIoPortServices';

configure({
    enforceActions: 'always'
})

const SettingKeyCurrentSet = "CurrentSet";

class AppStore {
    @observable isBusy;
    @observable errorMessage;
    @observable sets = [];
    @observable settings = [];
    @observable devices = [];
    @observable audioFiles = [];
    @observable effects = [];
    @observable effectsAvailable = [];
    @observable pinsAvailable = [];
    @observable devicePatterns = [];

    @action isBusySet(busyState) {
        clearTimeout(this.timer);
        if (!busyState) {
            this.isBusy = false;
            return;
        }

        this.timer = setTimeout(() => runInAction(() => this.isBusy = true), 500);
    }

    @action errorMessageSet(message) {
        this.errorMessage = message;
    }

    @computed get currentSet() {

        // hack for the settings to refresh (computed)
        // TODO: fix this hack
        console.log(this.settings.length);

        var currentSet = this.settingGet(SettingKeyCurrentSet);

        if (currentSet == null) {
            return -1;
        }
        return currentSet.valueDouble;
    }

    @action currentSetSet = async (setId) => {
        this.settingSet(SettingKeyCurrentSet, '', setId);
    }

    @action setsGetAll = async () => {
        try {
            this.isBusySet(true);
            let sets = await SetServices.getAllSets();
            runInAction(() => {
                this.sets = sets;
            });
        } catch (e) {
            this.errorMessageSet(e.Message);
        } finally {
            this.isBusySet(false);
        }
    }

    @action settingsGetAll = async () => {

        try {
            this.isBusySet(true);
            let settings = await SettingServices.getAllSettings();
            runInAction(() => {
                this.settings = settings;
            })
        } catch (e) {
            this.errorMessageSet(e.Message);
        } finally {
            this.isBusySet(false);
        }
    }

    settingGetAsync = async (settingName) => {
        if (this.sets.length === 0) {
            await this.settingsGetAll();
        }

        return Enumerable.asEnumerable(this.settings)
            .Where(setting => setting.settingName === settingName)
            .FirstOrDefault();
    }

    @action settingGet(settingName) {
        return Enumerable.asEnumerable(this.settings)
            .Where(setting => setting.settingName === settingName)
            .FirstOrDefault();
    }

    @action settingSet = async (settingKey, valueString, valueDouble) => {
        let set = this.settingGet(settingKey);
        if (set == null) {
            set = {
                settingName: settingKey,
                valueString: valueString,
                valueDouble: valueDouble
            };
            this.setting.push(set);
        }

        // Update the object
        set.valueDouble = valueDouble;
        set.valueString = valueString;

        // Save to the API
        try {
            this.isBusySet(true);
            await SettingServices.saveSetting(set);
        } catch (e) {
            this.errorMessageSet(e.Message);
        } finally {
            this.isBusySet(false);
        }
    }

    @action exectionSet = async (value) => {
        try {
            this.isBusySet(true);
            await SettingServices.exectionSet(value);
        } catch (e) {
            this.errorMessageSet(e.Message);
        } finally {
            this.isBusySet(false);
        }
    }

    @action executionRestart = async () => {
        try {
            this.isBusySet(true);
            await SettingServices.executionRestart();
        } catch (e) {
            this.errorMessageSet(e.Message);
        } finally {
            this.isBusySet(false);
        }
    }

    @action executionCurrentOnly = async () => {
        try {
            this.isBusySet(true);
            await SettingServices.executionCurrentOnly();
        } catch (e) {
            this.errorMessageSet(e.Message);
        } finally {
            this.isBusySet(false);
        }
    }

    @action executionRandom = async () => {
        try {
            this.isBusySet(true);
            await SettingServices.executionRandom();
        } catch (e) {
            this.errorMessageSet(e.Message);
        } finally {
            this.isBusySet(false);
        }
    }

    @action devicesGetAllAsync = async () => {
        try {
            this.isBusySet(true);

            let d = await DeviceServices.getAllDevices()
            runInAction(() => {
                this.devices = d;
            })

            return d;
        } catch (e) {
            this.errorMessageSet(e.Message);
        } finally {
            this.isBusySet(false);
        }
    }

    deviceGetByDeviceId(deviceId) {
        return Enumerable.asEnumerable(this.devices)
            .Where(d => d.deviceId === Number(deviceId))
            .FirstOrDefault();
    }

    deviceSave = async (device) => {
        try {
            this.isBusySet(true);
            await DeviceServices.saveDevice(device);
        } catch (e) {
            this.errorMessageSet(e.message);
        } finally {
            this.isBusySet(false);
        }
    }

    audioLoadAsync = async () => {
        try {
            this.isBusySet(true);
            let audioFiles = await AudioServices.getAllAudioOptions();
            audioFiles.forEach(audioFile => {
                audioFile.value = audioFile.audioId;
                audioFile.label = `${audioFile.name} (${this.millisecondsToStr(audioFile.audioDuration*1)})`;
            });
            runInAction(() => {
                this.audioFiles = audioFiles;
            })
            return audioFiles;
        } catch (e) {
            this.errorMessageSet(e.message);
        } finally {
            this.isBusySet(false);
        }
    }

    /**
     * https://stackoverflow.com/a/8212878/1004187
     */
    millisecondsToStr (milliseconds) {
        var oneHour = 3600000;
        var oneMinute = 60000;
        var oneSecond = 1000;
        var seconds = 0;
        var minutes = 0;
        var hours = 0;
        var result;

        if (milliseconds >= oneHour) {
            hours = Math.floor(milliseconds / oneHour);
        }

        milliseconds = hours > 0 ? (milliseconds - hours * oneHour) : milliseconds;

        if (milliseconds >= oneMinute) {
            minutes = Math.floor(milliseconds / oneMinute);
        }

        milliseconds = minutes > 0 ? (milliseconds - minutes * oneMinute) : milliseconds;

        if (milliseconds >= oneSecond) {
            seconds = Math.floor(milliseconds / oneSecond);
        }

        milliseconds = seconds > 0 ? (milliseconds - seconds * oneSecond) : milliseconds;

        if (hours > 0) {
            result = (hours > 9 ? hours : "0" + hours) + ":";
        } else {
            result = "00:";
        }

        if (minutes > 0) {
            result += (minutes > 9 ? minutes : "0" + minutes) + ":";
        } else {
            result += "00:";
        }

        if (seconds > 0) {
            result += (seconds > 9 ? seconds : "0" + seconds) + ":";
        } else {
            result += "00:";
        }

        if (milliseconds > 0) {
            result += (milliseconds > 9 ? milliseconds : "0" + milliseconds);
        } else {
            result += "00";
        }

        return result;
    }

    effectsLoadAsync = async () =>{
        try {
            this.isBusySet(true);
            let effects = await EffectServices.getAllEffects();
            runInAction(() => {
                this.effects = effects;
            });
            return effects;
        } catch (e) {
            this.errorMessageSet(e.message);
        } finally {
            this.isBusySet(false);
        }
    }

    effectDelete = async (effect) => {
        try {
            this.isBusySet(true);

            await EffectServices.deleteEffect(effect.effectId);
            
            runInAction(() => {
                this.effects.splice(this.effects.indexOf(effect), 1);
            });

        } catch (e) {
            this.errorMessageSet(e.message);
        } finally {
            this.isBusySet(false);
        }
    }

    effectCreate = async (effect) =>{
        try {
            this.isBusySet(true);

            effect = await EffectServices.createEffect(effect);
            runInAction(() => {
                this.effects.push(effect);
            });

            return effect;

        } catch (e) {
            this.errorMessageSet(e.message);
        } finally {
            this.isBusySet(false);
        }
    }

    effectsAvailableLoadAsync = async () => {
        try {
            this.isBusySet(true);
            let effectsAvailable = await EffectsAvailableServices.getAllAvailableEffects();
            effectsAvailable.forEach(item => {
                item.label = item.displayName;
                item.value = item.effectInstructionId;
            });
                       
            runInAction(() => {
                this.effectsAvailable = effectsAvailable;
            });

            return effectsAvailable;
        } catch (e) {
            this.errorMessageSet(e.message);
        } finally {
            this.isBusySet(false);
        }
    }

    ioPortsLoadAsync = async () => {
        try {
            this.isBusySet(true);
            let pinsAvailable = await DeviceIoPortServices.ioPortGetAll();
          
            runInAction(() => {
                this.pinsAvailable = pinsAvailable;
            })
            return pinsAvailable;
        } catch (e) {
            this.errorMessageSet(e.message);
        } finally {
            this.isBusySet(false);
        }
    }

}

export default AppStore;