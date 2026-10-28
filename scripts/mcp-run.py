"""Persistent MCP stdio session: handshake, then one call with params read from a JSON file.
Usage: python mcp-run.py <cmd> <arg1> ... -- <method> <params.json>
Params file values may reference {"$file": "path"} to inline file text.
"""
import json, subprocess, sys, os

sep = sys.argv.index('--')
cmd = sys.argv[1:sep]
method = sys.argv[sep + 1]
params_path = sys.argv[sep + 2] if len(sys.argv) > sep + 2 else None

def expand(v):
    if isinstance(v, dict):
        if set(v.keys()) == {'$file'}:
            with open(v['$file'], 'r', encoding='utf-8') as f:
                return f.read()
        return {k: expand(x) for k, x in v.items()}
    if isinstance(v, list):
        return [expand(x) for x in v]
    return v

params = {}
if params_path:
    with open(params_path, encoding='utf-8') as f:
        params = expand(json.load(f))

p = subprocess.Popen(cmd, stdin=subprocess.PIPE, stdout=subprocess.PIPE,
                     stderr=subprocess.DEVNULL, text=True, bufsize=1,
                     cwd=os.environ.get('CWD', os.getcwd()))
rid = [0]
def call(m, pr, timeout=60):
    rid[0] += 1
    p.stdin.write(json.dumps({'jsonrpc': '2.0', 'id': rid[0], 'method': m, 'params': pr}) + '\n')
    p.stdin.flush()
    import select
    out = ''
    while True:
        line = p.stdout.readline()
        if not line:
            raise RuntimeError('server closed stdout')
        line = line.strip()
        if not line:
            continue
        try:
            msg = json.loads(line)
        except json.JSONDecodeError:
            continue
        if msg.get('id') == rid[0]:
            return msg

init = call('initialize', {'protocolVersion': '2024-11-05', 'capabilities': {},
                           'clientInfo': {'name': 'ww3-py', 'version': '0.1'}})
print('SERVER:', json.dumps((init.get('result') or {}).get('serverInfo')))
p.stdin.write(json.dumps({'jsonrpc': '2.0', 'method': 'notifications/initialized'}) + '\n')
p.stdin.flush()
res = call(method, params, timeout=120)
s = json.dumps(res)
print(s[:4000] + ('...[truncated]' if len(s) > 4000 else ''))
p.kill()
