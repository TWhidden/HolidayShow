import React, { Component } from 'react';
import TextField from '@material-ui/core/TextField';
import { withStyles } from '@material-ui/core/styles';
import PropTypes from 'prop-types';
import Switch from '@material-ui/core/Switch';

import Button from '@material-ui/core/Button';
import FindReplace from '@material-ui/icons/FindReplace'

const styles = theme => ({
    textField: {
        fontSize: '14px',
        paddingBottom: 0,
        marginTop: 4,
        fontWeight: 500
    },
    formHelperText: {
        fontSize: '16px'
    },
    input: {
        fontSize: '16px'
    }
});

class DeviceIoPortEditor extends Component {

    constructor(props) {
        super(props);
    }

    handleIoPortDangerChange = (ioPort, evt) => {
        ioPort.isDanger = evt.target.checked;

        this.props.onIoPortChanged(evt);
    }

    handleIoPortNameChange = (ioPort, evt) => {
        ioPort.description = evt.target.value;

        this.props.onIoPortChanged(evt);

        this.setState({});
    }

    render() {

        const { classes } = this.props;


        return (
            <div>
                <div>
                    <div style={{ display: "flex", flexFlow: "column" }}>
                        {
                            (this.props.device && this.props.device.deviceIoPorts) && this.props.device.deviceIoPorts.map((ioPort, i) => {
                                return (
                                    <div style={{ display: "flex", flexFlow: "row" }} key={i}>

                                        <TextField
                                            label={"PIN: " + ioPort.commandPin + ""}
                                            value={ioPort.description}
                                            onChange={(evt) => this.handleIoPortNameChange(ioPort, evt)}
                                            margin="normal"
                                            className={classes.textField}
                                            InputLabelProps={{
                                                style: {
                                                    // backgroundColor: "red",
                                                    fontSize: "18px"
                                                },
                                            }}
                                        />

                                        <Switch
                                            checked={ioPort.isDanger}
                                            onChange={(evt) => this.handleIoPortDangerChange(ioPort, evt)}
                                        />

                                        <Button onClick={(evt) => this.props.onIoPortDetect(ioPort.deviceIoPortId, evt)}><FindReplace /></Button>
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