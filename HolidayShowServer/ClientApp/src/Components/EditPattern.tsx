// src/components/EditPattern.tsx

import React, { useEffect, useState, useRef, ChangeEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import { DevicePatternSequences } from '../Clients/Api';
import { TextField, IconButton, Tooltip } from '@mui/material';
import FindReplaceIcon from '@mui/icons-material/FindReplace';
import DeleteIcon from '@mui/icons-material/Delete';
import ComboSelect from 'react-select';
import { ApiStoreContextItem } from '../Stores/ApiStore';

interface EditPatternProps {
    sequence: DevicePatternSequences;
    portOptions: { label: string; value: number }[];
    onDelete: (sequence: DevicePatternSequences) => void;
}

const EditPattern: React.FC<EditPatternProps> = observer(({ sequence, portOptions, onDelete }) => {
    const store = AppStoreContextItem.useStore();
    const apiStore = ApiStoreContextItem.useStore();
    const { audioOptions } = store;

    const [port, setPort] = useState<{ label: string; value: number } | null>(null);
    const [audio, setAudio] = useState<{ label: string; value: number } | null>(null);
    const [onAt, setOnAt] = useState<string>(sequence.onAt.toString());
    const [duration, setDuration] = useState<string>(sequence.duration?.toString() || '0');

    const timerRef = useRef<number | null>(null);

    useEffect(() => {
        const selectedAudioItem = audioOptions.find(a => a.audioId === sequence.audioId);
        const selectedIoPort = portOptions.find(p => p.value === sequence.deviceIoPortId);

        setAudio(selectedAudioItem ? { label: selectedAudioItem.displayName ?? "", value: selectedAudioItem.audioId } : null);
        setPort(selectedIoPort || null);
        setOnAt(sequence.onAt.toString());
        setDuration(sequence.duration?.toString() || '0');
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [sequence, audioOptions, portOptions]);

    const handleDelaySave = () => {
        if (timerRef.current !== null) {
            clearTimeout(timerRef.current);
        }

        timerRef.current = window.setTimeout(() => {
            handleSave();
        }, 1000);
    };

    const handleSave = async () => {
        try {
            const updatedSequence: DevicePatternSequences = {
                ...sequence,
                onAt: Number(onAt),
                duration: Number(duration),
                audioId: audio ? audio.value : 1,
                deviceIoPortId: port ? port.value : sequence.deviceIoPortId,
            };
            await store.updateDevicePatternSequence(updatedSequence.devicePatternSeqenceId, updatedSequence); // Ensure this method exists in AppStore
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to save sequence: ${error.message}`);
        }
    };

    const handleIoPortDetect = async () => {
        try {
            if (sequence.deviceIoPortId) {
                await apiStore.getApi().deviceIoPortsPutDeviceIoPortIdentifyUpdate(sequence.deviceIoPortId); // Ensure this method exists in AppStore
            }
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to detect IO port: ${error.message}`);
        }
    };

    return (
        <div style={{ display: "flex", flexDirection: "row", alignItems: 'center', marginBottom: '1rem' }}>
            <TextField
                label="On At"
                value={onAt}
                onChange={(e: ChangeEvent<HTMLInputElement>) => {
                    setOnAt(e.target.value);
                    handleDelaySave();
                }}
                margin="normal"
                style={{ marginRight: '1rem', flex: 1 }}
            />

            <TextField
                label="Duration"
                value={duration}
                onChange={(e: ChangeEvent<HTMLInputElement>) => {
                    setDuration(e.target.value);
                    handleDelaySave();
                }}
                margin="normal"
                style={{ marginRight: '1rem', flex: 1 }}
            />

            <ComboSelect
                isClearable={false}
                options={portOptions}
                onChange={(selectedOption) => {
                    setPort(selectedOption);
                    handleDelaySave();
                }}
                value={port}
                placeholder="Select GPIO Port"
                styles={{ container: (provided) => ({ ...provided, flex: 3, marginRight: '1rem' }) }}
            />

            <ComboSelect
                isClearable={false}
                options={audioOptions.map(a => ({ label: a.displayName ?? "na", value: a.audioId }))}
                onChange={(selectedOption) => {
                    setAudio(selectedOption);
                    handleDelaySave();
                }}
                value={audio}
                placeholder="Select Audio File"
                styles={{ container: (provided) => ({ ...provided, flex: 3, marginRight: '1rem' }) }}
            />

            <Tooltip title="Detect IO Port">
                <IconButton onClick={handleIoPortDetect}>
                    <FindReplaceIcon />
                </IconButton>
            </Tooltip>

            <Tooltip title="Delete Sequence">
                <IconButton onClick={() => onDelete(sequence)}>
                    <DeleteIcon />
                </IconButton>
            </Tooltip>
        </div>
    );
});

export default EditPattern;
