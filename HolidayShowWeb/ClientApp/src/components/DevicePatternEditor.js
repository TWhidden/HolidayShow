import React, { Component } from 'react';
import DeviceServices from '../Services/DeviceServices'

import BusyContent from './controls/BusyContent';
import { withStyles } from '@material-ui/core/styles';
import Select from '@material-ui/core/Select';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import FormControl from '@material-ui/core/FormControl';

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

        this.state = {
            devices: [],
            deviceSelected: null,
            isBusy: false,
        }
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
        this.setState({ deviceSelected: evt.target.value });
    }

    getPatternsForSelectedDevice = async () => {
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

    setIsBusy(busyState) {
        this.setState({ isBusy: busyState });
    }

    render() {

        const { classes } = this.props;

        return (
            <div style={{ display: "flex", flex: "1", height: "100vh" }}>
                <form className={classes.root} autoComplete="off">
                    <FormControl className={classes.formControl}>
                        <InputLabel htmlFor="devices">Devices</InputLabel>
                        <Select
                            value={this.state.deviceSelected}
                            onChange={(evt) => this.handleDeviceChange(evt)}
                            inputProps={{
                                name: 'device',
                                id: 'devices',
                            }}
                        >

                            {this.state.devices.map((device, i) =>
                                (
                                    <MenuItem value={device}>{device.name}</MenuItem>
                                ))}
                        </Select>
                        <InputLabel htmlFor="patterns">Patterns</InputLabel>
                        <Select
                            value={this.state.deviceSelected}
                            onChange={(evt) => this.handleDeviceChange(evt)}
                            inputProps={{
                                name: 'patterns',
                                id: 'patterns',
                            }}
                        >

                            {this.state.devices.map((device, i) =>
                                (
                                    <MenuItem value={device}>{device.name}</MenuItem>
                                ))}
                        </Select>
                    </FormControl>
                </form>
                {
                    this.state.isBusy ? (
                        <BusyContent />
                    ) : (<div />)
                }
            </div>
        );
    }
}

export default withStyles(styles)(DevicePattern);