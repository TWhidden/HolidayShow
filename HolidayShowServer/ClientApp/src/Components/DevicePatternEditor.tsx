// src/components/DevicePattern.tsx

import React, { ChangeEvent, useEffect, useState } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import { Devices, DevicePatterns, DevicePatternSequences } from '../Clients/Api';
import { Select, InputLabel, MenuItem, FormControl, TextField, IconButton, Tooltip, Typography } from '@mui/material';
import { Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material';
import EditPattern from './EditPattern';

const sessionLastDeviceSelected = "PatternEdit-selectedDevice";
const sessionLastPatternSelected = "PatternEdit-selectedPattern";

const DevicePattern: React.FC = observer(() => {
    const store = AppStoreContextItem.useStore();

    const [patternSequences, setPatternSequences] = useState<DevicePatternSequences[]>([]);
    const [ioPortOptions, setIoPortOptions] = useState<{ label: string; value: number }[]>([]);

    const [deviceSelected, setDeviceSelected] = useState<Devices | undefined>(undefined);
    const [deviceIdSelected, setDeviceIdSelected] = useState<number>(0);
    const [patternSelected, setPatternSelected] = useState<DevicePatterns | undefined>(undefined);
    const [patternIdSelected, setPatternIdSelected] = useState<number>(0);

    // Fetch audio on mount
    useEffect(() => {
        getDevices();
    }, []);

    const getDevices = async () => {
        try {
            let devices = store.devices;

            let selectedDevice: Devices | undefined = devices[0];

            const lastSelectedDeviceId = sessionStorage.getItem(sessionLastDeviceSelected);
            if (lastSelectedDeviceId !== null) {
                const id = Number(lastSelectedDeviceId);
                const foundDevice = devices.find(device => device.deviceId === id);
                if (foundDevice) {
                    selectedDevice = foundDevice;
                }
            }

            if (selectedDevice) {
                setDeviceIdSelected(selectedDevice.deviceId);
                setDeviceSelected(selectedDevice);
                sessionStorage.setItem(sessionLastDeviceSelected, selectedDevice.deviceId.toString());
            }

            // Load patterns and IO ports for the selected device
            if (selectedDevice) {
                await handleDeviceChange(selectedDevice.deviceId);
            }

        } catch (error: any) {
            store.setError(`Failed to get devices: ${error.message}`);
        }
    };

    const handleDeviceChange = async (deviceId: number) => {
        const device = store.devices.find(d => d.deviceId === deviceId);
        if (!device) return;

        console.log(`setting ${sessionLastDeviceSelected}: ${deviceId}`);

        sessionStorage.setItem(sessionLastDeviceSelected, deviceId.toString());
        setDeviceIdSelected(deviceId);
        setDeviceSelected(device);

        await getPatternsForSelectedDevice();
        await getIoPortsForSelectedDevice(device);
    };

    const getPatternsForSelectedDevice = async () => {
        try {
            let patternId = 0;
            const lastSelectedPatternId = sessionStorage.getItem(sessionLastPatternSelected);
            if (lastSelectedPatternId !== null) {
                const id = Number(lastSelectedPatternId);
                const foundPattern = store.devicePatterns.find(p => p.devicePatternId === id);
                if (foundPattern) {
                    patternId = foundPattern.devicePatternId;
                }
            }

            if (patternId === 0 && store.devicePatterns.length > 0) {
                patternId = store.devicePatterns[0].devicePatternId;
            }

            await handlePatternChange(patternId);
        } catch (error: any) {
            store.setError(`Failed to get patterns: ${error.message}`);
        }
    };

    const getIoPortsForSelectedDevice = async (device: Devices) => {
        try {
            const ports = await store.deviceIoPorts.filter(x => x.deviceId === device.deviceId); // Assuming store has fetchIoPortsByDeviceId
            const formattedPorts = ports.map(port => ({
                label: `${port.commandPin}: ${port.description}`,
                value: port.deviceIoPortId
            }));
            setIoPortOptions(formattedPorts);
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to get IO ports: ${error.message}`);
        }
    };

    const handlePatternChange = async (patternId: number) => {
        if (!patternId) return;

        const pattern = store.devicePatterns.find(p => p.devicePatternId === patternId);
        if (!pattern) return;

        console.log(`setting ${sessionLastPatternSelected}: ${patternId}`);

        sessionStorage.setItem(sessionLastPatternSelected, patternId.toString());
        setPatternIdSelected(patternId);
        setPatternSelected(pattern);

        await handlePatternSequencesLoad(patternId);
    };

    const handlePatternSequencesLoad = (patternId: number) => {
        try {
            const sequences = store.devicePatternSequences.filter(x => x.devicePatternId === patternId); // Assuming store has fetchSequencesByPatternId
            setPatternSequences(sequences);
        } catch (error: any) {
            store.setError(`Failed to load pattern sequences: ${error.message}`);
        }
    };

    const handlePatternDelete = async () => {
        if (!patternSelected) return;

        try {
            await store.deleteDevicePattern(patternIdSelected); // Assuming store has deletePattern
            setPatternSelected(undefined);
            setPatternIdSelected(0);
            setPatternSequences([]);
            store.clearError();

            await getPatternsForSelectedDevice();
        } catch (error: any) {
            store.setError(`Failed to delete pattern: ${error.message}`);
        }
    };

    const handlePatternCreate = async () => {
        if (!deviceSelected) return;

        try {

            const devicePattern: DevicePatterns = {
                devicePatternId: 0,
                deviceId: deviceSelected.deviceId,
                patternName: "New Pattern"

            }

            const newPattern = await store.createDevicePattern(devicePattern);
            await store.fetchDevicePatterns();

            if (newPattern) {
                const patternCreated = store.devicePatterns.find(p => p.devicePatternId === newPattern.devicePatternId);
                setPatternSelected(patternCreated);
                if (patternCreated) setPatternIdSelected(patternCreated.devicePatternId);
                setPatternSequences([]);
                handlePatternSequencesLoad(newPattern.devicePatternId);
            }
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to create pattern: ${error.message}`);
        }
    };

    const handlePatternNameChange = async (pattern: DevicePatterns, value: string) => {
        try {
            const updatedPattern = { ...pattern, patternName: value };
            await store.updateDevicePattern(updatedPattern.devicePatternId, updatedPattern); // Assuming store has updatePattern

            setPatternSelected(updatedPattern);
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to update pattern name: ${error.message}`);
        }
    };

    const handleSequenceCreate = async () => {
        if (!patternSelected) return;

        try {
            // Find the next sequence's onAt
            let nextOnAt = 1000;
            if (patternSequences.length > 0) {
                const lastSequence = patternSequences.reduce((prev, current) => (prev.onAt ?? 0 > (current.onAt ?? 0) ? prev : current));
                const audio = store.audioOptions.find(a => a.audioId === lastSequence.audioId);
                const audioDuration = audio ? audio.audioDuration ?? 0 : 1000;
                nextOnAt = lastSequence.onAt ?? 0 + audioDuration + 1000;
            }

            const newSequence: DevicePatternSequences = {
                onAt: nextOnAt,
                devicePatternId: patternIdSelected,
                audioId: 1, // default audioId
                deviceIoPortId: 0, // default
                devicePatternSeqenceId: 0, // assuming backend assigns
                duration: 0, // default duration
            };

            const createdSequence = await store.createDevicePatternSequence(deviceIdSelected, newSequence); // Assuming store has createSequence
            if (createdSequence) {
                setPatternSequences(prevSequences => [...prevSequences, createdSequence]);
            }
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to create sequence: ${error.message}`);
        }
    };

    const handleCommandDelete = async (sequence: DevicePatternSequences) => {
        try {
            await store.deleteDevicePatternSequence(sequence.devicePatternSeqenceId); // Assuming store has deleteSequence
            setPatternSequences(prevSequences => prevSequences.filter(seq => seq.devicePatternSeqenceId !== sequence.devicePatternSeqenceId));
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to delete sequence: ${error.message}`);
        }
    };

    return (
        <div style={{ display: "flex", flexDirection: "column", height: "100vh", padding: "1rem" }}>
            {store.error && (
                <div style={{ color: "red", marginBottom: "1rem" }}>
                    {store.error}
                </div>
            )}

            <div style={{ display: "flex", flexDirection: "row", marginBottom: "1rem" }}>
                <FormControl style={{ minWidth: 120, marginRight: '1rem' }}>
                    <InputLabel id="devices-label">Devices</InputLabel>
                    <Select
                        labelId="devices-label"
                        id="devices-select"
                        value={deviceIdSelected}
                        onChange={(e) => handleDeviceChange(Number(e.target.value))}
                    >
                        {store.devices.map((device) => (
                            <MenuItem key={device.deviceId} value={device.deviceId}>
                                {device.deviceId}: {device.name}
                            </MenuItem>
                        ))}
                    </Select>
                </FormControl>

                {store.devicePatterns.length > 0 && (
                    <FormControl style={{ minWidth: 120, marginRight: '1rem' }}>
                        <InputLabel id="patterns-label">Patterns</InputLabel>
                        <Select
                            labelId="patterns-label"
                            id="patterns-select"
                            value={patternIdSelected}
                            onChange={(e) => handlePatternChange(Number(e.target.value))}
                        >
                            {store.devicePatterns.map((pattern) => (
                                <MenuItem key={pattern.devicePatternId} value={pattern.devicePatternId}>
                                    {pattern.patternName}
                                </MenuItem>
                            ))}
                        </Select>
                    </FormControl>
                )}

                {patternSelected && (
                    <>
                        <Tooltip title="Delete Pattern">
                            <IconButton onClick={handlePatternDelete}>
                                <DeleteIcon />
                            </IconButton>
                        </Tooltip>

                        <Tooltip title="Create New Pattern">
                            <IconButton onClick={handlePatternCreate}>
                                <AddIcon />
                            </IconButton>
                        </Tooltip>
                    </>
                )}
            </div>

            {patternSelected && (
                <div>
                    <div style={{ display: "flex", flexDirection: "column", marginBottom: '1rem' }}>
                        <div style={{ display: "flex", flexDirection: "row", alignItems: 'center', marginBottom: '1rem' }}>
                            <TextField
                                label="Pattern Name"
                                value={patternSelected.patternName}
                                onChange={(e: ChangeEvent<HTMLInputElement>) => handlePatternNameChange(patternSelected, e.target.value)}
                                margin="normal"
                                style={{ marginRight: '1rem', flex: 1 }}
                            />

                            <Tooltip title="Create New Event">
                                <IconButton onClick={handleSequenceCreate}>
                                    <AddIcon />
                                </IconButton>
                            </Tooltip>
                        </div>

                        <div style={{ display: "flex", flexDirection: "row", marginBottom: '0.5rem' }}>
                            <Typography variant="body2" style={{ flex: '1 1 75px' }}>
                                On At:
                            </Typography>
                            <Typography variant="body2" style={{ flex: '1 1 75px' }}>
                                Duration:
                            </Typography>
                            <Typography variant="body2" style={{ flex: '3 1 300px' }}>
                                Gpio Port:
                            </Typography>
                            <Typography variant="body2" style={{ flex: '3 1 300px' }}>
                                Audio File:
                            </Typography>
                            <Typography variant="body2" style={{ flex: '1 1 75px' }}>
                                Delete
                            </Typography>
                        </div>

                        {patternSequences.map((sequence) => (
                            <EditPattern
                                key={sequence.devicePatternSeqenceId}
                                sequence={sequence}
                                portOptions={ioPortOptions}
                                onDelete={handleCommandDelete}
                            />
                        ))}
                    </div>
                </div>
            )}
        </div>
    );
});

export default DevicePattern;
