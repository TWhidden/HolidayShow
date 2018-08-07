
import React, { Component } from 'react';
import TextField from '@material-ui/core/TextField';
import Button from '@material-ui/core/Button';

import DeviceServices from '../Services/DeviceServices'
import CircularProgress from '@material-ui/core/CircularProgress'
import SaveIcon from '@material-ui/icons/Save'

export default class DeviceEditor extends Component {

    constructor(props) {
        super(props);

        this.DeviceServices = DeviceServices;

        this.state = {
            isBusy: false,
        }

    }

    handleIoPortNameChange = (ioPort, evt) => {
        ioPort.description = evt.target.value;

        this.setState({});
    }

    handleDeviceSave = async () => {
        try {
            this.setState({ isBusy: true })
            let saved = await this.DeviceServices.saveDevice(this.props.device);
        } catch (e) {

        } finally {
            this.setState({ isBusy: false })
        }

        if(this.props.onSave){
            this.props.onSave();
        }

    }

    setProgressRef = (ref)=>{
        this.progress = ref;
    }

    render() {
        return (
            <div>

                {
                    this.state.isBusy ? (
                        <CircularProgress ref={this.setProgressRef} />
                    ) :
                        (

                            <div>

                                <Button onClick={this.handleDeviceSave}><SaveIcon/></Button>

                                <div style={{ display: "flex", flexFlow: "column" }}>

                                    {
                                        (this.props.device && this.props.device.deviceIoPorts) && this.props.device.deviceIoPorts.map((ioPort) => {

                                            return (
                                                <div style={{ display: "flex", flexFlow: "row" }}>
                                                    <TextField
                                                        label="Name"
                                                        value={ioPort.description}
                                                        onChange={(evt) => this.handleIoPortNameChange(ioPort, evt)}
                                                        margin="normal"
                                                    />


                                                </div>
                                            )

                                        })
                                    }
                                </div>

                            </div>
                        )
                }
            </div>
        );
    }

}