import React, { Component } from 'react';

import EffectServices from '../Services/EffectServices';
import EffectsAvailableServices from '../Services/EffectsAvailableServices';
import DeviceIoPortServices from '../Services/DeviceIoPortServices';

import BusyContent from './controls/BusyContent';
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
import ErrorContent from './controls/ErrorContent';
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
        marginTop: theme.spacing.unit * 2,
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

class EffectsEditor extends Component {
    displayName = EffectsEditor.name

    constructor(props) {
        super(props)

        this.EffectServices = EffectServices;
        this.EffectsAvailableServices = EffectsAvailableServices;
        this.DeviceIoPortServices = DeviceIoPortServices;

        this.state = {
            effects: [],
            effectSelected: "",
            effectIdSelected: 0,
            effectInstructionsAvailable: [],
            effectInstructionSelectedId: 0,
            errorMessage: null,
            pinsAvailable: [],
            pinOrdering: [],
            metaDataMap: new Map(),
            ioPortsAvailable: new Map(),
        };
    }

    currentPinId = 1;

    componentDidMount = async () => {

        try {
            this.setIsBusy(true);

            await this.getAllEffects();

            let effectsAvailable = await this.EffectsAvailableServices.getAllAvailableEffects();

            effectsAvailable = effectsAvailable.map((item) => ({ label: `${item.displayName}`, value: item.effectInstructionId }));

            let pinsAvailable = await this.DeviceIoPortServices.ioPortGetAll();

            let ioPortsAvailable = new Map();
            pinsAvailable.forEach(pin => {
                ioPortsAvailable.set(`${pin.deviceId}:${pin.commandPin}`, pin);
            });

            pinsAvailable = Enumerable.asEnumerable(pinsAvailable)
                .Where(x => x.commandPin !== -1)
                .Select(pin => ({ content: `${pin.deviceId}:${pin.commandPin} ${pin.description}`, id: this.currentPinId++, pinData: pin }))
                .OrderBy(x => x.content)
                .ToArray();

            this.setState({
                effectInstructionsAvailable: effectsAvailable,
                pinsAvailable,
                ioPortsAvailable
            });

            this.parseMetaData(this.state.effectSelected);

        } catch (e) {
            this.setState({ errorMessage: e.message })
        } finally {
            this.setIsBusy(false);
        }
    }

    getAllEffects = async () => {
        try {
            this.setIsBusy(true);

            let effects = await this.EffectServices.getAllEffects();

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

            this.setState({
                effects,
                effectSelected,
                effectIdSelected,
                effectInstructionSelectedId: effectSelected.effectInstructionId,
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

    handleEffectChange = async (evt) => {
        let effectId = evt.target.value;

        var effect = Enumerable.asEnumerable(this.state.effects)
            .Where(x => x.effectId === effectId)
            .FirstOrDefault();

        if (effect == null) return;

        console.log(`setting ${sessionEffectSelected}: ${effectId}`)
        sessionStorage.setItem(sessionEffectSelected, effectId);

        this.parseMetaData(effect);

        this.setState({
            effectSelected: effect,
            effectIdSelected: effect.effectId,
            effectInstructionSelectedId: effect.effectInstructionId,
        });
    }

    handleEffectDelete = async () => {
        try {
            this.setIsBusy(true);

            if (this.state.effectSelected == null) return;

            await this.EffectServices.deleteEffect(this.state.effectIdSelected);

        } catch (e) {
            this.setState({ errorMessage: e.message })
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
            let effectAvailable = Enumerable.asEnumerable(this.state.effectInstructionsAvailable).FirstOrDefault();
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
            this.setState({ errorMessage: e.message })
        } finally {
            this.setIsBusy(false);
        }
    }

    parseMetaData(effectSelected) {

        if (effectSelected == null) {
            this.setState({
                metaDataMap: new Map(),
                pinOrdering: []
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

            this.buildPinOrdering(metaDataMap);

        } catch (e) {
            this.setState({ errorMessage: e.message })
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

                let pin = this.state.ioPortsAvailable.get(devPin);
                if (pin == null) return;

                pinOrdering.push({ content: `${pin.deviceId}:${pin.commandPin} ${pin.description}`, id: this.currentPinId++, pinData: pin });
            });


            this.setState({ pinOrdering, metaDataMap });

        } catch (e) {
            this.setState({ errorMessage: e.message })
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

            this.state.metaDataMap.set("DEVPINS", devPins);

        } catch (e) {
            this.setState({ errorMessage: e.message })
        }

        this.setMetaData();
    }

    setMetaData() {
        try {
            let effectSelected = Object.assign({}, this.state.effectSelected);

            let str = [];
            this.state.metaDataMap.forEach((value, key) => {
                str.push(`${key}=${value}`);
            })

            effectSelected.instructionMetaData = str.join(';');

            // set the effectInstructionSelected so the dropdown is selected
            // let effectInstructionSelected = Enumerable.AsEnumerable(this.state.effectInstructionsAvailable)
            //                                             .Where(x => x.value === effectSelected.effectInstructionId)
            //                                             .FirstOrDefault();

            this.setState({
                effectSelected,
                effectInstructionSelectedId: effectSelected.effectInstructionId
            });

            this.handleEffectSave(effectSelected);

        } catch (e) {
            this.setState({ errorMessage: e.message })
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

            var newEffect = Object.assign({}, effect);

            this.setState({ effectSelected: newEffect });

            this.parseMetaData(newEffect);

            await this.EffectServices.saveEffect(newEffect);
        } catch (e) {
            this.setState({ errorMessage: e.message })
        } finally {
            this.setIsBusy(false);
        }
    }

    handleRemoveFromMap(id) {
        try {

            let pinOrdering = this.state.pinOrdering;

            pinOrdering = Enumerable.asEnumerable(pinOrdering)
                .Where(pin => pin.id !== id)
                .ToArray();

            console.log("post remove: ");
            console.log(pinOrdering);

            this.setPinOrderingInMap(pinOrdering);

            this.setState({ pinOrdering });

        } catch (e) {
            this.setState({ errorMessage: e.message })
        } finally {
            this.setIsBusy(false);
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
        let items = this.state.pinsAvailable;
        let pinOrdering = this.state.pinOrdering;

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
                this.setState({ pinOrdering: lists[1] });
                this.setPinOrderingInMap(lists[1]);

            }

        } else { //If it was moved within the same list, then just reorder that list
            console.log(`Source is the same as destination`);
            console.log(`reordering ${sourceId}`);
            if (sourceId === "effectOrdering") {
                pinOrdering = reorder(
                    this.state.pinOrdering,
                    source.index,
                    destination.index);
                this.setState({ pinOrdering });
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

                        
                                        <FormControl style={{ width: "200px" }} margin="normal">
                                            <InputLabel htmlFor="instructionsAvailable">Effects</InputLabel>
                                            <Select
                                                value={this.state.effectInstructionSelectedId}
                                                onChange={(evt) => {
                                                    let effectInstructionId = evt.target.value;
                                                    if (effectInstructionId === null) return;

                                                    let effect = this.state.effectSelected;
                                                    effect.effectInstructionId = effectInstructionId;
                                                    this.setState({ effectInstructionSelectedId: effectInstructionId })
                                                    this.handleEffectSave(effect);
                                                }
                                                }
                                                inputProps={{
                                                    name: 'dev',
                                                    id: 'instructionsAvailable',
                                                }}
                                            >
                                                {this.state.effectInstructionsAvailable.map((effect, i) =>
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
                                        {this.state.pinsAvailable.map((item, index) => (
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
                                        {this.state.pinOrdering.map((item, index) => (
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

                {
                    this.state.isBusy && (<BusyContent />)
                }
                <ErrorContent errorMessage={this.state.errorMessage} errorClear={() => { this.setState({ errorMessage: null }) }} />
            </div >
        );
    }
}

export default withStyles(styles)(EffectsEditor);
