import React, { Component } from 'react';
import DeviceServices from '../Services/DeviceServices'
import DeviceIoPortServices from '../Services/DeviceIoPortServices'

import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import DeviceIoPortEditor from './DeviceIoPortEditor';
import SaveIcon from '@material-ui/icons/Save'
import Button from '@material-ui/core/Button';
import TextField from '@material-ui/core/TextField';
import BusyContent from './controls/BusyContent';
import { withStyles } from '@material-ui/core/styles';
import PropTypes from 'prop-types';

import './CommonStyles.css';

const styles = theme => ({
    textField: {
        fontSize: '14px',
        paddingBottom: 0,
        marginTop: 4,
        fontWeight: 500
    },

    input: {
        fontSize: '16px'
    }
});

class DeviceManager extends Component {
    displayName = DeviceManager.name

    constructor(props) {
        super(props)

        this.DeviceServices = DeviceServices;
        this.DeviceIoPortServices = DeviceIoPortServices;

        this.state = {
            devices: [],
            selectedDevice: "",
            isBusy: false,
        };
    }

    handleDeviceSelection = (device) => {
        this.setState({ selectedDevice: device });
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

    handleNameChange = (device, evt) => {
        device.name = evt.target.value;

        this.setState({});
    }

    handleDeviceSave = async (device) => {
        try {
            this.setIsBusy(true);
            await this.DeviceServices.saveDevice(device);
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
                <div style={{ display: "grid", gridTemplateColumns: "auto 1fr", flex: "1" }}>
                    <List component="nav">

                        {this.state.devices.map((device, i) =>
                            (
                                <ListItem button onClick={() => this.handleDeviceSelection(device)} key={i}>
                                    <TextField
                                        label={`Device ID: ${device.deviceId}`}
                                        value={device.name}
                                        onChange={(evt) => this.handleNameChange(device, evt)}
                                        margin="normal"
                                        className={classes.textField}
                                        InputProps={{
                                            className: classes.input,
                                        }}
                                    />
                                    {/* <Button onClick={(evt) => this.handleDeviceSave(device, evt)} className={this.getDirty(device) ? "visibile" : "hidden"}><SaveIcon /></Button> */}
                                </ListItem>
                            ))}

                    </List>

                    <div style={{ overflow: "auto" }}>
                        <DeviceIoPortEditor device={this.state.selectedDevice} />
                    </div>
                </div>
                {
                    this.state.isBusy && (<BusyContent />)
                }
            </div>
        );
    }
}

export default withStyles(styles)(DeviceManager);