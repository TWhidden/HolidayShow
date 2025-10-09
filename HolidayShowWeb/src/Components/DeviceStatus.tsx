// src/Components/DeviceStatus.tsx

import React, { useEffect, useState } from 'react';
import { observer } from 'mobx-react';
import { Box, Typography, Card, CardContent, Chip, Grid, Alert } from '@mui/material';
import { Wifi, WifiOff, SettingsInputHdmi, Error, Sync } from '@mui/icons-material';
import { AppStoreContextItem } from '../Stores/AppStore';
import { signalRService, DeviceStatusInfo } from '../Services/SignalRService';

const DeviceStatus: React.FC = observer(() => {
  const appStore = AppStoreContextItem.useStore();
  const [connectionStatus, setConnectionStatus] = useState<string>('Disconnected');

  useEffect(() => {
    // Start SignalR connection
    signalRService.startConnection().catch(err => {
      console.error('Failed to start SignalR connection:', err);
    });

    // Subscribe to device status updates
    const handleDeviceStatusUpdate = (status: DeviceStatusInfo) => {
      appStore.updateDeviceStatus(status);
    };

    // Subscribe to connection status updates
    const handleConnectionStatusChange = (status: string) => {
      setConnectionStatus(status);
    };

    signalRService.onDeviceStatusUpdate(handleDeviceStatusUpdate);
    signalRService.onConnectionStatusChange(handleConnectionStatusChange);

    // Cleanup on unmount
    return () => {
      signalRService.offDeviceStatusUpdate(handleDeviceStatusUpdate);
      signalRService.offConnectionStatusChange(handleConnectionStatusChange);
      signalRService.stopConnection();
    };
  }, [appStore]);

  const formatDate = (date?: Date) => {
    if (!date) return 'Never';
    return date.toLocaleString();
  };

  const getStatusColor = (isConnected: boolean) => {
    return isConnected ? 'success' : 'error';
  };

  const getStatusIcon = (isConnected: boolean) => {
    return isConnected ? <Wifi /> : <WifiOff />;
  };

  const getConnectionStatusIcon = () => {
    switch (connectionStatus) {
      case 'Connected':
        return <Wifi color="success" />;
      case 'Reconnecting':
        return <Sync color="warning" />;
      case 'Failed':
        return <Error color="error" />;
      default:
        return <WifiOff color="error" />;
    }
  };

  const getConnectionStatusColor = () => {
    switch (connectionStatus) {
      case 'Connected':
        return 'success';
      case 'Reconnecting':
        return 'warning';
      case 'Failed':
        return 'error';
      default:
        return 'error';
    }
  };

  return (
    <Box>
      <Box display="flex" alignItems="center" justifyContent="space-between" mb={2}>
        <Typography variant="h6">
          Device Status
        </Typography>
        <Chip
          icon={getConnectionStatusIcon()}
          label={`SignalR: ${connectionStatus}`}
          color={getConnectionStatusColor()}
          size="small"
        />
      </Box>

      {connectionStatus !== 'Connected' && (
        <Alert severity="warning" sx={{ mb: 2 }}>
          Real-time device status updates are not available. Attempting to reconnect...
        </Alert>
      )}

      {appStore.deviceStatuses.length === 0 ? (
        <Typography variant="body2" color="textSecondary">
          No devices connected yet. Devices will appear here when they connect.
        </Typography>
      ) : (
        <Grid container spacing={2}>
          {appStore.deviceStatuses.map((device) => (
            <Grid item xs={12} sm={6} md={6} key={device?.deviceId || Math.random()}>
              <Card
                variant="outlined"
                sx={{
                  opacity: device?.isConnected ? 1 : 0.6,
                  transition: 'opacity 0.3s ease',
                  minHeight: 180,
                }}
              >
                <CardContent>
                  <Box display="flex" alignItems="center" mb={1}>
                    <SettingsInputHdmi sx={{ mr: 1 }} />
                    <Typography variant="h6" component="div">
                      {device?.deviceName || 'Unknown Device'}
                    </Typography>
                  </Box>

                  <Box display="flex" alignItems="center" mb={1}>
                    <Chip
                      icon={getStatusIcon(device?.isConnected || false)}
                      label={device?.isConnected ? 'Connected' : 'Disconnected'}
                      color={getStatusColor(device?.isConnected || false)}
                      size="small"
                      sx={{ mr: 1 }}
                    />
                    {device?.isConnected && device?.ipAddress && (
                      <Typography variant="body2" color="textSecondary">
                        {device.ipAddress}
                      </Typography>
                    )}
                  </Box>

                  <Box mb={1}>
                    <Typography variant="body2" color="textSecondary">
                      Instructions Sent: {(device?.instructionsSent || 0).toLocaleString()}
                    </Typography>
                  </Box>

                  <Box>
                    <Typography variant="caption" color="textSecondary">
                      Connected: {formatDate(device?.connectedAt)}
                    </Typography>
                    <br />
                    <Typography variant="caption" color="textSecondary">
                      Last Seen: {formatDate(device?.disconnectedAt || device?.connectedAt)}
                    </Typography>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      )}
    </Box>
  );
});

export default DeviceStatus;