export type Platform = 'win32' | 'darwin' | 'linux' | 'other';

export function detectPlatform(): Platform {
  const p = process.platform;
  if (p === 'win32') return 'win32';
  if (p === 'darwin') return 'darwin';
  if (p === 'linux') return 'linux';
  return 'other';
}
