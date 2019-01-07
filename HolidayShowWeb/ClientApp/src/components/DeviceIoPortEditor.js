import React, { Component } from 'react';
import {inject, observer} from 'mobx-react';
import {observable, runInAction} from 'mobx';
import DeviceIoPortServices from '../Services/DeviceIoPortServices';
import TextField from '@material-ui/core/TextField';
import { withStyles } from '@material-ui/core/styles';
import PropTypes from 'prop-types';
import Switch from '@material-ui/core/Switch';
import Button from '@material-ui/core/Button';
import FindReplace from '@material-ui/icons/FindReplace'
import * as Enumerable from "linq-es5";

const styles = theme => ({});

@inject("appStore")
@observer
class DeviceIoPortEditor extends Component {

    @observable ports = [];

    componentDidUpdate = async (prevProps, prevState) => {

        let { device } = this.props

        if (device == null) return;

        if (this.props.device.deviceId === prevProps.device.deviceId) return;

        try {
            this.props.appStore.isBusySet(true);

            let ports = await DeviceIoPortServices.ioPortGetByDeviceId(device.deviceId);

            // Remove the -1 pin, thats an internal pin used for NONE reference. We dont want that edited.
            runInAction(()=>{
                this.ports = Enumerable.asEnumerable(ports).Where(x => x.commandPin !== -1).ToArray();
            });
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    componentWillUnmount() {
        clearTimeout(this.timer);
    }

    handleIoPortDangerChange = (ioPort, evt) => {
        runInAction(()=>{
            ioPort.isDanger = evt.target.checked;
        });

        this.handleSave(ioPort);
    }

    handleIoPortNameChange = (ioPort, evt) => {

        runInAction(()=>{
            ioPort.description = evt.target.value;
        });

        this.handleSave(ioPort);
    }

    handleSave = async (ioPort) => {
        try {
            this.props.appStore.isBusySet(true);

            await DeviceIoPortServices.ioPortUpdate(ioPort);
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleIoPortDetect = async (ioPortId) => {
        try {
            this.props.appStore.isBusySet(true);
            await DeviceIoPortServices.ioPortIdentify(ioPortId);
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }


    setIsBusy(busyState) {
        clearTimeout(this.timer);
        if (!busyState) {
            this.props.appStore.isBusySet(false);
            return;
        }

        this.timer = setTimeout(() => {
            this.props.appStore.isBusySet(true);
        }
            , 250);
    }

    render() {
        return (
            <div>
                <div>
                    <div style={{ display: "flex", flexDirection: "column" }}>
                        {
                            this.ports.length > 0 && this.ports.map((ioPort, i) => {
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
            </div>
        );
    }

}

DeviceIoPortEditor.propTypes = {
    classes: PropTypes.object.isRequired,
};

export default withStyles(styles)(DeviceIoPortEditor);