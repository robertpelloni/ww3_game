# GAME_DESIGN v0.1 — WW3: Escape from North Korea

## High concept
WW3 erupts. You are **Jun-ho, a 24-year-old factory worker in Chongjin**. The front is collapsing
north. You have **72 hours** to get out: cross the city under curfew, choose your exit
(forged papers / bribed checkpoint / Tumen river / smuggler tunnel), and reach the DMZ south.

Genre: **top-down stealth-survival (low-poly 3D, URP)**. No run-and-gun. Getting seen is the fail state.

## Pillars
1. **Stealth > shooting** — vision cones, noise rings, light/shadow, curfew searchlights.
2. **Survival clock** — hunger, cold, stamina, batteries, medicine. Every detour costs.
3. **Choices with cost** — papers are safe but slow; river is fast but freezing; bribes cost food.
4. **Runs on a potato** — 1 realtime light + fog + baked AO. Mood over polys.

## Acts
- **Act 1 — Blackout Chongjin (vertical slice):** tutorial street. Avoid 2 patrols, 1 checkpoint,
  reach the safehouse. Win = knock twice. Lose = spotlight + siren.
- **Act 2 — Papers or River:** earn/forge documents or risk the freezing Tumen. Hunger + cold bite.
- **Act 3 — DMZ Run:** minefield + flare + final choice: run alone or carry the injured kid (slower, better ending).

## Core systems (MVP)
- Player: crouch/sprint/prone, stamina, noise emit, hide in shadow/dumpster/fog.
- AI: patrol waypoints, suspicion meter (0-100), investigate → chase → search → reset. Whistle lure.
- Light model: `exposure = inSpotlight ? 1 : inLampRadius ? 0.5 : inShadow ? 0.05`. Detection rate scales with exposure + distance + stance.
- Items: rice, antibiotics, batteries (flashlight), forged stamp, wire cutters.
- Checkpoints: papers check (timed dialogue) or bribe (costs 2 rice) or sneak (vent/river).
- Endings: Alone / Together / Caught (prison → Act 1 retry with one extra item = roguelite mercy).

## Vertical slice acceptance (Act 1)
- 1 street (~60x20m), night + fog + 1 searchlight, 2 patrols + 1 checkpoint guard.
- Player can win by pure stealth in <5 min, no script errors, 30fps+ on HD 520 Very Low.
- Lose state + retry in <3s. Minimal UI: stamina bar, suspicion diamonds, item count.

## Controls
WASD move, Shift sprint, C crouch, E interact, Q whistle, F flashlight, Esc pause. Gamepad optional later.

## Audio (lo-fi, laptop-safe)
- Wind loop + distant sirens + footsteps on gravel/snow. No orchestral stems until v2.
- Generate placeholders with free tools; keep .wav 22kHz mono to save RAM.

## What we are NOT building (v1)
Multiplayer, vehicles, guns, open world, HDRP, ray tracing, English+Korean VO (text only, KR flavor lines).
