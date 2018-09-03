import React, { Component } from 'react';
import DeviceServices from '../Services/DeviceServices';
import PatternServices from '../Services/DevicePatternServices';
import DevicePatternSequenceServices from '../Services/DevicePatternSequenceServices';
import AudioServices from '../Services/AudioServices';
import DeviceIoPortServices from '../Services/DeviceIoPortServices';

import 'react-select/dist/react-select.css'
import 'react-virtualized/styles.css'
import 'react-virtualized-select/styles.css'

import BusyContent from './controls/BusyContent';
import { withStyles } from '@material-ui/core/styles';
import Select from '@material-ui/core/Select';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import FormControl from '@material-ui/core/FormControl';
import TextField from '@material-ui/core/TextField';
import * as Enumerable from "linq-es2015";
import IconButton from '@material-ui/core/IconButton';
import DeleteIcon from '@material-ui/icons/Delete';
import AddIcon from '@material-ui/icons/Add';
import Tooltip from '@material-ui/core/Tooltip';
import VirtualizedSelect from 'react-virtualized-select'

import './CommonStyles.css';

const styles = theme => ({
    root: {
        display: 'flex',
        flexWrap: 'wrap',
    },
    formControl: {
        margin: 0,
        minWidth: 120,
    },
    selectEmpty: {
        marginTop: theme.spacing.unit * 2,
    },
});

// MyStateless = (props) => {
//     <div>
//         <TextField value={props.myVal} />
//     </div>
// }

class DevicePattern extends Component {
    displayName = DevicePattern.name

    constructor(props) {
        super(props)

        this.DeviceServices = DeviceServices;
        this.PatternServices = PatternServices;
        this.DevicePatternSequenceServices = DevicePatternSequenceServices;
        this.AudioServices = AudioServices;
        this.DeviceIoPortServices = DeviceIoPortServices;

        this.state = {
            devices: [],
            deviceSelected: "",
            deviceIdSelected: 0,
            patterns: [],
            patternSelected: "",
            patternIdSelected: 0,
            patternSequences: [],
            isBusy: false,
            audioOptions: [],
            ioPortOptions: []
        };
    }

    componentDidMount = async () => {

        try {
            this.setIsBusy(true);
            let devices = await this.DeviceServices.getAllDevices();

            let audioOptions = await this.AudioServices.getAllAudioOptions();

            audioOptions = audioOptions.map((item) => ({ label: item.name, value: item.audioId }));

            this.setState({
                devices,
                audioOptions,
            });

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    componentWillUnmount() {
        clearTimeout(this.timer);
    }

    handleDeviceChange = async (evt) => {
        let deviceId = evt.target.value;

        this.setState({
            deviceIdSelected: deviceId
        });

        var device = Enumerable.asEnumerable(this.state.devices)
            .Where(x => x.deviceId == deviceId)
            .FirstOrDefault();

        if (device == null) return;

        await this.getPatternsForSelectedDevice(device);
        await this.getIoPortsForSelectedDevice(device);
    }

    getPatternsForSelectedDevice = async (device) => {
        try {
            this.setIsBusy(true);
            let patterns = await this.PatternServices.getDevicePatternsByDeviceId(device.deviceId);

            this.setState({
                patterns,
                patternSelected: "",
                patternIdSelected: 0,
                patternSequences: [],
                deviceSelected: device
            });

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    getIoPortsForSelectedDevice = async (device) => {
        try {
            this.setIsBusy(true);
            let ports = await this.DeviceIoPortServices.ioPortGetByDeviceId(device.deviceId);

            ports = ports.map((item) => ({ label: `${item.commandPin}: ${item.description}`, value: item.deviceIoPortId }));

            this.setState({
                ioPortOptions: ports
            })

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    handlePatternChange = async (patternId) => {

        let patternSelected = Enumerable.asEnumerable(this.state.patterns)
            .Where(x => x.devicePatternId == patternId)
            .FirstOrDefault();

        this.setState({
            patternSelected,
            patternIdSelected: patternId
        });

        await this.handlePatternSequencesLoad(patternId);
    }

    handlePatternSequencesLoad = async (patternId) => {

        try {
            this.setIsBusy(true);

            let sequences = await this.DevicePatternSequenceServices.sequenceGetByPatternId(patternId);

            this.setState({
                patternSequences: sequences,
            });

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }

    }

    handlePatternDelete = async (evt) => {
        try {
            this.setIsBusy(true);

            if (this.state.patternSelected == null) return;

            await this.PatternServices.deletePatternByPatternId(this.state.patternIdSelected);

            let patterns = this.state.patterns;

            // Remove the element from the existing list
            patterns.splice(patterns.indexOf(this.state.patternSelected), 1);

            this.setState({
                patternSelected: "",
                patternIdSelected: 0,
                patternSequences: [],
                patterns
            })

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    handlePatternCreate = async (evt) => {
        try {
            this.setIsBusy(true);

            let newPattern = {
                deviceId: this.state.deviceSelected.deviceId,
                patternName: "New Pattern",
            };

            newPattern = await this.PatternServices.createPattern(newPattern);

            let patterns = this.state.patterns;
            patterns.push(newPattern);

            this.setState({
                patterns,
                patternSelected: newPattern,
                patternIdSelected: newPattern.devicePatternId
            })

        } catch (e) {

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

    handlePatternNameChange = (pattern, evt) => {
        pattern.patternName = evt.target.value;
        this.handlePatternSave(pattern);
    }

    handlePatternSave = async (pattern) => {
        try {
            this.setIsBusy(true);

            await this.PatternServices.updatePattern(pattern);

            this.setState({});
        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    handleSequenceCreate = async () => {
        try {
            this.setIsBusy(true);

            // find the next sequence, and add it at the end.

            var lastOnAt = Enumerable.asEnumerable(this.state.patternSequences)
                .Select(x => x.onAt)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            let nextOnAt = 0;
            if (lastOnAt != null) nextOnAt = lastOnAt * 1;
            nextOnAt = nextOnAt + 1000;

            let newSequence = {
                onAt: nextOnAt,
                devicePatternId: this.state.patternIdSelected,
            };

            newSequence = await this.DevicePatternSequenceServices.sequenceCreate(this.state.deviceIdSelected, newSequence);

            let sequences = this.state.patternSequences;
            sequences.push(newSequence);

            this.setState({
                patternSequences: sequences
            });

        } catch (error) {
            let v = error;
            console.log(error);

        } finally {
            this.setIsBusy(false);
        }
    }

    render() {

        const { classes } = this.props;

        return (
            <div style={{ display: "flex", flexDirection: "column", }}>
                <div style={{ display: "flex", flexDirection: "row" }}>
                    <form className={classes.root} autoComplete="off">
                        <FormControl className={classes.formControl}>
                            <InputLabel htmlFor="devices1">Devices</InputLabel>
                            <Select
                                value={this.state.deviceIdSelected}
                                onChange={(evt) => this.handleDeviceChange(evt)}
                                inputProps={{
                                    name: 'dev',
                                    id: 'devices1',
                                }}
                            >
                                {this.state.devices.map((device, i) =>
                                    (
                                        <MenuItem value={device.deviceId} key={i}>{device.deviceId}: {device.name}</MenuItem>
                                    ))}
                            </Select>
                        </FormControl>
                    </form>
                    {this.state.patterns && this.state.patterns.length > 0 && (
                        <form className={classes.root} autoComplete="off">
                            <FormControl className={classes.formControl}>
                                <InputLabel htmlFor="patterns1">Patterns</InputLabel>
                                <Select
                                    value={this.state.patternIdSelected}
                                    onChange={(evt) => this.handlePatternChange(evt.target.value)}
                                    inputProps={{
                                        name: 'pattern',
                                        id: 'patterns1',
                                    }}
                                >
                                    {this.state.patterns && this.state.patterns.map((pattern, i) =>
                                        (
                                            <MenuItem value={pattern.devicePatternId} key={i}>{pattern.patternName}</MenuItem>
                                        ))}
                                </Select>
                            </FormControl>
                        </form>
                    )}

                    {this.state.patternSelected && (

                        <Tooltip title="Delete Pattern">
                            <IconButton onClick={(evt) => this.handlePatternDelete()}><DeleteIcon /></IconButton>
                        </Tooltip>

                    )}

                    {this.state.deviceSelected && (
                        <Tooltip title="Create New Pattern">
                            <IconButton onClick={(evt) => this.handlePatternCreate()}><AddIcon /></IconButton>
                        </Tooltip>

                    )}
                </div>


                {this.state.patternSelected && (
                    <div>
                        <div style={{ display: "flex", flexDirection: "column" }}>
                            <div style={{ display: "flex", flexDirection: "row" }}>
                                <TextField
                                    label={"Pattern Name"}
                                    value={this.state.patternSelected.patternName}
                                    onChange={(evt) => this.handlePatternNameChange(this.state.patternSelected, evt)}
                                    margin="normal"
                                />

                                <Tooltip title="Create New Event">
                                    <IconButton onClick={(evt) => this.handleSequenceCreate()}><AddIcon /></IconButton>
                                </Tooltip>
                            </div>
                            <div>

                                <div style={{ display: "flex", flexDirection: "row", }}>
                                    <div className="child">
                                        On At:
                                     </div>

                                     <div className="child">
                                        Duration:
                                     </div>

                                     <div className="child">
                                        Gpio Port:
                                     </div>

                                    <div className="child">
                                        Audio File:
                                </div>

                                </div>

                                {/* {this.state.patternSequences && this.state.patternSequences.map((sequence, i) => */}
                                {this.state.patternSequences.map((sequence, i) =>
                                    (
                                        <EditPattern sequence={sequence} audioOptions={this.state.audioOptions} portOptions={this.state.ioPortOptions} key={i} />
                                    ))}


                            </div>
                        </div>
                    </div>

                )
                }

                {
                    this.state.isBusy && (<BusyContent />)
                }
            </div >
        );
    }
}

export default withStyles(styles)(DevicePattern);

class EditPattern extends Component {
    constructor(props) {
        super(props)

        this.state = {
            onAt: this.props.sequence.onAt,
            duration: this.props.sequence.duration,
            port: this.props.sequence.deviceIoPortId,
            audio: this.props.sequence.audioId
        };

        this.DevicePatternSequenceServices = DevicePatternSequenceServices;
    }

    handleSave = async () => {
        var sequence = this.props.sequence;
        let sequenceId = sequence.devicePatternSeqenceId;

        sequence.onAt = this.state.onAt;
        sequence.duration = this.state.duration;
        sequence.audioId = this.state.audio;
        sequence.deviceIoPortId = this.state.port;

        try {
            await this.DevicePatternSequenceServices.sequenceSave(sequenceId, sequence);
        } catch (e) {

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
                    className="child"
                    value={this.state.onAt}
                    onChange={(evt) => {
                        this.setState(
                            {
                                onAt: evt.target.value
                            }
                        );
                        this.handleDelaySave();
                    }}
                />

                <TextField
                    className="child"
                    value={this.state.duration}
                    onChange={(evt) => {
                        this.setState(
                            {
                                duration: evt.target.value
                            }
                        );
                        this.handleDelaySave();
                    }}
                />

                <VirtualizedSelect
                    className="child"
                    options={this.props.portOptions}
                    onChange={(selectValue) => {
                        if(selectValue == null) return;
                        this.setState({ port: selectValue.value })
                        this.handleDelaySave();
                    }
                    }
                    value={this.state.port}
                />

                <VirtualizedSelect
                    className="child"
                    options={this.props.audioOptions}
                    onChange={(selectValue) => {
                        if(selectValue == null) return;
                        this.setState({ audio: selectValue.value })
                        this.handleDelaySave();
                    }
                    }
                    value={this.state.audio}
                />
            </div>
        )
    }
}