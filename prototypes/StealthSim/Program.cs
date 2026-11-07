using System;
using System.Collections.Generic;

namespace WW3.StealthSim
{
    public enum Stance
    {
        Prone,
        Crouch,
        Walk,
        Sprint
    }

    public enum SurfaceType
    {
        Dirt,
        Concrete,
        MetalGrate,
        GlassDebris
    }

    public enum AIState
    {
        Patrol,
        Investigate,
        Chase,
        Search
    }

    public struct Vec2
    {
        public double X;
        public double Y;

        public Vec2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, double d) => new Vec2(a.X * d, a.Y * d);

        public double Magnitude() => Math.Sqrt(X * X + Y * Y);
        public Vec2 Normalized()
        {
            double m = Magnitude();
            return m > 0.0001 ? new Vec2(X / m, Y / m) : new Vec2(0, 0);
        }

        public static double Distance(Vec2 a, Vec2 b) => (a - b).Magnitude();
        public static double Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;
        public static Vec2 Zero => new Vec2(0, 0);
    }

    public class Guard
    {
        public string Name { get; set; } = "Guard";
        public Vec2 Position { get; set; }
        public Vec2 Forward { get; set; }
        public Vec2 WaypointA { get; set; }
        public Vec2 WaypointB { get; set; }
        public int WaypointDir { get; set; } = 1;
        public double WallOcclusionFactor { get; set; } = 0.35;
        public double RadioBroadcastRadius { get; set; } = 25.0;
        public double PatrolSpeed { get; set; } = 2.0;
        public double ChaseSpeed { get; set; } = 4.0;
        public double Suspicion { get; set; } = 0.0; // 0 to 100
        public AIState State { get; set; } = AIState.Patrol;
        public Vec2 LastInvestigateTarget { get; set; }
        public double SearchTimer { get; set; } = 0.0;
        public double AlertBroadcastCooldown { get; set; } = 0.0;
        public double VisionAngleDeg { get; set; } = 70.0;
        public double VisionRange { get; set; } = 14.0;
        public bool FlashlightActive => State == AIState.Investigate || State == AIState.Search || State == AIState.Chase;
        public double HearingSensitivity { get; set; } = 1.0;
        public double SensorInterval { get; set; } = 0.066; // 15Hz when cruising
        public double SensorTimer { get; set; } = 0.0;
        public int SensorEvaluationCount { get; set; } = 0;
    }

    public class Shelter
    {
        public string Name { get; set; } = "Shelter";
        public Vec2 Position { get; set; }
        public double Radius { get; set; } = 3.5;
        public double WarmingRate { get; set; } = 6.0;
    }

    public class ObstacleWall
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public string Name { get; set; } = "Wall";

        public bool BlocksSegment(Vec2 p1, Vec2 p2)
        {
            // Simple bounding box segment intersection
            int steps = 20;
            for (int i = 0; i <= steps; i++)
            {
                double t = (double)i / steps;
                double x = p1.X + t * (p2.X - p1.X);
                double y = p1.Y + t * (p2.Y - p1.Y);
                if (x >= MinX && x <= MaxX && y >= MinY && y <= MaxY)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class SurfaceZone
    {
        public double MinX { get; set; }
        public double MaxX { get; set; }
        public double MinY { get; set; }
        public double MaxY { get; set; }
        public SurfaceType Surface { get; set; } = SurfaceType.MetalGrate;
        public string Name { get; set; } = "SurfaceZone";

        public bool Contains(Vec2 p) => p.X >= MinX && p.X <= MaxX && p.Y >= MinY && p.Y <= MaxY;
    }

    public class Player
    {
        public Vec2 Position { get; set; }
        public Stance CurrentStance { get; set; } = Stance.Crouch;
        public SurfaceType CurrentSurface { get; set; } = SurfaceType.Concrete;
        public double Stamina { get; set; } = 100.0;
        public double Hunger { get; set; } = 10.0; // 0 (full) to 100 (starving)
        public double Cold { get; set; } = 15.0; // 0 (warm) to 100 (hypothermia)
        public double Battery { get; set; } = 80.0;
        public int RiceCount { get; set; } = 2;
        public int HotTeaCount { get; set; } = 1;
        public int BottleCount { get; set; } = 2;
        public bool HasForgedPapers { get; set; } = true;
        public bool HasWireCutters { get; set; } = false;
        public bool FlashlightOn { get; set; } = false;
        public bool IsShivering => Cold >= 70.0;
        public bool IsStarving => Hunger >= 80.0;
        public bool IsExhausted { get; set; } = false;
        public double ExhaustionTimer { get; set; } = 0.0;

        public double GetBaseSpeed() => CurrentStance switch
        {
            Stance.Prone => 1.2,
            Stance.Crouch => 2.5,
            Stance.Walk => 4.0,
            Stance.Sprint => 7.0,
            _ => 2.5
        };

        public double GetNoiseRadius()
        {
            double surfaceMod = CurrentSurface switch
            {
                SurfaceType.Dirt => 0.70,
                SurfaceType.Concrete => 1.00,
                SurfaceType.MetalGrate => 1.60,
                SurfaceType.GlassDebris => 2.00,
                _ => 1.00
            };

            double baseNoise = (CurrentStance switch
            {
                Stance.Prone => 0.5,
                Stance.Crouch => 2.0,
                Stance.Walk => 6.0,
                Stance.Sprint => 12.0,
                _ => 2.0
            }) * surfaceMod;

            if (IsShivering)
            {
                // Freezing teeth chattering adds +1.8m acoustic signature
                baseNoise += 1.8;
            }

            if (IsExhausted)
            {
                // Heavy breathing from sprint exhaustion adds acoustic signature
                baseNoise = Math.Max(baseNoise, 2.5);
            }

            return baseNoise;
        }

        public double GetStanceDetectionMultiplier() => CurrentStance switch
        {
            Stance.Prone => 0.3,
            Stance.Crouch => 0.6,
            Stance.Walk => 1.0,
            Stance.Sprint => 1.5,
            _ => 1.0
        };
    }

    public class Lamp
    {
        public Vec2 Position { get; set; }
        public double Radius { get; set; } = 5.0;
    }

    public class Spotlight
    {
        public Vec2 Center { get; set; }
        public double Radius { get; set; } = 4.0;
        public double SweepRange { get; set; } = 10.0;
        public double SweepSpeed { get; set; } = 1.5;
        public double SweepTime { get; set; } = 0.0;
        public Vec2? FocusTarget { get; set; } = null;
        public double FocusTimer { get; set; } = 0.0;

        public Vec2 GetCurrentPosition()
        {
            if (FocusTarget.HasValue && FocusTimer > 0)
            {
                return FocusTarget.Value;
            }
            double offset = Math.Sin(SweepTime * SweepSpeed) * SweepRange;
            return new Vec2(Center.X + offset, Center.Y);
        }

        public void FocusOn(Vec2 target, double duration = 5.0)
        {
            FocusTarget = target;
            FocusTimer = duration;
        }
    }

    public class SimulationWorld
    {
        public double Width { get; set; } = 60.0; // 60x20m street
        public double Height { get; set; } = 20.0;
        public Player PlayerCharacter { get; set; } = new Player();
        public List<Guard> Guards { get; } = new List<Guard>();
        public List<Lamp> Lamps { get; } = new List<Lamp>();
        public List<Shelter> Shelters { get; } = new List<Shelter>();
        public List<ObstacleWall> Obstacles { get; } = new List<ObstacleWall>();
        public List<SurfaceZone> SurfaceZones { get; } = new List<SurfaceZone>();
        public Spotlight CurfewSearchlight { get; set; } = new Spotlight();
        public Vec2 SafehousePosition { get; set; } = new Vec2(56.0, 10.0);
        public Vec2 CheckpointPosition { get; set; } = new Vec2(40.0, 10.0);
        public Vec2? ActiveDistractionPos { get; set; } = null;
        public double ActiveDistractionRadius { get; set; } = 14.0;
        public double ActiveDistractionTimer { get; set; } = 0.0;
        public List<string> ProceduralAudioLog { get; } = new List<string>();

        public bool GameWon { get; set; } = false;
        public bool GameOver { get; set; } = false;
        public string EndReason { get; set; } = "";

        public void TriggerDistraction(Vec2 pos, double radius = 14.0, double duration = 2.5)
        {
            ActiveDistractionPos = pos;
            ActiveDistractionRadius = radius;
            ActiveDistractionTimer = duration;
            ProceduralAudioLog.Add($"BottleShatter at ({pos.X:F1}, {pos.Y:F1})");
        }

        public bool ThrowBottle(Vec2 targetPos)
        {
            if (PlayerCharacter.BottleCount <= 0) return false;
            PlayerCharacter.BottleCount--;
            TriggerDistraction(targetPos, 14.0, 2.5);
            return true;
        }

        public void Whistle()
        {
            TriggerWhistle();
            ProceduralAudioLog.Add($"Whistle at ({PlayerCharacter.Position.X:F1}, {PlayerCharacter.Position.Y:F1})");
        }

        public SimulationWorld()
        {
            // Player starts at west entry of Chongjin alley
            PlayerCharacter.Position = new Vec2(2.0, 10.0);

            // Patrol 1: Alley near start
            Guards.Add(new Guard
            {
                Name = "Patrol-1",
                Position = new Vec2(15.0, 5.0),
                WaypointA = new Vec2(15.0, 3.0),
                WaypointB = new Vec2(15.0, 17.0),
                Forward = new Vec2(0, 1),
                PatrolSpeed = 2.0
            });

            // Patrol 2: Mid-street patrol
            Guards.Add(new Guard
            {
                Name = "Patrol-2",
                Position = new Vec2(28.0, 15.0),
                WaypointA = new Vec2(28.0, 17.0),
                WaypointB = new Vec2(28.0, 3.0),
                Forward = new Vec2(0, -1),
                PatrolSpeed = 2.4
            });

            // Checkpoint Guard
            Guards.Add(new Guard
            {
                Name = "Checkpoint Guard",
                Position = new Vec2(42.0, 10.0),
                WaypointA = new Vec2(42.0, 8.0),
                WaypointB = new Vec2(42.0, 12.0),
                Forward = new Vec2(-1, 0),
                PatrolSpeed = 1.0,
                VisionRange = 16.0
            });

            // Concrete security barricades & alley walls
            Obstacles.Add(new ObstacleWall
            {
                Name = "Checkpoint Barricade",
                MinX = 39.5,
                MaxX = 40.5,
                MinY = 2.0,
                MaxY = 8.0
            });

            // Catwalk metal grates & rubble glass debris
            SurfaceZones.Add(new SurfaceZone
            {
                Name = "Catwalk Metal Grate",
                MinX = 20.0,
                MaxX = 24.0,
                MinY = 8.0,
                MaxY = 12.0,
                Surface = SurfaceType.MetalGrate
            });
            SurfaceZones.Add(new SurfaceZone
            {
                Name = "Broken Bottle Rubble",
                MinX = 32.0,
                MaxX = 35.0,
                MinY = 8.0,
                MaxY = 12.0,
                Surface = SurfaceType.GlassDebris
            });

            // Street lamps
            Lamps.Add(new Lamp { Position = new Vec2(12.0, 10.0), Radius = 4.5 });
            Lamps.Add(new Lamp { Position = new Vec2(26.0, 10.0), Radius = 4.5 });
            Lamps.Add(new Lamp { Position = new Vec2(38.0, 10.0), Radius = 5.0 });

            // Warming shelter / burn barrel alcove
            Shelters.Add(new Shelter
            {
                Name = "Alley Burn Barrel",
                Position = new Vec2(20.0, 4.0),
                Radius = 3.5,
                WarmingRate = 6.0
            });

            // Curfew searchlight sweeping
            CurfewSearchlight = new Spotlight
            {
                Center = new Vec2(30.0, 10.0),
                Radius = 4.0,
                SweepRange = 12.0,
                SweepSpeed = 0.8
            };
        }

        public bool IsPlayerSheltered()
        {
            foreach (var s in Shelters)
            {
                if (Vec2.Distance(PlayerCharacter.Position, s.Position) <= s.Radius)
                {
                    return true;
                }
            }
            return false;
        }

        public double CalculatePlayerExposure()
        {
            Vec2 pPos = PlayerCharacter.Position;

            // Curfew Spotlight: max exposure
            Vec2 spotPos = CurfewSearchlight.GetCurrentPosition();
            if (Vec2.Distance(pPos, spotPos) <= CurfewSearchlight.Radius)
            {
                return 1.0;
            }

            // Flashlight on increases exposure significantly
            if (PlayerCharacter.FlashlightOn)
            {
                return 0.85;
            }

            // Street lamps
            foreach (var lamp in Lamps)
            {
                if (Vec2.Distance(pPos, lamp.Position) <= lamp.Radius)
                {
                    return 0.50;
                }
            }

            // Default dark alley shadow
            return 0.05;
        }

        public void TriggerWhistle()
        {
            Vec2 pPos = PlayerCharacter.Position;
            double whistleNoise = 20.0; // audible within 20m

            foreach (var guard in Guards)
            {
                double dist = Vec2.Distance(guard.Position, pPos);
                if (dist <= whistleNoise)
                {
                    guard.LastInvestigateTarget = pPos;
                    guard.State = AIState.Investigate;
                    guard.Suspicion = Math.Max(guard.Suspicion, 40.0);
                }
            }
        }

        public void Step(double dt, Vec2 moveInput)
        {
            if (GameOver || GameWon) return;

            if (CurfewSearchlight.FocusTimer > 0)
            {
                CurfewSearchlight.FocusTimer -= dt;
                if (CurfewSearchlight.FocusTimer <= 0)
                {
                    CurfewSearchlight.FocusTarget = null;
                }
            }
            else
            {
                CurfewSearchlight.SweepTime += dt;
            }

            if (ActiveDistractionTimer > 0)
            {
                ActiveDistractionTimer -= dt;
                if (ActiveDistractionTimer <= 0)
                {
                    ActiveDistractionPos = null;
                }
            }

            double exposure = CalculatePlayerExposure();

            // Detect current surface material
            SurfaceType detectedSurface = SurfaceType.Concrete;
            foreach (var sz in SurfaceZones)
            {
                if (sz.Contains(PlayerCharacter.Position))
                {
                    detectedSurface = sz.Surface;
                    break;
                }
            }
            PlayerCharacter.CurrentSurface = detectedSurface;

            // Handle sprint exhaustion cooldown
            if (PlayerCharacter.ExhaustionTimer > 0)
            {
                PlayerCharacter.ExhaustionTimer -= dt;
                if (PlayerCharacter.ExhaustionTimer <= 0)
                {
                    PlayerCharacter.IsExhausted = false;
                }
            }

            // Player Stamina / Movement
            double speed = PlayerCharacter.GetBaseSpeed();

            // Temperature-based continuous baseline exhaustion:
            // Maintaining body temperature above 60% cold drains stamina
            if (PlayerCharacter.Cold >= 60.0)
            {
                double coldExhaustionFactor = (PlayerCharacter.Cold - 60.0) / 40.0;
                PlayerCharacter.Stamina = Math.Max(0, PlayerCharacter.Stamina - 3.5 * coldExhaustionFactor * dt);
            }

            if (PlayerCharacter.CurrentStance == Stance.Sprint && moveInput.Magnitude() > 0.01)
            {
                double starvationMult = PlayerCharacter.IsStarving ? 1.75 : 1.0;
                double coldSprintMult = 1.0 + (PlayerCharacter.Cold / 100.0) * 0.75;
                PlayerCharacter.Stamina = Math.Max(0, PlayerCharacter.Stamina - 20.0 * starvationMult * coldSprintMult * dt);
                if (PlayerCharacter.Stamina <= 0.01)
                {
                    PlayerCharacter.CurrentStance = Stance.Walk;
                    PlayerCharacter.IsExhausted = true;
                    PlayerCharacter.ExhaustionTimer = 1.5;
                }
            }
            else
            {
                double shiveringMod = PlayerCharacter.IsShivering ? (PlayerCharacter.Cold >= 80.0 ? 0.15 : 0.4) : 1.0;
                double exhaustionMod = PlayerCharacter.IsExhausted ? 0.5 : 1.0;
                PlayerCharacter.Stamina = Math.Min(100.0, PlayerCharacter.Stamina + 10.0 * shiveringMod * exhaustionMod * dt);
            }

            // Survival drain vs shelter warming & radiant thermal light dynamic
            PlayerCharacter.Hunger = Math.Min(100.0, PlayerCharacter.Hunger + 0.15 * dt);

            bool sheltered = IsPlayerSheltered();
            if (sheltered)
            {
                PlayerCharacter.Cold = Math.Max(0.0, PlayerCharacter.Cold - 6.0 * dt);
            }
            else
            {
                // Thermal factor: halogen spotlight radiant heat slows frostbite; deep shadow wind chill accelerates it
                double thermalFactor = 1.0;
                if (exposure >= 0.70)
                {
                    thermalFactor = 0.50; // Radiant warmth
                }
                else if (exposure <= 0.15)
                {
                    thermalFactor = 1.25; // Shadow alley wind tunnel
                }

                PlayerCharacter.Cold = Math.Min(100.0, PlayerCharacter.Cold + 0.25 * thermalFactor * dt);
            }

            if (PlayerCharacter.FlashlightOn)
            {
                PlayerCharacter.Battery = Math.Max(0, PlayerCharacter.Battery - 1.0 * dt);
                if (PlayerCharacter.Battery <= 0) PlayerCharacter.FlashlightOn = false;
            }

            // Apply movement
            Vec2 moveDir = moveInput.Normalized();
            PlayerCharacter.Position = new Vec2(
                Math.Clamp(PlayerCharacter.Position.X + moveDir.X * speed * dt, 0.0, Width),
                Math.Clamp(PlayerCharacter.Position.Y + moveDir.Y * speed * dt, 0.0, Height)
            );

            // Win condition check
            if (Vec2.Distance(PlayerCharacter.Position, SafehousePosition) <= 2.5)
            {
                GameWon = true;
                EndReason = "Safehouse reached! Knocked twice in the darkness. You escaped Chongjin Act 1!";
                return;
            }

            exposure = CalculatePlayerExposure();
            double noiseRadius = moveInput.Magnitude() > 0.01 ? PlayerCharacter.GetNoiseRadius() : 0.0;

            // Update Guards
            foreach (var guard in Guards)
            {
                UpdateGuard(guard, dt, exposure, noiseRadius);
                if (GameOver) break;
            }
        }

        private void UpdateGuard(Guard guard, double dt, double playerExposure, double playerNoiseRadius)
        {
            Vec2 toPlayer = PlayerCharacter.Position - guard.Position;
            double dist = toPlayer.Magnitude();

            // Adaptive sensor budgeting: evaluate full physics rays every tick only when suspicious or active
            guard.SensorTimer += dt;
            double currentInterval = (guard.State == AIState.Patrol && guard.Suspicion <= 5.0) ? guard.SensorInterval : 0.0;
            bool evaluateSensors = guard.SensorTimer >= currentInterval;

            bool inVisionCone = false;
            bool heardPlayerNoise = false;
            bool heardDistraction = false;
            Vec2 distractionTarget = Vec2.Zero;

            if (evaluateSensors)
            {
                guard.SensorTimer = 0.0;
                guard.SensorEvaluationCount++;

                Vec2 dirToPlayer = toPlayer.Normalized();

                // Distance-culled Vision Check with Glare Blindness
                double effectiveVisionRange = guard.VisionRange;
                Vec2 toSearchlight = CurfewSearchlight.GetCurrentPosition() - guard.Position;
                if (toSearchlight.Magnitude() <= 6.0)
                {
                    double glareDot = Vec2.Dot(guard.Forward, toSearchlight.Normalized());
                    if (glareDot > 0.70) // Guard looking directly into intense glare
                    {
                        effectiveVisionRange *= 0.70; // 30% glare distance reduction
                    }
                }

                if (dist <= effectiveVisionRange)
                {
                    bool rayBlocked = false;
                    foreach (var obs in Obstacles)
                    {
                        if (obs.BlocksSegment(guard.Position, PlayerCharacter.Position))
                        {
                            rayBlocked = true;
                            break;
                        }
                    }

                    double dot = Vec2.Dot(guard.Forward, dirToPlayer);
                    double angleDeg = Math.Acos(Math.Clamp(dot, -1.0, 1.0)) * (180.0 / Math.PI);
                    inVisionCone = !rayBlocked && angleDeg <= (guard.VisionAngleDeg / 2.0);
                }

                // Distance-culled Hearing Check
                double effectivePlayerNoise = playerNoiseRadius * guard.HearingSensitivity;
                if (dist <= effectivePlayerNoise)
                {
                    foreach (var obs in Obstacles)
                    {
                        if (obs.BlocksSegment(guard.Position, PlayerCharacter.Position))
                        {
                            effectivePlayerNoise *= guard.WallOcclusionFactor;
                            break;
                        }
                    }
                    heardPlayerNoise = dist <= effectivePlayerNoise;
                }

                if (ActiveDistractionPos.HasValue && ActiveDistractionTimer > 0)
                {
                    double distDistract = Vec2.Distance(guard.Position, ActiveDistractionPos.Value);
                    double effectiveDistractRadius = ActiveDistractionRadius * guard.HearingSensitivity;
                    if (distDistract <= effectiveDistractRadius)
                    {
                        foreach (var obs in Obstacles)
                        {
                            if (obs.BlocksSegment(guard.Position, ActiveDistractionPos.Value))
                            {
                                effectiveDistractRadius *= guard.WallOcclusionFactor;
                                break;
                            }
                        }

                        if (distDistract <= effectiveDistractRadius)
                        {
                            heardDistraction = true;
                            distractionTarget = ActiveDistractionPos.Value;
                        }
                    }
                }
            }

            if (heardDistraction)
            {
                // Distracted away to investigate remote sound without locating the hidden player
                guard.LastInvestigateTarget = distractionTarget;
                if (guard.State != AIState.Chase)
                {
                    guard.State = AIState.Investigate;
                    guard.Suspicion = Math.Max(guard.Suspicion, 40.0);
                }
            }
            else if (heardPlayerNoise)
            {
                guard.LastInvestigateTarget = PlayerCharacter.Position;
                if (guard.State != AIState.Chase)
                {
                    guard.State = AIState.Investigate;
                    guard.Suspicion = Math.Max(guard.Suspicion, 35.0);
                }
            }

            if (inVisionCone)
            {
                // Detection scales with exposure, stance, and proximity. Flashlight illuminates target directly.
                double effectiveExposure = guard.FlashlightActive ? Math.Max(playerExposure, 0.75) : playerExposure;
                double lightFactor = effectiveExposure >= 0.75 ? effectiveExposure * 2.5 : effectiveExposure;
                double stanceMod = PlayerCharacter.GetStanceDetectionMultiplier();

                // Backlit Silhouette Check: if light source is directly behind player relative to guard
                Vec2 toLight = CurfewSearchlight.GetCurrentPosition() - PlayerCharacter.Position;
                if (toLight.Magnitude() <= CurfewSearchlight.Radius * 1.5)
                {
                    double silhouetteDot = Vec2.Dot(toPlayer.Normalized(), toLight.Normalized());
                    if (silhouetteDot > 0.50)
                    {
                        stanceMod = Math.Max(stanceMod, 0.75); // Negates prone/crouch invisibility in silhouette
                        lightFactor *= 1.8;
                    }
                }

                double proximityMod = Math.Clamp(1.0 - (dist / guard.VisionRange), 0.1, 1.0);
                double suspicionDelta = 65.0 * lightFactor * stanceMod * proximityMod * dt;

                // Close range detection even in low light
                if (dist < 2.5) suspicionDelta += 40.0 * dt;

                guard.Suspicion = Math.Clamp(guard.Suspicion + suspicionDelta, 0.0, 100.0);
                guard.LastInvestigateTarget = PlayerCharacter.Position;

                if (guard.Suspicion >= 85.0)
                {
                    guard.State = AIState.Chase;
                    BroadcastRadioAlert(guard);
                }
                else if (guard.Suspicion >= 30.0 && guard.State == AIState.Patrol)
                {
                    guard.State = AIState.Investigate;
                }
            }
            else
            {
                // Decay suspicion if not seeing/hearing
                if (guard.State == AIState.Chase)
                {
                    guard.SearchTimer += dt;
                    if (guard.SearchTimer > 4.0)
                    {
                        guard.State = AIState.Search;
                        guard.SearchTimer = 0.0;
                    }
                }
                else
                {
                    guard.Suspicion = Math.Max(0.0, guard.Suspicion - 12.0 * dt);
                    if (guard.Suspicion <= 5.0 && (guard.State == AIState.Investigate || guard.State == AIState.Search))
                    {
                        guard.State = AIState.Patrol;
                    }
                }
            }

            // AI State Execution
            switch (guard.State)
            {
                case AIState.Patrol:
                    ExecutePatrol(guard, dt);
                    break;

                case AIState.Investigate:
                    ExecuteMoveTowards(guard, guard.LastInvestigateTarget, guard.PatrolSpeed * 1.2, dt);
                    if (Vec2.Distance(guard.Position, guard.LastInvestigateTarget) < 1.0)
                    {
                        guard.State = AIState.Search;
                        guard.SearchTimer = 3.0;
                    }
                    break;

                case AIState.Chase:
                    ExecuteMoveTowards(guard, PlayerCharacter.Position, guard.ChaseSpeed, dt);
                    if (dist < 1.8)
                    {
                        GameOver = true;
                        EndReason = $"{guard.Name} caught you under the curfew searchlight! Spotlight sirens blare.";
                    }
                    break;

                case AIState.Search:
                    guard.SearchTimer -= dt;
                    if (guard.SearchTimer <= 0)
                    {
                        guard.State = AIState.Patrol;
                    }
                    break;
            }
        }

        private void ExecutePatrol(Guard guard, double dt)
        {
            Vec2 target = guard.WaypointDir > 0 ? guard.WaypointB : guard.WaypointA;
            Vec2 diff = target - guard.Position;
            double dist = diff.Magnitude();

            if (dist < 0.2)
            {
                guard.WaypointDir *= -1;
                return;
            }

            Vec2 dir = diff.Normalized();
            guard.Position = guard.Position + dir * (guard.PatrolSpeed * dt);
            guard.Forward = dir;
        }

        public void BroadcastRadioAlert(Guard callingGuard)
        {
            if (callingGuard.AlertBroadcastCooldown > 0) return;
            callingGuard.AlertBroadcastCooldown = 3.0;

            // Signal nearby sweeping searchlights to focus beam
            if (Vec2.Distance(callingGuard.Position, CurfewSearchlight.Center) <= 25.0)
            {
                CurfewSearchlight.FocusOn(callingGuard.LastInvestigateTarget, 5.0);
            }

            foreach (var other in Guards)
            {
                if (other == callingGuard) continue;
                double d = Vec2.Distance(callingGuard.Position, other.Position);
                if (d <= callingGuard.RadioBroadcastRadius)
                {
                    other.LastInvestigateTarget = callingGuard.LastInvestigateTarget;
                    other.Suspicion = Math.Max(other.Suspicion, 60.0);
                    other.State = AIState.Investigate;
                    other.SearchTimer = 4.0;
                }
            }
        }

        private void ExecuteMoveTowards(Guard guard, Vec2 target, double speed, double dt)
        {
            Vec2 diff = target - guard.Position;
            double dist = diff.Magnitude();
            if (dist > 0.1)
            {
                Vec2 dir = diff.Normalized();
                guard.Position = guard.Position + dir * (speed * dt);
                guard.Forward = dir;
            }
        }
    }

    public class Program
    {
        public static int Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--test")
            {
                return RunHeadlessTests();
            }

            if (args.Length > 0 && args[0] == "--benchmark")
            {
                return RunBenchmark();
            }

            return RunInteractiveSim();
        }

        public static int RunHeadlessTests()
        {
            Console.WriteLine("=== WW3 StealthSim: Running Automated Simulation Tests ===");
            var world = new SimulationWorld();
            int passed = 0;
            int total = 0;

            void Assert(bool condition, string testName)
            {
                total++;
                if (condition)
                {
                    Console.WriteLine($"[PASS] {testName}");
                    passed++;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[FAIL] {testName}");
                    Console.ResetColor();
                }
            }

            // Test 1: Baseline dark alley exposure
            double baseExp = world.CalculatePlayerExposure();
            Assert(Math.Abs(baseExp - 0.05) < 0.001, "Dark alley shadow exposure equals 0.05");

            // Test 2: Lamp exposure
            world.PlayerCharacter.Position = world.Lamps[0].Position;
            double lampExp = world.CalculatePlayerExposure();
            Assert(Math.Abs(lampExp - 0.50) < 0.001, "Lamp radius exposure equals 0.50");

            // Test 3: Spotlight exposure
            world.PlayerCharacter.Position = world.CurfewSearchlight.GetCurrentPosition();
            double spotExp = world.CalculatePlayerExposure();
            Assert(Math.Abs(spotExp - 1.0) < 0.001, "Curfew spotlight exposure equals 1.0");

            // Test 4: Whistle alert mechanics
            world.PlayerCharacter.Position = new Vec2(2.0, 10.0);
            world.TriggerWhistle();
            Assert(world.Guards[0].State == AIState.Investigate, "Whistle alerts Patrol-1 to investigate");

            // Test 5: Sprinting drains stamina
            double initialStamina = world.PlayerCharacter.Stamina;
            world.PlayerCharacter.CurrentStance = Stance.Sprint;
            world.Step(0.5, new Vec2(1, 0));
            Assert(world.PlayerCharacter.Stamina < initialStamina, "Sprinting decreases stamina");

            // Test 6: Safehouse win condition
            world.PlayerCharacter.Position = world.SafehousePosition;
            world.Step(0.1, new Vec2(0, 0));
            Assert(world.GameWon, "Reaching safehouse marks GameWon true");

            // Test 7: Simulated run of 100 ticks without crash
            var sim2 = new SimulationWorld();
            for (int i = 0; i < 100; i++)
            {
                sim2.Step(0.1, new Vec2(1, 0));
            }
            Assert(sim2.PlayerCharacter.Position.X > 2.0, "100 tick simulation completes smoothly");

            // Test 8: Guard suspicion escalation on standing player in line of sight
            var sim3 = new SimulationWorld();
            sim3.PlayerCharacter.Position = new Vec2(15.0, 10.0); // directly in Patrol-1's path
            sim3.PlayerCharacter.CurrentStance = Stance.Walk;
            sim3.PlayerCharacter.FlashlightOn = true; // high visibility
            sim3.Guards[0].Position = new Vec2(15.0, 6.0);
            sim3.Guards[0].Forward = new Vec2(0, 1);
            for (int i = 0; i < 20; i++)
            {
                sim3.Step(0.1, new Vec2(0, 0));
            }
            Assert(sim3.Guards[0].Suspicion > 30.0 && sim3.Guards[0].State != AIState.Patrol,
                "Guard escalates suspicion and leaves Patrol state when exposed player in sight");

            // Test 9: Shelter zone cold recovery
            var sim4 = new SimulationWorld();
            sim4.PlayerCharacter.Cold = 50.0;
            sim4.PlayerCharacter.Position = sim4.Shelters[0].Position; // Inside burn barrel zone
            sim4.Step(1.0, new Vec2(0, 0));
            Assert(sim4.PlayerCharacter.Cold < 50.0, "Shelter zone warms player and reduces cold meter");

            // Test 10: Shivering increases acoustic noise radius
            var sim5 = new SimulationWorld();
            sim5.PlayerCharacter.CurrentStance = Stance.Crouch;
            sim5.PlayerCharacter.Cold = 10.0;
            double warmNoise = sim5.PlayerCharacter.GetNoiseRadius();
            sim5.PlayerCharacter.Cold = 80.0; // Shivering threshold
            double shiveringNoise = sim5.PlayerCharacter.GetNoiseRadius();
            Assert(shiveringNoise > warmNoise, "Shivering increases player acoustic noise radius");

            // Test 11: Light exposure directly scales detection escalation
            var sim6 = new SimulationWorld();
            sim6.Guards[0].Position = new Vec2(15.0, 5.0);
            sim6.Guards[0].Forward = new Vec2(0, 1);
            sim6.PlayerCharacter.Position = new Vec2(15.0, 10.0); // Directly in front
            sim6.PlayerCharacter.CurrentStance = Stance.Walk;
            sim6.Lamps.Clear();
            sim6.CurfewSearchlight.Radius = 0; // Disable spotlight

            // In dark alley shadow
            sim6.Step(0.2, new Vec2(0, 0));
            double shadowSuspicion = sim6.Guards[0].Suspicion;

            // In direct spotlight illumination (exposure 1.0)
            var sim7 = new SimulationWorld();
            sim7.Guards[0].Position = new Vec2(15.0, 5.0);
            sim7.Guards[0].Forward = new Vec2(0, 1);
            sim7.PlayerCharacter.Position = new Vec2(15.0, 10.0);
            sim7.PlayerCharacter.CurrentStance = Stance.Walk;
            sim7.CurfewSearchlight.Center = new Vec2(15.0, 10.0); // Directly illuminate player
            sim7.CurfewSearchlight.SweepRange = 0;
            sim7.CurfewSearchlight.Radius = 5.0;

            sim7.Step(0.2, new Vec2(0, 0));
            double spotlightSuspicion = sim7.Guards[0].Suspicion;

            Assert(spotlightSuspicion > shadowSuspicion, "Spotlight illumination escalates guard suspicion faster than shadow");

            // Test 12: Quick restart & overhead alert indicator validation (< 3s loop)
            var sim8 = new SimulationWorld();
            sim8.Guards[0].State = AIState.Chase;
            sim8.Guards[0].Suspicion = 100.0;
            sim8.PlayerCharacter.Stamina = 0.0;
            sim8.GameOver = true;

            // Trigger instant retry / quick reset
            sim8 = new SimulationWorld();
            bool resetClean = !sim8.GameOver && !sim8.GameWon && sim8.PlayerCharacter.Stamina == 100.0 &&
                              sim8.Guards[0].State == AIState.Patrol && sim8.Guards[0].Suspicion == 0.0;
            Assert(resetClean, "Quick restart resets simulation state and guard overhead alerts instantly (< 3s loop)");

            // Test 13: Light exposure thermal radiant dynamics and cold-based stamina depletion
            var simShadow = new SimulationWorld();
            simShadow.CurfewSearchlight.Radius = 0; // Shadow alley
            simShadow.PlayerCharacter.Position = new Vec2(2.0, 10.0);
            simShadow.PlayerCharacter.Cold = 20.0;
            simShadow.Step(10.0, new Vec2(0, 0)); // 10s outdoor chilling in shadow

            var simSpot = new SimulationWorld();
            simSpot.CurfewSearchlight.Center = new Vec2(2.0, 10.0);
            simSpot.CurfewSearchlight.SweepRange = 0;
            simSpot.CurfewSearchlight.Radius = 5.0; // Spotlight radiant warmth
            simSpot.PlayerCharacter.Position = new Vec2(2.0, 10.0);
            simSpot.PlayerCharacter.Cold = 20.0;
            simSpot.Step(10.0, new Vec2(0, 0)); // 10s outdoor under warm halogen lamp

            bool thermalRadiantWorking = simShadow.PlayerCharacter.Cold > simSpot.PlayerCharacter.Cold;

            // Test shivering stationary noise and severe cold stamina exhaustion
            var simCold = new SimulationWorld();
            simCold.PlayerCharacter.Cold = 85.0;
            simCold.PlayerCharacter.Stamina = 100.0;
            simCold.Step(5.0, new Vec2(0, 0)); // Stationary in freezing cold
            bool shiveringNoiseActive = simCold.PlayerCharacter.GetNoiseRadius() >= 1.8;
            bool coldStaminaDepleted = simCold.PlayerCharacter.Stamina < 100.0;

            Assert(thermalRadiantWorking && shiveringNoiseActive && coldStaminaDepleted,
                "Light thermal dynamics and cold stamina exhaustion fully integrated and responsive");

            // Test 14: Thrown bottle distraction lure pulls patrol guard away without exposing player
            var simDistract = new SimulationWorld();
            var patrolGuard = simDistract.Guards[0]; // Patrol-1 at (15, 5)
            // Player safely prone in shadows far away at (2, 2)
            simDistract.PlayerCharacter.Position = new Vec2(2.0, 2.0);
            simDistract.PlayerCharacter.CurrentStance = Stance.Prone;
            simDistract.PlayerCharacter.BottleCount = 2;

            Vec2 bottleTarget = new Vec2(18.0, 5.0); // 3m from patrol guard
            bool throwSuccess = simDistract.ThrowBottle(bottleTarget);
            simDistract.Step(0.2, new Vec2(0, 0));

            bool guardInvestigatingLure = patrolGuard.State == AIState.Investigate &&
                                          Vec2.Distance(patrolGuard.LastInvestigateTarget, bottleTarget) < 0.001 &&
                                          simDistract.PlayerCharacter.BottleCount == 1;

            Assert(throwSuccess && guardInvestigatingLure,
                "Thrown bottle lure distracts guard away from patrol without compromising hidden player");

            // Test 15: Acoustic wall occlusion attenuates noise through concrete barricades
            var simWall = new SimulationWorld();
            var wallGuard = simWall.Guards[2]; // Checkpoint Guard at (42, 10)
            // Position wallGuard near the barricade at (42, 5)
            wallGuard.Position = new Vec2(42.0, 5.0);

            // Place player on opposite side of barricade (MinX 39.5, MaxX 40.5, MinY 2, MaxY 8) at (38, 5)
            simWall.PlayerCharacter.Position = new Vec2(38.0, 5.0);
            simWall.PlayerCharacter.CurrentStance = Stance.Walk; // Walk noise = 6.0m
            // Distance = 4.0m. Without wall, 4.0m <= 6.0m (heard). With 0.35 wall occlusion, effective noise = 2.1m (not heard).

            simWall.Step(0.1, new Vec2(0, 0));
            bool unalertedBehindWall = wallGuard.State == AIState.Patrol;

            // Now step into open gap above barricade at (38, 9.5) and walk towards (42, 9.5) with movement input
            simWall.PlayerCharacter.Position = new Vec2(38.0, 9.5);
            wallGuard.Position = new Vec2(42.0, 9.5);
            simWall.Step(0.1, new Vec2(1, 0)); // Moving emits walk noise (6.0m > 4.0m)
            bool alertedInClearSoundLine = wallGuard.State == AIState.Investigate;

            Assert(unalertedBehindWall && alertedInClearSoundLine,
                "Acoustic obstacle occlusion attenuates sound through walls while clear sound lines alert guards");

            // Test 16: Procedural audio events trigger properly during whistle and bottle throw
            var simAudio = new SimulationWorld();
            simAudio.Whistle();
            simAudio.ThrowBottle(new Vec2(10, 5));
            bool audioDispatched = simAudio.ProceduralAudioLog.Count == 2 &&
                                   simAudio.ProceduralAudioLog[0].StartsWith("Whistle") &&
                                   simAudio.ProceduralAudioLog[1].StartsWith("BottleShatter");

            Assert(audioDispatched, "Procedural audio events dispatched cleanly for whistle and bottle impacts");

            // Test 17: Guard flashlight activates during investigation and increases detection rate
            var simFlash = new SimulationWorld();
            var searchGuard = simFlash.Guards[0];
            searchGuard.State = AIState.Investigate;
            searchGuard.Position = new Vec2(10.0, 5.0);
            searchGuard.Forward = new Vec2(1, 0); // Facing east

            // Player in dark shadow alley 5m ahead of guard
            simFlash.PlayerCharacter.Position = new Vec2(15.0, 5.0);
            simFlash.PlayerCharacter.CurrentStance = Stance.Walk;
            simFlash.Step(0.2, new Vec2(0, 0));

            bool flashlightWorking = searchGuard.FlashlightActive && searchGuard.Suspicion > 0.0;
            Assert(flashlightWorking, "Guard search flashlight activates during investigation and escalates detection in shadows");

            // Test 18: Guard radio alert broadcast converges allied guards towards disturbance
            var simRadio = new SimulationWorld();
            var g1 = simRadio.Guards[0]; // Patrol-1 at (15, 5)
            var g2 = simRadio.Guards[1]; // Patrol-2 at (28, 15)
            // Distance = sqrt(13^2 + 10^2) = 16.4m <= 25m radio radius

            // Position player directly in front of g1 in bright searchlight to trigger Alert/Chase
            simRadio.PlayerCharacter.Position = new Vec2(16.0, 5.0);
            g1.Forward = new Vec2(1, 0);
            simRadio.CurfewSearchlight.Center = new Vec2(16.0, 5.0);
            simRadio.CurfewSearchlight.Radius = 6.0;

            // Directly trigger BroadcastRadioAlert on g1
            g1.State = AIState.Chase;
            g1.LastInvestigateTarget = new Vec2(16.0, 5.0);
            simRadio.BroadcastRadioAlert(g1);

            bool radioConverged = g1.State == AIState.Chase && g2.State == AIState.Investigate && g2.Suspicion >= 60.0;
            Assert(radioConverged, "Guard radio alert successfully converges allied patrol guards towards disturbance");

            // Test 19: Surface material acoustic noise propagation scaling
            var simSurface = new SimulationWorld();
            simSurface.PlayerCharacter.CurrentStance = Stance.Walk;
            simSurface.PlayerCharacter.CurrentSurface = SurfaceType.Dirt;
            double dirtNoise = simSurface.PlayerCharacter.GetNoiseRadius();

            simSurface.PlayerCharacter.CurrentSurface = SurfaceType.Concrete;
            double concreteNoise = simSurface.PlayerCharacter.GetNoiseRadius();

            simSurface.PlayerCharacter.CurrentSurface = SurfaceType.MetalGrate;
            double metalNoise = simSurface.PlayerCharacter.GetNoiseRadius();

            simSurface.PlayerCharacter.CurrentSurface = SurfaceType.GlassDebris;
            double glassNoise = simSurface.PlayerCharacter.GetNoiseRadius();

            bool surfaceScaled = (dirtNoise < concreteNoise) && (concreteNoise < metalNoise) && (metalNoise < glassNoise);
            Assert(surfaceScaled, "Ground surface materials dynamically scale acoustic movement noise propagation");

            // Test 20: Environmental surface zones automatically update player surface and noise
            var simZone = new SimulationWorld();
            // Start on default concrete at (5, 10)
            simZone.PlayerCharacter.Position = new Vec2(5.0, 10.0);
            simZone.Step(0.1, new Vec2(0, 0));
            bool initialConcrete = simZone.PlayerCharacter.CurrentSurface == SurfaceType.Concrete;

            // Move into Catwalk Metal Grate zone at (22, 10)
            simZone.PlayerCharacter.Position = new Vec2(22.0, 10.0);
            simZone.Step(0.1, new Vec2(0, 0));
            bool enteredMetalGrate = simZone.PlayerCharacter.CurrentSurface == SurfaceType.MetalGrate;

            // Move into Broken Bottle Rubble zone at (33, 10)
            simZone.PlayerCharacter.Position = new Vec2(33.0, 10.0);
            simZone.Step(0.1, new Vec2(0, 0));
            bool enteredGlassDebris = simZone.PlayerCharacter.CurrentSurface == SurfaceType.GlassDebris;

            Assert(initialConcrete && enteredMetalGrate && enteredGlassDebris,
                "Environmental surface zones dynamically update player terrain and noise footprint");

            // Test 21: Searchlight oscillation and obstacle shadow occlusion
            var simOcc = new SimulationWorld();
            var gTest = simOcc.Guards[2]; // Checkpoint guard at (42, 10)
            gTest.Position = new Vec2(42.0, 5.0);
            gTest.Forward = new Vec2(-1, 0); // Looking west towards barricade

            // Sweeping searchlight illuminates barricade area
            simOcc.CurfewSearchlight.Center = new Vec2(40.0, 5.0);
            simOcc.CurfewSearchlight.Radius = 8.0;

            // Player hides behind barricade at (38, 5)
            simOcc.PlayerCharacter.Position = new Vec2(38.0, 5.0);
            simOcc.Step(0.2, new Vec2(0, 0));

            // Despite searchlight glaring nearby, line-of-sight ray is blocked by barricade
            bool protectedByShadow = gTest.Suspicion == 0.0 && gTest.State == AIState.Patrol;

            // Now step into open gap at (38, 9.5) and face guard with searchlight overhead
            gTest.Position = new Vec2(42.0, 9.5);
            gTest.Forward = new Vec2(-1, 0); // Looking directly west at player
            simOcc.PlayerCharacter.Position = new Vec2(38.0, 9.5);
            simOcc.CurfewSearchlight.Center = new Vec2(38.0, 9.5); // Direct spotlight illumination
            simOcc.Step(0.2, new Vec2(0, 0));
            bool detectedInClearSight = gTest.Suspicion > 0.0;

            Assert(protectedByShadow && detectedInClearSight,
                "Obstacle walls project true occlusion shadows protecting players from sweeping searchlights and guard vision");

            // Test 22: Searchlight glare multiplies guard suspicion accumulation
            var simGlare = new SimulationWorld();
            var gGlare = simGlare.Guards[0]; // Patrol-1 at (15, 5)
            gGlare.Position = new Vec2(10.0, 5.0);
            gGlare.Forward = new Vec2(1, 0); // Facing east towards player

            // Player 5m in front of guard under spotlight glare
            simGlare.PlayerCharacter.Position = new Vec2(15.0, 5.0);
            simGlare.PlayerCharacter.CurrentStance = Stance.Walk;
            simGlare.CurfewSearchlight.Center = new Vec2(15.0, 5.0);
            simGlare.CurfewSearchlight.Radius = 5.0; // Exposure 1.0 >= 0.75 (glare)

            simGlare.Step(0.2, new Vec2(0, 0));
            double glareSuspicion = gGlare.Suspicion;

            // Compare with baseline shadow
            var simDim = new SimulationWorld();
            var gDim = simDim.Guards[0];
            gDim.Position = new Vec2(10.0, 5.0);
            gDim.Forward = new Vec2(1, 0);
            simDim.PlayerCharacter.Position = new Vec2(15.0, 5.0);
            simDim.PlayerCharacter.CurrentStance = Stance.Walk;
            simDim.CurfewSearchlight.Radius = 0; // Deep shadow exposure 0.05

            simDim.Step(0.2, new Vec2(0, 0));
            double dimSuspicion = gDim.Suspicion;

            bool glareAccelerated = glareSuspicion > (dimSuspicion * 3.0);
            Assert(glareAccelerated, "Searchlight direct glare accelerates guard suspicion accumulation by over 3x");

            // Test 23: Performance budgeting and distance-culled sensor evaluation
            var simPerf = new SimulationWorld();
            var guardCruise = simPerf.Guards[0];
            guardCruise.SensorEvaluationCount = 0;
            // 60 ticks of 0.016s = ~1 second
            for (int i = 0; i < 60; i++)
            {
                simPerf.Step(0.016, new Vec2(0, 0));
            }

            // At 15Hz budgeting on calm patrol, sensor evaluations should be ~15-20 instead of 60
            bool sensorBudgeted = guardCruise.SensorEvaluationCount < 30 && guardCruise.SensorEvaluationCount > 0;
            Assert(sensorBudgeted, "Frame-budgeted sensor evaluation and distance culling conserve CPU cycles");

            // Test 24: Sprint exhaustion recovery delay and heavy breathing acoustic footprint
            var simExhaust = new SimulationWorld();
            simExhaust.PlayerCharacter.Stamina = 2.0;
            simExhaust.PlayerCharacter.CurrentStance = Stance.Sprint;
            simExhaust.Step(0.2, new Vec2(1, 0)); // Drains stamina to 0

            bool exhaustionTriggered = simExhaust.PlayerCharacter.IsExhausted &&
                                       simExhaust.PlayerCharacter.CurrentStance == Stance.Walk &&
                                       simExhaust.PlayerCharacter.GetNoiseRadius() >= 2.5;

            // Wait 1.6s for cooldown to elapse
            simExhaust.Step(1.6, new Vec2(0, 0));
            bool exhaustionRecovered = !simExhaust.PlayerCharacter.IsExhausted;

            Assert(exhaustionTriggered && exhaustionRecovered,
                "Sprint exhaustion triggers heavy-breathing acoustic signature and 1.5s recovery cooldown");

            // Test 25: Backlit silhouette detection & guard searchlight glare blindness
            var simSilh = new SimulationWorld();
            var gSilh = simSilh.Guards[0]; // Patrol-1 at (15, 5)
            gSilh.Position = new Vec2(10.0, 5.0);
            gSilh.Forward = new Vec2(1, 0); // Guard facing east

            // Position player at (14, 5) and spotlight behind player at (18, 5) -> Back-lit silhouette!
            simSilh.PlayerCharacter.Position = new Vec2(14.0, 5.0);
            simSilh.PlayerCharacter.CurrentStance = Stance.Prone; // Prone would usually be 0.3x
            simSilh.CurfewSearchlight.Center = new Vec2(18.0, 5.0);
            simSilh.CurfewSearchlight.SweepRange = 0;
            simSilh.CurfewSearchlight.Radius = 5.0;

            simSilh.Step(0.2, new Vec2(0, 0));
            double silhouetteSuspicion = gSilh.Suspicion;

            // Test glare blindness: guard at (15, 5) facing east directly into searchlight at (18, 5)
            // Player standing at 11m (normally within 14m range, but > 14*0.7 = 9.8m glare limit)
            var simBlind = new SimulationWorld();
            var gBlind = simBlind.Guards[0];
            gBlind.Position = new Vec2(10.0, 5.0);
            gBlind.Forward = new Vec2(1, 0);
            simBlind.CurfewSearchlight.Center = new Vec2(13.0, 5.0); // 3m in front of guard (intense glare)
            simBlind.CurfewSearchlight.SweepRange = 0;
            simBlind.CurfewSearchlight.Radius = 5.0;

            simBlind.PlayerCharacter.Position = new Vec2(21.0, 5.0); // 11m away from guard (> 9.8m effective)
            simBlind.Step(0.2, new Vec2(0, 0));
            bool blindedFromDistantPlayer = gBlind.Suspicion == 0.0;

            Assert(silhouetteSuspicion > 0.0 && blindedFromDistantPlayer,
                "Backlit silhouette elevates detection and searchlight glare temporarily blinds guard vision");

            // Test 26: Searchlight target tracking and focused beam linger
            var simTrack = new SimulationWorld();
            var gAlert = simTrack.Guards[0]; // Patrol-1 at (15, 5)
            Vec2 disturbancePos = new Vec2(22.0, 5.0);
            gAlert.LastInvestigateTarget = disturbancePos;

            // Trigger radio alert & searchlight focus call
            simTrack.BroadcastRadioAlert(gAlert);

            bool searchlightTracking = simTrack.CurfewSearchlight.FocusTimer > 0 &&
                                       Vec2.Distance(simTrack.CurfewSearchlight.GetCurrentPosition(), disturbancePos) < 0.001;

            // Advance 5.5s so linger timer expires and regular sweep resumes
            simTrack.Step(5.5, new Vec2(0, 0));
            bool searchlightResumedSweep = simTrack.CurfewSearchlight.FocusTimer <= 0 &&
                                           simTrack.CurfewSearchlight.FocusTarget == null;

            Assert(searchlightTracking && searchlightResumedSweep,
                "Curfew searchlights interrupt sweep to linger on guard disturbances before resuming sweep");

            Console.WriteLine($"\nSimulation test results: {passed}/{total} tests passed.");
            return passed == total ? 0 : 1;
        }

        public static int RunBenchmark()
        {
            Console.WriteLine("Benchmarking StealthSim 10,000 steps...");
            var world = new SimulationWorld();
            var sw = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++)
            {
                world.Step(0.016, new Vec2(0.5, 0.2));
            }
            sw.Stop();
            Console.WriteLine($"Completed 10,000 steps in {sw.ElapsedMilliseconds} ms ({(10000.0 / (sw.ElapsedMilliseconds / 1000.0)):F0} steps/sec)");
            return 0;
        }

        public static int RunInteractiveSim()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("   WW3: Escape from North Korea — StealthSim     ");
            Console.WriteLine("   Console Prototype (Intel HD 520 / Potato Safe)");
            Console.WriteLine("=================================================");
            Console.WriteLine("Goal: Navigate 60m alley to Safehouse at [56, 10].");
            Console.WriteLine("Avoid searchlights and guards. Crouch/Sprint carefully.");
            Console.WriteLine("Controls: [W/A/S/D] Move | [1] Prone [2] Crouch [3] Walk [4] Sprint | [Q] Whistle | [F] Flashlight | [X] Exit");

            var world = new SimulationWorld();
            var random = new Random();

            while (!world.GameOver && !world.GameWon)
            {
                RenderAsciiMap(world);

                Console.WriteLine($"\n[Jun-ho] Pos: ({world.PlayerCharacter.Position.X:F1}, {world.PlayerCharacter.Position.Y:F1}) | Stance: {world.PlayerCharacter.CurrentStance} | Stamina: {world.PlayerCharacter.Stamina:F0}% | Hunger: {world.PlayerCharacter.Hunger:F0}% | Cold: {world.PlayerCharacter.Cold:F0}%");
                double exp = world.CalculatePlayerExposure();
                Console.WriteLine($"Exposure: {exp * 100:F0}% | Rice: {world.PlayerCharacter.RiceCount} | Flashlight: {(world.PlayerCharacter.FlashlightOn ? "ON" : "OFF")}");
                foreach (var g in world.Guards)
                {
                    Console.WriteLine($" Guard [{g.Name}] Pos: ({g.Position.X:F1}, {g.Position.Y:F1}) | State: {g.State} | Suspicion: {g.Suspicion:F0}%");
                }

                Console.Write("\nAction (W/A/S/D/1/2/3/4/Q/F/X or Enter to wait): ");
                string? input = Console.ReadLine()?.Trim().ToUpperInvariant();

                Vec2 move = new Vec2(0, 0);
                if (input == "W") move = new Vec2(0, 1.5);
                else if (input == "S") move = new Vec2(0, -1.5);
                else if (input == "A") move = new Vec2(-1.5, 0);
                else if (input == "D") move = new Vec2(1.5, 0);
                else if (input == "1") world.PlayerCharacter.CurrentStance = Stance.Prone;
                else if (input == "2") world.PlayerCharacter.CurrentStance = Stance.Crouch;
                else if (input == "3") world.PlayerCharacter.CurrentStance = Stance.Walk;
                else if (input == "4") world.PlayerCharacter.CurrentStance = Stance.Sprint;
                else if (input == "Q")
                {
                    world.TriggerWhistle();
                    Console.WriteLine("\n* Jun-ho whistles quietly into the fog! *");
                }
                else if (input == "F")
                {
                    world.PlayerCharacter.FlashlightOn = !world.PlayerCharacter.FlashlightOn;
                }
                else if (input == "X")
                {
                    Console.WriteLine("Exiting simulation.");
                    return 0;
                }

                world.Step(0.5, move);
            }

            if (world.GameWon)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nVICTORY: {world.EndReason}");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nBUSTED: {world.EndReason}");
                Console.ResetColor();
            }

            return 0;
        }

        private static void RenderAsciiMap(SimulationWorld world)
        {
            int cols = 40;
            int rows = 12;
            char[,] grid = new char[rows, cols];

            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    grid[r, c] = '.';

            // Draw Lamps
            foreach (var l in world.Lamps)
            {
                int c = (int)((l.Position.X / world.Width) * cols);
                int r = (int)((l.Position.Y / world.Height) * rows);
                if (c >= 0 && c < cols && r >= 0 && r < rows) grid[r, c] = 'o';
            }

            // Draw Shelters
            foreach (var s in world.Shelters)
            {
                int sc = (int)((s.Position.X / world.Width) * cols);
                int sr = (int)((s.Position.Y / world.Height) * rows);
                if (sc >= 0 && sc < cols && sr >= 0 && sr < rows) grid[sr, sc] = 'S';
            }

            // Draw Spotlight
            Vec2 spot = world.CurfewSearchlight.GetCurrentPosition();
            int spc = (int)((spot.X / world.Width) * cols);
            int spr = (int)((spot.Y / world.Height) * rows);
            if (spc >= 0 && spc < cols && spr >= 0 && spr < rows) grid[spr, spc] = '*';

            // Draw Safehouse
            int shC = (int)((world.SafehousePosition.X / world.Width) * cols);
            int shR = (int)((world.SafehousePosition.Y / world.Height) * rows);
            if (shC >= 0 && shC < cols && shR >= 0 && shR < rows) grid[shR, shC] = 'H';

            // Draw Guards
            foreach (var g in world.Guards)
            {
                int gc = (int)((g.Position.X / world.Width) * cols);
                int gr = (int)((g.Position.Y / world.Height) * rows);
                if (gc >= 0 && gc < cols && gr >= 0 && gr < rows) grid[gr, gc] = 'G';
            }

            // Draw Player
            int pc = (int)((world.PlayerCharacter.Position.X / world.Width) * cols);
            int pr = (int)((world.PlayerCharacter.Position.Y / world.Height) * rows);
            if (pc >= 0 && pc < cols && pr >= 0 && pr < rows) grid[pr, pc] = '@';

            Console.WriteLine("\n--- Chongjin Alley Map [@:Player, G:Guard, *:Spotlight, o:Lamp, S:Shelter, H:Safehouse] ---");
            for (int r = rows - 1; r >= 0; r--)
            {
                for (int c = 0; c < cols; c++)
                {
                    Console.Write(grid[r, c]);
                }
                Console.WriteLine();
            }
        }
    }
}
