import React, { Component } from 'react';

import DeviceServices from '../Services/DeviceServices';
import PatternServices from '../Services/DevicePatternServices';
import DevicePatternSequenceServices from '../Services/DevicePatternSequenceServices';
import AudioServices from '../Services/AudioServices';
import DeviceIoPortServices from '../Services/DeviceIoPortServices';

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
import Typography from '@material-ui/core/Typography';
import ErrorContent from './controls/ErrorContent';

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
            ioPortOptions: [],
            errorMessage: null,
        };
    }

    componentDidMount = async () => {

        try {
            this.setIsBusy(true);

            let audioOptions = await this.AudioServices.getAllAudioOptions();

            audioOptions = audioOptions.map((item) => ({ label: item.name, value: item.audioId }));

            this.setState({
                audioOptions,
            });

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }

        await this.getDevices();
    }

    getDevices  = async () => {
        let deviceIdSelected = 0;

        try {
            this.setIsBusy(true);

            let devices = await this.DeviceServices.getAllDevices();

            let deviceSelected = Enumerable.asEnumerable(devices).FirstOrDefault();
            
            if(deviceSelected != null){
                deviceIdSelected = deviceSelected.deviceId;
            }

            this.setState({
                devices,
                deviceIdSelected: deviceIdSelected,
                deviceSelected: deviceSelected
            });

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }

        this.handleDeviceChange(deviceIdSelected);
    }

    componentWillUnmount() {
        clearTimeout(this.timer);
    }

    handleDeviceChange = async (deviceId) => {
        var device = Enumerable.asEnumerable(this.state.devices)
            .Where(x => x.deviceId === deviceId)
            .FirstOrDefault();

        if (device == null) return;

        this.setState({
            deviceIdSelected: deviceId,
            deviceSelected: device
        });

        await this.getPatternsForSelectedDevice(device);
        await this.getIoPortsForSelectedDevice(device);
    }

    getPatternsForSelectedDevice = async (device) => {

        let patternId = 0;

        try {
            this.setIsBusy(true);
            let patterns = await this.PatternServices.getDevicePatternsByDeviceId(device.deviceId);

            patternId = Enumerable.asEnumerable(patterns).Select(x=> x.devicePatternId).FirstOrDefault();

            this.setState({
                patterns,
                patternSequences: [],
                deviceSelected: device
            });

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }

        this.handlePatternChange(patternId);
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
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    handlePatternChange = async (patternId) => {

        if(patternId == null) return;

        let patternSelected = Enumerable.asEnumerable(this.state.patterns)
            .Where(x => x.devicePatternId === patternId)
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
            this.setState({errorMessage: e.message})
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
            this.setState({errorMessage: e.message})
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
            this.setState({errorMessage: e.message})
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
            this.setState({errorMessage: e.message})
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

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    handleCommandDelete = async (pattern) => {
        try {
            this.setIsBusy(true);

            // find the next sequence, and add it at the end.
            await this.DevicePatternSequenceServices.sequenceDelete(pattern.devicePatternSeqenceId);

            this.setState({
                patternSequences: []
            })
        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }

        await this.handlePatternSequencesLoad(this.state.patternIdSelected);
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
                                value={this.state.deviceIdSelected}
                                onChange={(evt) => this.handleDeviceChange(evt.target.value)}
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

                                <div style={{ display: "flex", flexDirection: "row" }}>
                                    <div className="child"  style={{ width: "100px" }}>
                                        <Typography variant="body2" gutterBottom>
                                        On At:
                                        </Typography>
                                    </div>

                                    <div className="child" style={{ width: "100px" }}>
                                    <Typography variant="body2" gutterBottom>
                                        Duration:
                                        </Typography>
                                     </div>

                                    <div className="child">
                                    <Typography variant="body2" gutterBottom>
                                        Gpio Port:
                                        </Typography>
                                     </div>

                                    <div className="child">
                                    <Typography variant="body2" gutterBottom>
                                        Audio File:
                                        </Typography>
                                    </div>

                                    <div className="child">
                                    <Typography variant="body2" gutterBottom>
                                        Delete
                                        </Typography>
                                    </div>

                                </div>

                                {/* {this.state.patternSequences && this.state.patternSequences.map((sequence, i) => */}
                                {this.state.patternSequences.map((sequence, i) =>
                                    (
                                        <EditPattern
                                            sequence={sequence}
                                            audioOptions={this.state.audioOptions}
                                            portOptions={this.state.ioPortOptions}
                                            onDelete={(s) => this.handleCommandDelete(s)}
                                            key={i} />
                                    ))}


                            </div>
                        </div>
                    </div>

                )
                }

                {
                    this.state.isBusy && (<BusyContent />)
                }
                <ErrorContent errorMessage={this.state.errorMessage} errorClear={()=>{this.setState({errorMessage: null})}}/>
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
            audio: this.props.sequence.audioId,
            errorMessage: null,
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
            this.setState({errorMessage: e.message})
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
                    style={{ width: "100px" }}
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
                    style={{ width: "100px" }}
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
                        if (selectValue == null) return;
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
                        if (selectValue == null) return;
                        this.setState({ audio: selectValue.value })
                        this.handleDelaySave();
                    }
                    }
                    value={this.state.audio}
                />

                <Tooltip title="Delete Command">
                    <IconButton onClick={(evt) => this.props.onDelete(this.props.sequence)}><DeleteIcon /></IconButton>
                </Tooltip>
                <ErrorContent errorMessage={this.state.errorMessage} errorClear={()=>{this.setState({errorMessage: null})}}/>
            </div>
        )
    }
}