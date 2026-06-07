export type MouseButton = 'left' | 'right' | 'middle';

export function computeClickDelayMs(cps: number): number {
  const safeCps = Math.max(1, Math.floor(cps || 1));
  return Math.max(1, Math.floor(1000 / safeCps));
}

export function clampCps(value: number): number {
  return Math.min(60, Math.max(1, Math.round(value)));
}
