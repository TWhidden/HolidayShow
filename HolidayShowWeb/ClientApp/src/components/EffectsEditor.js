import React, { Component } from 'react';
import {inject, observer} from 'mobx-react';
import {observable, runInAction} from 'mobx';

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
import EffectServices from '../Services/EffectServices';

import { DragDropContext, Droppable, Draggable } from 'react-beautiful-dnd';
import { Label, Segment } from 'semantic-ui-react';
import 'semantic-ui-css/semantic.min.css';

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

//'reorder' reorders the list it is given, moving the draggable from the startIndex to the endIndex and returning an array
//it's a function- see the => below
// it takes three parameters- an array and two numbers
//the stuff after : defines the acceptable types of the parameters
const reorder = (
    list,
    startIndex,
    endIndex) => {
    const result = Array.from(list);
    const [removed] = result.splice(startIndex, 1);
    result.splice(endIndex, 0, removed);
    return result;
};

//move and reorder first moves the draggable from one list to the other
//then reorders the destination list
//and returns two arrays
//this is a function (see the => below!) with many parameters
//the stuff after : defines the acceptable types of the parameters
const moveAndReorder = (
    sourceList,
    sourceStartIndex,
    destinationList,
    destinationEndIndex,
    nextCounterId
) => {
    let sourceResult = Array.from(sourceList);
    //designate the draggable to be removed from sourceResult

    let draggedItem = sourceResult[sourceStartIndex];

    // make a copy, so we can give it a new ID
    let newDraggedItem = Object.assign({}, draggedItem);
    newDraggedItem.id = nextCounterId;
    console.log(`Id Passed: ${nextCounterId}; Old Id: ${draggedItem.id}; New id: ${newDraggedItem.id}`);

    //let [removed] = sourceResult.splice(sourceStartIndex, 1);
    //because we used splice, sourceresult no longer contains the element that was moved out of it

    const destinationResult = Array.from(destinationList);
    //add the draggable that we removed from the sourceList into the destinationResult
    destinationResult.splice(destinationEndIndex, 0, newDraggedItem);

    //return the two arrays
    //sourceResult should be the source droppable but without the draggable that got moved
    //destinationResult should be the destination droppable with the moved draggable added
    //in the correct position
    return [sourceResult, destinationResult]
};

const sessionEffectSelected = "EffectEdit-EffectSelected";

@inject("appStore")
@observer
class EffectsEditor extends Component {
    displayName = EffectsEditor.name

    @observable effectSelected = "";
    @observable effectIdSelected = 0;
    @observable effectInstructionSelectedId = 0;
    @observable metaDataMap = new Map();
    @observable ioPortsAvailable = new Map();
    @observable pinsAvailable = [];
    @observable pinOrdering = [];

    currentPinId = 1;

    componentDidMount = async () => {

        try {
            this.props.appStore.isBusySet(true);

            await this.getAllEffects();
            await this.props.appStore.effectsAvailableLoadAsync();

            let pinsAvailable = await this.props.appStore.ioPortsLoadAsync();

            let ioPortsAvailable = new Map();
            pinsAvailable.forEach(pin => {
                ioPortsAvailable.set(`${pin.deviceId}:${pin.commandPin}`, pin);
            });

            pinsAvailable = Enumerable.asEnumerable(pinsAvailable)
                .Where(x => x.commandPin !== -1)
                .Select(pin => ({ content: `${pin.deviceId}:${pin.commandPin} ${pin.description}`, id: this.currentPinId++, pinData: pin }))
                .OrderBy(x => x.content)
                .ToArray();

            runInAction(()=>{
                this.ioPortsAvailable = ioPortsAvailable;
                this.pinsAvailable = pinsAvailable;
            });

            console.log("Effect ID Selected: " + this.effectSelected.effectId);

            this.parseMetaData(this.effectSelected);

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    getAllEffects = async () => {
        try {

            let effects = await this.props.appStore.effectsLoadAsync();

            let effectSelected = Enumerable.AsEnumerable(effects).FirstOrDefault();

            let lastSelectedId = sessionStorage.getItem(sessionEffectSelected);
            if (lastSelectedId != null) {

                console.log(`${sessionEffectSelected}: ${Number(lastSelectedId)}`)

                let lastSelected = Enumerable.asEnumerable(effects)
                    .Where(d => d.effectId === Number(lastSelectedId))
                    .FirstOrDefault();

                if (lastSelected != null) {
                    effectSelected = lastSelected;
                }
            }

            let effectIdSelected = 0;
            if (effectSelected != null) {
                effectIdSelected = effectSelected.effectId;
            }

            runInAction(()=>{
                this.effectSelected = effectSelected;
                this.effectIdSelected = effectIdSelected;
                this.effectInstructionSelectedId = effectSelected.effectInstructionId
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        }
    }

    componentWillUnmount() {
        clearTimeout(this.timer);
    }

    handleEffectChange = async (evt) => {
        let effectId = evt.target.value;

        var effect = Enumerable.asEnumerable(this.props.appStore.effects)
            .Where(x => x.effectId === effectId)
            .FirstOrDefault();

        if (effect == null) return;

        console.log(`setting ${sessionEffectSelected}: ${effectId}`)
        sessionStorage.setItem(sessionEffectSelected, effectId);

        this.parseMetaData(effect);

        runInAction(()=>{
            this.effectSelected = effect;
            this.effectIdSelected = effect.effectId;
            this.effectInstructionSelectedId = effect.effectInstructionId;
        });
    }

    handleEffectDelete = async () => {
        try {
            this.props.appStore.isBusySet(true);

            if (this.effectSelected == null) return;

            await this.props.appStore.effectDelete(this.effectIdSelected);

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }

        // rebuild the list.
        this.getAllEffects();
    }

    handleEffectCreate = async () => {
        try {
            this.props.appStore.isBusySet(true);

            // Get the default Instruction to use
            let effectAvailable = Enumerable.asEnumerable(this.props.appStore.effectsAvailable).FirstOrDefault();
            if (effectAvailable == null) return;

            let effect = {
                effectName: "New Effect",
                effectInstructionId: effectAvailable.value,
                instructionMetaData: "DEVPINS=;DUR=500",
                duration: 5000,
                timeOn: "",
                timeOff: ""
            };

            effect = await this.props.appStore.effectCreate(effect);

            runInAction(()=>{
                this.effectSelected = effect;
                this.effectIdSelected = effect.effectId;
            });
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    parseMetaData(effectSelected) {

        if (effectSelected == null) {
            console.log("effect selected is null. cant select");
            runInAction(()=>{
                this.metaDataMap = new Map();
                this.pinOrdering = [];
            });
            return;
        }

        try {
            // Read the current Config lines
            let metaDataMap = new Map();
            let keyValues = effectSelected.instructionMetaData.split(';');
            keyValues.forEach(kv => {
                let kvArray = kv.split('=');
                if (kvArray.length === 2) {
                    metaDataMap.set(kvArray[0], kvArray[1]);
                }
            });

            console.log("KeyValues: " + keyValues.length);

            this.buildPinOrdering(metaDataMap);

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        }
    }

    // when the stat
    buildPinOrdering(metaDataMap) {
        try {
            // Read the current Config lines

            let ordering = metaDataMap.get("DEVPINS");

            let pins = ordering.split(',');

            let pinOrdering = [];

            pins.forEach(devPin => {

                let pin = this.ioPortsAvailable.get(devPin);
                if (pin == null) 
                {
                    console.log("Pin is null");
                    return;
                }
                
                let content = `${pin.deviceId}:${pin.commandPin} ${pin.description}`;
                console.log("content: " + content);
                pinOrdering.push({ content, id: this.currentPinId++, pinData: pin });
            });

            runInAction(()=>{
                this.pinOrdering = pinOrdering;
                this.metaDataMap = metaDataMap;
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        }
    }

    setPinOrderingInMap(pinOrdering) {
        try {
            let str = [];
            pinOrdering.forEach(pin => {
                str.push(`${pin.pinData.deviceId}:${pin.pinData.commandPin}`)
            });

            let devPins = str.join(',');

            console.log(`devpins=${devPins}`);

            runInAction(()=>{
                this.metaDataMap.set("DEVPINS", devPins);
            });

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        }

        this.setMetaData();
    }

    setMetaData() {
        try {
            //let effectSelected = Object.assign({}, this.effectSelected);

            let str = [];
            this.metaDataMap.forEach((value, key) => {
                str.push(`${key}=${value}`);
            })

            // set the effectInstructionSelected so the dropdown is selected
            // let effectInstructionSelected = Enumerable.AsEnumerable(this.state.effectInstructionsAvailable)
            //                                             .Where(x => x.value === effectSelected.effectInstructionId)
            //                                             .FirstOrDefault();

            runInAction(()=>{
                this.effectSelected.instructionMetaData = str.join(';');
                this.effectInstructionSelectedId = this.effectSelected.effectInstructionId;
                
            });

            this.handleEffectSave(this.effectSelected);

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        }
    }


    handlePatternNameChange = (effect, evt) => {
        runInAction(()=>{
            effect.effectName = evt.target.value;
        });
        this.handleEffectSave(effect);
    }

    handleEffectSave = async (effect) => {
        try {
            this.props.appStore.isBusySet(true);

            //var newEffect = Object.assign({}, effect);

            //this.setState({ effectSelected: newEffect });

            this.parseMetaData(effect);

            await EffectServices.saveEffect(effect);

        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    handleRemoveFromMap(id) {
        try {

            let pinOrdering = this.pinOrdering;

            pinOrdering = Enumerable.asEnumerable(pinOrdering)
                .Where(pin => pin.id !== id)
                .ToArray();

            console.log("post remove: " + id);
            console.log(pinOrdering);

            this.setPinOrderingInMap(pinOrdering);

            runInAction(()=>{
                this.pinOrdering = pinOrdering;
            });
        } catch (e) {
            this.props.appStore.errorMessageSet(e.message);
        } finally {
            this.props.appStore.isBusySet(false);
        }
    }

    //when Dragging ends, we look at the place the item was dropped -
    //outside a droppable, in the same droppable, or in another droppable
    onDragEnd = (result) => {

        // if it's dropped outside a droppable
        //result does not have a destination
        //we return null
        if (!result.destination) {
            return;
        }

        //if source.droppableId does not equal destination.droppableId,
        //that means the draggable item was dragged from one droppable into another droppable
        //then you need to remove the Draggable from the source.droppableId list
        //and add it into the correct position of the destination.droppableId list.

        //prepare to compare the source to the destination
        const source = result.source;
        const destination = result.destination;
        let sourceId = source.droppableId;
        let destinationId = destination.droppableId;

        console.log(`moving from ${sourceId} to ${destinationId}`);

        //just a short form of the two item arrays from state
        let items = this.pinsAvailable;
        let pinOrdering = this.pinOrdering;

        //If the place we moved the draggable out of is different from the place we moved it to, execute this
        if (sourceId !== destinationId) {
            console.log(`Hey, looks like source droppable (${sourceId}) is different from destination droppable (${destinationId})`)
            //we only have two lists- droppable1 and droppable2
            //so if the source is availablePins, then the destination is droppable2
            if (sourceId === "availablePins") {
                let sourceList = items;
                let destinationList = pinOrdering;
                //Note: source.index and destination.index are generated onDragEnd-
                //source.index is the index where the dragged item started out in the source droppable
                //destination.index is the index where the dragged item was placed by the user, in the destination droppable
                //after we pass the parameters to moveAndReorder, we will get back an array of two arrays
                //lists[0] will be the source droppable with the moved draggable taken out
                //lists[1] will be the target droppable with the moved draggable added in at the correct index
                let lists = moveAndReorder(
                    sourceList,
                    source.index,
                    destinationList,
                    destination.index,
                    this.currentPinId++
                );
                //so now we set the state to our two lists
                runInAction(()=>{
                    this.pinOrdering = lists[1];
                });
                
                this.setPinOrderingInMap(lists[1]);

            }

        } else { //If it was moved within the same list, then just reorder that list
            console.log(`Source is the same as destination`);
            console.log(`reordering ${sourceId}`);
            if (sourceId === "effectOrdering") {
                pinOrdering = reorder(
                    this.pinOrdering,
                    source.index,
                    destination.index);
                    runInAction(()=>{
                        this.pinOrdering = pinOrdering;
                    });
                this.setPinOrderingInMap(pinOrdering);
            }
        }
    }

    render() {

        const { classes } = this.props;

        return (
            <div style={{ display: "flex", flexDirection: "column" }}>
                <div style={{ display: "flex", flexDirection: "row" }}>
                    <form className={classes.root} autoComplete="off">

                        <FormControl className={classes.formControl}>
                            <InputLabel htmlFor="devices1">Effects</InputLabel>
                            <Select
                                value={this.effectIdSelected}
                                onChange={(evt) => this.handleEffectChange(evt)}
                                inputProps={{
                                    name: 'dev',
                                    id: 'devices1',
                                }}
                            >
                                {this.props.appStore.effects.map((effect, i) =>
                                    (
                                        <MenuItem value={effect.effectId} key={i}>{effect.effectName}</MenuItem>
                                    ))}
                            </Select>
                        </FormControl>
                    </form>

                    {this.effectSelected && (

                        <Tooltip title="Delete Effect">
                            <IconButton onClick={(evt) => this.handleEffectDelete()}><DeleteIcon /></IconButton>
                        </Tooltip>

                    )}


                    <Tooltip title="Create New Effect">
                        <IconButton onClick={(evt) => this.handleEffectCreate()}><AddIcon /></IconButton>
                    </Tooltip>


                </div>


                {this.effectSelected && (
                    <div>
                        <div style={{ display: "flex", flexDirection: "column" }}>
                            <div style={{ display: "flex", flexDirection: "row" }}>
                                <TextField
                                    label={"Effect Name"}
                                    value={this.effectSelected.effectName}
                                    onChange={(evt) => {
                                        runInAction(()=>{
                                            this.effectSelected.effectName = evt.target.value;
                                        });
                                        this.handleEffectSave(this.effectSelected);
                                    }}
                                    margin="normal"
                                />

                                <TextField
                                    label={"Duration"}
                                    style={{ width: "100px" }}
                                    value={this.effectSelected.duration}
                                    margin="normal"
                                    onChange={(evt) => {
                                        runInAction(()=>{
                                            this.effectSelected.duration = evt.target.value;
                                        });
                                        this.handleEffectSave(this.effectSelected);
                                    }}
                                />

                        
                                        <FormControl style={{ width: "200px" }} margin="normal">
                                            <InputLabel htmlFor="instructionsAvailable">Effects</InputLabel>
                                            <Select
                                                value={this.effectInstructionSelectedId}
                                                onChange={(evt) => {
                                                    let effectInstructionId = evt.target.value;
                                                    if (effectInstructionId === null) return;

                                                    let effect = this.effectSelected;

                                                    runInAction(()=>{
                                                        effect.effectInstructionId = effectInstructionId;
                                                        this.effectInstructionSelectedId = effectInstructionId;
                                                    });
                                                    this.handleEffectSave(effect);
                                                }
                                                }
                                                inputProps={{
                                                    name: 'dev',
                                                    id: 'instructionsAvailable',
                                                }}
                                            >
                                                {this.props.appStore.effectsAvailable.map((effect, i) =>
                                                    (
                                                        <MenuItem value={effect.value} key={i}>{effect.label}</MenuItem>
                                                    ))}
                                            </Select>
                                        </FormControl>
                             
                            </div>
                            <div>

                                <TextField
                                    label={"Effect Configuration"}
                                    style={{ width: "100%" }}
                                    value={this.effectSelected.instructionMetaData}
                                    margin="normal"
                                    onChange={(evt) => {
                                        let effect = this.effectSelected;
                                        runInAction(()=>{
                                            effect.instructionMetaData = evt.target.value;
                                        });
                                        this.handleEffectSave(effect);
                                    }}
                                />

                            </div>
                        </div>
                        <DragDropContext onDragEnd={this.onDragEnd}>

                            <div style={{ display: "flex", flexDirection: "row", margin: 0 }}>

                                <Droppable droppableId="availablePins">
                                    {(provided, snapshot) => (
                                        <Segment color={snapshot.isDraggingOver ? 'blue' : 'yellow'}
                                            inverted={snapshot.isDraggingOver}
                                            tertiary={snapshot.isDraggingOver}
                                            style={{ margin: 0 }}
                                        >
                                            <div
                                                ref={provided.innerRef}
                                            > Source Pins:
                                        {this.pinsAvailable.map((item, index) => (
                                                    <Draggable key={item.id} draggableId={item.id} index={index}>
                                                        {(provided, snapshot) => (
                                                            <div style={{ margin: '1px' }}>
                                                                <div
                                                                    ref={provided.innerRef}
                                                                    {...provided.draggableProps}
                                                                    {...provided.dragHandleProps}

                                                                >
                                                                    <Label size='large'
                                                                        color={snapshot.isDragging ? 'green' : (item.pinData.isDanger ? 'red' : 'yellow')}
                                                                        content={item.content} />
                                                                </div>
                                                                {provided.placeholder}
                                                            </div>
                                                        )}
                                                    </Draggable>
                                                ))}
                                                {provided.placeholder}
                                            </div>
                                        </Segment>
                                    )}
                                </Droppable>
                                <Droppable droppableId="effectOrdering">
                                    {(provided, snapshot) => (
                                        <Segment color={snapshot.isDraggingOver ? 'blue' : 'yellow'}
                                            inverted={snapshot.isDraggingOver}
                                            tertiary={snapshot.isDraggingOver}
                                            style={{ margin: 0 }}
                                        >
                                            <div
                                                ref={provided.innerRef}
                                            > Execution Order:
                                        {this.pinOrdering.map((item, index) => (
                                                    <Draggable key={item.id} draggableId={item.id} index={index}>
                                                        {(provided, snapshot) => (
                                                            <div style={{ margin: '1px' }}>
                                                                <div style={{ display: "flex", flexDirection: "row" }}
                                                                    ref={provided.innerRef}
                                                                    {...provided.draggableProps}
                                                                    {...provided.dragHandleProps}

                                                                >
                                                                    <Label size='large'
                                                                        color={snapshot.isDragging ? 'green' : (item.pinData.isDanger ? 'red' : 'yellow')}
                                                                        content={item.content} />

                                                                    <IconButton style={{ height: 25, width: 25 }} onClick={(evt) => this.handleRemoveFromMap(item.id)}><DeleteIcon /></IconButton>
                                                                </div>
                                                                {provided.placeholder}


                                                            </div>
                                                        )}
                                                    </Draggable>
                                                ))}
                                                {provided.placeholder}
                                            </div>
                                        </Segment>
                                    )}
                                </Droppable>

                            </div>
                        </DragDropContext>

                    </div>

                )
                }
            </div >
        );
    }
}

export default withStyles(styles)(EffectsEditor);
