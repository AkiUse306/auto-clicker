import { homedir } from 'os';
import { join } from 'path';
import { mkdirSync, readFileSync, writeFileSync, existsSync } from 'fs';

export type Platform = 'win32' | 'darwin' | 'linux' | 'other';

export function detectPlatform(): Platform {
  const p = process.platform;
  if (p === 'win32') return 'win32';
  if (p === 'darwin') return 'darwin';
  if (p === 'linux') return 'linux';
  return 'other';
}

export function getConfigDir(): string {
  const appName = 'auto-clicker';
  if (detectPlatform() === 'win32') {
    return join(process.env.APPDATA || homedir(), appName);
  }
  if (detectPlatform() === 'darwin') {
    return join(homedir(), 'Library', 'Application Support', appName);
  }
  return join(homedir(), `.config/${appName}`);
}

export function ensureConfigDir(): string {
  const dir = getConfigDir();
  mkdirSync(dir, { recursive: true });
  return dir;
}

export class ConfigStore {
  private filePath: string;

  constructor(fileName: string = 'config.json') {
    ensureConfigDir();
    this.filePath = join(getConfigDir(), fileName);
  }

  load<T>(defaultValue: T): T {
    try {
      if (!existsSync(this.filePath)) {
        return defaultValue;
      }
      const data = readFileSync(this.filePath, 'utf-8');
      return JSON.parse(data);
    } catch {
      return defaultValue;
    }
  }

  save<T>(data: T): void {
    writeFileSync(this.filePath, JSON.stringify(data, null, 2), 'utf-8');
  }
}
