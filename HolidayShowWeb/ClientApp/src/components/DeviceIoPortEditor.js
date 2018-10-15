import React, { Component } from 'react';

import DeviceIoPortServices from '../Services/DeviceIoPortServices';

import TextField from '@material-ui/core/TextField';
import { withStyles } from '@material-ui/core/styles';
import PropTypes from 'prop-types';
import Switch from '@material-ui/core/Switch';
import BusyContent from './controls/BusyContent';
import Button from '@material-ui/core/Button';
import FindReplace from '@material-ui/icons/FindReplace'
import * as Enumerable from "linq-es5";
import ErrorContent from './controls/ErrorContent';

const styles = theme => ({});

class DeviceIoPortEditor extends Component {

    constructor(props) {
        super(props);

        this.state = {
            ports: [],
            isBusy: false,
            errorMessage: null,
        };

    }

    DeviceIoPortServices = DeviceIoPortServices;

    componentDidUpdate = async (prevProps, prevState) => {

        let { device } = this.props

        if (device == null) return;

        if (this.props.device.deviceId === prevProps.device.deviceId) return;

        try {
            this.setIsBusy(true);

            let ports = await this.DeviceIoPortServices.ioPortGetByDeviceId(device.deviceId);

            // Remove the -1 pin, thats an internal pin used for NONE reference. We dont want that edited.
            ports = Enumerable.asEnumerable(ports).Where(x => x.commandPin !== -1).ToArray();

            this.setState({
                ports,
            });

        } catch (e) {
            this.setState({ errorMessage: e.message })
        } finally {
            this.setIsBusy(false);
        }
    }

    componentWillUnmount() {
        clearTimeout(this.timer);
    }

    handleIoPortDangerChange = (ioPort, evt) => {
        ioPort.isDanger = evt.target.checked;
        this.handleSave(ioPort);
    }

    handleIoPortNameChange = (ioPort, evt) => {

        ioPort.description = evt.target.value;

        this.handleSave(ioPort);
    }

    handleSave = async (ioPort) => {
        try {
            this.setIsBusy(true);

            await this.DeviceIoPortServices.ioPortUpdate(ioPort);

        } catch (e) {
            this.setState({ errorMessage: e.message })
        } finally {
            this.setIsBusy(false);
        }
    }

    handleIoPortDetect = async (ioPortId) => {
        try {
            this.setIsBusy(true);
            await this.DeviceIoPortServices.ioPortIdentify(ioPortId);
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
        return (
            <div>
                <div>
                    <div style={{ display: "flex", flexDirection: "column" }}>
                        {
                            this.state.ports.length > 0 && this.state.ports.map((ioPort, i) => {
                                return (
                                    <div style={{ display: "flex", flexDirection: "row", verticalAlign: "center" }} key={i}>

                                        <TextField
                                        style={{marginTop: "0px", marginBottom: "0px"}}
                                            label={"PIN: " + ioPort.commandPin + ""}
                                            value={ioPort.description}
                                            onChange={(evt) => this.handleIoPortNameChange(ioPort, evt)}
                                            margin="normal"
                                        />

                                        <div style={{ verticalAlign: "center" }}>
                                            <Switch
                                            style={{marginTop: "0px", marginBottom: "0px"}}
                                                checked={ioPort.isDanger}
                                                onChange={(evt) => this.handleIoPortDangerChange(ioPort, evt)}
                                            />
                                        </div>

                                        <Button onClick={(evt) => this.handleIoPortDetect(ioPort.deviceIoPortId)}
                                        style={{marginTop: "0px", marginBottom: "0px"}}
                                        ><FindReplace /></Button>
                                    </div>
                                )
                            })
                        }
                    </div>
                </div>
                {
                    this.state.isBusy && (<BusyContent />)
                }
                <ErrorContent errorMessage={this.state.errorMessage} errorClear={() => { this.setState({ errorMessage: null }) }} />
            </div>
        );
    }

}

DeviceIoPortEditor.propTypes = {
    classes: PropTypes.object.isRequired,
};

export default withStyles(styles)(DeviceIoPortEditor);