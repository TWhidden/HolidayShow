import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr';

export interface DeviceStatusInfo {
  deviceId: number;
  deviceName: string;
  ipAddress: string;
  isConnected: boolean;
  connectedAt?: Date;
  disconnectedAt?: Date;
  instructionsSent?: number;
}

export class SignalRService {
  private connection: HubConnection | null = null;
  private deviceStatusCallbacks: ((status: DeviceStatusInfo) => void)[] = [];
  private connectionStatusCallbacks: ((status: string) => void)[] = [];
  private reconnectTimer: ReturnType<typeof setTimeout> | null = null;

  async startConnection(): Promise<void> {
    try {
      this.connection = new HubConnectionBuilder()
        .withUrl('/websocket/deviceStatusHub')
        .withAutomaticReconnect({
          nextRetryDelayInMilliseconds: (retryContext) => {
            this.notifyConnectionStatus('Reconnecting');
            // Exponential backoff: 0s, 2s, 10s, 30s, then 60s
            const delays = [0, 2000, 10000, 30000];
            const delay = delays[Math.min(retryContext.previousRetryCount, delays.length - 1)] || 60000;
            console.log(`SignalR reconnect attempt ${retryContext.previousRetryCount + 1} in ${delay}ms`);
            return delay;
          }
        })
        .configureLogging(LogLevel.Information)
        .build();

      // Handle connection state changes
      this.connection.onclose(() => {
        this.notifyConnectionStatus('Disconnected');
        this.scheduleReconnect();
      });

      this.connection.onreconnecting(() => {
        this.notifyConnectionStatus('Reconnecting');
      });

      this.connection.onreconnected(() => {
        this.notifyConnectionStatus('Connected');
        if (this.reconnectTimer) {
          clearTimeout(this.reconnectTimer);
          this.reconnectTimer = null;
        }
      });

      this.connection.on('DeviceStatusUpdate', (status: DeviceStatusInfo | DeviceStatusInfo[]) => {
        // Handle both single status updates and initial status list
        const processStatus = (deviceStatus: DeviceStatusInfo) => {
          // Ensure all required properties are present
          if (!deviceStatus || typeof deviceStatus.deviceId !== 'number') {
            console.warn('Received invalid device status:', deviceStatus);
            return null;
          }

          // Convert date strings to Date objects
          if (deviceStatus.connectedAt) {
            deviceStatus.connectedAt = new Date(deviceStatus.connectedAt);
          }
          if (deviceStatus.disconnectedAt) {
            deviceStatus.disconnectedAt = new Date(deviceStatus.disconnectedAt);
          }
          return deviceStatus;
        };

        if (Array.isArray(status)) {
          // Initial status list - update all at once
          const validStatuses = status
            .map(processStatus)
            .filter((s): s is DeviceStatusInfo => s !== null);
          validStatuses.forEach(deviceStatus => {
            this.deviceStatusCallbacks.forEach(callback => callback(deviceStatus));
          });
        } else {
          // Single status update
          const processedStatus = processStatus(status);
          if (processedStatus) {
            this.deviceStatusCallbacks.forEach(callback => callback(processedStatus));
          }
        }
      });

      await this.connection.start();
      this.notifyConnectionStatus('Connected');
      console.log('SignalR connection started');
    } catch (err) {
      console.error('Error starting SignalR connection:', err);
      this.notifyConnectionStatus('Failed');
      this.scheduleReconnect();
      throw err;
    }
  }

  private scheduleReconnect(): void {
    if (!this.reconnectTimer) {
      this.reconnectTimer = setTimeout(() => {
        console.log('Attempting to reconnect SignalR...');
        this.startConnection().catch(err => {
          console.error('Reconnection failed:', err);
        });
      }, 5000); // Try to reconnect after 5 seconds
    }
  }

  async stopConnection(): Promise<void> {
    if (this.reconnectTimer) {
      clearTimeout(this.reconnectTimer);
      this.reconnectTimer = null;
    }

    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
      this.notifyConnectionStatus('Disconnected');
      console.log('SignalR connection stopped');
    }
  }

  onDeviceStatusUpdate(callback: (status: DeviceStatusInfo) => void): void {
    this.deviceStatusCallbacks.push(callback);
  }

  offDeviceStatusUpdate(callback: (status: DeviceStatusInfo) => void): void {
    const index = this.deviceStatusCallbacks.indexOf(callback);
    if (index > -1) {
      this.deviceStatusCallbacks.splice(index, 1);
    }
  }

  onConnectionStatusChange(callback: (status: string) => void): void {
    this.connectionStatusCallbacks.push(callback);
  }

  offConnectionStatusChange(callback: (status: string) => void): void {
    const index = this.connectionStatusCallbacks.indexOf(callback);
    if (index > -1) {
      this.connectionStatusCallbacks.splice(index, 1);
    }
  }

  private notifyConnectionStatus(status: string): void {
    this.connectionStatusCallbacks.forEach(callback => callback(status));
  }

  getConnectionState(): string {
    if (!this.connection) return 'Disconnected';
    return this.connection.state;
  }
}

// Singleton instance
export const signalRService = new SignalRService();