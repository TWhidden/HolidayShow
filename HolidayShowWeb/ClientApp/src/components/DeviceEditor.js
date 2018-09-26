import React, { Component } from 'react';
import DeviceServices from '../Services/DeviceServices'
import DeviceIoPortServices from '../Services/DeviceIoPortServices'

import DeviceIoPortEditor from './DeviceIoPortEditor';
import TextField from '@material-ui/core/TextField';
import BusyContent from './controls/BusyContent';
import { withStyles } from '@material-ui/core/styles';
import ErrorContent from './controls/ErrorContent';
import FormControl from '@material-ui/core/FormControl';
import Select from '@material-ui/core/Select';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import * as Enumerable from "linq-es5";

const styles = theme => ({
    formControl: {
        margin: 0,
        minWidth: 120,
    },
});

const sessionDeviceSelected = "DeviceEdit-DeviceSelected";

class DeviceManager extends Component {
    displayName = DeviceManager.name

    constructor(props) {
        super(props)

        this.DeviceServices = DeviceServices;
        this.DeviceIoPortServices = DeviceIoPortServices;

        this.state = {
            devices: [],
            deviceIdSelected: 0,
            deviceSelected: "",
            isBusy: false,
            errorMessage: null,
        };
    }

    componentDidMount = async () => {

        try {
            this.setIsBusy(true);
            let devices = await this.DeviceServices.getAllDevices();

            let deviceSelected = devices[0];

            let lastSelectedId = sessionStorage.getItem(sessionDeviceSelected);
            if (lastSelectedId != null) {

                console.log(`${sessionDeviceSelected}: ${parseInt(lastSelectedId)}`)

                let lastSelected = Enumerable.asEnumerable(devices)
                    .Where(d => d.deviceId == parseInt(lastSelectedId))
                    .FirstOrDefault();

                if(lastSelected != null){
                    deviceSelected = lastSelected;
                }
            }
            
            if (deviceSelected != null) {
                this.setState({
                    devices,
                    deviceSelected,
                    deviceIdSelected: deviceSelected.deviceId,

                });
            } else {
                this.setState({ devices });
            }

        } catch (e) {
            this.setState({ errorMessage: e.message })
        } finally {
            this.setIsBusy(false);
        }
    }

    componentWillUnmount() {
        clearTimeout(this.timer);
    }

    handleNameChange = (device, evt) => {
        device.name = evt.target.value;

        this.handleDeviceSave(device);
    }

    handleDeviceSave = async (device) => {
        try {
            this.setIsBusy(true);
            await this.DeviceServices.saveDevice(device);
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

        const { classes } = this.props;

        return (
            <div style={{ display: "flex", flexDirection: "column" }}>
                <div style={{ display: "flex", flexDirection: "row" }}>

                    <form autoComplete="off">
                        <FormControl className={classes.formControl}>
                            <InputLabel htmlFor="devices1">Devices</InputLabel>
                            <Select
                                value={this.state.deviceIdSelected}
                                onChange={(evt) => {

                                    let device = Enumerable.asEnumerable(this.state.devices)
                                        .Where(x => x.deviceId === evt.target.value)
                                        .FirstOrDefault();

                                    if (device == null) {
                                        this.setState({
                                            deviceSelected: "",
                                            deviceIdSelected: 0
                                        });
                                    } else {
                                        sessionStorage.setItem(sessionDeviceSelected, device.deviceId);
                                        this.setState({
                                            deviceSelected: device,
                                            deviceIdSelected: device.deviceId
                                        });
                                    }
                                }}

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
                   

                    {this.state.deviceSelected != "" && (
                        <TextField
                            label={"Device Name"}
                            value={this.state.deviceSelected.name}
                            onChange={(evt) => {
                                let device = this.state.deviceSelected;
                                device.name = evt.target.value;

                                this.handleDeviceSave(device);
                                this.setState({
                                    deviceSelected: device
                                })
                            }}
                            margin="normal"
                        />

                    )}

                     </form>

                </div>

                <div style={{ overflow: "auto" }}>
                    <DeviceIoPortEditor device={this.state.deviceSelected} />
                </div>


                {
                    this.state.isBusy && (<BusyContent />)
                }
                <ErrorContent errorMessage={this.state.errorMessage} errorClear={() => { this.setState({ errorMessage: null }) }} />
            </div >
        );
    }
}

export default withStyles(styles)(DeviceManager);