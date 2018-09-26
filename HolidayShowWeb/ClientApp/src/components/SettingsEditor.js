import React, { Component } from 'react';

import SettingServices from '../Services/SettingServices';

import TextField from '@material-ui/core/TextField';
import FormControlLabel from '@material-ui/core/FormControlLabel';
import Switch from '@material-ui/core/Switch';
import BusyContent from './controls/BusyContent';
import ErrorContent from './controls/ErrorContent';
import * as Enumerable from "linq-es2015";

const delayMs = "SetDelayMs";
const audioTimeOffAt = "AudioTimeOffAt";
const audioTimeOnAt = "AudioTimeOnAt";
const timeOffAt = "TimeOffAt";
const timeOnAt = "TimeOnAt";
const fileBasePath = "FileBasePath";
const isAudioEnabled = "IsAudioEnabled";
const isDangerEnabled = "IsDangerEnabled";

export default class SettingsEditor extends Component {
    constructor(props) {
        super(props)

        this.state = {
            delayBetweenSets: 0,
            onAt: "",
            offAt: "",
            audioOnAt: "",
            audioOffAt: "",
            enableDangerPins: "",
            enableAudio: "",
            audioFileLocation: "",
            settings: [],
            errorMessage: null,
        };

        this.SettingServices = SettingServices;
    }

    componentDidMount = async () => {
        this.getAllSetting();
    }

    getAllSetting = async () => {
        try {
            this.setIsBusy(true);

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

            this.setState({
                delayBetweenSets,
                onAt,
                offAt,
                audioOnAt,
                audioOffAt,
                enableDangerPins,
                enableAudio,
                audioFileLocation,
                settings,
            })
        } catch (e) {
            this.setState({ errorMessage: e.message })
        } finally {
            this.setIsBusy(false);
        }
    }

    handleSaveSetting = async (settingName, valueString, valueDouble) => {
        try {
            this.setIsBusy(true);

            let setting = {
                settingName,
                valueString,
                valueDouble
            }

            // see if the current state has this key.
            // if it does not, we need to create the object
            if (this.state.settings.Where(x => x.settingName == settingName).FirstOrDefault() == null) {
                await this.SettingServices.createSetting(setting);
                await this.getAllSetting();
            } else {
                await this.SettingServices.saveSetting(setting);
            }

        } catch (e) {
            this.setState({ errorMessage: e.message })
        } finally {
            this.setIsBusy(false);
        }
    }

    setIsBusy(busyState) {
        clearTimeout(this.timer);
        if (!busyState) {
            this.setState({ isBusy: false });
            return;
        }

        this.timer = setTimeout(() => this.setState({ isBusy: true }), 250);
    }

    render() {
        return (
            <div style={{ display: "flex", flexDirection: "column", height: "100vh" }}>

                <TextField
                    style={{ width: "200px" }}
                    label={`Delay between executions:`}
                    value={this.state.delayBetweenSets}
                    onChange={(evt) => {
                        this.handleSaveSetting(delayMs, "", evt.target.value);
                        this.setState({
                            delayBetweenSets: evt.target.value
                        })
                    }}
                    margin="normal"
                />

                <FormControlLabel
                    control={
                        <Switch
                            checked={this.state.enableAudio == 1}
                            onChange={(evt) => {
                                let result = evt.target.checked ? 1 : 0;
                                this.handleSaveSetting(isAudioEnabled, "", result);
                                this.setState({
                                    enableAudio: result
                                })
                            }} />
                    }
                    label="Audio Enabled"
                />

                <FormControlLabel
                    control={
                        <Switch
                            checked={this.state.enableDangerPins == 1}
                            onChange={(evt) => {
                                let result = evt.target.checked ? 1 : 0;
                                this.handleSaveSetting(isDangerEnabled, "", result);
                                this.setState({
                                    enableDangerPins: result
                                })
                            }} />
                    }
                    label="Danger Pins Enabled"
                />

                <TextField
                    style={{ width: "300px" }}
                    label={`Base File Path:`}
                    value={this.state.audioFileLocation}
                    onChange={(evt) => {
                        this.handleSaveSetting(fileBasePath, evt.target.value, 0);
                        this.setState({
                            audioFileLocation: evt.target.value
                        })
                    }}
                    margin="normal"
                />

                <div style={{ display: "flex", flexDirection: "row" }}>
                    <TextField
                        type="time"
                        defaultValue="16:30"
                        style={{ width: "102px" }}
                        label={`Schudule On:`}
                        value={this.state.onAt}
                        onChange={(evt) => {
                            this.handleSaveSetting(timeOnAt, evt.target.value, 0);
                            this.setState({
                                onAt: evt.target.value
                            })
                        }}
                        margin="normal"
                    />

                    <TextField
                        type="time"
                        defaultValue="23:30"
                        style={{ width: "102px" }}
                        label={`Off:`}
                        value={this.state.offAt}
                        onChange={(evt) => {
                            this.handleSaveSetting(timeOffAt, evt.target.value, 0);
                            this.setState({
                                offAt: evt.target.value
                            })
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
                        value={this.state.audioOnAt}
                        onChange={(evt) => {
                            this.handleSaveSetting(audioTimeOnAt, evt.target.value, 0);
                            this.setState({
                                audioOnAt: evt.target.value
                            })
                        }}
                        margin="normal"
                    />

                    <TextField
                        type="time"
                        defaultValue="23:30"
                        style={{ width: "102px" }}
                        label={`Off:`}
                        value={this.state.audioOffAt}
                        onChange={(evt) => {
                            this.handleSaveSetting(audioTimeOffAt, evt.target.value, 0);
                            this.setState({
                                audioOffAt: evt.target.value
                            })
                        }}
                        margin="normal"
                    />

                </div>
                {
                    this.state.isBusy && (<BusyContent />)
                }
                <ErrorContent errorMessage={this.state.errorMessage} errorClear={() => { this.setState({ errorMessage: null }) }} />
            </div>
        )
    }
}
