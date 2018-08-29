import React, { Component } from 'react';
import DeviceServices from '../Services/DeviceServices';
import PatternServices from '../Services/DevicePatternServices';

import BusyContent from './controls/BusyContent';
import { withStyles } from '@material-ui/core/styles';
import Select from '@material-ui/core/Select';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import FormControl from '@material-ui/core/FormControl';
import TextField from '@material-ui/core/TextField';
import * as Enumerable from "linq-es2015";
import IconButton from '@material-ui/core/IconButton';
import DeleteIcon from '@material-ui/icons/Delete';
import AddIcon from '@material-ui/icons/Add';
import Tooltip from '@material-ui/core/Tooltip';
import Link, { LinkedComponent } from 'valuelink';
import PropTypes from 'proptypes';
import { Input, isRequired, isEmail } from 'valuelink/tags';

import './CommonStyles.css';

const styles = theme => ({
    root: {
        display: 'flex',
        flexWrap: 'wrap',
    },
    formControl: {
        margin: 0,
        minWidth: 120,
    },
    selectEmpty: {
        marginTop: theme.spacing.unit * 2,
    },
});

// MyStateless = (props) => {
//     <div>
//         <TextField value={props.myVal} />
//     </div>
// }

class DevicePattern extends LinkedComponent {
    displayName = DevicePattern.name

    constructor(props) {
        super(props)

        this.DeviceServices = DeviceServices;
        this.PatternServices = PatternServices;

        this.state = {
            devices: [],
            deviceSelected: "",
            patterns: [],
            patternSelected: "",
            patternSequences: [],
            isBusy: false,
        };
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
        let device = evt.target.value;
        //device = Object.assign({}, device);

        await this.getPatternsForSelectedDevice(device);
    }

    getPatternsForSelectedDevice = async (device) => {
        try {
            //this.setIsBusy(true);
            let patterns = await this.PatternServices.getDevicePatternsByDeviceId(device.deviceId);

            this.setState({
                patterns,
                patternSelected: "",
                patternSequences: [],
                deviceSelected: device
            });

        } catch (e) {

        } finally {
            //this.setIsBusy(false);
        }
    }

    handlePatternChange = async (pattern) => {
        let patternIdSelected = pattern.devicePatternId;

        let patternSelected = Enumerable.asEnumerable(this.state.patterns)
            .Where(x => x.devicePatternId == patternIdSelected)
            .FirstOrDefault();

        this.setState({
            patternSelected
        });
    }

    handlePatternDelete = async (evt) => {
        try {
            this.setIsBusy(true);

            if (this.state.patternSelected == null) return;

            await this.PatternServices.deletePatternByPatternId(this.state.patternSelected.devicePatternId);

            let patterns = this.state.patterns;

            // Remove the element from the existing list
            patterns.splice(patterns.indexOf(this.state.patternSelected), 1);

            this.setState({
                patternSelected: "",
                patternSequences: [],
                patterns
            })

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    handlePatternCreate = async (evt) => {
        try {
            this.setIsBusy(true);

            let newPattern = {
                deviceId: this.state.deviceSelected.deviceId,
                patternName: "New Pattern",
            };

            newPattern = await this.PatternServices.createPattern(newPattern);

            let patterns = this.state.patterns;
            patterns.push(newPattern);

            this.setState({
                patterns,
                patternSelected: newPattern
            })

        } catch (e) {

        } finally {
            this.setIsBusy(false);
        }
    }

    setIsBusy(busyState) {
        this.setState({ isBusy: busyState });
    }

    handlePatternNameChange = (pattern, evt) => {
        pattern.patternName = evt.target.value;
        this.setState({ patternSelected: pattern });
    }

    render() {

        const { classes } = this.props;

        const usersLink = this.linkAt('patternSelected.devicePatternSequences');

        return (
            <div style={{ display: "flex", flexDirection: "column", height: "100vh" }}>
                <div style={{ display: "flex", flexDirection: "row" }}>
                    <form className={classes.root} autoComplete="off">
                        <FormControl className={classes.formControl}>
                            <InputLabel htmlFor="devices1">Devices</InputLabel>
                            <Select
                                value={this.state.deviceSelected}
                                onChange={(evt) => this.handleDeviceChange(evt)}
                                inputProps={{
                                    name: 'dev',
                                    id: 'devices1',
                                }}
                            >
                                {this.state.devices.map((device, i) =>
                                    (
                                        <MenuItem value={device} key={i}>{device.deviceId}: {device.name}</MenuItem>
                                    ))}
                            </Select>
                        </FormControl>
                    </form>
                    {this.state.patterns && this.state.patterns.length > 0 && (
                        <form className={classes.root} autoComplete="off">
                            <FormControl className={classes.formControl}>
                                <InputLabel htmlFor="patterns1">Patterns</InputLabel>
                                <Select
                                    value={this.state.patternSelected}
                                    onChange={(evt) => this.handlePatternChange(evt.target.value)}
                                    inputProps={{
                                        name: 'pattern',
                                        id: 'patterns1',
                                    }}
                                >
                                    {this.state.patterns && this.state.patterns.map((pattern, i) =>
                                        (
                                            <MenuItem value={pattern}  key={i}>{pattern.patternName}</MenuItem>
                                        ))}
                                </Select>
                            </FormControl>
                        </form>
                    )}

                    {this.state.patternSelected && (

                        <Tooltip title="Delete Pattern" stule={{ verticalAlign: "middle" }}>
                            <IconButton onClick={(evt) => this.handlePatternDelete()}><DeleteIcon /></IconButton>
                        </Tooltip>

                    )}

                    {this.state.deviceSelected && (
                        <Tooltip title="Create New Pattern">
                            <IconButton onClick={(evt) => this.handlePatternCreate()}><AddIcon /></IconButton>
                        </Tooltip>

                    )}
                </div>


                {this.state.patternSelected && (
                    <div>
                        <div style={{ display: "flex", flexDirection: "column" }}>
                            <div>
                                <TextField
                                    label={"Pattern Name"}
                                    value={this.state.patternSelected.patternName}
                                    onChange={(evt) => this.handlePatternNameChange(this.state.patternSelected, evt)}
                                    margin="normal"
                                />
                            </div>
                            <div>



                                {this.state.patternSequences && this.state.patternSequences.map((sequence, i) =>
                                    (
                                        <EditPattern userLink={Link.value({}, x => usersLink.push(x))} />
                                    ))}


                            </div>
                        </div>
                    </div>

                )}

                {
                    this.state.isBusy && (<BusyContent />)
                }
            </div>
        );
    }
}

export default withStyles(styles)(DevicePattern);

class EditPattern extends LinkedComponent {
    static proTypes = {
        userLink: PropTypes.instanceOf(Link).isRequired,
    }

    state = {
        name: '',
        email: '',
        isActive: true
    };

    componentWillMount() {
        this.setState(this.props.userLink.value);
    }

    render() {

        const linked = this.linkAll();

        return (

            <div><TextField valueLink={linked.OnAt} /></div>
        )
    }



}