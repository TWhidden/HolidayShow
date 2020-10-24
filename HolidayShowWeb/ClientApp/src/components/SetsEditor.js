import React, { Component } from 'react';
import {inject, observer} from 'mobx-react';
import {observable, runInAction} from 'mobx';

import DevicePatternServices from '../Services/DevicePatternServices';
import EffectServices from '../Services/EffectServices';
import SetSequenceServices from '../Services/SetSequenceServices';
import SetServices from '../Services/SetServices';

import { withStyles } from '@material-ui/core/styles';
import Select from '@material-ui/core/Select';
import InputLabel from '@material-ui/core/InputLabel';
import MenuItem from '@material-ui/core/MenuItem';
import FormControl from '@material-ui/core/FormControl';
import TextField from '@material-ui/core/TextField';
import * as Enumerable from "linq-es5";
import IconButton from '@material-ui/core/IconButton';
import DeleteIcon from '@material-ui/icons/Delete';
import AddIcon from '@material-ui/icons/Add';
import Tooltip from '@material-ui/core/Tooltip';
import ComboSelect from 'react-select';
import Typography from '@material-ui/core/Typography';


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
        marginTop: 2,
    },
});

const sessionSetSelected = "SetEdit-SetSelected";

@inject("appStore")
@observer
class SetsEditor extends Component {
    displayName = SetsEditor.name

    @observable setIdSelected = 0;
    @observable setSelected = null;
    @observable setSequences = [];
    @observable patterns = [];
    @observable effects = [];
    @observable sets = [];

    componentDidMount = async () => {
        try {
            this.props.appStore.isBusySet(true);

            let devices = await this.props.appStore.devicesGetAllAsync();

            let none = {label: "NONE", value: null};

            let patterns = await DevicePatternServices.getAllPatterns();

            patterns = Enumerable.asEnumerable(patterns)
                                .Join(devices, pattern => pattern.deviceId, device => device.deviceId, (pattern, device)=>{return {deviceName: device.name, deviceId: device.deviceId, patternName: pattern.patternName, patternId: pattern.devicePatternId}})
                                .OrderBy(item => item.deviceName)
                                .ThenBy(item => item.patternName)
                                .Select(item => ({ label: `${item.deviceName}: ${item.patternName}` , value: item.patternId }))
                                .ToArray();

            patterns.splice(0, 0, none);

            let effects = await EffectServices.getAllEffects();

            effects = effects.map((item) => ({ label: item.effectName, value: item.effectId }));

            effects.splice(0, 0, none);

            runInAction(()=>{
                this.patterns = patterns;
                this.effects = effects;
            });

            await this.getAllSets();

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    getAllSets = async () => {
        
        let setIdSelected = 0;

        try {
            this.props.appStore.isBusySet(true);
            
            let sets = await SetServices.getAllSets();

            let setSelected = Enumerable.asEnumerable(sets).FirstOrDefault();

            let lastSelectedId = sessionStorage.getItem(sessionSetSelected);
            if (lastSelectedId != null) {

                console.log(`${sessionSetSelected}: ${Number(lastSelectedId)}`)

                let lastSelected = Enumerable.asEnumerable(sets)
                    .Where(d => d.setId === Number(lastSelectedId))
                    .FirstOrDefault();

                if(lastSelected != null){
                    setSelected = lastSelected;
                }
            }
            
            if(setSelected != null){
                setIdSelected = setSelected.setId;
            }

            runInAction(()=>{
                this.sets = sets;
                this.setIdSelected = setIdSelected;
                this.setSelected = setSelected;
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }

        if(setIdSelected !== 0){
            this.handleSetChange(setIdSelected);
        }
    }

    componentWillUnmount() {
        clearTimeout(this.timer);
    }

    handleSetChange = async (setId) => {

        var set = Enumerable.asEnumerable(this.sets)
            .Where(x => x.setId === setId)
            .FirstOrDefault();

        if (set == null) return;

        console.log(`setting ${sessionSetSelected}: ${Number(setId)}`)
        sessionStorage.setItem(sessionSetSelected, setId);

        runInAction(()=>{
            this.setSelected = set;
            this.setIdSelected = setId;
            this.setSequences = [];
        });

        await this.getSequencesForSet(set);
    }

    getSequencesForSet = async (set) => {
        try {
            this.props.appStore.isBusySet(true);
            let setSequences = await SetSequenceServices.getSetSequencesBySetId(set.setId)

            if(setSequences == null) setSequences = [];

            runInAction(()=>{
                this.setSequences = setSequences;
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleSetDelete = async (evt) => {
        try {
            this.props.appStore.isBusySet(true);

            if (this.setIdSelected == null) return;

            await SetServices.setDelete(this.setIdSelected)

            runInAction(()=>{
                this.sets.splice(this.sets.indexOf(this.setSelected), 1);
                this.setIdSelected = 0;
                this.setSelected = null;
                this.setSequences = [];
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleSetCreate = async (evt) => {
        try {
            this.props.appStore.isBusySet(true);

            let set = {
                setName: "New Set"
            };

            set = await SetServices.createSet(set);

            runInAction(()=>{
                this.sets.push(set);
                this.setSelected = set;
                this.setIdSelected = set.setId;
                this.setSequences = [];
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleSetSave = async (set) => {
        try {
            this.props.appStore.isBusySet(true);

            await SetServices.saveSet(set)

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleSequenceCreate = async () => {
        try {
            this.props.appStore.isBusySet(true);

            // find the next sequence, and add it at the end.

            var lastOnAt = Enumerable.asEnumerable(this.setSequences)
                .Select(x => x.onAt)
                .OrderByDescending(x => x)
                .FirstOrDefault();

            let nextOnAt = 0;
            if (lastOnAt != null) nextOnAt = lastOnAt * 1;
            nextOnAt = nextOnAt + 1000;

            let setSequence = {
                onAt: nextOnAt,
                setId: this.setIdSelected,
            };

            setSequence = await SetSequenceServices.createSetSequence(setSequence);
            
            runInAction(()=>{
                this.setSequences.push(setSequence);
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleSequenceDelete = async (setSequence) => {
        try {
            this.props.appStore.isBusySet(true);

            // find the next sequence, and add it at the end.
            await SetSequenceServices.deleteSetSequence(setSequence.setSequenceId);

            runInAction(()=>{
                this.setSequences = [];
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }

        await this.getSequencesForSet(this.setSelected);
    }

    render() {

        const { classes } = this.props;

        return (
            <div style={{ display: "flex", flexDirection: "column", height: "100vh", overflow: "auto"}}>
                <div style={{ display: "flex", flexDirection: "row" }}>
                    <form className={classes.root} autoComplete="off">
                        <FormControl className={classes.formControl}>
                            <InputLabel htmlFor="devices1">Sets</InputLabel>
                            <Select
                                value={this.setIdSelected}
                                onChange={(evt) => this.handleSetChange(evt.target.value)}
                                inputProps={{
                                    name: 'dev',
                                    id: 'devices1',
                                }}
                            >
                                {this.sets.map((set, i) =>
                                    (
                                        <MenuItem value={set.setId} key={i}>{set.setName}</MenuItem>
                                    ))}
                            </Select>
                        </FormControl>
                        
                    </form>
                   
                    {this.setSelected && (

                        <Tooltip title="Delete Set">
                            <IconButton onClick={(evt) => this.handleSetDelete()}><DeleteIcon /></IconButton>
                        </Tooltip>

                    )}

                    
                        <Tooltip title="Create New Set">
                            <IconButton onClick={(evt) => this.handleSetCreate()}><AddIcon /></IconButton>
                        </Tooltip>

                    
                </div>


                {this.setSelected && (
                    <div>
                        <div style={{ display: "flex", flexDirection: "column" }}>
                            <div style={{ display: "flex", flexDirection: "row" }}>
                                <TextField
                                    label={"Set Name"}
                                    value={this.setSelected.setName}
                                    onChange={(evt) => {
                                        runInAction(()=>{
                                            this.setSelected.setName = evt.target.value;
                                        });
                                        this.handleSetSave(this.setSelected);
                                    }}
                                    margin="normal"
                                />

                                <Tooltip title="Create New Sequence">
                                    <IconButton onClick={(evt) => this.handleSequenceCreate()}><AddIcon /></IconButton>
                                </Tooltip>
                            </div>
                            <div>

                                <div style={{ display: "flex", flexDirection: "row", }}>
                                    <div className="child75">
                                    <Typography variant="body2" gutterBottom>
                                        On At:
                                        </Typography>
                                     </div>

                                     <div className="child300">
                                     <Typography variant="body2" gutterBottom>
                                        Device Pattern
                                        </Typography>
                                     </div>

                                     <div className="child300">
                                     <Typography variant="body2" gutterBottom>
                                        Effect
                                        </Typography>
                                     </div>

                                       <div className="child75">
                                       <Typography variant="body2" gutterBottom>
                                        Delete
                                        </Typography>
                                     </div>

                                </div>

                                {this.setSequences.map((sequence, i) =>
                                    (
                                        <SetSequenceEdit 
                                            effects={this.effects} 
                                            patterns={this.patterns}
                                            onDelete={(s)=>this.handleSequenceDelete(s)}
                                            sequence={sequence} 
                                            key={i} />
                                    ))}


                            </div>
                        </div>
                    </div>

                )
                }
            </div >
        );
    }
}

export default withStyles(styles)(SetsEditor);

@inject("appStore")
@observer
class SetSequenceEdit extends Component {
    
    @observable devicePattern = "";
    @observable effect = "";

    componentDidMount = async () => {

        let { effects, patterns } = this.props;

        if (effects === null || patterns === null) return;

        try {

            let devicePattern = Enumerable.AsEnumerable(this.props.patterns)
                                        .Where(x => x.value === this.props.sequence.devicePatternId)
                                        .FirstOrDefault();

            let effect = Enumerable.AsEnumerable(this.props.effects)
                                        .Where(x => x.value === this.props.sequence.effectId)
                                        .FirstOrDefault();

            runInAction(()=>{
                this.devicePattern = devicePattern;
                this.effect = effect;
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        }
    }

    handleSave = async () => {

        runInAction(()=>{
            this.props.sequence.devicePatternId = this.devicePattern.value;
            this.props.sequence.effectId = this.effect.value;
        });

        try {
            await SetSequenceServices.saveSetSequence(this.props.sequence);
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
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
                    className="child75"
                    value={this.props.sequence.onAt}
                    onChange={(evt) => {

                        runInAction(()=>{
                            this.props.sequence.onAt = evt.target.value;
                        });
                        
                        this.handleDelaySave();
                    }}
                />

                <ComboSelect
                    className="child300"
                    clearable={false}
                    options={this.props.patterns}
                    onChange={(selectValue) => {
                        if(selectValue == null) return;

                        runInAction(()=>{
                            this.devicePattern = selectValue;
                        });

                        this.handleDelaySave();
                    }
                    }
                    value={this.devicePattern}
                />

                <ComboSelect
                    className="child300"
                    clearable={false}
                    options={this.props.effects}
                    onChange={(selectValue) => {
                        if(selectValue == null) return;
                        runInAction(()=>{
                            this.effect = selectValue;
                        });
                        this.handleDelaySave();
                    }
                    }
                    value={this.effect}
                />

                  <Tooltip title="Delete Sequence">
                            <IconButton onClick={(evt) => this.props.onDelete(this.props.sequence)}><DeleteIcon /></IconButton>
                   </Tooltip>
            </div>
        )
    }
}