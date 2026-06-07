import { app, BrowserWindow, ipcMain } from 'electron';
import path from 'node:path';
import robot from 'robotjs';
import { autoUpdater } from 'electron-updater';
import { clampCps, computeClickDelayMs, type MouseButton } from './clicker.js';

let clickTimer: NodeJS.Timeout | null = null;
let clickerState = {
  running: false,
  cps: 10,
  button: 'left' as MouseButton,
  holdToClick: false,
};

function stopClicker() {
  if (clickTimer) {
    clearInterval(clickTimer);
    clickTimer = null;
  }

  if (clickerState.holdToClick) {
    robot.mouseToggle('up', clickerState.button);
  }

  clickerState = { ...clickerState, running: false };
  BrowserWindow.getAllWindows().forEach((win) => {
    win.webContents.send('clicker:status', clickerState);
  });
}

function startClicker(config: Partial<typeof clickerState>) {
  clickerState = {
    ...clickerState,
    ...config,
    cps: clampCps(config.cps ?? clickerState.cps),
    running: true,
  };

  stopClicker();

  if (clickerState.holdToClick) {
    robot.mouseToggle('down', clickerState.button);
    BrowserWindow.getAllWindows().forEach((win) => {
      win.webContents.send('clicker:status', clickerState);
    });
    return;
  }

  clickTimer = setInterval(() => {
    robot.mouseClick(clickerState.button);
  }, computeClickDelayMs(clickerState.cps));

  BrowserWindow.getAllWindows().forEach((win) => {
    win.webContents.send('clicker:status', clickerState);
  });
}

function createWindow() {
  const mainWindow = new BrowserWindow({
    width: 1280,
    height: 900,
    minWidth: 1100,
    minHeight: 760,
    title: 'Auto Clicker',
    backgroundColor: '#07111f',
    webPreferences: {
      preload: path.join(__dirname, 'preload.js'),
      contextIsolation: true,
      nodeIntegration: false,
    },
  });

  if (app.isPackaged) {
    mainWindow.loadFile(path.join(__dirname, 'renderer/index.html'));
  } else {
    mainWindow.loadURL('http://127.0.0.1:5173');
  }

  autoUpdater.checkForUpdatesAndNotify().catch(() => undefined);
}

app.whenReady().then(() => {
  ipcMain.handle('clicker:start', (_event, config: Partial<typeof clickerState>) => {
    startClicker(config);
  });

  ipcMain.handle('clicker:stop', () => {
    stopClicker();
  });

  ipcMain.handle('clicker:status', () => clickerState);

  createWindow();

  app.on('activate', () => {
    if (BrowserWindow.getAllWindows().length === 0) {
      createWindow();
    }
  });
});

app.on('window-all-closed', () => {
  if (process.platform !== 'darwin') {
    app.quit();
  }
});
