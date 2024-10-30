import React, { useEffect, useState, ChangeEvent, useCallback, useRef } from 'react';
import { observer } from 'mobx-react-lite';
import { AppStoreContextItem } from '../Stores/AppStore';
import { Devices, DeviceIoPorts } from '../Clients/Api';
import { Button, Switch, TextField, Grid, Typography, Tooltip } from '@mui/material';
import FindReplaceIcon from '@mui/icons-material/FindReplace';
import { ApiStoreContextItem } from '../Stores/ApiStore';
import debounce from 'lodash/debounce';

interface DeviceIoPortEditorProps {
  device?: Devices;
}

interface PendingUpdates {
  [key: number]: DeviceIoPorts;
}

const DeviceIoPortEditor: React.FC<DeviceIoPortEditorProps> = observer(({ device }) => {
  const store = AppStoreContextItem.useStore();
  const apiStore = ApiStoreContextItem.useStore();
  const [ports, setPorts] = useState<DeviceIoPorts[]>([]);
  const pendingUpdatesRef = useRef<PendingUpdates>({});

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

  const processPendingUpdates = useCallback(async () => {
    const updates = pendingUpdatesRef.current;
    const updatePromises: Promise<void>[] = [];

    try {
      // Process all pending updates
      Object.entries(updates).forEach(([ioPortId, updatedPort]) => {
        updatePromises.push(
          store.updateDeviceIoPort(parseInt(ioPortId), updatedPort)
        );
      });

      await Promise.all(updatePromises);
      store.clearError();
      
      // Clear the pending updates after successful save
      pendingUpdatesRef.current = {};
    } catch (error: any) {
      store.setError(`Failed to update IO ports: ${error.message}`);
      // Keep the failed updates in the pending dictionary for retry
    }
  }, [store]);

  const debouncedProcessUpdates = useCallback(
    debounce(() => {
      processPendingUpdates();
    }, 2000),
    [processPendingUpdates]
  );

  const queuePortUpdate = useCallback((ioPortId: number, updatedPort: DeviceIoPorts) => {
    // Add or update the port in the pending updates dictionary
    pendingUpdatesRef.current[ioPortId] = updatedPort;
    
    // Trigger the debounced processing
    debouncedProcessUpdates();
  }, [debouncedProcessUpdates]);

  const handleIoPortDangerChange = (ioPortId: number, checked: boolean) => {
    const updatedPort = ports.find(port => port.deviceIoPortId === ioPortId);
    if (!updatedPort) return;

    const newPort = { ...updatedPort, isDanger: checked };
    setPorts(prevPorts => prevPorts.map(port => 
      port.deviceIoPortId === ioPortId ? newPort : port
    ));

    queuePortUpdate(ioPortId, newPort);
  };

  const handleIoPortNameChange = (ioPortId: number, value: string) => {
    const updatedPort = ports.find(port => port.deviceIoPortId === ioPortId);
    if (!updatedPort) return;

    const newPort = { ...updatedPort, description: value };
    setPorts(prevPorts => prevPorts.map(port => 
      port.deviceIoPortId === ioPortId ? newPort : port
    ));

    queuePortUpdate(ioPortId, newPort);
  };

  const handleIoPortDetect = async (ioPortId: number) => {
    try {
      await apiStore.getApi().deviceIoPortsPutDeviceIoPortIdentifyUpdate(ioPortId);
      store.clearError();
    } catch (error: any) {
      store.setError(`Failed to detect IO port: ${error.message}`);
    }
  };

  // Clean up debounced function on unmount
  useEffect(() => {
    return () => {
      debouncedProcessUpdates.cancel();
      
      // Process any remaining updates before unmounting
      if (Object.keys(pendingUpdatesRef.current).length > 0) {
        processPendingUpdates();
      }
    };
  }, [debouncedProcessUpdates, processPendingUpdates]);

  return (
    <div style={{ padding: '0.5rem' }}>
      {store.error && (
        <Typography color="error" style={{ marginBottom: '0.5rem' }}>
          {store.error}
        </Typography>
      )}
      {store.isLoadingDeviceIoPorts ? (
        <Typography align="center" style={{ paddingTop: '0.5rem' }}>
          Loading IO Ports...
        </Typography>
      ) : (
        <Grid container direction="column" spacing={0.5}>
          {ports.length === 0 ? (
            <Typography>No IO Ports Available</Typography>
          ) : (
            ports.map((ioPort) => (
              <Grid item key={ioPort.deviceIoPortId}>
                <Grid container alignItems="center" spacing={1}>
                  <Grid item xs={1}>
                    <Typography variant="body2">PIN {ioPort.commandPin}</Typography>
                  </Grid>
                  <Grid item xs={6}>
                    <TextField
                      label="Description"
                      value={ioPort.description || ''}
                      onChange={(e: ChangeEvent<HTMLInputElement>) => 
                        handleIoPortNameChange(ioPort.deviceIoPortId, e.target.value)
                      }
                      fullWidth
                      margin="dense"
                    />
                  </Grid>
                  <Grid item xs={2}>
                    <Tooltip title="Danger Pin">
                    <Switch
                      checked={ioPort.isDanger}
                      onChange={(e: ChangeEvent<HTMLInputElement>) => 
                        handleIoPortDangerChange(ioPort.deviceIoPortId, e.target.checked)
                      }
                      color="primary"
                      size="small"
                    />
                    </Tooltip>
                  </Grid>
                  <Grid item xs={3}>
                    <Button
                      onClick={() => handleIoPortDetect(ioPort.deviceIoPortId)}
                      variant="contained"
                      color="primary"
                      size="small"
                      startIcon={<FindReplaceIcon />}
                    >
                      Detect
                    </Button>
                  </Grid>
                </Grid>
              </Grid>
            ))
          )}
        </Grid>
      )}
    </div>
  );
});

export default DeviceIoPortEditor;