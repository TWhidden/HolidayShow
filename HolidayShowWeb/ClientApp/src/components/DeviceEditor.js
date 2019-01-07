import {inject, observer} from 'mobx-react';
import {observable, runInAction} from 'mobx';
import React, { Component } from 'react';
import DeviceIoPortEditor from './DeviceIoPortEditor';
import TextField from '@material-ui/core/TextField';
import { withStyles } from '@material-ui/core/styles';
import FormControl from '@material-ui/core/FormControl';
import Select from '@material-ui/core/Select';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';

const styles = theme => ({
    formControl: {
        margin: 0,
        minWidth: 120,
    },
});

const sessionDeviceSelected = "DeviceEdit-DeviceSelected";

@inject("appStore")
@observer
class DeviceManager extends Component {
    displayName = DeviceManager.name

    @observable deviceIdSelected = 0;
    @observable deviceSelected = "";

    componentDidMount = async () => {

        try {
            // preload the devices
            await this.props.appStore.devicesGetAllAsync();

            // select the first device in the list
            if(this.props.appStore.devices.length === 0) return; 

            let deviceSelected = this.props.appStore.devices[0];

            let lastSelectedId = sessionStorage.getItem(sessionDeviceSelected);
            if (lastSelectedId != null) {

                console.log(`${sessionDeviceSelected}: ${Number(lastSelectedId)}`)

                let lastSelected = this.props.appStore.deviceGetByDeviceId(lastSelectedId);

                if(lastSelected != null){
                    deviceSelected = lastSelected;
                }
            }
            
            if (deviceSelected != null) {
                runInAction(() => {
                    this.deviceIdSelected = deviceSelected.deviceId;
                    this.deviceSelected = deviceSelected;
                });
            }

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        }
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
                                value={this.deviceIdSelected}
                                onChange={(evt) => {

                                    let device = this.props.appStore.deviceGetByDeviceId(evt.target.value);
                   
                                    if (device == null) {
                                        runInAction(() => {
                                            this.deviceSelected = "";
                                            this.deviceIdSelected = 0;
                                        });
                                    } else {
                                        sessionStorage.setItem(sessionDeviceSelected, device.deviceId);
                                        runInAction(() => {
                                            this.deviceSelected = device;
                                            this.deviceIdSelected = device.deviceId;
                                        });
                                    }
                                }}

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
                   

                    {this.deviceSelected !== "" && (
                        <TextField
                            label={"Device Name"}
                            value={this.deviceSelected.name}
                            onChange={(evt) => {
                                runInAction(() => {
                                    let device = this.deviceSelected;
                                    device.name = evt.target.value;

                                    this.props.appStore.deviceSave(device);
                                    this.deviceSelected = device;
                                });
                            }}
                            margin="normal"
                        />
                    )}
                     </form>
                </div>

                <div style={{ overflow: "auto" }}>
                    <DeviceIoPortEditor device={this.deviceSelected} />
                </div>
            </div >
        );
    }
}

export default withStyles(styles)(DeviceManager);