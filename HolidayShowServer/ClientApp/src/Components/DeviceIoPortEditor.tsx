// src/components/DeviceIoPortEditor.tsx

import React, { useEffect, useState, ChangeEvent } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import { Devices, DeviceIoPorts } from '../Clients/Api';
import { Button, Switch, TextField } from '@mui/material';
import FindReplaceIcon from '@mui/icons-material/FindReplace';
import { ApiStoreContextItem } from '../Stores/ApiStore';

interface DeviceIoPortEditorProps {
  device?: Devices;
}

const DeviceIoPortEditor: React.FC<DeviceIoPortEditorProps> = observer(({ device }) => {
  const store = AppStoreContextItem.useStore();
  const apiStore = ApiStoreContextItem.useStore();
  const [ports, setPorts] = useState<DeviceIoPorts[]>([]);

  useEffect(() => {
    const fetchPorts = async () => {
      if (!device) {
        setPorts([]);
        return;
      }

      try {
        store.setLoadingDeviceIoPorts(true);
        const allPorts = await apiStore.getApi().deviceIoPortsByDeviceIdDetail(device.deviceId);
        const filteredPorts = allPorts.filter(port => port.commandPin !== -1);
        setPorts(filteredPorts);
        store.clearError();
      } catch (error: any) {
        store.setError(`Failed to fetch device IO ports: ${error.message}`);
      } finally {
        store.setLoadingDeviceIoPorts(false);
      }
    };

    fetchPorts();
  }, [device, store, apiStore]);


  const handleIoPortDangerChange = async (ioPortId: number, checked: boolean) => {
    try {
      const updatedPort = ports.find(port => port.deviceIoPortId === ioPortId);
      if (!updatedPort) return;

      const newPort = { ...updatedPort, isDanger: checked };
      setPorts(prevPorts => prevPorts.map(port => port.deviceIoPortId === ioPortId ? newPort : port));

      await store.updateDeviceIoPort(ioPortId, newPort);
      store.clearError();
    } catch (error: any) {
      store.setError(`Failed to update IO port danger status: ${error.message}`);
    }
  };

  const handleIoPortNameChange = async (ioPortId: number, value: string) => {
    try {
      const updatedPort = ports.find(port => port.deviceIoPortId === ioPortId);
      if (!updatedPort) return;

      const newPort = { ...updatedPort, description: value };
      setPorts(prevPorts => prevPorts.map(port => port.deviceIoPortId === ioPortId ? newPort : port));

      await store.updateDeviceIoPort(ioPortId, newPort);
      store.clearError();
    } catch (error: any) {
      store.setError(`Failed to update IO port name: ${error.message}`);
    }
  };

  const handleIoPortDetect = async (ioPortId: number) => {
    try {
      await apiStore.getApi().deviceIoPortsPutDeviceIoPortIdentifyUpdate(ioPortId);
      store.clearError();
    } catch (error: any) {
      store.setError(`Failed to detect IO port: ${error.message}`);
    }
  };

  return (
    <div style={{ padding: '1rem' }}>
      {store.error && (
        <div style={{ color: 'red', marginBottom: '1rem' }}>
          {store.error}
        </div>
      )}
      {store.isLoadingDeviceIoPorts ? (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '100%', paddingTop: '1rem' }}>
          <span>Loading IO Ports...</span>
        </div>
      ) : (
        <div style={{ display: 'flex', flexDirection: 'column' }}>
          {ports.length === 0 ? (
            <div>No IO Ports Available</div>
          ) : (
            ports.map((ioPort) => (
              <div
                key={ioPort.deviceIoPortId}
                style={{
                  display: 'flex',
                  flexDirection: 'row',
                  alignItems: 'center',
                  marginBottom: 'rem',
                }}
              >
                <TextField
                  label={`PIN: ${ioPort.commandPin}`}
                  value={ioPort.description || ''}
                  onChange={(e: ChangeEvent<HTMLInputElement>) => handleIoPortNameChange(ioPort.deviceIoPortId, e.target.value)}
                  margin="normal"
                  style={{ marginRight: '1rem', flex: 1 }}
                />
                <Switch
                  checked={ioPort.isDanger}
                  onChange={(e: ChangeEvent<HTMLInputElement>) => handleIoPortDangerChange(ioPort.deviceIoPortId, e.target.checked)}
                  color="primary"
                  style={{ marginRight: '1rem' }}
                />
                <Button
                  onClick={() => handleIoPortDetect(ioPort.deviceIoPortId)}
                  variant="contained"
                    color="primary"
                  startIcon={<FindReplaceIcon />}
                >
                  Detect
                </Button>
              </div>
            ))
          )}
        </div>
      )}
    </div>
  );
});

export default DeviceIoPortEditor;
