import { contextBridge, ipcRenderer } from 'electron';

contextBridge.exposeInMainWorld('appInfo', {
  name: 'Auto Clicker',
  version: '2.0.0',
  platform: process.platform,
});

contextBridge.exposeInMainWorld('electronAPI', {
  start: (config: { cps?: number; button?: 'left' | 'right' | 'middle'; holdToClick?: boolean }) =>
    ipcRenderer.invoke('clicker:start', config),
  stop: () => ipcRenderer.invoke('clicker:stop'),
  status: () => ipcRenderer.invoke('clicker:status'),
  onStatus: (callback: (status: { running: boolean; cps: number; button: string; holdToClick: boolean }) => void) => {
    const channel = 'clicker:status';
    const handler = (_event: unknown, status: unknown) => callback(status as never);
    ipcRenderer.on(channel, handler);
    return () => ipcRenderer.removeListener(channel, handler);
  },
});
