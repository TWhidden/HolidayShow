// src/components/EditPattern.tsx

import React, { useEffect, useState, useCallback, useRef, ChangeEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import { DevicePatternSequences } from '../Clients/Api';
import {
    TextField,
    IconButton,
    Tooltip,
    Dialog,
    DialogTitle,
    DialogContent,
    DialogContentText,
    DialogActions,
    Button,
    useTheme,
} from '@mui/material';
import FindReplaceIcon from '@mui/icons-material/FindReplace';
import DeleteIcon from '@mui/icons-material/Delete';
import ComboSelect from 'react-select';
import { ApiStoreContextItem } from '../Stores/ApiStore';
import debounce from 'lodash/debounce';

interface EditPatternProps {
    sequence: DevicePatternSequences;
    portOptions: { label: string; value: number }[];
    onDelete: (sequence: DevicePatternSequences) => void;
}

const EditPattern: React.FC<EditPatternProps> = observer(({ sequence, portOptions, onDelete }) => {
    const store = AppStoreContextItem.useStore();
    const apiStore = ApiStoreContextItem.useStore();
    const theme = useTheme();
    const { audioOptions } = store;

    const [port, setPort] = useState<{ label: string; value: number } | null>(null);
    const [audio, setAudio] = useState<{ label: string; value: number } | null>(null);
    const [onAt, setOnAt] = useState<string>(sequence.onAt.toString());
    const [duration, setDuration] = useState<string>(sequence.duration?.toString() || '0');

    // Refs to hold latest state values
    const onAtRef = useRef(onAt);
    const durationRef = useRef(duration);
    const audioRef = useRef(audio);
    const portRef = useRef(port);

    // Update refs when state changes
    useEffect(() => {
        onAtRef.current = onAt;
    }, [onAt]);

    useEffect(() => {
        durationRef.current = duration;
    }, [duration]);

    useEffect(() => {
        audioRef.current = audio;
    }, [audio]);

    useEffect(() => {
        portRef.current = port;
    }, [port]);

    // State for the confirmation dialog
    const [isDialogOpen, setIsDialogOpen] = useState<boolean>(false);

    useEffect(() => {
        const selectedAudioItem = audioOptions.find(a => a.audioId === sequence.audioId);
        const selectedIoPort = portOptions.find(p => p.value === sequence.deviceIoPortId);

        setAudio(selectedAudioItem ? { label: selectedAudioItem.displayName ?? "", value: selectedAudioItem.audioId } : null);
        setPort(selectedIoPort || null);
        setOnAt(sequence.onAt.toString());
        setDuration(sequence.duration?.toString() || '0');
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [sequence, audioOptions, portOptions]);

    // Debounced handleSave using lodash/debounce with stable function
    const debouncedHandleSave = useCallback(
        debounce(async () => {
            try {
                const updatedSequence: DevicePatternSequences = {
                    ...sequence,
                    onAt: Number(onAtRef.current),
                    duration: Number(durationRef.current),
                    audioId: audioRef.current ? audioRef.current.value : 1,
                    deviceIoPortId: portRef.current ? portRef.current.value : sequence.deviceIoPortId,
                };
                await store.updateDevicePatternSequence(updatedSequence.devicePatternSeqenceId, updatedSequence);
                store.clearError();
            } catch (error: any) {
                store.setError(`Failed to save sequence: ${error.message}`);
            }
        }, 2000), // 2000ms debounce delay
        [sequence, store] // Ensure these are stable or manage dependencies accordingly
    );

    // Cleanup the debounced function on component unmount
    useEffect(() => {
        return () => {
            console.log('cleanup debouncedHandleSave');
            debouncedHandleSave.cancel();
        };
    }, [debouncedHandleSave]);

    const handleIoPortDetect = async () => {
        try {
            if (sequence.deviceIoPortId) {
                await apiStore.getApi().deviceIoPortsPutDeviceIoPortIdentifyUpdate(sequence.deviceIoPortId);
            }
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to detect IO port: ${error.message}`);
        }
    };

    // Handlers for the confirmation dialog
    const handleDeleteClick = () => {
        setIsDialogOpen(true);
    };

    const handleDialogClose = () => {
        setIsDialogOpen(false);
    };

    const handleConfirmDelete = () => {
        onDelete(sequence);
        setIsDialogOpen(false);
    };

    return (
        <div style={{ display: "flex", flexDirection: "row", alignItems: 'center', marginBottom: '0.5rem', gap: '0.25rem', flexWrap: 'wrap', maxWidth: '100%' }}>
            <TextField
                label="On At"
                value={onAt}
                onChange={(e: ChangeEvent<HTMLInputElement>) => {
                    setOnAt(e.target.value);
                    debouncedHandleSave();
                }}
                margin="dense"
                size="small"
                style={{ flex: '1 1 80px', minWidth: '80px', maxWidth: '100%' }}
            />

            <TextField
                label="Duration"
                value={duration}
                onChange={(e: ChangeEvent<HTMLInputElement>) => {
                    setDuration(e.target.value);
                    debouncedHandleSave();
                }}
                margin="dense"
                size="small"
                style={{ flex: '1 1 80px', minWidth: '80px', maxWidth: '100%' }}
            />

            <ComboSelect
                isClearable={false}
                options={portOptions}
                onChange={(selectedOption) => {
                    setPort(selectedOption);
                    debouncedHandleSave();
                }}
                value={port}
                placeholder="Select GPIO Port"
                menuPortalTarget={document.body}
                menuPosition="fixed"
                styles={{ 
                    container: (provided) => ({ ...provided, flex: 3, minWidth: '120px', maxWidth: '100%' }),
                    control: (provided) => ({ 
                        ...provided, 
                        minHeight: '30px',
                        backgroundColor: theme.palette.background.paper,
                        borderColor: theme.palette.mode === 'dark' ? theme.palette.grey[700] : theme.palette.grey[300],
                    }),
                    menuPortal: (provided) => ({ ...provided, zIndex: 9999 }),
                    menu: (provided) => ({ 
                        ...provided, 
                        backgroundColor: theme.palette.background.paper,
                    }),
                    option: (provided, state) => ({
                        ...provided,
                        backgroundColor: state.isFocused 
                            ? theme.palette.action.hover 
                            : theme.palette.background.paper,
                        color: theme.palette.text.primary,
                    }),
                    singleValue: (provided) => ({
                        ...provided,
                        color: theme.palette.text.primary,
                    }),
                    input: (provided) => ({ 
                        ...provided, 
                        margin: '0px',
                        color: theme.palette.text.primary,
                    }),
                    indicatorsContainer: (provided) => ({ ...provided, height: '30px' }),
                }}
            />

            <ComboSelect
                isClearable={false}
                options={audioOptions.map(a => ({ label: a.displayName ?? "na", value: a.audioId }))}
                onChange={(selectedOption) => {
                    setAudio(selectedOption);
                    debouncedHandleSave();
                }}
                value={audio}
                placeholder="Select Audio File"
                menuPortalTarget={document.body}
                menuPosition="fixed"
                styles={{ 
                    container: (provided) => ({ ...provided, flex: 3, minWidth: '120px', maxWidth: '100%' }),
                    control: (provided) => ({ 
                        ...provided, 
                        minHeight: '30px',
                        backgroundColor: theme.palette.background.paper,
                        borderColor: theme.palette.mode === 'dark' ? theme.palette.grey[700] : theme.palette.grey[300],
                    }),
                    menuPortal: (provided) => ({ ...provided, zIndex: 9999 }),
                    menu: (provided) => ({ 
                        ...provided, 
                        backgroundColor: theme.palette.background.paper,
                    }),
                    option: (provided, state) => ({
                        ...provided,
                        backgroundColor: state.isFocused 
                            ? theme.palette.action.hover 
                            : theme.palette.background.paper,
                        color: theme.palette.text.primary,
                    }),
                    singleValue: (provided) => ({
                        ...provided,
                        color: theme.palette.text.primary,
                    }),
                    input: (provided) => ({ 
                        ...provided, 
                        margin: '0px',
                        color: theme.palette.text.primary,
                    }),
                    indicatorsContainer: (provided) => ({ ...provided, height: '30px' }),
                }}
            />

            <Tooltip title="Detect IO Port">
                <IconButton onClick={handleIoPortDetect} size="small">
                    <FindReplaceIcon fontSize="small" />
                </IconButton>
            </Tooltip>

            <Tooltip title="Delete Sequence">
                <IconButton onClick={handleDeleteClick} size="small">
                    <DeleteIcon fontSize="small" />
                </IconButton>
            </Tooltip>

            {/* Confirmation Dialog */}
            <Dialog
                open={isDialogOpen}
                onClose={handleDialogClose}
                aria-labelledby="confirm-delete-dialog-title"
                aria-describedby="confirm-delete-dialog-description"
            >
                <DialogTitle id="confirm-delete-dialog-title">Confirm Delete</DialogTitle>
                <DialogContent>
                    <DialogContentText id="confirm-delete-dialog-description">
                        Are you sure you want to delete this sequence?
                    </DialogContentText>
                </DialogContent>
                <DialogActions>
                    <Button onClick={handleDialogClose} color="primary">
                        Cancel
                    </Button>
                    <Button onClick={handleConfirmDelete} color="secondary" autoFocus>
                        Delete
                    </Button>
                </DialogActions>
            </Dialog>
        </div>
    );
});

export default EditPattern;
