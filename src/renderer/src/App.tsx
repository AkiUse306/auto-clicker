import { useEffect, useMemo, useState } from 'react';

const appInfo = window.appInfo;

export default function App() {
  const [cps, setCps] = useState(10);
  const [button, setButton] = useState<'left' | 'right' | 'middle'>('left');
  const [holdToClick, setHoldToClick] = useState(false);
  const [running, setRunning] = useState(false);

  useEffect(() => {
    const unsubscribe = window.electronAPI?.onStatus((status) => {
      setRunning(status.running);
      setCps(status.cps);
      setButton(status.button as 'left' | 'right' | 'middle');
      setHoldToClick(status.holdToClick);
    });

    window.electronAPI?.status().then((status) => {
      setRunning(status.running);
      setCps(status.cps);
      setButton(status.button as 'left' | 'right' | 'middle');
      setHoldToClick(status.holdToClick);
    });

    return () => unsubscribe?.();
  }, []);

  const statusText = useMemo(() => {
    if (!running) return 'Ready to click';
    return holdToClick ? `Holding ${button} clicks at ${cps} CPS` : `Clicking ${button} at ${cps} CPS`;
  }, [button, cps, holdToClick, running]);

  const toggleClicker = async () => {
    if (running) {
      await window.electronAPI?.stop();
      return;
    }

    await window.electronAPI?.start({ cps, button, holdToClick });
  };

  return (
    <main className="min-h-screen bg-[radial-gradient(circle_at_top,_#111827,_#020617_55%)] text-slate-100">
      <section className="mx-auto flex min-h-screen w-full max-w-6xl flex-col justify-center px-6 py-16 lg:px-8">
        <div className="rounded-3xl border border-slate-800 bg-slate-900/80 p-8 shadow-2xl shadow-black/40 backdrop-blur-xl">
          <p className="text-sm uppercase tracking-[0.35em] text-cyan-300">Auto Clicker 2.0</p>
          <h1 className="mt-4 text-4xl font-semibold text-white md:text-5xl">Real click automation, now wired into the desktop app.</h1>
          <p className="mt-4 max-w-2xl text-slate-300">Use the controls below to start or stop the active click loop, tune CPS, and switch the mouse button.</p>

          <div className="mt-8 grid gap-6 lg:grid-cols-[1.2fr_0.8fr]">
            <article className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6 shadow-inner shadow-black/20">
              <div className="flex items-center justify-between gap-4">
                <div>
                  <p className="text-sm uppercase tracking-[0.25em] text-cyan-200">Live status</p>
                  <h2 className="mt-2 text-2xl font-semibold text-white">{statusText}</h2>
                </div>
                <button
                  type="button"
                  onClick={toggleClicker}
                  className={`rounded-2xl px-5 py-3 text-sm font-semibold transition ${running ? 'bg-rose-500/90 hover:bg-rose-400' : 'bg-cyan-400/90 hover:bg-cyan-300'} text-slate-950`}
                >
                  {running ? 'Stop auto clicker' : 'Start auto clicker'}
                </button>
              </div>

              <label className="mt-6 block text-sm text-slate-200">
                <span className="mb-2 block text-slate-300">Clicks per second: {cps}</span>
                <input
                  type="range"
                  min="1"
                  max="30"
                  value={cps}
                  onChange={(event) => setCps(Number(event.target.value))}
                  className="w-full accent-cyan-400"
                />
              </label>

              <div className="mt-6 grid gap-4 md:grid-cols-2">
                <label className="rounded-2xl border border-slate-800 bg-slate-900/80 p-4 text-sm text-slate-200">
                  <span className="mb-2 block text-slate-300">Mouse button</span>
                  <select
                    value={button}
                    onChange={(event) => setButton(event.target.value as 'left' | 'right' | 'middle')}
                    className="w-full rounded-xl border border-slate-700 bg-slate-950 px-3 py-2 text-slate-100"
                  >
                    <option value="left">Left click</option>
                    <option value="right">Right click</option>
                    <option value="middle">Middle click</option>
                  </select>
                </label>

                <label className="rounded-2xl border border-slate-800 bg-slate-900/80 p-4 text-sm text-slate-200">
                  <span className="mb-2 block text-slate-300">Mode</span>
                  <button
                    type="button"
                    onClick={() => setHoldToClick((prev) => !prev)}
                    className={`w-full rounded-xl border px-3 py-2 text-left ${holdToClick ? 'border-emerald-400/70 bg-emerald-400/10 text-emerald-100' : 'border-slate-700 bg-slate-950 text-slate-100'}`}
                  >
                    {holdToClick ? 'Hold-to-click enabled' : 'Click-repeat mode'}
                  </button>
                </label>
              </div>
            </article>

            <aside className="rounded-3xl border border-slate-800 bg-slate-950/80 p-6 text-slate-200 shadow-inner shadow-black/20">
              <p className="text-sm uppercase tracking-[0.25em] text-cyan-200">Current build</p>
              <ul className="mt-4 space-y-3 text-sm text-slate-300">
                <li>Electron + React + TypeScript desktop shell</li>
                <li>Native mouse automation via robotjs</li>
                <li>Start / stop controls with live CPS feedback</li>
                <li>Platform: {appInfo?.platform ?? 'unknown'}</li>
              </ul>
            </aside>
          </div>
        </div>
      </section>
    </main>
  );
}
