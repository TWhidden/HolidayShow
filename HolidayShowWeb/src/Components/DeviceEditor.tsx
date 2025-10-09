// src/components/DeviceManager.tsx

import React, { useState, useEffect, ChangeEvent, useCallback } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import { Devices } from '../Clients/Api';
import DeviceIoPortEditor from './DeviceIoPortEditor';
import {
  Box,
  Select,
  MenuItem,
  TextField,
  CircularProgress,
  Alert,
  InputLabel,
  FormControl,
  SelectChangeEvent,
} from '@mui/material';
import debounce from 'lodash/debounce';

const sessionDeviceSelected = "DeviceEdit-DeviceSelected";

const DeviceManager: React.FC = observer(() => {
  const store = AppStoreContextItem.useStore();

  const [deviceIdSelected, setDeviceIdSelected] = useState<number>(0);
  const [deviceSelected, setDeviceSelected] = useState<Devices | undefined>(undefined);
  const [nameInput, setNameInput] = useState<string>('');

  useEffect(() => {
    const initializeSelection = () => {
      if (store.devices.length === 0) return;

      let selectedDevice: Devices | undefined = store.devices[0];

      const lastSelectedId = sessionStorage.getItem(sessionDeviceSelected);
      if (lastSelectedId !== null) {
        const id = Number(lastSelectedId);
        selectedDevice = store.devices.find(device => device.deviceId === id) || selectedDevice;
      }

      if (selectedDevice) {
        setDeviceIdSelected(selectedDevice.deviceId);
        setDeviceSelected(selectedDevice);
        setNameInput(selectedDevice.name || '');
      }
    };

    initializeSelection();
  }, [store.devices]);

  const handleDeviceChange = (event: SelectChangeEvent<number>) => {
    const id = Number(event.target.value);  
    const selected = store.devices.find(device => device.deviceId === id);

    if (selected) {
      sessionStorage.setItem(sessionDeviceSelected, selected.deviceId.toString());
      setDeviceIdSelected(selected.deviceId);
      setDeviceSelected(selected);
      setNameInput(selected.name || '');
    } else {
      sessionStorage.removeItem(sessionDeviceSelected);
      setDeviceIdSelected(0);
      setDeviceSelected(undefined);
      setNameInput('');
    }
  };

  const debouncedSave = useCallback(
    debounce((updatedDevice: Devices) => {
      store.updateDevice(updatedDevice.deviceId, updatedDevice);
    }, 2000),
    [store]
  );

  const handleNameChange = (event: ChangeEvent<HTMLInputElement>) => {
    const newName = event.target.value;
    setNameInput(newName);

    if (deviceSelected) {
      const updatedDevice = { ...deviceSelected, name: newName };
      debouncedSave(updatedDevice);
    }
  };

  if (store.isLoadingDevices) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" height="100%" pt={6}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box display="flex" flexDirection="column" p={2}>
      {store.error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {store.error}
        </Alert>
      )}

      <Box display="flex" flexDirection={{ xs: 'column', sm: 'row' }} mb={2} gap={2}>
        <FormControl fullWidth variant="outlined">
          <InputLabel id="device-select-label">Select Device</InputLabel>
          <Select
            labelId="device-select-label"
            value={deviceIdSelected}
            onChange={handleDeviceChange}
            label="Select Device"
          >
            <MenuItem value={0}>
              <em>Select Device</em>
            </MenuItem>
            {store.devices.map((device) => (
              <MenuItem key={device.deviceId} value={device.deviceId}>
                {device.deviceId}: {device.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        {deviceSelected && (
          <TextField
            fullWidth
            label="Device Name"
            variant="outlined"
            value={nameInput}
            onChange={handleNameChange}
          />
        )}
      </Box>

      {deviceSelected && (
        <Box overflow="auto">
          <DeviceIoPortEditor device={deviceSelected} />
        </Box>
      )}
    </Box>
  );
});

export default DeviceManager;
