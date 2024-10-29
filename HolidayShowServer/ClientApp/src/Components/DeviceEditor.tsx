// src/components/DeviceManager.tsx

import React, { useState, useEffect, ChangeEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import { Devices } from '../Clients/Api';
import DeviceIoPortEditor from './DeviceIoPortEditor';

const sessionDeviceSelected = "DeviceEdit-DeviceSelected";

const DeviceManager: React.FC = observer(() => {
  const store = AppStoreContextItem.useStore();

  const [deviceIdSelected, setDeviceIdSelected] = useState<number>(0);
  const [deviceSelected, setDeviceSelected] = useState<Devices | undefined>(undefined);

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
      }
    };

    initializeSelection();
  }, [store.devices]);

  const handleDeviceChange = (event: ChangeEvent<HTMLSelectElement>) => {
    const id = Number(event.target.value);
    const selected = store.devices.find(device => device.deviceId === id);

    if (selected) {
      sessionStorage.setItem(sessionDeviceSelected, selected.deviceId.toString());
      setDeviceIdSelected(selected.deviceId);
      setDeviceSelected(selected);
    } else {
      sessionStorage.removeItem(sessionDeviceSelected);
      setDeviceIdSelected(0);
      setDeviceSelected(undefined);
    }
  };

  const handleNameChange = (event: ChangeEvent<HTMLInputElement>) => {
    const newName = event.target.value;
    if (deviceSelected) {
      const updatedDevice: Devices = { ...deviceSelected, name: newName };
      setDeviceSelected(updatedDevice);
      store.updateDevice(updatedDevice.deviceId, updatedDevice);
    }
  };

  if (store.isLoadingDevices) {
    return (
      <div style={{ display: "flex", justifyContent: "center", alignItems: "center", height: "100%", paddingTop: "3rem" }}>
        <span>Loading...</span>
      </div>
    );
  }

  return (
    <div style={{ display: "flex", flexDirection: "column", padding: "1rem" }}>
      {store.error && (
        <div style={{ color: "red", marginBottom: "1rem" }}>
          {store.error}
        </div>
      )}

      <div style={{ display: "flex", flexDirection: "row", marginBottom: "1rem" }}>
        <select value={deviceIdSelected} onChange={handleDeviceChange} style={{ marginRight: "1rem", padding: "0.5rem" }}>
          <option value={0}>Select Device</option>
          {store.devices.map((device) => (
            <option key={device.deviceId} value={device.deviceId}>
              {device.deviceId}: {device.name}
            </option>
          ))}
        </select>

        {deviceSelected && (
          <input
            type="text"
            value={deviceSelected.name || ''}
            onChange={handleNameChange}
            placeholder="Device Name"
            style={{ padding: "0.5rem", flex: 1 }}
          />
        )}
      </div>

      {deviceSelected && (
        <div style={{ overflow: "auto" }}>
          <DeviceIoPortEditor device={deviceSelected} />
        </div>
      )}
    </div>
  );
});

export default DeviceManager;
