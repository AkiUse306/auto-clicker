declare interface Window {
  appInfo?: {
    name: string;
    version: string;
    platform: string;
  };
  electronAPI?: {
    start(config: { cps?: number; button?: 'left' | 'right' | 'middle'; holdToClick?: boolean }): Promise<void>;
    stop(): Promise<void>;
    status(): Promise<{ running: boolean; cps: number; button: 'left' | 'right' | 'middle'; holdToClick: boolean }>;
    onStatus(callback: (status: { running: boolean; cps: number; button: 'left' | 'right' | 'middle'; holdToClick: boolean }) => void): () => void;
  };
}
