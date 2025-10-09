// src/components/SetsEditor.tsx

import React, { useEffect, useState, ChangeEvent, useCallback } from 'react';
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
    Dialog,
    DialogTitle,
    DialogContent,
    DialogContentText,
    DialogActions,
    Button,
    CircularProgress,
    Switch,
    FormControlLabel,
} from '@mui/material';
import { Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material';
import SetSequenceEdit from './SetSequenceEdit';
import debounce from 'lodash/debounce';

const sessionSetSelected = 'SetEdit-SetSelected';

const SetsEditor: React.FC = observer(() => {
    const store = AppStoreContextItem.useStore();

    const [setIdSelected, setSetIdSelected] = useState<number>(0);
    const [setSelected, setSetSelected] = useState<Sets | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    // State for confirmation dialog
    const [confirmDialogOpen, setConfirmDialogOpen] = useState<boolean>(false);
    const [confirmDialogTitle, setConfirmDialogTitle] = useState<string>('');
    const [confirmDialogContent, setConfirmDialogContent] = useState<string>('');
    const [confirmDialogAction, setConfirmDialogAction] = useState<() => void>(() => { });

    // Fetch sets when store.sets changes
    useEffect(() => {
        const initialize = () => {
            if (store.sets.length === 0) {
                // If sets are not loaded yet, do nothing
                return;
            }

            try {
                getAllSets();
            } catch (error: any) {
                store.setError(`Initialization failed: ${error.message}`);
            } finally {
                setLoading(false);
            }
        };

        initialize();
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [store.sets]); // Run this effect whenever store.sets changes

    const getAllSets = () => {
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
                handleSetChange(selectedSet.setId);
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

    // Debounced save function to limit store updates
    const debouncedHandleSetSave = useCallback(
        debounce(async (updatedSet: Sets) => {
            try {
                await store.updateSet(updatedSet.setId, updatedSet);
                store.clearError();
            } catch (error: any) {
                store.setError(`Failed to save set: ${error.message}`);
            }
        }, 2000),
        [store]
    );

    const handleSetSave = (updatedSet: Sets) => {
        debouncedHandleSetSave(updatedSet);
    };

    const handleSetDelete = () => {
        if (!setSelected) return;

        setConfirmDialogTitle('Delete Set');
        setConfirmDialogContent(`Are you sure you want to delete the set "${setSelected.setName}"? This action cannot be undone.`);
        setConfirmDialogAction(() => async () => {
            try {
                await store.deleteSet(setSelected.setId);
                setSetIdSelected(0);
                setSetSelected(null);
                store.clearError();
                await getAllSets();
            } catch (error: any) {
                store.setError(`Failed to delete set: ${error.message}`);
            } finally {
                setConfirmDialogOpen(false);
            }
        });
        setConfirmDialogOpen(true);
    };

    const handleSequenceDelete = (sequence: SetSequences) => {
        setConfirmDialogTitle('Delete Sequence');
        setConfirmDialogContent(`Are you sure you want to delete the sequence with "On At" value ${sequence.onAt}? This action cannot be undone.`);
        setConfirmDialogAction(() => async () => {
            try {
                await store.deleteSetSequence(sequence.setSequenceId);
                store.clearError();
            } catch (error: any) {
                store.setError(`Failed to delete sequence: ${error.message}`);
            } finally {
                setConfirmDialogOpen(false);
            }
        });
        setConfirmDialogOpen(true);
    };

    const handleSetCreate = async () => {
        try {
            const newSet = await store.createSet({ setId: 0, setName: 'New Set', isDisabled: false });

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
                setSequenceId: 0,
            };

            await store.createSetSequence(newSequence);
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to create sequence: ${error.message}`);
        }
    };

    const handleToggleDisabled = (event: ChangeEvent<HTMLInputElement>) => {
        if (!setSelected) return;
        const updatedSet = { ...setSelected, isDisabled: event.target.checked };
        setSetSelected(updatedSet);
        handleSetSave(updatedSet);
    };

    if (loading) {
        return (
            <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', }}>
                <CircularProgress />
            </div>
        );
    }

    return (
        <div style={{ display: 'flex', flexDirection: 'column', padding: '1rem' }}>
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

                            <Tooltip title={setSelected.isDisabled ? "Enable Set" : "Disable Set"}>
                                <FormControlLabel
                                    control={
                                        <Switch
                                            checked={setSelected.isDisabled}
                                            onChange={handleToggleDisabled}
                                            color="primary"
                                        />
                                    }
                                    label={setSelected.isDisabled ? "Disabled" : "Enabled"}
                                />
                            </Tooltip>

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

                        {store.setSequences
                            .filter((sequence) => sequence.setId === setSelected.setId)
                            .slice() // Creates a shallow copy to avoid mutating the original array
                            .sort((a, b) => a.onAt - b.onAt) // Sorts by onAt in ascending order
                            .map((sequence) => (
                                <SetSequenceEdit
                                    key={sequence.setSequenceId}
                                    sequence={sequence}
                                    onDelete={() => handleSequenceDelete(sequence)}
                                />
                            ))}
                    </div>
                </div>
            )}

            <Dialog
                open={confirmDialogOpen}
                onClose={() => setConfirmDialogOpen(false)}
                aria-labelledby="confirm-dialog-title"
                aria-describedby="confirm-dialog-description"
            >
                <DialogTitle id="confirm-dialog-title">{confirmDialogTitle}</DialogTitle>
                <DialogContent>
                    <DialogContentText id="confirm-dialog-description">
                        {confirmDialogContent}
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setConfirmDialogOpen(false)} color="primary">
                        Cancel
                    </Button>
                    <Button onClick={confirmDialogAction} color="secondary" autoFocus>
                        Confirm
                    </Button>
                </DialogActions>
            </Dialog>
        </div>
    )
});

export default SetsEditor;
