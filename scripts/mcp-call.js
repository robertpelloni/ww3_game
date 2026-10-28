// Minimal MCP stdio client: handshake + one call. Usage:
// node mcp-call.js <workdir> <cmd> <arg1> ... -- <method> [<paramsJSON>]
const { spawn } = require('child_process');
const sep = process.argv.indexOf('--');
const [workdir, cmd, ...rest] = process.argv.slice(2, sep);
const args = rest;
const method = process.argv[sep + 1];
const params = process.argv[sep + 2] ? JSON.parse(process.argv[sep + 2]) : {};
const child = spawn(cmd, args, { cwd: workdir || process.cwd(), stdio: ['pipe', 'pipe', 'inherit'] });
let buf = ''; let id = 0; const pending = new Map();
child.stdout.on('data', d => {
  buf += d.toString();
  let i;
  while ((i = buf.indexOf('\n')) >= 0) {
    const line = buf.slice(0, i).trim(); buf = buf.slice(i + 1);
    if (!line) continue;
    try {
      const m = JSON.parse(line);
      if (m.id !== undefined && pending.has(m.id)) { pending.get(m.id)(m); pending.delete(m.id); }
    } catch {}
  }
});
function call(m, p) {
  return new Promise((res, rej) => {
    const myId = ++id;
    pending.set(myId, res);
    child.stdin.write(JSON.stringify({ jsonrpc: '2.0', id: myId, method: m, params: p }) + '\n');
    setTimeout(() => { if (pending.has(myId)) { pending.delete(myId); rej(new Error('timeout ' + m)); } }, 25000);
  });
}
(async () => {
  const init = await call('initialize', { protocolVersion: '2024-11-05', capabilities: {}, clientInfo: { name: 'ww3-test', version: '0.1' } });
  console.log('SERVER:', JSON.stringify(init.result && init.result.serverInfo));
  child.stdin.write(JSON.stringify({ jsonrpc: '2.0', method: 'notifications/initialized' }) + '\n');
  const out = await call(method, params);
  const s = JSON.stringify(out, null, 1);
  console.log(s.length > 4000 ? s.slice(0, 4000) + '\n...[truncated ' + s.length + ' chars]' : s);
  child.kill(); process.exit(0);
})().catch(e => { console.error('ERR:', e.message); child.kill(); process.exit(1); });
