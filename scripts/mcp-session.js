// Persistent MCP session driver: run_project -> poll debug -> stop, all in ONE server lifetime.
// Usage: node mcp-session.js <serverCmd...> -- <projectPath>
const { spawn } = require('child_process');
const sep = process.argv.indexOf('--');
const [cmd, ...args] = process.argv.slice(2, sep);
const projectPath = process.argv[sep + 1];
const child = spawn(cmd, args, { cwd: process.env.CWD || process.cwd(), stdio: ['pipe', 'pipe', 'inherit'], env: process.env });
let buf = '', id = 0; const pending = new Map();
child.stdout.on('data', d => {
  buf += d.toString();
  let i;
  while ((i = buf.indexOf('\n')) >= 0) {
    const line = buf.slice(0, i).trim(); buf = buf.slice(i + 1);
    if (!line) continue;
    try { const m = JSON.parse(line); if (m.id !== undefined && pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); } } catch {}
  }
});
const call = (m, p, t = 30000) => new Promise((res, rej) => {
  const k = ++id; pending.set(k, res);
  child.stdin.write(JSON.stringify({ jsonrpc: '2.0', id: k, method: m, params: p }) + '\n');
  setTimeout(() => { if (pending.has(k)) { pending.delete(k); rej(new Error('timeout ' + m)); } }, t);
});
(async () => {
  const init = await call('initialize', { protocolVersion: '2024-11-05', capabilities: {}, clientInfo: { name: 'ww3-session', version: '0.1' } });
  console.log('SERVER:', JSON.stringify(init.result.serverInfo));
  child.stdin.write(JSON.stringify({ jsonrpc: '2.0', method: 'notifications/initialized' }) + '\n');
  console.log('--- run_project ---');
  console.log(JSON.stringify(await call('tools/call', { name: 'run_project', arguments: { projectPath } })).slice(0, 400));
  await new Promise(r => setTimeout(r, 9000));
  console.log('--- get_debug_output ---');
  const dbg = await call('tools/call', { name: 'get_debug_output', arguments: {} });
  const s = JSON.stringify(dbg);
  const hits = s.match(/WW3_TEST[^"\\]*/g) || [];
  console.log('WW3 LINES:', JSON.stringify(hits));
  console.log('--- stop_project ---');
  console.log(JSON.stringify(await call('tools/call', { name: 'stop_project', arguments: {} })).slice(0, 300));
  child.kill(); process.exit(0);
})().catch(e => { console.error('ERR:', e.message); child.kill(); process.exit(1); });
