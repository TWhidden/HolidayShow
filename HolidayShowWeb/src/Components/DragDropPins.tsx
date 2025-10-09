import React, { useState, useEffect } from 'react';
import {
    DndContext,
    closestCenter,
    PointerSensor,
    useSensor,
    useSensors,
    DragEndEvent,
    DragOverlay,
    DragStartEvent,
} from '@dnd-kit/core';
import {
    arrayMove,
    SortableContext,
    useSortable,
    verticalListSortingStrategy,
} from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import {
    Box,
    Typography,
    List,
    ListItem,
    Paper,
    IconButton,
    useTheme,
    alpha,
} from '@mui/material';
import DeleteIcon from '@mui/icons-material/Delete';
import LocalFireDepartmentIcon from '@mui/icons-material/LocalFireDepartment';
import { v4 as uuidv4 } from 'uuid';

export interface DeviceIoPorts {
    deviceIoPortId: number;
    deviceId?: number;
    commandPin?: number;
    description?: string | null;
    isNotVisable?: boolean;
    isDanger: boolean;
    devices?: any;
}

interface DragDropPinsProps {
    availablePins: DeviceIoPorts[];
    initialOrder: DeviceIoPorts[];
    onOrderChange: (newOrder: DeviceIoPorts[]) => void;
}

interface ExecutionItem extends DeviceIoPorts {
    uniqueId: string;
}

const PinContent: React.FC<{ item: DeviceIoPorts | ExecutionItem }> = ({ item }) => {
    const theme = useTheme();

    return (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            {item.isDanger && (
                <LocalFireDepartmentIcon
                    sx={{
                        color: theme.palette.error.main,
                        fontSize: '1.2rem'
                    }}
                />
            )}
            <Typography variant="body1" sx={{ color: theme.palette.text.primary }}>
                {item.deviceId}:{item.commandPin} {item.description}
            </Typography>
        </Box>
    );
};

const DraggableItem: React.FC<{
    item: DeviceIoPorts;
}> = ({ item }) => {
    const theme = useTheme();
    const {
        attributes,
        listeners,
        setNodeRef,
        transform,
        transition,
        isDragging,
    } = useSortable({
        id: `source-${item.deviceIoPortId}`,
        data: { type: 'source', item }
    });

    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.5 : 1,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: theme.spacing(1),
        marginBottom: theme.spacing(0.5),
        backgroundColor: theme.palette.mode === 'dark'
            ? alpha(theme.palette.background.paper, 0.8)
            : alpha(theme.palette.background.paper, 0.6),
        borderRadius: theme.shape.borderRadius,
        cursor: 'grab',
        border: `1px solid ${theme.palette.divider}`,
    };

    return (
        <ListItem
            ref={setNodeRef}
            style={style}
            {...attributes}
            {...listeners}
            sx={{
                '&:hover': {
                    backgroundColor: theme.palette.mode === 'dark'
                        ? alpha(theme.palette.background.paper, 0.9)
                        : alpha(theme.palette.background.paper, 0.8),
                },
            }}
        >
            <PinContent item={item} />
        </ListItem>
    );
};

const SortableItem: React.FC<{
    item: ExecutionItem;
    onDelete: (id: string) => void;
}> = ({ item, onDelete }) => {
    const theme = useTheme();
    const {
        attributes,
        listeners,
        setNodeRef,
        transform,
        transition,
        isDragging,
    } = useSortable({
        id: item.uniqueId,
        data: { type: 'execution', item }
    });

    const style = {
        transform: CSS.Transform.toString(transform),
        transition,
        opacity: isDragging ? 0.5 : 1,
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        padding: theme.spacing(1),
        marginBottom: theme.spacing(0.5),
        backgroundColor: theme.palette.mode === 'dark'
            ? alpha(theme.palette.background.paper, 0.8)
            : alpha(theme.palette.background.paper, 0.6),
        borderRadius: theme.shape.borderRadius,
        border: `1px solid ${theme.palette.divider}`,
    };

    const handleDeleteClick = (e: React.MouseEvent) => {
        e.stopPropagation();
        onDelete(item.uniqueId);
    };

    return (
        <ListItem ref={setNodeRef} style={style}>
            <Box
                {...attributes}
                {...listeners}
                sx={{
                    flex: 1,
                    cursor: 'grab',
                    display: 'flex',
                    alignItems: 'center',
                    '&:hover': {
                        backgroundColor: theme.palette.mode === 'dark'
                            ? alpha(theme.palette.background.paper, 0.9)
                            : alpha(theme.palette.background.paper, 0.8),
                    },
                }}
            >
                <PinContent item={item} />
            </Box>

            <IconButton
                edge="end"
                aria-label="delete"
                size="small"
                onClick={handleDeleteClick}
                sx={{
                    ml: 1,
                    color: theme.palette.mode === 'dark'
                        ? theme.palette.error.light
                        : theme.palette.error.dark,
                    '&:hover': {
                        backgroundColor: alpha(theme.palette.error.main, 0.1),
                    },
                }}
            >
                <DeleteIcon />
            </IconButton>
        </ListItem>
    );
};

const DragDropPins: React.FC<DragDropPinsProps> = ({
    availablePins,
    initialOrder,
    onOrderChange,
}) => {
    const theme = useTheme();
    const [executionOrder, setExecutionOrder] = useState<ExecutionItem[]>([]);
    const [activeItem, setActiveItem] = useState<DeviceIoPorts | ExecutionItem | null>(null);

    useEffect(() => {
        const hydratedOrder = initialOrder.map((item) => ({
            ...item,
            uniqueId: uuidv4(),
        }));
        setExecutionOrder(hydratedOrder);
    }, [initialOrder]);

    const sensors = useSensors(useSensor(PointerSensor));

    const handleDragStart = (event: DragStartEvent) => {
        const { active } = event;

        if (active.data.current?.type === 'source') {
            setActiveItem(active.data.current.item);
        } else {
            const item = executionOrder.find((i) => i.uniqueId === active.id);
            setActiveItem(item || null);
        }
    };

    const handleDragEnd = (event: DragEndEvent) => {
        const { active, over } = event;

        if (!over) {
            setActiveItem(null);
            return;
        }

        // Handle dropping from source to execution
        if (active.data.current?.type === 'source') {
            const sourceItem = active.data.current.item as DeviceIoPorts;
            const newItem: ExecutionItem = {
                ...sourceItem,
                uniqueId: uuidv4(),
            };

            const overIndex = executionOrder.findIndex((item) => item.uniqueId === over.id);

            if (overIndex >= 0) {
                // Insert at specific position
                const newItems = [...executionOrder];
                newItems.splice(overIndex, 0, newItem);
                setExecutionOrder(newItems);
                onOrderChange(newItems);
            } else {
                // Add to end
                const newItems = [...executionOrder, newItem];
                setExecutionOrder(newItems);
                onOrderChange(newItems);
            }
        }
        // Handle reordering within execution list
        else if (active.id !== over.id) {
            const oldIndex = executionOrder.findIndex(
                (item) => item.uniqueId === active.id
            );
            const newIndex = executionOrder.findIndex(
                (item) => item.uniqueId === over.id
            );

            if (oldIndex !== -1 && newIndex !== -1) {
                setExecutionOrder((items) => {
                    const newItems = arrayMove(items, oldIndex, newIndex);
                    onOrderChange(newItems);
                    return newItems;
                });
            }
        }

        setActiveItem(null);
    };

    const handleDelete = (id: string) => {
        setExecutionOrder((items) => {
            const newItems = items.filter((item) => item.uniqueId !== id);
            onOrderChange(newItems);
            return newItems;
        });
    };

    return (
        <DndContext
            sensors={sensors}
            collisionDetection={closestCenter}
            onDragStart={handleDragStart}
            onDragEnd={handleDragEnd}
        >
            <Box
                display="flex"
                justifyContent="space-between"
                height="300px"
                padding={theme.spacing(2)}
                boxSizing="border-box"
            >
                {/* Source Pins */}
                <Paper
                    elevation={3}
                    sx={{
                        width: '45%',
                        padding: theme.spacing(2),
                        overflowY: 'auto',
                        backgroundColor: theme.palette.background.paper,
                    }}
                >
                    <Typography
                        variant="h6"
                        gutterBottom
                        sx={{ color: theme.palette.text.primary }}
                    >
                        Available Pins
                    </Typography>
                    <SortableContext
                        items={availablePins.map(pin => `source-${pin.deviceIoPortId}`)}
                        strategy={verticalListSortingStrategy}
                    >
                        <List>
                            {availablePins.map((pin) => (
                                <DraggableItem key={pin.deviceIoPortId} item={pin} />
                            ))}
                        </List>
                    </SortableContext>
                </Paper>

                {/* Execution Order */}
                <Paper
                    elevation={3}
                    sx={{
                        width: '45%',
                        padding: theme.spacing(2),
                        overflowY: 'auto',
                        backgroundColor: theme.palette.background.paper,
                    }}
                >
                    <Typography
                        variant="h6"
                        gutterBottom
                        sx={{ color: theme.palette.text.primary }}
                    >
                        Execution Order
                    </Typography>
                    <SortableContext
                        items={executionOrder.map((item) => item.uniqueId)}
                        strategy={verticalListSortingStrategy}
                    >
                        <List>
                            {executionOrder.map((item) => (
                                <SortableItem
                                    key={item.uniqueId}
                                    item={item}
                                    onDelete={handleDelete}
                                />
                            ))}
                        </List>
                    </SortableContext>
                </Paper>
            </Box>

            <DragOverlay>
                {activeItem && (
                    <Paper
                        sx={{
                            padding: theme.spacing(1),
                            backgroundColor: theme.palette.background.paper,
                            boxShadow: theme.shadows[4],
                            borderRadius: theme.shape.borderRadius,
                            width: 'auto',
                        }}
                    >
                        <PinContent item={activeItem} />
                    </Paper>
                )}
            </DragOverlay>
        </DndContext>
    );
};

export default DragDropPins;