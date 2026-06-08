import execa from 'execa';

export interface CliHandle {
  proc: execa.ExecaChildProcess;
  send: (cmd: string) => void;
  stop: () => Promise<void>;
}

export async function startCli(): Promise<CliHandle> {
  // Runs the dotnet CLI in the repo if available.
  const proc = execa('dotnet', ['run', '--project', 'apps/cli-dotnet', '--no-build', '--verbosity', 'quiet'], {
    stdin: 'pipe',
    stderr: 'pipe',
    stdout: 'pipe',
  });

  // forward errors to console
  proc.stderr?.on('data', (d) => process.stderr.write(d.toString()));

  return {
    proc,
    send: (cmd: string) => {
      if (proc.stdin) proc.stdin.write(cmd.trim() + '\n');
    },
    stop: async () => {
      try {
        proc.kill('SIGTERM');
        await proc;
      } catch {
        // ignore
      }
    },
  };
}
