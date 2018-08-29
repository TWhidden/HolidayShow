import React, { Component } from 'react';
import TextField from '@material-ui/core/TextField';
import { withStyles } from '@material-ui/core/styles';
import PropTypes from 'prop-types';
import Switch from '@material-ui/core/Switch';
import BusyContent from './controls/BusyContent';
import Button from '@material-ui/core/Button';
import FindReplace from '@material-ui/icons/FindReplace'

import DeviceIoPortServices from '../Services/DeviceIoPortServices';

const styles = theme => ({

});

class DeviceIoPortEditor extends Component {

    constructor(props) {
        super(props);

        this.state = {
            ports: [],
            isBusy: false
        };

    }

    DeviceIoPortServices = DeviceIoPortServices;

    componentDidUpdate = async (prevProps, prevState) => {

        let { device } = this.props

        if (this.props.device.deviceId == prevProps.device.deviceId) return;

        try {
            this.setIsBusy(true);

            let ports = await this.DeviceIoPortServices.ioPortGetByDeviceId(device.deviceId);

            this.setState({
                ports,
            });

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    handleIoPortDangerChange = (ioPort, evt) => {
        ioPort.isDanger = evt.target.checked;
        this.handleSave(ioPort);
    }

    handleIoPortNameChange = async (ioPort, evt) => {
        ioPort.description = evt.target.value;
        await this.handleSave(ioPort);
    }

    handleSave = async (ioPort) => {
        try {
            this.setIsBusy(true);

            await this.DeviceIoPortServices.ioPortUpdate(ioPort);
            
        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    handleIoPortDetect = async (ioPortId) => {
        try {
            this.setIsBusy(true);
            await this.DeviceIoPortServices.ioPortIdentify(ioPortId);
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
            <div>
                <div>
                    <div style={{ display: "flex", flexDirection: "column" }}>
                        {
                            this.state.ports.length > 0 && this.state.ports.map((ioPort, i) => {
                                return (
                                    <div style={{ display: "flex", flexDirection: "row" }} key={i}>

                                        <TextField
                                            label={"PIN: " + ioPort.commandPin + ""}
                                            value={ioPort.description}
                                            onChange={(evt) => this.handleIoPortNameChange(ioPort, evt)}
                                            margin="normal"
                                        />

                                        <Switch
                                            checked={ioPort.isDanger}
                                            onChange={(evt) => this.handleIoPortDangerChange(ioPort, evt)}
                                        />

                                        <Button onClick={(evt) => this.handleIoPortDetect(ioPort.deviceIoPortId)}><FindReplace /></Button>
                                    </div>
                                )
                            })
                        }
                    </div>
                </div>
                {
                    this.state.isBusy && (<BusyContent />)
                }
            </div>
        );
    }

}

DeviceIoPortEditor.propTypes = {
    classes: PropTypes.object.isRequired,
};

export default withStyles(styles)(DeviceIoPortEditor);