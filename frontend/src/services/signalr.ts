import * as signalR from '@microsoft/signalr';
import { SIGNALR_HUB_URL } from '../config';

export class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private token: string | null = null;
    private isStopped = false;
    private startPromise: Promise<void> | null = null;

    public setToken(token: string) {
        this.token = token;
    }


    public async startConnection() {
        if (this.startPromise) {
            return this.startPromise;
        }

        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return Promise.resolve();
        }

        if (this.connection?.state === signalR.HubConnectionState.Connecting) {
            return Promise.resolve();
        }

        this.isStopped = false;

        if (!this.connection) {
            this.connection = new signalR.HubConnectionBuilder()
                .withUrl(SIGNALR_HUB_URL, {
                    accessTokenFactory: () => this.token || '',
                })
                .withAutomaticReconnect()
                .build();
        }

        this.startPromise = (async () => {
            try {
                if (this.connection && this.connection.state === signalR.HubConnectionState.Disconnected) {
                    await this.connection.start();
                    console.log('SignalR Connected');
                }
            } catch (err) {
                console.error('SignalR Connection Error: ', err);
                throw err;
            } finally {
                this.startPromise = null;
            }
        })();

        return this.startPromise;
    }

    public async stopConnection() {
        this.isStopped = true;

        // Wait for any pending start to complete
        if (this.startPromise) {
            try {
                await this.startPromise;
            } catch (err) {
                // Ignore errors from pending start
                console.log('SignalR Connection Error: ', err);
            }
        }

        if (this.connection) {
            try {
                await this.connection.stop();
            } catch (err) {
                console.error('Error stopping connection:', err);
            }
            this.connection = null;
        }
    }

    public async joinGroup(groupId: string) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('JoinGroup', groupId);
        }
    }

    public async leaveGroup(groupId: string) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('LeaveGroup', groupId);
        }
    }

    public async sendMessageToGroup(groupId: string, message: string) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            await this.connection.invoke('SendMessageToGroup', groupId, message);
        }
    }

    public onReceiveMessage(callback: (message: any) => void) {
        if (this.connection) {
            this.connection.on('ReceiveMessage', callback);
        }
    }

    public offReceiveMessage(callback: (message: any) => void) {
        if (this.connection) {
            this.connection.off('ReceiveMessage', callback);
        }
    }

    public onMessageUpdated(callback: (message: any) => void) {
        if (this.connection) {
            this.connection.on('MessageUpdated', callback);
        }
    }

    public offMessageUpdated(callback: (message: any) => void) {
        if (this.connection) {
            this.connection.off('MessageUpdated', callback);
        }
    }

    public onMessageDeleted(callback: (messageId: string) => void) {
        if (this.connection) {
            this.connection.on('MessageDeleted', callback);
        }
    }

    public offMessageDeleted(callback: (messageId: string) => void) {
        if (this.connection) {
            this.connection.off('MessageDeleted', callback);
        }
    }
}

export const signalRService = new SignalRService();
