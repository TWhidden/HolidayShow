import React, { Component } from 'react';
import {inject, observer} from 'mobx-react';
import {observable, runInAction} from 'mobx';
import SettingServices from '../Services/SettingServices';
import TextField from '@material-ui/core/TextField';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Switch from '@material-ui/core/Switch';
import * as Enumerable from "linq-es5";

const delayMs = "SetDelayMs";
const audioTimeOffAt = "AudioTimeOffAt";
const audioTimeOnAt = "AudioTimeOnAt";
const timeOffAt = "TimeOffAt";
const timeOnAt = "TimeOnAt";
const fileBasePath = "FileBasePath";
const isAudioEnabled = "IsAudioEnabled";
const isDangerEnabled = "IsDangerEnabled";

@inject("appStore")
@observer
class SettingsEditor extends Component {

    @observable delayBetweenSets = 0;
    @observable onAt = "";
    @observable offAt = "";
    @observable audioOnAt = "";
    @observable audioOffAt = "";
    @observable enableDangerPins = "";
    @observable enableAudio = "";
    @observable audioFileLocation = "";
    @observable settings = [];

    componentDidMount = async () => {
        this.getAllSetting();
    }

    getAllSetting = async () => {
        try {
            this.props.appStore.isBusySet(true);

            let settings = await SettingServices.getAllSettings();

            settings = Enumerable.AsEnumerable(settings);

            let delayBetweenSets = settings.Where(x => x.settingName === delayMs).Select(x => x.valueDouble).FirstOrDefault();
            let onAt = settings.Where(x => x.settingName === timeOnAt).Select(x => x.valueString).FirstOrDefault();
            let offAt = settings.Where(x => x.settingName === timeOffAt).Select(x => x.valueString).FirstOrDefault();
            let audioOnAt = settings.Where(x => x.settingName === audioTimeOnAt).Select(x => x.valueString).FirstOrDefault();
            let audioOffAt = settings.Where(x => x.settingName === audioTimeOffAt).Select(x => x.valueString).FirstOrDefault();
            let enableDangerPins = settings.Where(x => x.settingName === isDangerEnabled).Select(x => x.valueDouble).FirstOrDefault();
            let enableAudio = settings.Where(x => x.settingName === isAudioEnabled).Select(x => x.valueDouble).FirstOrDefault();
            let audioFileLocation = settings.Where(x => x.settingName === fileBasePath).Select(x => x.valueString).FirstOrDefault();

            runInAction(()=>{
                this.delayBetweenSets = delayBetweenSets;
                this.onAt = onAt;
                this.offAt = offAt;
                this.audioOnAt = audioOnAt;
                this.audioOffAt = audioOffAt;
                this.enableDangerPins = enableDangerPins;
                this.enableAudio = enableAudio;
                this.audioFileLocation = audioFileLocation;
                this.settings = settings;
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleSaveSetting = async (settingName, valueString, valueDouble) => {
        try {
            this.props.appStore.isBusySet(true);

            let setting = {
                settingName,
                valueString,
                valueDouble
            }

            // see if the current state has this key.
            // if it does not, we need to create the object
            if (this.settings.Where(x => x.settingName === settingName).FirstOrDefault() == null) {
                await SettingServices.createSetting(setting);
                await this.getAllSetting();
            } else {
                await SettingServices.saveSetting(setting);
            }

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    setIsBusy(busyState) {
        clearTimeout(this.timer);
        if (!busyState) {
            this.props.appStore.isBusySet(false);
            return;
        }

        this.timer = setTimeout(() => this.props.appStore.isBusySet(true), 250);
    }

    render() {
        return (
            <div style={{ display: "flex", flexDirection: "column", height: "100vh" }}>

                <TextField
                    style={{ width: "75px" }}
                    label={`Delay between executions:`}
                    value={this.delayBetweenSets}
                    onChange={(evt) => {
                        this.handleSaveSetting(delayMs, "", evt.target.value);
                        runInAction(()=>{
                            this.delayBetweenSets = evt.target.value;
                        });
                    }}
                    margin="normal"
                />

                <FormControlLabel
                    control={
                        <Switch
                            checked={this.enableAudio === 1}
                            onChange={(evt) => {
                                let result = evt.target.checked ? 1 : 0;
                                this.handleSaveSetting(isAudioEnabled, "", result);
                                runInAction(()=>{
                                    this.enableAudio = result;
                                });
                            }} />
                    }
                    label="Audio Enabled"
                />

                <FormControlLabel
                    control={
                        <Switch
                            checked={this.enableDangerPins === 1}
                            onChange={(evt) => {
                                let result = evt.target.checked ? 1 : 0;
                                this.handleSaveSetting(isDangerEnabled, "", result);
                                runInAction(()=>{
                                    this.enableDangerPins = result;
                                });
                            }} />
                    }
                    label="Danger Pins Enabled"
                />

                <TextField
                    style={{ width: "300px" }}
                    label={`Base File Path:`}
                    value={this.audioFileLocation}
                    onChange={(evt) => {
                        this.handleSaveSetting(fileBasePath, evt.target.value, 0);
                        runInAction(()=>{
                            this.audioFileLocation = evt.target.value;
                        });
                    }}
                    margin="normal"
                />

                <div style={{ display: "flex", flexDirection: "row" }}>
                    <TextField
                        type="time"
                        defaultValue="16:30"
                        style={{ width: "102px" }}
                        label={`Schudule On:`}
                        value={this.onAt}
                        onChange={(evt) => {
                            this.handleSaveSetting(timeOnAt, evt.target.value, 0);
                            runInAction(()=>{
                                this.onAt = evt.target.value;
                            });
                        }}
                        margin="normal"
                    />

                    <TextField
                        type="time"
                        defaultValue="23:30"
                        style={{ width: "102px" }}
                        label={`Off:`}
                        value={this.offAt}
                        onChange={(evt) => {
                            this.handleSaveSetting(timeOffAt, evt.target.value, 0);
                            runInAction(()=>{
                                this.offAt = evt.target.value;
                            });
                        }}
                        margin="normal"
                    />

                </div>

                <div style={{ display: "flex", flexDirection: "row" }}>
                    <TextField
                        type="time"
                        defaultValue="16:30"
                        style={{ width: "102px" }}
                        label={`Audio On:`}
                        value={this.audioOnAt}
                        onChange={(evt) => {
                            this.handleSaveSetting(audioTimeOnAt, evt.target.value, 0);
                            runInAction(()=>{
                                this.audioOnAt = evt.target.value;
                            });
                        }}
                        margin="normal"
                    />

                    <TextField
                        type="time"
                        defaultValue="23:30"
                        style={{ width: "102px" }}
                        label={`Off:`}
                        value={this.audioOffAt}
                        onChange={(evt) => {
                            this.handleSaveSetting(audioTimeOffAt, evt.target.value, 0);
                            runInAction(()=>{
                                this.audioOffAt = evt.target.value;
                            });
                        }}
                        margin="normal"
                    />

                </div>
            </div>
        )
    }
}

export default (SettingsEditor);