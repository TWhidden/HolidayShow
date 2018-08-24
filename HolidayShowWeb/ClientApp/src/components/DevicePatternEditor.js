import React, { Component } from 'react';
import DeviceServices from '../Services/DeviceServices'
import PatternServices from '../Services/DevicePatternServices'

import BusyContent from './controls/BusyContent';
import { withStyles } from '@material-ui/core/styles';
import Select from '@material-ui/core/Select';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import FormControl from '@material-ui/core/FormControl';
import TextField from '@material-ui/core/TextField';

import './CommonStyles.css';

const styles = theme => ({
    root: {
        display: 'flex',
        flexWrap: 'wrap',
    },
    formControl: {
        margin: theme.spacing.unit,
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

        this.state = {
            devices: [],
            deviceSelected: null,
            patterns: [],
            patternSelected: null,
            isBusy: false,
        };
    }

    componentDidMount = async () => {

        try {
            this.setIsBusy(true);
            let devices = await this.DeviceServices.getAllDevices();

            this.setState({
                devices,
            });

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    handleDeviceChange = async (evt) => {
        await this.getPatternsForSelectedDevice(evt.target.value);
        
        this.setState({ deviceSelected: evt.target.value });
    }

    getPatternsForSelectedDevice = async (device) => {
        try {
            this.setIsBusy(true);
            let patterns = await this.PatternServices.getDevicePatternsByDeviceId(device.deviceId);

            this.setState({
                patterns,
            });

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    handlePatternChange = async (evt) => {
        this.setState({ patternSelected: evt.target.value });
    }

    setIsBusy(busyState) {
        this.setState({ isBusy: busyState });
    }

    handlePatternNameChange = (pattern, evt) => {
        pattern.patternName = evt.target.value;
        this.setState({pattern});
    }

    render() {

        const { classes } = this.props;

        return (
            <div style={{ display: "flex", flex: "1", height: "100vh" }}>
                <form className={classes.root} autoComplete="off">
                    <FormControl className={classes.formControl}>
                        <InputLabel htmlFor="devices1">Devices</InputLabel>
                        <Select
                            value={this.state.deviceSelected}
                            onChange={(evt) => this.handleDeviceChange(evt)}
                            inputProps={{
                                name: 'dev',
                                id: 'devices1',
                            }}
                        >
                            {this.state.devices.map((device, i) =>
                                (
                                    <MenuItem value={device}>{device.name}</MenuItem>
                                ))}
                        </Select>
                    </FormControl>
                </form>
                <form className={classes.root} autoComplete="off">
                    <FormControl className={classes.formControl}>
                        <InputLabel htmlFor="patterns1">Patterns</InputLabel>
                        <Select
                            value={this.state.patternSelected}
                            onChange={(evt) => this.handlePatternChange(evt)}
                            inputProps={{
                                name: 'pattern',
                                id: 'patterns1',
                            }}
                        >
                            {this.state.patterns.map((pattern, i) =>
                                (
                                    <MenuItem value={pattern}>{pattern.patternName}</MenuItem>
                                ))}
                        </Select>
                    </FormControl>
                </form>

                { this.state.patternSelected && (


                <TextField
                    label={"Pattern Name"}
                    value={this.state.patternSelected.patternName}
                    onChange={(evt) => this.handlePatternNameChange(this.state.patternSelected, evt)}
                    margin="normal"
                />
                
            )}    

                {
                    this.state.isBusy && (<BusyContent />)
                }
            </div>
        );
    }
}

export default withStyles(styles)(DevicePattern);