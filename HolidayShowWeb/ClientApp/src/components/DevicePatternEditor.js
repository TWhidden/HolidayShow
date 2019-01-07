import React, { Component } from 'react';
import {inject, observer} from 'mobx-react';
import {observable, runInAction} from 'mobx';
import PatternServices from '../Services/DevicePatternServices';
import DevicePatternSequenceServices from '../Services/DevicePatternSequenceServices';
import DeviceIoPortServices from '../Services/DeviceIoPortServices';
import { withStyles } from '@material-ui/core/styles';
import Select from '@material-ui/core/Select';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import FormControl from '@material-ui/core/FormControl';
import TextField from '@material-ui/core/TextField';
import * as Enumerable from "linq-es5";
import IconButton from '@material-ui/core/IconButton';
import DeleteIcon from '@material-ui/icons/Delete';
import AddIcon from '@material-ui/icons/Add';
import Tooltip from '@material-ui/core/Tooltip';
import ComboSelect from 'react-select';
import Typography from '@material-ui/core/Typography';

const styles = theme => ({
    formControl: {
        margin: 0,
        minWidth: 120,
    },
});

const sessionLastDeviceSelected = "PatternEdit-selectedDevice";
const sessionLastPatternSelected = "PatternEdit-selectedPattern";

@inject("appStore")
@observer
class DevicePattern extends Component {
    displayName = DevicePattern.name

    @observable patterns = [];
    @observable patternSequences = [];
    @observable ioPortOptions = [];

    @observable deviceSelected = "";
    @observable deviceIdSelected = 0;
    @observable patternSelected = "";
    @observable patternIdSelected = 0;
    
    componentDidMount = async () => {

        try {
            this.props.appStore.isBusySet(true);

            await this.props.appStore.audioLoadAsync();

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }

        await this.getDevices();
    }

    getDevices = async () => {
        let deviceIdSelected = 0;

        try {
            this.props.appStore.isBusySet(true);

            let devices = await this.props.appStore.devicesGetAllAsync();

            let deviceSelected = Enumerable.asEnumerable(devices).FirstOrDefault();

            let lastSelectedDeviceId = sessionStorage.getItem(sessionLastDeviceSelected);
            if (lastSelectedDeviceId != null) {

                console.log(`${sessionLastDeviceSelected}: ${Number(lastSelectedDeviceId)}`)

                let lastSelectedDevice = Enumerable.asEnumerable(devices)
                    .Where(d => d.deviceId === Number(lastSelectedDeviceId))
                    .FirstOrDefault();

                if (lastSelectedDevice != null) {
                    deviceSelected = lastSelectedDevice;
                }
            }

            if (deviceSelected != null) {
                deviceIdSelected = deviceSelected.deviceId;
            }

            runInAction(()=>{
                this.deviceIdSelected = deviceIdSelected;
                this.deviceSelected = deviceSelected;
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }

        this.handleDeviceChange(deviceIdSelected);
    }

    componentWillUnmount() {
        clearTimeout(this.timer);
    }

    handleDeviceChange = async (deviceId) => {
        var device = Enumerable.asEnumerable(this.props.appStore.devices)
            .Where(x => x.deviceId === deviceId)
            .FirstOrDefault();

        if (device == null) return;

        console.log(`setting ${sessionLastDeviceSelected}: ${deviceId}`)

        sessionStorage.setItem(sessionLastDeviceSelected, deviceId);

        runInAction(()=>{
            this.deviceIdSelected = deviceId;
            this.deviceSelected = device;
        });

        await this.getPatternsForSelectedDevice(device);
        await this.getIoPortsForSelectedDevice(device);
    }

    getPatternsForSelectedDevice = async (device) => {

        let patternId = 0;

        try {
            this.props.appStore.isBusySet(true);
            let patterns = await PatternServices.getDevicePatternsByDeviceId(device.deviceId);

            let lastSelectedPatternId = sessionStorage.getItem(sessionLastPatternSelected);
            if (lastSelectedPatternId != null) {

                console.log(`${sessionLastPatternSelected}: ${Number(lastSelectedPatternId)}`)

                let lastSelectedPattern = Enumerable.asEnumerable(patterns)
                    .Where(d => d.devicePatternId === Number(lastSelectedPatternId))
                    .FirstOrDefault();

                if (lastSelectedPattern != null) {
                    patternId = lastSelectedPattern.devicePatternId;
                }
            }

            // If the state returned something that didint exist, select the first one.
            if (patternId === 0) {
                patternId = Enumerable.asEnumerable(patterns).Select(x => x.devicePatternId).FirstOrDefault();
            }

            runInAction(()=>{
                this.patterns = patterns;
                this.patternSequences = [];
                this.deviceSelected = device;
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }

        this.handlePatternChange(patternId);
    }

    getIoPortsForSelectedDevice = async (device) => {

        try {
            this.props.appStore.isBusySet(true);

            let ports = await DeviceIoPortServices.ioPortGetByDeviceId(device.deviceId);

            ports = ports.map((item) => ({ label: `${item.commandPin}: ${item.description}`, value: item.deviceIoPortId }));

            runInAction(()=>{
                this.ioPortOptions = ports;
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handlePatternChange = async (patternId) => {

        if (patternId == null) return;

        let patternSelected = Enumerable.asEnumerable(this.patterns)
            .Where(x => x.devicePatternId === patternId)
            .FirstOrDefault();

        console.log(`setting ${sessionLastPatternSelected}: ${patternId}`)

        sessionStorage.setItem(sessionLastPatternSelected, patternId);

        runInAction(()=>{
            this.patternSelected = patternSelected;
            this.patternIdSelected = patternId;
        });

        await this.handlePatternSequencesLoad(patternId);
    }

    handlePatternSequencesLoad = async (patternId) => {

        try {
            this.props.appStore.isBusySet(true);

            let sequences = await DevicePatternSequenceServices.sequenceGetByPatternId(patternId);

            runInAction(()=>{
                this.patternSequences = sequences;
            });
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }

    }

    handlePatternDelete = async (evt) => {
        try {
            this.props.appStore.isBusySet(true);

            if (this.patternSelected == null) return;

            await PatternServices.deletePatternByPatternId(this.patternIdSelected);

            runInAction(()=>{
                this.patternSelected = "";
                this.patternIdSelected = 0;
                this.patternSequences = [];
                this.patterns.splice(this.patterns.indexOf(this.patternSelected), 1);    
            });

            this.getPatternsForSelectedDevice(this.deviceSelected);
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handlePatternCreate = async (evt) => {
        try {
            this.props.appStore.isBusySet(true);

            let newPattern = {
                deviceId: this.deviceSelected.deviceId,
                patternName: "New Pattern",
            };

            newPattern = await PatternServices.createPattern(newPattern);

            runInAction(()=>{
                this.patterns.push(newPattern);
                this.patternSelected = newPattern;
                this.patternIdSelected = newPattern.devicePatternId;
                this.patternSequences = [];
            });

            this.handlePatternSequencesLoad(newPattern.devicePatternId);

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

    handlePatternNameChange = (pattern, evt) => {
        runInAction(()=>{
            pattern.patternName = evt.target.value;
        });
        this.handlePatternSave(pattern);
    }

    handlePatternSave = async (pattern) => {
        try {
            this.props.appStore.isBusySet(true);

            await PatternServices.updatePattern(pattern);
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleSequenceCreate = async () => {
        try {
            this.props.appStore.isBusySet(true);

            // find the next sequence, and add it at the end.

            var lastOnAt = Enumerable.asEnumerable(this.patternSequences)
                .OrderByDescending(x => x.onAt)
                .FirstOrDefault();

            let nextOnAt = 0;
            if (lastOnAt != null) {
                nextOnAt = lastOnAt.onAt * 1;


                // if the last on-at has an audio file, lets move this to the next period
                // for the user so the time doesnt have to be calculated
                var audioDuration = Enumerable.asEnumerable(this.props.appStore.audioFiles)
                                    .Where(x => x.audioId === lastOnAt.audioId)
                                    .Select(x => x.audioDuration)
                                    .FirstOrDefault();
                
                nextOnAt += audioDuration;

            }
            nextOnAt = nextOnAt + 1000;
            

            let newSequence = {
                onAt: nextOnAt,
                devicePatternId: this.patternIdSelected,
                audioId: 1
            };

            newSequence = await DevicePatternSequenceServices.sequenceCreate(this.deviceIdSelected, newSequence);

            let sequences = this.patternSequences;
            
            runInAction(()=>{
                sequences.push(newSequence);
                this.patternSequences = sequences;
            });
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleCommandDelete = async (pattern) => {
        try {
            this.props.appStore.isBusySet(true);

            // find the next sequence, and add it at the end.
            await DevicePatternSequenceServices.sequenceDelete(pattern.devicePatternSeqenceId);

            runInAction(()=>{
                this.patternSequences.splice(this.patternSequences.indexOf(pattern), 1);
             });
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    render() {

        const { classes } = this.props;

        return (
            <div style={{ display: "flex", flexDirection: "column", height: "100vh" }}>
                <div style={{ display: "flex", flexDirection: "row" }}>
                    <form className={classes.root} autoComplete="off">
                        <FormControl className={classes.formControl}>
                            <InputLabel htmlFor="devices1">Devices</InputLabel>
                            <Select
                                value={this.deviceIdSelected}
                                onChange={(evt) => this.handleDeviceChange(evt.target.value)}
                                inputProps={{
                                    name: 'dev',
                                    id: 'devices1',
                                }}
                            >
                                {this.props.appStore.devices.map((device, i) =>
                                    (
                                        <MenuItem value={device.deviceId} key={i}>{device.deviceId}: {device.name}</MenuItem>
                                    ))}
                            </Select>
                        </FormControl>
                    </form>
                    {this.patterns && this.patterns.length > 0 && (
                        <form className={classes.root} autoComplete="off">
                            <FormControl className={classes.formControl}>
                                <InputLabel htmlFor="patterns1">Patterns</InputLabel>
                                <Select
                                    value={this.patternIdSelected}
                                    onChange={(evt) => this.handlePatternChange(evt.target.value)}
                                    inputProps={{
                                        name: 'pattern',
                                        id: 'patterns1',
                                    }}
                                >
                                    {this.patterns && this.patterns.map((pattern, i) =>
                                        (
                                            <MenuItem value={pattern.devicePatternId} key={i}>{pattern.patternName}</MenuItem>
                                        ))}
                                </Select>
                            </FormControl>
                        </form>
                    )}

                    {this.patternSelected && (

                        <Tooltip title="Delete Pattern">
                            <IconButton onClick={(evt) => this.handlePatternDelete()}><DeleteIcon /></IconButton>
                        </Tooltip>

                    )}

                    {this.deviceSelected && (
                        <Tooltip title="Create New Pattern">
                            <IconButton onClick={(evt) => this.handlePatternCreate()}><AddIcon /></IconButton>
                        </Tooltip>

                    )}
                </div>


                {this.patternSelected && (
                    <div>
                        <div style={{ display: "flex", flexDirection: "column" }}>
                            <div style={{ display: "flex", flexDirection: "row" }}>
                                <TextField
                                    label={"Pattern Name"}
                                    value={this.patternSelected.patternName}
                                    onChange={(evt) => this.handlePatternNameChange(this.patternSelected, evt)}
                                    margin="normal"
                                />

                                <Tooltip title="Create New Event">
                                    <IconButton onClick={(evt) => this.handleSequenceCreate()}><AddIcon /></IconButton>
                                </Tooltip>
                            </div>
                            <div>

                                <div style={{ display: "flex", flexDirection: "row" }}>
                                    <div className="child75">
                                        <Typography variant="body2" gutterBottom>
                                            On At:
                                        </Typography>
                                    </div>

                                    <div className="child75">
                                        <Typography variant="body2" gutterBottom>
                                            Duration:
                                        </Typography>
                                    </div>

                                    <div className="child300">
                                        <Typography variant="body2" gutterBottom>
                                            Gpio Port:
                                        </Typography>
                                    </div>

                                    <div className="child300">
                                        <Typography variant="body2" gutterBottom>
                                            Audio File:
                                        </Typography>
                                    </div>

                                    <div className="child75">
                                        <Typography variant="body2" gutterBottom>
                                            Delete
                                        </Typography>
                                    </div>

                                </div>

                                {this.patternSequences.map((sequence, i) =>
                                    (
                                        <EditPattern
                                            sequence={sequence}
                                            audioOptions={this.props.appStore.audioFiles}
                                            portOptions={this.ioPortOptions}
                                            onDelete={(s) => this.handleCommandDelete(s)}
                                            key={i} />
                                    ))}


                            </div>
                        </div>
                    </div>

                )
                }
            </div >
        );
    }
}

export default withStyles(styles)(DevicePattern);

@inject("appStore")
@observer
class EditPattern extends Component {

    @observable port = "";
    @observable audio = "";

    componentDidUpdate(prevProps, prevState) {
        // only update if the data has changed
        if (prevProps.sequence !== this.props.sequence || prevProps.audioOptions !== this.props.audioOptions || prevProps.portOptions !== this.props.portOptions) {
            this.updateMe();
        }
    }

    componentDidMount(){
        this.updateMe();
    }

    updateMe(){
        try {
            let selectedAudioItem = Enumerable.AsEnumerable(this.props.audioOptions)
                                        .Where(x => x.value === this.props.sequence.audioId)
                                        .FirstOrDefault();

            let selectIoPort = Enumerable.AsEnumerable(this.props.portOptions)
                                        .Where(x => x.value === this.props.sequence.deviceIoPortId)
                                        .FirstOrDefault();

            runInAction(()=>{
                this.port = selectIoPort;
                this.audio = selectedAudioItem;
            });

        }catch (e) {
           this.props.appStore.errorMessageSet("Error Setting Compoent: " + e.message);
        }
    }

    handleSave = async () => {
        var sequence = this.props.sequence;
        let sequenceId = sequence.devicePatternSeqenceId;

        runInAction(()=>{
            
            if(this.audio === null || this.audio === ""){
                sequence.audioId = 1;
            }else{
                sequence.audioId = this.audio.value;
            }
            
            if(this.port === null || this.port === ""){
                //sequence.deviceIoPortId = this.port.value;
            }else{
                sequence.deviceIoPortId = this.port.value;
            }
            
        });

        try {
            await DevicePatternSequenceServices.sequenceSave(sequenceId, sequence);
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {

        }
    }

    handleDelaySave() {
        clearTimeout(this.saveTimer);

        this.saveTimer = setTimeout(() => this.handleSave(), 1000);
    }

    render() {

        return (

            <div style={{ display: "flex", flexDirection: "row", }}>
                <TextField
                    className="child75"
                    value={this.props.sequence.onAt}
                    onChange={(evt) => {

                        runInAction(()=>{
                            this.props.sequence.onAt = evt.target.value;
                        });

                        this.handleDelaySave();
                    }}
                />

                <TextField
                    className="child75"
                    value={this.props.sequence.duration}
                    onChange={(evt) => {

                        runInAction(()=>{
                            this.props.sequence.duration = evt.target.value;
                        });

                        this.handleDelaySave();
                    }}
                />

                <ComboSelect
                    className="child300"
                    clearable={false}
                    options={this.props.portOptions}
                    onChange={(selectValue) => {
                        if (selectValue == null) return;

                        runInAction(()=>{
                            this.props.sequence.deviceIoPortId = selectValue.deviceIoPortId;
                            this.port = selectValue;
                        });

                        this.handleDelaySave();
                    }
                    }
                    value={this.port}
                />

                <ComboSelect
                    className="child300"
                    options={this.props.audioOptions}
                    clearable={false}
                    onChange={(selectValue) => {
                        if (selectValue == null) return;

                        runInAction(()=>{
                            console.log("1");
                            this.props.sequence.audioId = selectValue.audioId;
                            console.log("2");
                            this.audio = selectValue;
                            console.log("3");
                        });

                        console.log("4");
                        this.handleDelaySave();
                        console.log("5");
                    }
                    }
                    value={this.audio}
                />

                <Tooltip title="Delete Command">
                    <IconButton onClick={(evt) => this.props.onDelete(this.props.sequence)}><DeleteIcon /></IconButton>
                </Tooltip>
            </div>
        )
    }


}