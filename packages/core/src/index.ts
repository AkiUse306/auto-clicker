export type MouseButton = 'left' | 'right' | 'middle';
export type ClickMode = 'click' | 'hold';

export interface ClickerProfile {
  id: string;
  name: string;
  cps: number;
  button: MouseButton;
  mode: ClickMode;
  jitter: {
    enabled: boolean;
    minMs: number;
    maxMs: number;
  };
  burst: {
    enabled: boolean;
    clicksPerBurst: number;
    pauseMs: number;
  };
  activationHotkey?: {
    key: string;
    modifiers: ('ctrl' | 'shift' | 'alt' | 'meta')[];
  };
}

export const DEFAULT_PROFILE: ClickerProfile = {
  id: 'default',
  name: 'Default',
  cps: 10,
  button: 'left',
  mode: 'click',
  jitter: { enabled: false, minMs: 5, maxMs: 15 },
  burst: { enabled: false, clicksPerBurst: 5, pauseMs: 100 },
};

export interface ClickerState {
  running: boolean;
  profile: ClickerProfile;
  clickCount: number;
}

export function computeClickDelayMs(cps: number): number {
  const safeCps = Math.max(1, Math.floor(cps || 1));
  return Math.max(1, Math.floor(1000 / safeCps));
}

export function clampCps(value: number): number {
  return Math.min(60, Math.max(1, Math.round(value)));
}

export function applyJitter(baseMs: number, jitter: { enabled: boolean; minMs: number; maxMs: number }): number {
  if (!jitter.enabled) return baseMs;
  const randMs = Math.random() * (jitter.maxMs - jitter.minMs) + jitter.minMs;
  return Math.max(1, Math.floor(baseMs + randMs));
}

export function validateProfile(profile: Partial<ClickerProfile>): string[] {
  const errors: string[] = [];

  if (!profile.name || profile.name.trim().length === 0) errors.push('Profile name is required');
  if (!profile.cps || profile.cps < 1 || profile.cps > 60) errors.push('CPS must be between 1 and 60');
  if (!profile.button || !['left', 'right', 'middle'].includes(profile.button)) errors.push('Invalid mouse button');
  if (!profile.mode || !['click', 'hold'].includes(profile.mode)) errors.push('Invalid click mode');

  if (profile.jitter) {
    if (profile.jitter.minMs < 0 || profile.jitter.maxMs < 0) errors.push('Jitter values must be non-negative');
    if (profile.jitter.minMs > profile.jitter.maxMs) errors.push('Jitter min must be <= max');
  }

  if (profile.burst) {
    if (profile.burst.clicksPerBurst < 1) errors.push('Burst clicks must be >= 1');
    if (profile.burst.pauseMs < 0) errors.push('Burst pause must be non-negative');
  }

  return errors;
}
