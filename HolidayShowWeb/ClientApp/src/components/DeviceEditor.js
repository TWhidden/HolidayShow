import React, { Component } from 'react';
import DeviceServices from '../Services/DeviceServices'
import List from '@material-ui/core/List';
import ListItem from '@material-ui/core/ListItem';
import ListItemText from '@material-ui/core/ListItemText';
import DeviceEditor from './DeviceIoPortsEditor';
import { ListItemIcon } from '../../node_modules/@material-ui/core';


export default class DeviceManager extends Component {
    displayName = DeviceManager.name

    constructor(props) {
        super(props)

        this.DeviceServices = DeviceServices;

        this.state = {
            devices: [],
            selectedDevice: null
        }
    }

    handleDeviceSelection = (device) => {
        this.setState({selectedDevice: device});
    }

    componentDidMount = async () => {
        let devices = await this.DeviceServices.getAllDevices();
        this.setState({ devices });
        console.log({ devices });
    }

    handleOnSave = (device) => {

    }

    render() {
        return (
            <div style={{display: "flex", flex: "1", height: "100vh"}}>
                <div style={{ display: "grid", gridTemplateColumns: "auto 1fr", flex: "1" }}>
                    <List component="nav">

                        {this.state.devices.map(device => (
                            <ListItem button onClick={()=>this.handleDeviceSelection(device)}>
                                <ListItemText inset primary={device.name} />
                                <ListItemIcon>
                                    <SaveIcon/>
                                </ListItemIcon>
                            </ListItem>
                        ))}

                    </List>

                    <div style={{overflow: "auto"}}>
                    <DeviceEditor device={this.state.selectedDevice}/>
                    </div>
                </div>
            </div>
        );
    }
}
