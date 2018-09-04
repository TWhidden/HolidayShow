import React, { Component } from 'react';
import DevicePatternServices from '../Services/DevicePatternServices';
import SetServices from '../Services/SetServices';
import SetSequenceServices from '../Services/SetSequenceServices';
import EffectServices from '../Services/EffectServices';

import 'react-select/dist/react-select.css'
import 'react-virtualized/styles.css'
import 'react-virtualized-select/styles.css'

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
import VirtualizedSelect from 'react-virtualized-select'
import Typography from '@material-ui/core/Typography';
import ErrorContent from './controls/ErrorContent';


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

class SetsEditor extends Component {
    displayName = SetsEditor.name

    constructor(props) {
        super(props)

        this.DevicePatternServices = DevicePatternServices;
        this.SetServices = SetServices;
        this.SetSequenceServices = SetSequenceServices;
        this.EffectServices = EffectServices;

        this.state = {
            sets: [],
            setIdSelected: 0,
            setSelected: null,
            setSequences: [],
            patterns: [],
            effects: [],
            errorMessage: null,
        };
    }

    componentDidMount = async () => {

        try {
            this.setIsBusy(true);
            
            await this.getAllSets();

            let patterns = await this.DevicePatternServices.getAllPatterns();

            patterns = patterns.map((item) => ({ label: `${item.deviceId}: ${item.patternName}` , value: item.devicePatternId }));

            let effects = await this.EffectServices.getAllEffects();

            effects = effects.map((item) => ({ label: item.effectName, value: item.effectId }));

            this.setState({
                patterns,
                effects,
            });

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    getAllSets = async () => {
        try {
            this.setIsBusy(true);
            
            let sets = await this.SetServices.getAllSets();

            let setSelected = Enumerable.asEnumerable(sets).FirstOrDefault();
            let setIdSelected = 0;
            if(setSelected != null){
                setIdSelected = setSelected.setId;
            }

            this.setState({
                sets,
                setIdSelected,
                setSelected,
            });

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    componentWillUnmount() {
        clearTimeout(this.timer);
    }

    handleSetChange = async (evt) => {
        let setId = evt.target.value;

        var set = Enumerable.asEnumerable(this.state.sets)
            .Where(x => x.setId == setId)
            .FirstOrDefault();

        if (set == null) return;

        this.setState({
            setSelected: set,
            setIdSelected: setId
        });

        await this.getSequencesForSet(set);
    }

    getSequencesForSet = async (set) => {
        try {
            this.setIsBusy(true);
            let setSequences = await this.SetSequenceServices.getSetSequencesBySetId(set.setId)

            if(setSequences == null) setSequences = [];

            this.setState({
                setSequences,
            });

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    handleSetDelete = async (evt) => {
        try {
            this.setIsBusy(true);

            if (this.state.setIdSelected == null) return;

            await this.SetServices.setDelete(this.state.setIdSelected)

            let sets = this.state.sets;

            sets.splice(sets.indexOf(this.state.setSelected), 1);

            this.setState({
                sets,
                setIdSelected: 0,
                setSelected: null,
                setSequences: []
            })

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    handleSetCreate = async (evt) => {
        try {
            this.setIsBusy(true);

            let set = {
                setName: "New Set",
            };

            set = await this.SetServices.createSet(set);

            let sets = this.state.sets;
            sets.push(set);

            this.setState({
                sets,
                setSelected: set,
                setIdSelected: set.setId
            });

            await this.getSequencesForSet();

        } catch (e) {
            this.setState({errorMessage: e.message})
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

    handlePatternNameChange = (set, evt) => {
        set.setName = evt.target.value;
        this.handleSetSave(set);
    }

    handleSetSave = async (set) => {
        try {
            this.setIsBusy(true);

            await this.SetServices.saveSet(set)

            this.setState({});
        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    handleSequenceCreate = async () => {
        try {
            this.setIsBusy(true);

            // find the next sequence, and add it at the end.

            var lastOnAt = Enumerable.asEnumerable(this.state.setSequences)
                .Select(x => x.onAt)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            let nextOnAt = 0;
            if (lastOnAt != null) nextOnAt = lastOnAt * 1;
            nextOnAt = nextOnAt + 1000;

            let setSequence = {
                onAt: nextOnAt,
                setId: this.state.setIdSelected,

            };

            setSequence = await this.SetSequenceServices.createSetSequence(setSequence);

            let sequences = this.state.setSequences;
            sequences.push(setSequence);

            this.setState({
                sequences
            })

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    handleSequenceDelete = async (setSequence) => {
        try {
            this.setIsBusy(true);

            // find the next sequence, and add it at the end.
            await this.SetSequenceServices.deleteSetSequence(setSequence.setSequenceId);

            this.setState({
                 setSequences: []
            })
        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }

        await this.getSequencesForSet(this.state.setSelected);
    }

    render() {

        const { classes } = this.props;

        return (
            <div style={{ display: "flex", flexDirection: "column", height: "100vh" }}>
                <div style={{ display: "flex", flexDirection: "row" }}>
                    <form className={classes.root} autoComplete="off">
                        <FormControl className={classes.formControl}>
                            <InputLabel htmlFor="devices1">Sets</InputLabel>
                            <Select
                                value={this.state.setIdSelected}
                                onChange={(evt) => this.handleSetChange(evt)}
                                inputProps={{
                                    name: 'dev',
                                    id: 'devices1',
                                }}
                            >
                                {this.state.sets.map((set, i) =>
                                    (
                                        <MenuItem value={set.setId} key={i}>{set.setName}</MenuItem>
                                    ))}
                            </Select>
                        </FormControl>
                        
                    </form>
                   
                    {this.state.setSelected && (

                        <Tooltip title="Delete Set">
                            <IconButton onClick={(evt) => this.handleSetDelete()}><DeleteIcon /></IconButton>
                        </Tooltip>

                    )}

                    
                        <Tooltip title="Create New Set">
                            <IconButton onClick={(evt) => this.handleSetCreate()}><AddIcon /></IconButton>
                        </Tooltip>

                    
                </div>


                {this.state.setSelected && (
                    <div>
                        <div style={{ display: "flex", flexDirection: "column" }}>
                            <div style={{ display: "flex", flexDirection: "row" }}>
                                <TextField
                                    label={"Set Name"}
                                    value={this.state.setSelected.setName}
                                    onChange={(evt) => this.handlePatternNameChange(this.state.setSelected, evt)}
                                    margin="normal"
                                />

                                <Tooltip title="Create New Sequence">
                                    <IconButton onClick={(evt) => this.handleSequenceCreate()}><AddIcon /></IconButton>
                                </Tooltip>
                            </div>
                            <div>

                                <div style={{ display: "flex", flexDirection: "row", }}>
                                    <div className="child">
                                    <Typography variant="body2" gutterBottom>
                                        On At:
                                        </Typography>
                                     </div>

                                     <div className="child">
                                     <Typography variant="body2" gutterBottom>
                                        Device Pattern
                                        </Typography>
                                     </div>

                                     <div className="child">
                                     <Typography variant="body2" gutterBottom>
                                        Effect
                                        </Typography>
                                     </div>

                                       <div className="child">
                                       <Typography variant="body2" gutterBottom>
                                        Delete
                                        </Typography>
                                     </div>

                                </div>

                                {/* {this.state.patternSequences && this.state.patternSequences.map((sequence, i) => */}
                                {this.state.setSequences.map((sequence, i) =>
                                    (
                                        <SetSequenceEdit 
                                            sequence={sequence} 
                                            effects={this.state.effects} 
                                            patterns={this.state.patterns}
                                            onDelete={(s)=>this.handleSequenceDelete(s)}
                                            key={i} />
                                    ))}


                            </div>
                        </div>
                    </div>

                )
                }

                {
                    this.state.isBusy && (<BusyContent />)
                }
                <ErrorContent errorMessage={this.state.errorMessage} errorClear={()=>{this.setState({errorMessage: null})}}/>
            </div >
        );
    }
}

export default withStyles(styles)(SetsEditor);

class SetSequenceEdit extends Component {
    constructor(props) {
        super(props)

        this.state = {
            onAt: this.props.sequence.onAt,
            devicePatternId: this.props.sequence.devicePatternId,
            effectId: this.props.sequence.effectId,
        };

        this.SetSequenceServices = SetSequenceServices;
    }

    handleSave = async () => {
        var sequence = this.props.sequence;
        
        sequence.onAt = this.state.onAt;
        sequence.devicePatternId = this.state.devicePatternId;
        sequence.effectId = this.state.effectId;

        try {
            await this.SetSequenceServices.saveSetSequence(sequence);
        } catch (e) {

        } finally {

        }
    }

    handleDelaySave() {
        clearTimeout(this.saveTimer);

        this.saveTimer = setTimeout(() => this.handleSave(), 1000);
    }

    render() {

        return (

            <div style={{ display: "flex", flexDirection: "row", }}>
                <TextField
                    className="child"
                    value={this.state.onAt}
                    onChange={(evt) => {
                        this.setState(
                            {
                                onAt: evt.target.value
                            }
                        );
                        this.handleDelaySave();
                    }}
                />

                <VirtualizedSelect
                    className="child"
                    options={this.props.patterns}
                    onChange={(selectValue) => {
                        if(selectValue == null) return;
                        this.setState({ devicePatternId: selectValue.value })
                        this.handleDelaySave();
                    }
                    }
                    value={this.state.devicePatternId}
                />

                <VirtualizedSelect
                    className="child"
                    options={this.props.effects}
                    onChange={(selectValue) => {
                        if(selectValue == null) return;
                        this.setState({ effectId: selectValue.value })
                        this.handleDelaySave();
                    }
                    }
                    value={this.state.effectId}
                />

                  <Tooltip title="Delete Sequence">
                            <IconButton onClick={(evt) => this.props.onDelete(this.props.sequence)}><DeleteIcon /></IconButton>
                   </Tooltip>
            </div>
        )
    }
}