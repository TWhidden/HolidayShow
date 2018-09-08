import React, { Component } from 'react';

import EffectServices from '../Services/EffectServices';
import EffectsAvailableServices from '../Services/EffectsAvailableServices';

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
import ErrorContent from './controls/ErrorContent';

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


class EffectsEditor extends Component {
    displayName = EffectsEditor.name

    constructor(props) {
        super(props)

        this.EffectServices = EffectServices;
        this.EffectsAvailableServices = EffectsAvailableServices;

        this.state = {
            effects: [],
            effectSelected: null,
            effectIdSelected: 0,
            effectsAvailable: [],
            errorMessage: null,
        };
    }

    componentDidMount = async () => {

        try {
            this.setIsBusy(true);

            await this.getAllEffects();

            let effectsAvailable = await this.EffectsAvailableServices.getAllAvailableEffects();

            effectsAvailable = effectsAvailable.map((item) => ({ label: `${item.displayName}`, value: item.effectInstructionId }));

            this.setState({
                effectsAvailable,
            });

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    getAllEffects = async () => {
        try {
            this.setIsBusy(true);

            let effects = await this.EffectServices.getAllEffects();

            let effectSelected = Enumerable.AsEnumerable(effects).FirstOrDefault();
            let effectIdSelected = 0;
            if (effectSelected != null) {
                effectIdSelected = effectSelected.effectId;
            }

            this.setState({
                effects,
                effectSelected,
                effectIdSelected
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

    handleEffectChange = async (evt) => {
        let effectId = evt.target.value;

        var effect = Enumerable.asEnumerable(this.state.effects)
            .Where(x => x.effectId === effectId)
            .FirstOrDefault();

        if (effect == null) return;

        this.setState({
            effectSelected: effect,
            effectIdSelected: effect.effectId,
        });
    }

    handleEffectDelete = async () => {
        try {
            this.setIsBusy(true);

            if (this.state.effectSelected == null) return;

            await this.EffectServices.deleteEffect(this.state.effectIdSelected);

        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }

        // rebuild the list.
        this.getAllEffects();
    }

    handleEffectCreate = async () => {
        try {
            this.setIsBusy(true);

            // Get the default Instruction to use
            let effectAvailable = Enumerable.asEnumerable(this.state.effectsAvailable).FirstOrDefault();
            if (effectAvailable == null) return;

            let effect = {
                effectName: "New Effect",
                effectInstructionId: effectAvailable.value,
                instructionMetaData: "DEVPINS=;DUR=500",
                duration: 5000
            };

            effect = await this.EffectServices.createEffect(effect);

            let effects = this.state.effects;
            effects.push(effect);

            this.setState({
                effects,
                effectSelected: effect,
                effectIdSelected: effect.effectId
            });

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

    handlePatternNameChange = (effect, evt) => {
        effect.effectName = evt.target.value;
        this.handleEffectSave(effect);
    }

    handleEffectSave = async (effect) => {
        try {
            this.setIsBusy(true);

            this.setState({effect});

            await this.EffectServices.saveEffect(effect);
        } catch (e) {
            this.setState({errorMessage: e.message})
        } finally {
            this.setIsBusy(false);
        }
    }

    render() {

        const { classes } = this.props;

        return (
            <div style={{ display: "flex", flexDirection: "column", height: "100vh" }}>
                <div style={{ display: "flex", flexDirection: "row" }}>
                    <form className={classes.root} autoComplete="off">
                        
                        <FormControl className={classes.formControl}>
                            <InputLabel htmlFor="devices1">Effects</InputLabel>
                            <Select
                                value={this.state.effectIdSelected}
                                onChange={(evt) => this.handleEffectChange(evt)}
                                inputProps={{
                                    name: 'dev',
                                    id: 'devices1',
                                }}
                            >
                                {this.state.effects.map((effect, i) =>
                                    (
                                        <MenuItem value={effect.effectId} key={i}>{effect.effectName}</MenuItem>
                                    ))}
                            </Select>
                        </FormControl>
                    </form>

                    {this.state.effectSelected && (

                        <Tooltip title="Delete Effect">
                            <IconButton onClick={(evt) => this.handleEffectDelete()}><DeleteIcon /></IconButton>
                        </Tooltip>

                    )}


                    <Tooltip title="Create New Effect">
                        <IconButton onClick={(evt) => this.handleEffectCreate()}><AddIcon /></IconButton>
                    </Tooltip>


                </div>


                {this.state.effectSelected && (
                    <div>
                        <div style={{ display: "flex", flexDirection: "column" }}>
                            <div style={{ display: "flex", flexDirection: "row" }}>
                                <TextField
                                    label={"Effect Name"}
                                    value={this.state.effectSelected.effectName}
                                    onChange={(evt) => {
                                        let effect = this.state.effectSelected;
                                        effect.effectName = evt.target.value;
                                        this.handleEffectSave(effect);
                                    }}
                                    margin="normal"
                                />

                                <TextField
                                    label={"Duration"}
                                    style={{ width: "100px" }}
                                    value={this.state.effectSelected.duration}
                                    margin="normal"
                                    onChange={(evt) => {
                                        let effect = this.state.effectSelected;
                                        effect.duration = evt.target.value;
                                        this.handleEffectSave(effect);
                                    }}
                                />

                                <VirtualizedSelect
                                    className="child"
                                    value={this.state.effectSelected.effectInstructionId}
                                    options={this.state.effectsAvailable}
                                    onChange={(selectValue) => {
                                        if (selectValue == null) return;
                                        let effect = this.state.effectSelected;
                                        effect.effectInstructionId =selectValue.value;
                                        this.handleEffectSave(effect);
                                    }
                                    }

                                />
                            </div>
                            <div>

                                <TextField
                                    label={"Duration"}
                                    style={{ width: "100%" }}
                                    value={this.state.effectSelected.instructionMetaData}
                                    margin="normal"
                                    onChange={(evt) => {
                                        let effect = this.state.effectSelected;
                                        effect.instructionMetaData = evt.target.value;
                                        this.handleEffectSave(effect);
                                    }}
                                />

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

export default withStyles(styles)(EffectsEditor);
