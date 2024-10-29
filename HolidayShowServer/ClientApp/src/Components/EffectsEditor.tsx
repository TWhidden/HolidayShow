// src/components/EffectsEditor.tsx

import React, { useEffect, useState, useRef, ChangeEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import {
    Select,
    InputLabel,
    MenuItem,
    FormControl,
    TextField,
    IconButton,
    Tooltip,
    Typography,
    Box,
    Checkbox,
    FormControlLabel,
    Grid,
    SelectChangeEvent,
} from '@mui/material';
import {
    Delete as DeleteIcon,
    Add as AddIcon,
} from '@mui/icons-material';

import { Label } from 'semantic-ui-react';
import 'semantic-ui-css/semantic.min.css';
import ComboSelect from 'react-select';
import { DeviceEffects, DeviceIoPorts } from '../Clients/Api';

// Import @dnd-kit components
import {
    DndContext,
    closestCenter,
    PointerSensor,
    useSensor,
    useSensors,
    DragEndEvent,
} from '@dnd-kit/core';
import {
    arrayMove,
    SortableContext,
    verticalListSortingStrategy,
    useSortable,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { DragOverlay } from '@dnd-kit/core'; // Import DragOverlay

// Define TypeScript interfaces based on your API
interface PinOrdering {
    content: string;
    id: string;
    pinData: DeviceIoPorts;
}

// Define the structure for dynamic instruction fields
interface InstructionField {
    key: string;
    label: string;
    type: 'multi-select' | 'number' | 'boolean';
    options?: { label: string; value: string | number }[];
}

// Define sortable item component
const SortableItem: React.FC<{
    id: string;
    content: string;
    isDanger: boolean;
    onRemove?: () => void;
}> = ({ id, content, isDanger, onRemove }) => {
    const {
        attributes,
        listeners,
        setNodeRef,
        transform,
        transition,
        isDragging,
    } = useSortable({ id });

    const style: React.CSSProperties = {
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.5 : 1,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        margin: '4px 0',
        cursor: 'grab',
        // Remove any overflow-related styles to prevent clipping
    };

    return (
        <div ref={setNodeRef} style={style} {...attributes} {...listeners}>
            <Label
                size="large"
                color={isDanger ? 'red' : 'yellow'}
                content={content}
                style={{ flex: 1 }}
            />
            {onRemove && (
                <IconButton size="small" onClick={onRemove}>
                    <DeleteIcon fontSize="small" />
                </IconButton>
            )}
        </div>
    );
};

const EffectsEditor: React.FC = observer(() => {
    const store = AppStoreContextItem.useStore();

    const [effectSelected, setEffectSelected] = useState<DeviceEffects | null>(null);
    const [effectIdSelected, setEffectIdSelected] = useState<number>(0);
    const [effectInstructionSelectedId, setEffectInstructionSelectedId] = useState<number>(0);
    const [metaDataMap, setMetaDataMap] = useState<Map<string, string>>(new Map());
    const [ioPortsAvailable, setIoPortsAvailable] = useState<Map<string, DeviceIoPorts>>(new Map());
    const [pinsAvailable, setPinsAvailable] = useState<Array<{ content: string; id: string; pinData: DeviceIoPorts }>>([]);
    const [pinOrdering, setPinOrdering] = useState<Array<PinOrdering>>([]);

    const currentPinId = useRef<number>(1);

    // Track the currently dragged item for DragOverlay
    const [activeId, setActiveId] = useState<string | null>(null);

    // Define instruction fields based on EffectInstructionId
    const instructionFieldsMap: Record<number, InstructionField[]> = {
        1: [
            { key: 'DEVPINS', label: 'Device Pins', type: 'multi-select' },
            { key: 'DUR', label: 'Duration (ms)', type: 'number' },
        ],
        2: [
            { key: 'DEVPINS', label: 'Device Pins', type: 'multi-select' },
            { key: 'DUR', label: 'Duration (ms)', type: 'number' },
        ],
        3: [
            { key: 'DEVPINS', label: 'Device Pins', type: 'multi-select' },
        ],
        4: [
            { key: 'DEVPINS', label: 'Device Pins', type: 'multi-select' },
            { key: 'DUR', label: 'Duration (ms)', type: 'number' },
            { key: 'DELAYBETWEEN', label: 'Delay Between (ms)', type: 'number' },
            { key: 'EXECUTEFOR', label: 'Execute For (ms)', type: 'number' },
        ],
        5: [
            { key: 'DEVPINS', label: 'Device Pins', type: 'multi-select' },
            { key: 'DUR', label: 'Duration (ms)', type: 'number' },
            { key: 'REVERSE', label: 'Reverse Execution', type: 'boolean' },
        ],
        6: [
            { key: 'DEVPINS', label: 'Device Pins', type: 'multi-select' },
            { key: 'DUR', label: 'Duration (ms)', type: 'number' },
            { key: 'DELAYBETWEEN', label: 'Delay Between (ms)', type: 'number' },
            { key: 'EXECUTEFOR', label: 'Execute For (ms)', type: 'number' },
        ],
    };

    // Initialize data on component mount
    useEffect(() => {
        const initialize = async () => {
            try {
                // Fetch all effects
                await getAllEffects();

                // Create a map for IO ports
                const ioPortsMap = new Map<string, DeviceIoPorts>();
                store.deviceIoPorts.forEach((pin) => {
                    ioPortsMap.set(`${pin.deviceId}:${pin.commandPin}`, pin);
                });

                // Filter out pins with commandPin === -1 and map them
                const filteredPins = store.deviceIoPorts
                    .filter((pin) => pin.commandPin !== -1)
                    .map((pin) => ({
                        content: `${pin.deviceId}:${pin.commandPin} ${pin.description}`,
                        id: `${currentPinId.current++}`, // Unique string ID
                        pinData: pin,
                    }))
                    .sort((a, b) => a.content.localeCompare(b.content));

                setIoPortsAvailable(ioPortsMap);
                setPinsAvailable(filteredPins);
            } catch (error: any) {
                store.setError(error.message);
            }
        };

        initialize();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, []);

    // Fetch all effects and set the selected effect
    const getAllEffects = async () => {
        try {
            const effects = store.deviceEffects;
            let selectedEffect: DeviceEffects | undefined = effects[0];

            const lastSelectedId = sessionStorage.getItem('selectedEffectId'); // Use a fixed key
            if (lastSelectedId) {
                const id = Number(lastSelectedId);
                const foundEffect = effects.find((effect) => effect.effectId === id);
                if (foundEffect) {
                    selectedEffect = foundEffect;
                }
            }

            if (selectedEffect) {
                setEffectSelected(selectedEffect);
                setEffectIdSelected(selectedEffect.effectId);
                setEffectInstructionSelectedId(selectedEffect.effectInstructionId);
                sessionStorage.setItem('selectedEffectId', selectedEffect.effectId.toString());

                parseMetaData(selectedEffect);
            }
        } catch (error: any) {
            store.setError(`Failed to fetch effects: ${error.message}`);
        }
    };

    // Handle effect selection change
    const handleEffectChange = (event: SelectChangeEvent<number>) => {
        const effectId = Number(event.target.value);
        const selected = store.deviceEffects.find((effect) => effect.effectId === effectId);
        if (!selected) return;

        console.log(`Setting selectedEffectId: ${effectId}`);
        sessionStorage.setItem('selectedEffectId', effectId.toString());

        setEffectSelected(selected);
        setEffectIdSelected(selected.effectId);
        setEffectInstructionSelectedId(selected.effectInstructionId);

        parseMetaData(selected);
    };

    // Handle effect deletion
    const handleEffectDelete = async () => {
        if (!effectSelected) return;

        try {
            await store.deleteDeviceEffect(effectIdSelected);
            // Refresh effects list
            await getAllEffects();
        } catch (error: any) {
            store.setError(`Failed to delete effect: ${error.message}`);
        }
    };

    // Handle effect creation
    const handleEffectCreate = async () => {
        try {
            const effectAvailable = store.effectInstructionsAvailables[0];
            if (!effectAvailable) return;

            const newEffect: DeviceEffects = {
                effectName: 'New Effect',
                effectInstructionId: effectAvailable.effectInstructionId,
                instructionMetaData: 'DEVPINS=;DUR=500',
                effectId: 0,
                duration: 5000,
                timeOn: '',
                timeOff: '',
            };

            const createdEffect = await store.createDeviceEffect(newEffect);

            if (createdEffect) {
                setEffectSelected(createdEffect);
                setEffectIdSelected(createdEffect.effectId);
                setEffectInstructionSelectedId(createdEffect.effectInstructionId);
                sessionStorage.setItem('selectedEffectId', createdEffect.effectId.toString());

                parseMetaData(createdEffect);
            }
        } catch (error: any) {
            store.setError(`Failed to create effect: ${error.message}`);
        }
    };

    // Parse instructionMetaData into metaDataMap and build pin ordering
    const parseMetaData = (effect: DeviceEffects | null) => {
        if (!effect) {
            setMetaDataMap(new Map());
            setPinOrdering([]);
            return;
        }

        try {
            const map = new Map<string, string>();
            const keyValues = effect.instructionMetaData?.split(';') ?? [];
            keyValues.forEach((kv) => {
                const [key, value] = kv.split('=');
                if (key && value !== undefined) { // Allow empty string values
                    map.set(key, value);
                }
            });

            setMetaDataMap(map);
            buildPinOrdering(map);
        } catch (error: any) {
            store.setError(`Failed to parse metadata: ${error.message}`);
        }
    };

    // Build pin ordering based on metaDataMap
    const buildPinOrdering = (map: Map<string, string>) => {
        const ordering = map.get('DEVPINS');
        if (!ordering) {
            setPinOrdering([]);
            return;
        }

        const pins = ordering.split(',').filter((devPin) => devPin.trim() !== '');

        const newPinOrdering: PinOrdering[] = [];

        pins.forEach((devPin) => {
            const pin = ioPortsAvailable.get(devPin);
            if (pin) {
                newPinOrdering.push({
                    content: `${pin.deviceId}:${pin.commandPin} ${pin.description}`,
                    id: `${currentPinId.current++}`,
                    pinData: pin,
                });
            }
        });

        setPinOrdering(newPinOrdering);
    };

    // Update metaDataMap with new pin ordering and save metadata
    const setPinOrderingInMap = (newOrdering: PinOrdering[]) => {
        try {
            const devPins = newOrdering.map((pin) => `${pin.pinData.deviceId}:${pin.pinData.commandPin}`).join(',');
            const newMap = new Map(metaDataMap);
            newMap.set('DEVPINS', devPins);
            setMetaDataMap(newMap);
            setMetaData(newMap);
        } catch (error: any) {
            store.setError(`Failed to set pin ordering: ${error.message}`);
        }
    };

    // Update instructionMetaData and save effect
    const setMetaData = (map: Map<string, string>) => {
        try {
            const metaDataArray: string[] = [];
            map.forEach((value, key) => {
                metaDataArray.push(`${key}=${value}`);
            });
            const updatedMetaData = metaDataArray.join(';');

            if (effectSelected) {
                const updatedEffect: DeviceEffects = {
                    ...effectSelected,
                    instructionMetaData: updatedMetaData,
                };
                setEffectSelected(updatedEffect);
                handleEffectSave(updatedEffect);
            }
        } catch (error: any) {
            store.setError(`Failed to set metadata: ${error.message}`);
        }
    };

    // Handle effect name change
    const handleEffectNameChange = async (event: ChangeEvent<HTMLInputElement>) => {
        if (!effectSelected) return;

        const newName = event.target.value;
        const updatedEffect: DeviceEffects = { ...effectSelected, effectName: newName };
        setEffectSelected(updatedEffect);
        handleEffectSave(updatedEffect);
    };

    // Handle effect save
    const handleEffectSave = async (effect: DeviceEffects) => {
        try {
            await store.updateDeviceEffect(effect.effectId, effect);
        } catch (error: any) {
            store.setError(`Failed to save effect: ${error.message}`);
        }
    };

    // Handle removing a pin from pinOrdering
    const handleRemoveFromMap = (id: string) => {
        const updatedOrdering = pinOrdering.filter((pin) => pin.id !== id);
        setPinOrdering(updatedOrdering);
        setPinOrderingInMap(updatedOrdering);
    };

    // Define instruction fields based on EffectInstructionId
    const getInstructionFields = (instructionId: number): InstructionField[] => {
        return instructionFieldsMap[instructionId] || [];
    };

    // Handle changes in dynamic instruction fields
    const handleInstructionFieldChange = (
        key: string,
        value: string | number | boolean
    ) => {
        const updatedMap = new Map(metaDataMap);
        if (typeof value === 'boolean') {
            updatedMap.set(key, value ? '1' : '0');
        } else {
            updatedMap.set(key, value.toString());
        }
        setMetaDataMap(updatedMap);
        setMetaData(updatedMap);
    };

    // Setup @dnd-kit sensors
    const sensors = useSensors(
        useSensor(PointerSensor),
        // You can add more sensors if needed, e.g., KeyboardSensor for accessibility
    );

    // Handle drag start to set activeId for DragOverlay
    const handleDragStart = (event: any) => {
        setActiveId(event.active.id);
    };

    // Handle drag end event using @dnd-kit
    const handleDragEnd = (event: DragEndEvent) => {
        const { active, over } = event;

        setActiveId(null); // Reset activeId

        if (!over) return;

        const activeId = active.id;
        const overId = over.id;

        if (activeId === overId) return;

        // Determine if activeId is in pinsAvailable
        const isActiveInAvailablePins = pinsAvailable.some(pin => pin.id === activeId);
        const isOverInExecutionOrder = pinOrdering.some(pin => pin.id === overId);

        if (isActiveInAvailablePins && isOverInExecutionOrder) {
            // Move from availablePins to executionOrder
            const pin = pinsAvailable.find(pin => pin.id === activeId);
            if (!pin) return;

            const newExecutionOrder = [...pinOrdering, {
                content: pin.content,
                id: `${currentPinId.current++}`,
                pinData: pin.pinData,
            }];

            setPinOrdering(newExecutionOrder);
            setPinOrderingInMap(newExecutionOrder);

            // Remove from availablePins
            setPinsAvailable(pinsAvailable.filter(p => p.id !== activeId));
        } else if (!isActiveInAvailablePins && isOverInExecutionOrder) {
            // Reorder within executionOrder
            const oldIndex = pinOrdering.findIndex(pin => pin.id === activeId);
            const newIndex = pinOrdering.findIndex(pin => pin.id === overId);

            if (oldIndex === -1 || newIndex === -1) return;

            const reordered = arrayMove(pinOrdering, oldIndex, newIndex);
            setPinOrdering(reordered);
            setPinOrderingInMap(reordered);
        }
        // Additional conditions can be handled here if needed
    };

    return (
        <div
            style={{
                display: 'flex',
                flexDirection: 'column',
                padding: '1rem',
                alignItems: 'flex-start', // Ensures top alignment
                width: '100%', // Ensures the component takes full width
            }}
        >
            {store.error && (
                <div style={{ color: 'red', marginBottom: '1rem' }}>{store.error}</div>
            )}

            {/* Effect Selection and Controls */}
            <Box
                display="flex"
                flexDirection="row"
                marginBottom="1rem"
                alignItems="center"
                width="100%"
            >
                <FormControl style={{ minWidth: 200, marginRight: '1rem' }}>
                    <InputLabel id="effects-label">Effects</InputLabel>
                    <Select
                        labelId="effects-label"
                        id="effects-select"
                        value={effectIdSelected}
                        onChange={handleEffectChange}
                        label="Effects"
                        fullWidth
                    >
                        {store.deviceEffects.map((effect) => (
                            <MenuItem key={effect.effectId} value={effect.effectId}>
                                {effect.effectName}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                {effectSelected && (
                    <>
                        <Tooltip title="Delete Effect">
                            <IconButton onClick={handleEffectDelete}>
                                <DeleteIcon />
                            </IconButton>
                        </Tooltip>

                        <Tooltip title="Create New Effect">
                            <IconButton onClick={handleEffectCreate}>
                                <AddIcon />
                            </IconButton>
                        </Tooltip>
                    </>
                )}
            </Box>

            {/* Effect Details and Configuration */}
            {effectSelected && (
                <div style={{ width: '100%' }}>
                    {/* Effect Details */}
                    <Box display="flex" flexDirection="column" marginBottom="1rem" width="100%">
                        <Box
                            display="flex"
                            flexDirection="row"
                            alignItems="center"
                            marginBottom="1rem"
                            flexWrap="wrap"
                        >
                            <TextField
                                label="Effect Name"
                                value={effectSelected.effectName}
                                onChange={handleEffectNameChange}
                                margin="normal"
                                style={{ marginRight: '1rem', flex: 1, minWidth: '200px' }}
                            />

                            <TextField
                                label="Duration (ms)"
                                value={effectSelected.duration}
                                onChange={async (event: ChangeEvent<HTMLInputElement>) => {
                                    if (!effectSelected) return;
                                    const newDuration = Number(event.target.value);
                                    const updatedEffect: DeviceEffects = { ...effectSelected, duration: newDuration };
                                    setEffectSelected(updatedEffect);
                                    handleEffectSave(updatedEffect);
                                }}
                                margin="normal"
                                style={{ width: '150px', marginRight: '1rem' }}
                                type="number"
                            />

                            <FormControl style={{ minWidth: 200, marginRight: '1rem' }} margin="normal">
                                <InputLabel id="instructionsAvailable-label">Effect Instructions</InputLabel>
                                <Select
                                    labelId="instructionsAvailable-label"
                                    id="instructionsAvailable-select"
                                    value={effectInstructionSelectedId}
                                    onChange={(event: SelectChangeEvent<number>) => {
                                        const effectInstructionId = Number(event.target.value);
                                        if (!effectSelected) return;

                                        const updatedEffect: DeviceEffects = { ...effectSelected, effectInstructionId };
                                        setEffectSelected(updatedEffect);
                                        setEffectInstructionSelectedId(effectInstructionId);
                                        handleEffectSave(updatedEffect);
                                    }}
                                    label="Effect Instructions"
                                    fullWidth
                                >
                                    {store.effectInstructionsAvailables.map((instruction) => (
                                        <MenuItem key={instruction.effectInstructionId} value={instruction.effectInstructionId}>
                                            {instruction.displayName}
                                        </MenuItem>
                                    ))}
                                </Select>
                            </FormControl>
                        </Box>

                        {/* Dynamic Instruction Fields */}
                        <Grid
                            container
                            spacing={2}
                            marginBottom="1rem"
                        // Adjusted to wrap as needed
                        >
                            {getInstructionFields(effectInstructionSelectedId).map((field) => {
                                switch (field.type) {
                                    case 'multi-select':
                                        // Prepare options for react-select
                                        const availablePinsOptions = Array.from(ioPortsAvailable.values()).map((pin) => ({
                                            label: `${pin.deviceId}:${pin.commandPin} ${pin.description}`,
                                            value: `${pin.deviceId}:${pin.commandPin}`,
                                        }));
                                        const selectedPins = metaDataMap.get('DEVPINS')?.split(',').map((pin) => ({
                                            label: `${pin} ${ioPortsAvailable.get(pin)?.description || ''}`,
                                            value: pin,
                                        })) || [];

                                        return (
                                            <Grid
                                                item
                                                xs={12} // Spans the whole grid
                                                key={field.key}
                                            >
                                                <Typography variant="subtitle1" gutterBottom>
                                                    {field.label}
                                                </Typography>
                                                <ComboSelect
                                                    isMulti
                                                    options={availablePinsOptions}
                                                    value={selectedPins}
                                                    onChange={(selectedOptions) => {
                                                        if (!selectedOptions) {
                                                            handleInstructionFieldChange(field.key, '');
                                                            return;
                                                        }
                                                        const selectedValues = selectedOptions.map((option) => option.value);
                                                        handleInstructionFieldChange(field.key, selectedValues.join(','));
                                                    }}
                                                    placeholder={`Select ${field.label}`}
                                                />
                                            </Grid>
                                        );
                                    case 'number':
                                        return (
                                            <Grid
                                                item
                                                xs={12} sm={6} md={4} // Adjusted for horizontal layout
                                                key={field.key}
                                            >
                                                <TextField
                                                    label={field.label}
                                                    type="number"
                                                    value={metaDataMap.get(field.key) || ''}
                                                    onChange={(e: ChangeEvent<HTMLInputElement>) => {
                                                        handleInstructionFieldChange(field.key, e.target.value);
                                                    }}
                                                    margin="normal"
                                                    fullWidth
                                                />
                                            </Grid>
                                        );
                                    case 'boolean':
                                        return (
                                            <Grid
                                                item
                                                xs={12} sm={6} md={4} // Adjusted for horizontal layout
                                                key={field.key}
                                            >
                                                <FormControlLabel
                                                    control={
                                                        <Checkbox
                                                            checked={metaDataMap.get(field.key) === '1'}
                                                            onChange={(e) => handleInstructionFieldChange(field.key, e.target.checked)}
                                                            name={field.key}
                                                            color="primary"
                                                        />
                                                    }
                                                    label={field.label}
                                                    style={{ marginTop: '1.5rem' }} // Adjusted for alignment
                                                />
                                            </Grid>
                                        );
                                    default:
                                        return null;
                                }
                            })}
                        </Grid>

                        {/* Drag and Drop Configuration */}
                        <DndContext
                            sensors={sensors}
                            collisionDetection={closestCenter}
                            onDragStart={handleDragStart} // Handle drag start for DragOverlay
                            onDragEnd={handleDragEnd}
                        >
                            <SortableContext
                                items={[...pinsAvailable.map(pin => pin.id), ...pinOrdering.map(pin => pin.id)]}
                                strategy={verticalListSortingStrategy}
                            >
                                <Box display="flex" flexDirection={{ xs: 'column', sm: 'row' }} gap={2} width="100%">
                                    {/* Source Pins */}
                                    <Box
                                        style={{
                                            flex: 1,
                                            minHeight: '200px',
                                            maxHeight: '400px',
                                            overflowY: 'auto',
                                            padding: '1rem',
                                            border: '1px solid #ddd',
                                            borderRadius: '4px',
                                        }}
                                    >
                                        <Typography variant="h6" gutterBottom>
                                            Source Pins:
                                        </Typography>
                                        {pinsAvailable.map((item) => (
                                            <SortableItem
                                                key={item.id}
                                                id={item.id}
                                                content={item.content}
                                                isDanger={item.pinData.isDanger}
                                            />
                                        ))}
                                    </Box>

                                    {/* Execution Order Pins */}
                                    <Box
                                        style={{
                                            flex: 1,
                                            minHeight: '200px',
                                            maxHeight: '400px',
                                            overflowY: 'auto',
                                            padding: '1rem',
                                            border: '1px solid #ddd',
                                            borderRadius: '4px',
                                        }}
                                    >
                                        <Typography variant="h6" gutterBottom>
                                            Execution Order:
                                        </Typography>
                                        {pinOrdering.map((item) => (
                                            <SortableItem
                                                key={item.id}
                                                id={item.id}
                                                content={item.content}
                                                isDanger={item.pinData.isDanger}
                                                onRemove={() => handleRemoveFromMap(item.id)}
                                            />
                                        ))}
                                    </Box>
                                </Box>
                            </SortableContext>

                            {/* Drag Overlay to prevent clipping */}
                            <DragOverlay>
                                {activeId ? (
                                    <div
                                        style={{
                                            padding: '4px 8px',
                                            backgroundColor: '#fff',
                                            border: '1px solid #ccc',
                                            borderRadius: '4px',
                                            boxShadow: '0 2px 8px rgba(0,0,0,0.2)',
                                        }}
                                    >
                                        {pinsAvailable.find(pin => pin.id === activeId)?.content ||
                                            pinOrdering.find(pin => pin.id === activeId)?.content ||
                                            ''}
                                    </div>
                                ) : null}
                            </DragOverlay>


                            {/* Effects Configuration - Single Line */}
                            <Box mt={2} width="100%">
                                <Typography variant="subtitle1" gutterBottom>
                                    Effects Configuration:
                                </Typography>
                                <TextField
                                    value={effectSelected.instructionMetaData}
                                    margin="normal"
                                    style={{ width: '100%' }}
                                    // Remove multiline and set single line
                                    // multiline
                                    // rows={2}
                                    InputProps={{
                                        readOnly: true,
                                    }}
                                    variant="outlined"
                                />
                            </Box>

                        </DndContext>


                    </Box>
                </div>
            )}
        </div>
    );

});

export default EffectsEditor;
