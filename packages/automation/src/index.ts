import { spawn } from 'child_process';
import path from 'path';

export interface CliHandle {
  proc: ReturnType<typeof spawn>;
  send: (cmd: string) => void;
  stop: () => Promise<void>;
}

export async function startCli(): Promise<CliHandle> {
  // Spawns the dotnet CLI clicker with stdin/stdout piped
  const proc = spawn('dotnet', ['run', '--project', 'apps/cli-dotnet', '--no-build', '--verbosity', 'quiet'], {
    stdio: ['pipe', 'pipe', 'pipe'],
    cwd: path.resolve(process.cwd()),
  });

  // forward stderr to console for debugging
  proc.stderr?.on('data', (d) => process.stderr.write(d.toString()));

  return {
    proc,
    send: (cmd: string) => {
      if (proc.stdin && !proc.stdin.destroyed) {
        proc.stdin.write(cmd.trim() + '\n');
      }
    },
    stop: async () => {
      return new Promise((resolve) => {
        if (!proc.kill('SIGTERM')) {
          proc.kill('SIGKILL');
        }
        proc.once('exit', () => resolve());
        setTimeout(() => resolve(), 1000);
      });
    },
  };
}
