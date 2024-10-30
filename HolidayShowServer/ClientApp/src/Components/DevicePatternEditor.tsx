// src/components/DevicePattern.tsx

import React, { ChangeEvent, useEffect, useState, useCallback, useRef } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import { Devices, DevicePatterns, DevicePatternSequences } from '../Clients/Api';
import {
    Select,
    InputLabel,
    MenuItem,
    FormControl,
    TextField,
    IconButton,
    Tooltip,
    Typography,
    CircularProgress
} from '@mui/material';
import { Delete as DeleteIcon, Add as AddIcon } from '@mui/icons-material';
import EditPattern from './EditPattern';
import debounce from 'lodash/debounce';

const sessionLastDeviceSelected = "PatternEdit-selectedDevice";
const sessionLastPatternSelected = "PatternEdit-selectedPattern";

interface PendingPatternUpdates {
    [key: number]: DevicePatterns;
}

const DevicePattern: React.FC = observer(() => {
    const store = AppStoreContextItem.useStore();
    const pendingPatternUpdatesRef = useRef<PendingPatternUpdates>({});

    // State variables
    const [patternSequences, setPatternSequences] = useState<DevicePatternSequences[]>([]);
    const [ioPortOptions, setIoPortOptions] = useState<{ label: string; value: number }[]>([]);

    const [deviceSelected, setDeviceSelected] = useState<Devices | undefined>(undefined);
    const [deviceIdSelected, setDeviceIdSelected] = useState<number>(0);
    const [patternSelected, setPatternSelected] = useState<DevicePatterns | undefined>(undefined);
    const [patternIdSelected, setPatternIdSelected] = useState<number>(0);

    // Flags to prevent re-running selection logic multiple times
    const devicesLoadedRef = useRef<boolean>(false);
    const patternsLoadedRef = useRef<boolean>(false);
    const sequencesLoadedRef = useRef<boolean>(false);
    const ioPortsLoadedRef = useRef<boolean>(false);
    const audioOptionsLoadedRef = useRef<boolean>(false);
    
    // Loading states
    const [isLoadingDevices, setIsLoadingDevices] = useState<boolean>(false);
    const [isLoadingPatterns, setIsLoadingPatterns] = useState<boolean>(false);
    const [isLoadingSequences, setIsLoadingSequences] = useState<boolean>(false);
    
    // Process pending pattern updates
    const processPendingPatternUpdates = useCallback(async () => {
        const updates = pendingPatternUpdatesRef.current;
        const updatePromises: Promise<void>[] = [];

        try {
            Object.entries(updates).forEach(([patternId, pattern]) => {
                updatePromises.push(
                    store.updateDevicePattern(parseInt(patternId), pattern)
                );
            });

            await Promise.all(updatePromises);
            store.clearError();
            
            // Clear pending updates after successful save
            pendingPatternUpdatesRef.current = {};
        } catch (error: any) {
            store.setError(`Failed to update patterns: ${error.message}`);
            // Failed updates remain in the dictionary for retry
        }
    }, [store]);

    // Create debounced version of the update processor
    const debouncedProcessUpdates = useCallback(
        debounce(() => {
            processPendingPatternUpdates();
        }, 2000),
        [processPendingPatternUpdates]
    );

    // Queue pattern update
    const queuePatternUpdate = useCallback((patternId: number, updatedPattern: DevicePatterns) => {
        pendingPatternUpdatesRef.current[patternId] = updatedPattern;
        debouncedProcessUpdates();
    }, [debouncedProcessUpdates]);

    // Cleanup on unmount
    useEffect(() => {
        return () => {
            debouncedProcessUpdates.cancel();
            
            // Process any remaining updates
            if (Object.keys(pendingPatternUpdatesRef.current).length > 0) {
                processPendingPatternUpdates();
            }
        };
    }, [debouncedProcessUpdates, processPendingPatternUpdates]);

    /**
     * useEffect to monitor changes in store.devices
     * This will handle the case where devices are loaded after initial render
     */
    useEffect(() => {
        // If devices have been loaded and haven't been processed yet
        if (!devicesLoadedRef.current && store.devices.length > 0) {
            devicesLoadedRef.current = true; // Set the flag to prevent re-processing

            // Re-run the device selection logic
            getDevices();
        }
    }, [store.devices]); // Dependency on store.devices

    /**
     * useEffect to monitor changes in store.devicePatterns
     * This handles pattern selection once patterns are loaded
     */
    useEffect(() => {
        // If devicePatterns have been loaded and haven't been processed yet
        if (!patternsLoadedRef.current && store.devicePatterns.length > 0) {
            patternsLoadedRef.current = true; // Set the flag to prevent re-processing

            // Re-run the pattern selection logic
            getPatternsForSelectedDevice();
        }
    }, [store.devicePatterns]); // Dependency on store.devicePatterns

    /**
     * useEffect to monitor changes in store.devicePatternSequences
     * This handles loading pattern sequences once they are loaded
     */
    useEffect(() => {
        // If devicePatternSequences have been loaded and haven't been processed yet
        if (!sequencesLoadedRef.current && store.devicePatternSequences.length > 0 && patternIdSelected !== 0) {
            sequencesLoadedRef.current = true; // Set the flag to prevent re-processing

            // Re-run the pattern sequences loading logic
            handlePatternSequencesLoad(patternIdSelected);
        }
    }, [store.devicePatternSequences, patternIdSelected]); // Dependencies on devicePatternSequences and selected pattern

    /**
     * useEffect to monitor changes in store.deviceIoPorts
     * This handles loading IO port options once they are loaded
     */
    useEffect(() => {
        // If deviceIoPorts have been loaded and haven't been processed yet
        if (!ioPortsLoadedRef.current && store.deviceIoPorts.length > 0 && deviceSelected) {
            ioPortsLoadedRef.current = true; // Set the flag to prevent re-processing

            // Re-run the IO ports loading logic
            getIoPortsForSelectedDevice(deviceSelected);
        }
    }, [store.deviceIoPorts, deviceSelected]); // Dependencies on deviceIoPorts and selected device

    /**
     * useEffect to monitor changes in store.audioOptions
     * This handles updating dependent components once audioOptions are loaded
     */
    useEffect(() => {
        if (!audioOptionsLoadedRef.current && store.audioOptions.length > 0) {
            audioOptionsLoadedRef.current = true; // Set the flag to prevent re-processing

            // Re-render or perform any necessary actions now that audioOptions are loaded
            // For example, force re-render or update state if needed
            // In this case, EditPattern component will reactively update based on store.audioOptions
        }
    }, [store.audioOptions]); // Dependency on audioOptions

    /**
     * Device selection logic
     */
    const getDevices = async () => {
        setIsLoadingDevices(true);
        try {
            const devices = store.devices;
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

                await handleDeviceChange(selectedDevice.deviceId);
            }

            setIsLoadingDevices(false);
        } catch (error: any) {
            store.setError(`Failed to get devices: ${error.message}`);
            setIsLoadingDevices(false);
        }
    };

    const handleDeviceChange = async (deviceId: number) => {
        const device = store.devices.find(d => d.deviceId === deviceId);
        if (!device) return;

        sessionStorage.setItem(sessionLastDeviceSelected, deviceId.toString());
        setDeviceIdSelected(deviceId);
        setDeviceSelected(device);

        getIoPortsForSelectedDevice(device);
    };

    const getPatternsForSelectedDevice = async () => {
        setIsLoadingPatterns(true);
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

            // If patterns have been loaded and selection hasn't been set yet
            if (patternId !== 0) {
                await handlePatternChange(patternId);
            }
        } catch (error: any) {
            store.setError(`Failed to get patterns: ${error.message}`);
        }
        setIsLoadingPatterns(false);
    };

    const getIoPortsForSelectedDevice = (device: Devices) => {
        try {
            const ports = store.deviceIoPorts.filter(x => x.deviceId === device.deviceId);
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

        sessionStorage.setItem(sessionLastPatternSelected, patternId.toString());
        setPatternIdSelected(patternId);
        setPatternSelected(pattern);

        await handlePatternSequencesLoad(patternId);
    };

    const handlePatternSequencesLoad = (patternId: number) => {
        setIsLoadingSequences(true);
        try {
            const sequences = store.devicePatternSequences.filter(x => x.devicePatternId === patternId);
            setPatternSequences(sequences);
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to load pattern sequences: ${error.message}`);
        }
        setIsLoadingSequences(false);
    };

    const handlePatternDelete = async () => {
        if (!patternSelected) return;

        try {
            // Cancel any pending updates for this pattern
            if (pendingPatternUpdatesRef.current[patternIdSelected]) {
                delete pendingPatternUpdatesRef.current[patternIdSelected];
            }
            
            await store.deleteDevicePattern(patternIdSelected);
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
            };

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

    const handlePatternNameChange = (pattern: DevicePatterns, value: string) => {
        const updatedPattern = { ...pattern, patternName: value };
        setPatternSelected(updatedPattern);
        queuePatternUpdate(updatedPattern.devicePatternId, updatedPattern);
    };

    const handleSequenceCreate = async () => {
        if (!patternSelected) return;

        try {
            let nextOnAt = 1000;
            if (patternSequences.length > 0) {
                const lastSequence = patternSequences.reduce((prev, current) => 
                    (prev.onAt ?? 0 > (current.onAt ?? 0) ? prev : current));
                const audio = store.audioOptions.find(a => a.audioId === lastSequence.audioId);
                const audioDuration = audio ? audio.audioDuration ?? 0 : 1000;
                nextOnAt = (lastSequence.onAt ?? 0) + audioDuration + 1000;
            }

            const newSequence: DevicePatternSequences = {
                onAt: nextOnAt,
                devicePatternId: patternIdSelected,
                audioId: 1,
                deviceIoPortId: 0,
                devicePatternSeqenceId: 0,
                duration: 0,
            };

            const createdSequence = await store.createDevicePatternSequence(deviceIdSelected, newSequence);
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
            await store.deleteDevicePatternSequence(sequence.devicePatternSeqenceId);
            setPatternSequences(prevSequences => 
                prevSequences.filter(seq => seq.devicePatternSeqenceId !== sequence.devicePatternSeqenceId)
            );
            store.clearError();
        } catch (error: any) {
            store.setError(`Failed to delete sequence: ${error.message}`);
        }
    };

    /**
     * useEffect to monitor changes in store.devicePatternSequences
     * This handles loading pattern sequences once they are loaded
     */
    useEffect(() => {
        // If devicePatternSequences have been loaded and haven't been processed yet
        if (!sequencesLoadedRef.current && store.devicePatternSequences.length > 0 && patternIdSelected !== 0) {
            sequencesLoadedRef.current = true; // Set the flag to prevent re-processing

            // Re-run the pattern sequences loading logic
            handlePatternSequencesLoad(patternIdSelected);
        }
    }, [store.devicePatternSequences, patternIdSelected]); // Dependencies on devicePatternSequences and selected pattern

    /**
     * useEffect to monitor changes in store.deviceIoPorts
     * This handles loading IO port options once they are loaded
     */
    useEffect(() => {
        // If deviceIoPorts have been loaded and haven't been processed yet
        if (!ioPortsLoadedRef.current && store.deviceIoPorts.length > 0 && deviceSelected) {
            ioPortsLoadedRef.current = true; // Set the flag to prevent re-processing

            // Re-run the IO ports loading logic
            getIoPortsForSelectedDevice(deviceSelected);
        }
    }, [store.deviceIoPorts, deviceSelected]); // Dependencies on deviceIoPorts and selected device

    /**
     * useEffect to monitor changes in store.audioOptions
     * This handles updating dependent components once audioOptions are loaded
     */
    useEffect(() => {
        if (!audioOptionsLoadedRef.current && store.audioOptions.length > 0) {
            audioOptionsLoadedRef.current = true; // Set the flag to prevent re-processing

            // Re-render or perform any necessary actions now that audioOptions are loaded
            // For example, force re-render or update state if needed
            // In this case, EditPattern component will reactively update based on store.audioOptions
        }
    }, [store.audioOptions]); // Dependency on audioOptions

    return (
        <div style={{ display: "flex", flexDirection: "column", height: "100vh", padding: "1rem" }}>
            {store.error && (
                <div style={{ color: "red", marginBottom: "1rem" }}>
                    {store.error}
                </div>
            )}

            <div style={{ display: "flex", flexDirection: "row", marginBottom: "1rem" }}>
                <FormControl style={{ minWidth: 120, marginRight: '1rem', position: 'relative' }}>
                    <InputLabel id="devices-label">Devices</InputLabel>
                    <Select
                        labelId="devices-label"
                        id="devices-select"
                        value={deviceIdSelected}
                        onChange={(e) => handleDeviceChange(Number(e.target.value))}
                        disabled={store.devices.length === 0 || isLoadingDevices}
                    >
                        {store.devices.map((device) => (
                            <MenuItem key={device.deviceId} value={device.deviceId}>
                                {device.deviceId}: {device.name}
                            </MenuItem>
                        ))}
                    </Select>
                    {isLoadingDevices && <CircularProgress size={24} style={{ position: 'absolute', top: '50%', left: '50%', marginTop: -12, marginLeft: -12 }} />}
                </FormControl>

                {store.devicePatterns.length > 0 && (
                    <FormControl style={{ minWidth: 120, marginRight: '1rem', position: 'relative' }}>
                        <InputLabel id="patterns-label">Patterns</InputLabel>
                        <Select
                            labelId="patterns-label"
                            id="patterns-select"
                            value={patternIdSelected}
                            onChange={(e) => handlePatternChange(Number(e.target.value))}
                            disabled={store.devicePatterns.length === 0 || isLoadingPatterns}
                        >
                            {store.devicePatterns.map((pattern) => (
                                <MenuItem key={pattern.devicePatternId} value={pattern.devicePatternId}>
                                    {pattern.patternName}
                                </MenuItem>
                            ))}
                        </Select>
                        {isLoadingPatterns && <CircularProgress size={24} style={{ position: 'absolute', top: '50%', left: '50%', marginTop: -12, marginLeft: -12 }} />}
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
                                disabled={isLoadingPatterns}
                            />

                            <Tooltip title="Create New Event">
                                <IconButton onClick={handleSequenceCreate} disabled={isLoadingPatterns}>
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

                        {isLoadingSequences ? (
                            <CircularProgress />
                        ) : (
                            patternSequences.map((sequence) => (
                                <EditPattern
                                    key={sequence.devicePatternSeqenceId}
                                    sequence={sequence}
                                    portOptions={ioPortOptions}
                                    onDelete={handleCommandDelete}
                                />
                            ))
                        )}
                    </div>
                </div>
            )}
        </div>
    )});

    export default DevicePattern;
