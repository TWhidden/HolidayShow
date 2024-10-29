// src/components/SetsEditor.tsx

import React, { useEffect, useState, ChangeEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import {

    Sets,
    SetSequences,
} from '../Clients/Api';
import {
    Select,
    InputLabel,
    MenuItem,
    FormControl,
    TextField,
    IconButton,
    Tooltip,
    Typography,
} from '@mui/material';
import { Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material';
import SetSequenceEdit from './SetSequenceEdit';


const sessionSetSelected = 'SetEdit-SetSelected';

const SetsEditor: React.FC = observer(() => {
    const store = AppStoreContextItem.useStore();

    const [setIdSelected, setSetIdSelected] = useState<number>(0);
    const [setSelected, setSetSelected] = useState<Sets | null>(null);

    // Fetch patterns and effects on mount
    useEffect(() => {
        const initialize = async () => {
            try {

                // Fetch all sets
                getAllSets();
            } catch (error: any) {
                store.setError(`Initialization failed: ${error.message}`);
            }
        };

        initialize();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [store]);

    const getAllSets = async () => {
        try {


            let selectedSet: Sets | undefined = store.sets[0];
            const lastSelectedId = sessionStorage.getItem(sessionSetSelected);

            if (lastSelectedId) {
                const id = Number(lastSelectedId);
                const foundSet = store.sets.find((set) => set.setId === id);
                if (foundSet) {
                    selectedSet = foundSet;
                }
            }

            if (selectedSet) {
                setSetIdSelected(selectedSet.setId);
                setSetSelected(selectedSet);
                sessionStorage.setItem(sessionSetSelected, selectedSet.setId.toString());
                await handleSetChange(selectedSet.setId);
            }
        } catch (error: any) {
            store.setError(`Failed to fetch sets: ${error.message}`);
        }
    };

    const handleSetChange = (setId: number) => {
        const selected = store.sets.find((set) => set.setId === setId);
        if (!selected) return;

        console.log(`Setting ${sessionSetSelected}: ${setId}`);
        sessionStorage.setItem(sessionSetSelected, setId.toString());

        setSetIdSelected(setId);
        setSetSelected(selected);
    };

    const handleSetDelete = async () => {
        if (!setSelected) return;

        try {
            await store.deleteSet(setSelected.setId); // Ensure this method exists

            setSetIdSelected(0);
            setSetSelected(null);

            store.clearError();

            await getAllSets();
        } catch (error: any) {
            store.setError(`Failed to delete set: ${error.message}`);
        }
    };

    const handleSetCreate = async () => {
        try {
            const newSet = await store.createSet({ setId: 0, setName: 'New Set' }); // Ensure this method exists

            if (newSet) {
                setSetIdSelected(newSet.setId);
                setSetSelected(newSet);
                sessionStorage.setItem(sessionSetSelected, newSet.setId.toString());
                store.clearError();
            }

        } catch (error: any) {
            store.setError(`Failed to create set: ${error.message}`);
        }
    };

    const handleSetSave = async (updatedSet: Sets) => {
        try {
            await store.updateSet(updatedSet.setId, updatedSet); // Ensure this method exists
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to save set: ${error.message}`);
        }
    };

    const handleSequenceCreate = async () => {
        if (!setSelected) return;

        try {

            // Determine next onAt value
            let nextOnAt = 1000;
            if (store.setSequences.length > 0) {
                const lastSequence = store.setSequences.reduce((prev, current) =>
                    prev.onAt > current.onAt ? prev : current
                );
                nextOnAt = lastSequence.onAt + 1000;
            }

            const newSequence: SetSequences = {
                onAt: nextOnAt,
                setId: setSelected.setId,
                setSequenceId: 0, // Assuming backend assigns
            };

            await store.createSetSequence(newSequence); // Ensure this method exists
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to create sequence: ${error.message}`);
        }
    };

    const handleSequenceDelete = async (sequence: SetSequences) => {
        try {
            await store.deleteSetSequence(sequence.setSequenceId); // Ensure this method exists
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to delete sequence: ${error.message}`);
        }
    };

    return (
        <div style={{ display: 'flex', flexDirection: 'column', height: '100vh', padding: '1rem' }}>
            {store.error && (
                <div style={{ color: 'red', marginBottom: '1rem' }}>{store.error}</div>
            )}

            <div style={{ display: 'flex', flexDirection: 'row', marginBottom: '1rem' }}>
                <FormControl style={{ minWidth: 120, marginRight: '1rem' }}>
                    <InputLabel id="sets-label">Sets</InputLabel>
                    <Select
                        labelId="sets-label"
                        id="sets-select"
                        value={setIdSelected}
                        onChange={(e) => handleSetChange(Number(e.target.value))}
                    >
                        {store.sets.map((set) => (
                            <MenuItem key={set.setId} value={set.setId}>
                                {set.setName}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                {setSelected && (
                    <>
                        <Tooltip title="Delete Set">
                            <IconButton onClick={handleSetDelete}>
                                <DeleteIcon />
                            </IconButton>
                        </Tooltip>

                        <Tooltip title="Create New Set">
                            <IconButton onClick={handleSetCreate}>
                                <AddIcon />
                            </IconButton>
                        </Tooltip>
                    </>
                )}
            </div>

            {setSelected && (
                <div>
                    <div style={{ display: 'flex', flexDirection: 'column', marginBottom: '1rem' }}>
                        <div style={{ display: 'flex', flexDirection: 'row', alignItems: 'center', marginBottom: '1rem' }}>
                            <TextField
                                label="Set Name"
                                value={setSelected.setName}
                                onChange={(e: ChangeEvent<HTMLInputElement>) => {
                                    const updatedSet = { ...setSelected, setName: e.target.value };
                                    setSetSelected(updatedSet);
                                    handleSetSave(updatedSet);
                                }}
                                margin="normal"
                                style={{ marginRight: '1rem', flex: 1 }}
                            />

                            <Tooltip title="Create New Sequence">
                                <IconButton onClick={handleSequenceCreate}>
                                    <AddIcon />
                                </IconButton>
                            </Tooltip>
                        </div>

                        <div style={{ display: 'flex', flexDirection: 'row', marginBottom: '0.5rem' }}>
                            <Typography variant="body2" style={{ flex: '1 1 75px' }}>
                                On At:
                            </Typography>
                            <Typography variant="body2" style={{ flex: '3 1 300px' }}>
                                Device Pattern:
                            </Typography>
                            <Typography variant="body2" style={{ flex: '3 1 300px' }}>
                                Effect:
                            </Typography>
                            <Typography variant="body2" style={{ flex: '1 1 75px' }}>
                                Delete:
                            </Typography>
                        </div>

                        {store.setSequences.map((sequence) => (
                            <SetSequenceEdit
                                key={sequence.setSequenceId}
                                sequence={sequence}
                                onDelete={handleSequenceDelete}
                            />
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
});

export default SetsEditor;
