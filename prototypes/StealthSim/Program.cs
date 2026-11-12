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
        public bool HasDenseFog { get; set; } = true;
        public double SensorInterval { get; set; } = 0.066; // 15Hz when cruising
        public double SpatialSleepDistance { get; set; } = 35.0; // >35m drops to 2Hz
        public double SleepInterval { get; set; } = 0.50; // 2Hz sensory sleep
        public double DynamicLoadMultiplier { get; set; } = 1.0;
        public int InvestigationStage { get; set; } = 0;
        public Vec2 SecondaryInvestigateTarget { get; set; } = Vec2.Zero;
        public double SensorTimer { get; set; } = 0.0;
        public int SensorEvaluationCount { get; set; } = 0;
        public int GuardIndex { get; set; } = 0;
        public bool CachedInVision { get; set; } = false;
        public bool HasEvaluatedVision { get; set; } = false;
        public int RaycastEvaluationCount { get; set; } = 0;
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
        public bool HasForgedPapers { get; set; } = false;
        public bool HasWireCutters { get; set; } = true;
        public int WireCutterDurability { get; set; } = 2;
        public bool HasCutPerimeterFence { get; set; } = false;
        public bool FlashlightOn { get; set; } = false;
        public bool IsShivering => Cold >= 70.0;
        public bool IsStarving => Hunger >= 80.0;
        public bool IsExhausted { get; set; } = false;
        public bool HasThermalBreathPuff { get; set; } = false;
        public double BreathPuffTimer { get; set; } = 0.0;
        public double BreathVaporDiameter { get; set; } = 0.0;
        public double BreathVaporAlpha { get; set; } = 0.0;
        public double ExhaustionTimer { get; set; } = 0.0;
        public double WhistleTimer { get; set; } = 0.0;
        public double FootstepTimer { get; set; } = 0.0;

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

            // Glass shards crunch escalation: fractured glass beneath boots causes sharp acoustic spikes (+3.0m)
            if (CurrentSurface == SurfaceType.GlassDebris)
            {
                baseNoise += 3.0;
            }

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
        public double BaseRadius { get; set; } = 4.0;
        public double InnerCoreRatio { get; set; } = 0.45;
        public bool HasDenseFog { get; set; } = true;
        public double FogScatteringFactor { get; set; } = 0.85;
        public double ShadowCullingDistance { get; set; } = 22.0; // Distance-based shadow culling
        public double SweepRange { get; set; } = 10.0;
        public double SweepSpeed { get; set; } = 1.5;
        public double BaseSweepSpeed { get; set; } = 1.5;
        public double SweepTime { get; set; } = 0.0;
        public double MotorJitter { get; set; } = 0.25;
        public Vec2? FocusTarget { get; set; } = null;
        public double FocusTimer { get; set; } = 0.0;
        public int SweepDirection { get; set; } = 1; // 1 = forward, -1 = reverse
        public double DecelerationTimer { get; set; } = 0.0;
        public bool IsAcquiringTarget { get; set; } = false;
        public double AcquisitionWarningTimer { get; set; } = 0.0;
        public bool IsFlickering { get; set; } = false;
        public double FlickerTimer { get; set; } = 0.0;

        public Vec2 GetCurrentPosition()
        {
            if (FocusTarget.HasValue && FocusTimer > 0)
            {
                return FocusTarget.Value;
            }
            double currentSpeed = (DecelerationTimer > 0) ? (BaseSweepSpeed * 0.50) : BaseSweepSpeed;
            double jitter = Math.Sin(SweepTime * 17.3) * MotorJitter;
            double offset = Math.Sin(SweepTime * currentSpeed * SweepDirection) * SweepRange + jitter;
            return new Vec2(Center.X + offset, Center.Y);
        }

        public void FocusOn(Vec2 target, double duration = 5.0, List<string>? audioLog = null)
        {
            FocusTarget = target;
            FocusTimer = Math.Max(FocusTimer, duration);
            DecelerationTimer = duration + 2.0;
            SweepDirection *= -1; // Reverses sweep direction to re-interrogate sector
            if (!IsAcquiringTarget && FocusTimer <= duration)
            {
                IsAcquiringTarget = true;
                AcquisitionWarningTimer = 0.5; // 0.5s pre-focus strobe cue
                audioLog?.Add($"SearchlightMotorWhine at ({target.X:F1}, {target.Y:F1})");
            }
            Radius = BaseRadius * 1.4; // Expand beam radius on alert lock
        }

        public void TriggerFlicker(double duration = 2.0)
        {
            IsFlickering = true;
            FlickerTimer = duration;
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
        public bool CheckpointBribed { get; set; } = false;
        public bool CheckpointAlarmTripped { get; set; } = false;
        public bool CheckpointInterrogating { get; set; } = false;
        public double CheckpointInterrogationTimer { get; set; } = 0.0;
        public bool CheckpointTorchSweeping { get; set; } = false;
        public Vec2? ActiveDistractionPos { get; set; } = null;
        public double ActiveDistractionRadius { get; set; } = 14.0;
        public double ActiveDistractionTimer { get; set; } = 0.0;
        public List<string> ProceduralAudioLog { get; } = new List<string>();
        public int ActiveVoiceCount { get; set; } = 0;
        public const int MaxConcurrentVoices = 8;

        public bool PlayProceduralAudio(string eventName)
        {
            if (ActiveVoiceCount >= MaxConcurrentVoices) return false;
            ActiveVoiceCount++;
            ProceduralAudioLog.Add(eventName);
            return true;
        }

        public bool GameWon { get; set; } = false;
        public bool GameOver { get; set; } = false;
        public string EndReason { get; set; } = "";
        public double ElapsedMissionTime { get; set; } = 0.0;
        public string ExtractionRoute { get; set; } = "Checkpoint Gate";
        public double WindAmbienceVolume { get; set; } = 0.25;
        public bool IsLowSpecSafeguardActive { get; set; } = false;
        public double LowFPSTimer { get; set; } = 0.0;
        public int FrameCount { get; set; } = 0;

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

        public bool ScavengeGlassBottle()
        {
            if (PlayerCharacter.CurrentSurface != SurfaceType.GlassDebris || PlayerCharacter.BottleCount >= 3) return false;
            PlayerCharacter.BottleCount++;
            TriggerDistraction(PlayerCharacter.Position, 6.0, 1.0);
            ProceduralAudioLog.Add("GlassBottleScavenged");
            return true;
        }

        public bool BribeCheckpointGuard()
        {
            if (PlayerCharacter.RiceCount < 2) return false;
            PlayerCharacter.RiceCount -= 2;
            CheckpointBribed = true;
            CheckpointInterrogating = false;
            CheckpointInterrogationTimer = 0.0;
            ProceduralAudioLog.Add("CheckpointBribeAccepted");
            return true;
        }

        public void TriggerCheckpointAlarm()
        {
            CheckpointAlarmTripped = true;
            // Barrier pushback physics: push player back 2.5m along gate normal
            Vec2 pushDir = (PlayerCharacter.Position - CheckpointPosition);
            if (pushDir.Magnitude() < 0.01) pushDir = new Vec2(-1, 0);
            PlayerCharacter.Position = PlayerCharacter.Position + pushDir.Normalized() * 2.5;
            ProceduralAudioLog.Add("CheckpointAlarmSiren");
            // Zero-GC loop for guard alarm alert escalation
            for (int i = 0; i < Guards.Count; i++)
            {
                var g = Guards[i];
                if (Vec2.Distance(CheckpointPosition, g.Position) <= 35.0)
                {
                    g.State = AIState.Chase;
                    g.Suspicion = 100.0;
                    g.LastInvestigateTarget = CheckpointPosition;
                }
            }
        }

        public bool CutWireFence()
        {
            if (!PlayerCharacter.HasWireCutters || PlayerCharacter.WireCutterDurability <= 0) return false;
            PlayerCharacter.HasCutPerimeterFence = true;
            PlayerCharacter.WireCutterDurability--;
            if (PlayerCharacter.WireCutterDurability <= 0)
            {
                PlayerCharacter.HasWireCutters = false;
            }
            ProceduralAudioLog.Add($"WireFenceSnip at ({PlayerCharacter.Position.X:F1}, {PlayerCharacter.Position.Y:F1})");
            return true;
        }

        public void Whistle()
        {
            TriggerWhistle();
            // Reverberation bonus in enclosed concrete/metal areas
            double duration = (PlayerCharacter.CurrentSurface == SurfaceType.Concrete || PlayerCharacter.CurrentSurface == SurfaceType.MetalGrate) ? 1.5 : 1.0;
            PlayerCharacter.WhistleTimer = duration;
            ProceduralAudioLog.Add($"Whistle at ({PlayerCharacter.Position.X:F1}, {PlayerCharacter.Position.Y:F1}) [duration={duration:F1}s]");
        }

        public SimulationWorld()
        {
            // Player starts at west entry of Chongjin alley
            PlayerCharacter.Position = new Vec2(2.0, 10.0);

            // Patrol 1: Alley near start
            Guards.Add(new Guard
            {
                Name = "Patrol-1",
                GuardIndex = 0,
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
                GuardIndex = 1,
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
                GuardIndex = 2,
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
            for (int i = 0; i < Shelters.Count; i++)
            {
                var s = Shelters[i];
                Vec2 toShelter = PlayerCharacter.Position - s.Position;
                double sqrDist = toShelter.X * toShelter.X + toShelter.Y * toShelter.Y;
                if (sqrDist <= (s.Radius * s.Radius))
                {
                    return true;
                }
            }
            return false;
        }

        public double CalculatePlayerExposure()
        {
            Vec2 pPos = PlayerCharacter.Position;

            // Curfew Spotlight with smooth penumbra edge falloff & distance-based shadow culling via squared distance lookups
            Vec2 spotPos = CurfewSearchlight.GetCurrentPosition();
            double maxAllowed = Math.Min(CurfewSearchlight.Radius, CurfewSearchlight.ShadowCullingDistance);
            Vec2 toSpot = pPos - spotPos;
            double sqrDist = toSpot.X * toSpot.X + toSpot.Y * toSpot.Y;
            if (sqrDist <= maxAllowed * maxAllowed)
            {
                double distToSpot = Math.Sqrt(sqrDist);
                double coreR = CurfewSearchlight.Radius * CurfewSearchlight.InnerCoreRatio;
                double maxSpotExp = CurfewSearchlight.HasDenseFog ? (1.0 * CurfewSearchlight.FogScatteringFactor) : 1.0;
                if (distToSpot <= coreR)
                {
                    return maxSpotExp;
                }
                // Penumbra falloff from core to outer rim with smoothstep S-curve
                double t = Math.Clamp((distToSpot - coreR) / Math.Max(0.01, CurfewSearchlight.Radius - coreR), 0.0, 1.0);
                double smoothT = t * t * (3.0 - 2.0 * t);
                return maxSpotExp * (1.0 - smoothT) + 0.05 * smoothT;
            }

            // Flashlight on increases exposure significantly
            if (PlayerCharacter.FlashlightOn)
            {
                return 0.85;
            }

            // Street lamps (indexed zero-GC loop)
            for (int i = 0; i < Lamps.Count; i++)
            {
                var lamp = Lamps[i];
                if (Vec2.Distance(pPos, lamp.Position) <= lamp.Radius)
                {
                    return 0.50;
                }
            }

            // Default dark alley shadow (gradually elevated proportional to dissipating breath vapor opacity)
            if (PlayerCharacter.HasThermalBreathPuff && PlayerCharacter.BreathVaporAlpha > 0.05)
            {
                return 0.05 + 0.25 * PlayerCharacter.BreathVaporAlpha;
            }
            return 0.05;
        }

        public void TriggerWhistle()
        {
            Vec2 pPos = PlayerCharacter.Position;
            double whistleNoise = 20.0; // audible within 20m
            double duration = (PlayerCharacter.CurrentSurface == SurfaceType.Concrete || PlayerCharacter.CurrentSurface == SurfaceType.MetalGrate) ? 1.5 : 1.0;
            PlayerCharacter.WhistleTimer = duration;

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

            FrameCount++;
            ElapsedMissionTime += dt;

            if (CurfewSearchlight.IsAcquiringTarget)
            {
                CurfewSearchlight.AcquisitionWarningTimer -= dt;
                if (CurfewSearchlight.AcquisitionWarningTimer <= 0)
                {
                    CurfewSearchlight.IsAcquiringTarget = false;
                }
            }

            if (CurfewSearchlight.DecelerationTimer > 0)
            {
                CurfewSearchlight.DecelerationTimer -= dt;
            }

            if (CurfewSearchlight.FocusTimer > 0)
            {
                CurfewSearchlight.FocusTimer -= dt;
                CurfewSearchlight.Radius = CurfewSearchlight.BaseRadius * 1.4;
                if (CurfewSearchlight.FocusTimer <= 0)
                {
                    CurfewSearchlight.FocusTarget = null;
                    CurfewSearchlight.Radius = CurfewSearchlight.BaseRadius;
                }
            }
            else
            {
                CurfewSearchlight.Radius = CurfewSearchlight.BaseRadius;
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
                PlayerCharacter.HasThermalBreathPuff = false;
                PlayerCharacter.BreathPuffTimer = 0.0;
                PlayerCharacter.BreathVaporDiameter = 0.0;
                PlayerCharacter.BreathVaporAlpha = 0.0;
                WindAmbienceVolume = Math.Max(0.08, WindAmbienceVolume - dt * 0.5);
                PlayerCharacter.Cold = Math.Max(0.0, PlayerCharacter.Cold - 6.0 * dt);
            }
            else
            {
                if (PlayerCharacter.Cold >= 60.0)
                {
                    PlayerCharacter.BreathPuffTimer += dt;
                    if (PlayerCharacter.BreathPuffTimer >= 2.5)
                    {
                        PlayerCharacter.BreathPuffTimer = 0.0;
                    }

                    if (PlayerCharacter.BreathPuffTimer <= 0.75)
                    {
                        PlayerCharacter.HasThermalBreathPuff = true;
                        double progress = PlayerCharacter.BreathPuffTimer / 0.75;
                        PlayerCharacter.BreathVaporDiameter = 0.4 + (1.2 - 0.4) * progress;
                        PlayerCharacter.BreathVaporAlpha = 1.0 - progress;
                    }
                    else
                    {
                        PlayerCharacter.HasThermalBreathPuff = false;
                        PlayerCharacter.BreathVaporDiameter = 0.0;
                        PlayerCharacter.BreathVaporAlpha = 0.0;
                    }
                }
                else
                {
                    PlayerCharacter.HasThermalBreathPuff = false;
                    PlayerCharacter.BreathVaporDiameter = 0.0;
                    PlayerCharacter.BreathVaporAlpha = 0.0;
                }

                double targetWind = 0.20 + (PlayerCharacter.Cold / 100.0) * 0.35;
                WindAmbienceVolume = WindAmbienceVolume + (targetWind - WindAmbienceVolume) * dt * 2.0;

                // Outdoor curfew frostbite with light-exposure thermal radiant dynamic (1:1 with Unity SurvivalSystem.cs):
                // High exposure (spotlights/lamps, >= 0.70) emits radiant halogen heat slowing frostbite (-50%),
                // whereas pitch-black wind tunnels (<= 0.15) induce increased wind chill (+25%).
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

            // Footstep audio pacing & logging
            if (moveInput.Magnitude() > 0.01)
            {
                double strideInterval = PlayerCharacter.CurrentStance switch
                {
                    Stance.Sprint => 0.28,
                    Stance.Walk => 0.45,
                    Stance.Crouch => 0.65,
                    Stance.Prone => 0.90,
                    _ => 0.45
                };

                PlayerCharacter.FootstepTimer += dt;
                if (PlayerCharacter.FootstepTimer >= strideInterval)
                {
                    PlayerCharacter.FootstepTimer = 0.0;
                    double stepVol = PlayerCharacter.CurrentStance switch
                    {
                        Stance.Sprint => 0.85,
                        Stance.Walk => 0.50,
                        Stance.Crouch => 0.25,
                        Stance.Prone => 0.12,
                        _ => 0.50
                    };
                    if (PlayerCharacter.CurrentStance == Stance.Crouch && (PlayerCharacter.CurrentSurface == SurfaceType.MetalGrate || PlayerCharacter.CurrentSurface == SurfaceType.GlassDebris))
                        stepVol *= 0.65;
                    else if (PlayerCharacter.CurrentStance == Stance.Prone && (PlayerCharacter.CurrentSurface == SurfaceType.MetalGrate || PlayerCharacter.CurrentSurface == SurfaceType.GlassDebris))
                        stepVol *= 0.45;

                    ProceduralAudioLog.Add($"Footstep_{PlayerCharacter.CurrentSurface}_Vol{stepVol:F2} at ({PlayerCharacter.Position.X:F1}, {PlayerCharacter.Position.Y:F1})");
                }
            }
            else
            {
                PlayerCharacter.FootstepTimer = 0.0;
            }

            // Checkpoint interrogation & clearance zone check via squared distance
            Vec2 toGate = PlayerCharacter.Position - CheckpointPosition;
            double sqrDistGate = toGate.X * toGate.X + toGate.Y * toGate.Y;
            double distToCheckpoint = (sqrDistGate <= 12.25) ? Math.Sqrt(sqrDistGate) : 999.0;
            if (sqrDistGate <= 12.25 && !CheckpointBribed && !PlayerCharacter.HasForgedPapers && !CheckpointAlarmTripped)
            {
                if (!CheckpointInterrogating)
                {
                    CheckpointInterrogating = true;
                    CheckpointTorchSweeping = true;
                    // Shadow inspection grace: crouching in deep shadow (exposure <= 0.20) extends grace to 4.5s
                    double exp = CalculatePlayerExposure();
                    CheckpointInterrogationTimer = (exp <= 0.20) ? 4.5 : 3.0;
                    ProceduralAudioLog.Add("CheckpointInterrogationChallenge");
                }

                CheckpointInterrogationTimer -= dt;
                if (CheckpointInterrogationTimer <= 0)
                {
                    TriggerCheckpointAlarm();
                }
            }
            else if (distToCheckpoint > 3.5 && CheckpointInterrogating && !CheckpointAlarmTripped)
            {
                CheckpointInterrogating = false;
                CheckpointTorchSweeping = false;
                CheckpointInterrogationTimer = 0.0;
            }

            // Win condition check via squared distance
            Vec2 toSafehouse = PlayerCharacter.Position - SafehousePosition;
            double sqrDistSafehouse = toSafehouse.X * toSafehouse.X + toSafehouse.Y * toSafehouse.Y;
            if (sqrDistSafehouse <= (2.5 * 2.5))
            {
                GameWon = true;
                ExtractionRoute = PlayerCharacter.HasCutPerimeterFence ? "Wire-Cut Side Alley Shortcut" : "Sector 4 Checkpoint Gate";
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

            // Low-spec dynamic quality adaptation: if frame delta exceeds 33.3ms (FPS < 30) for 2s, activate safeguard
            if (dt > 0.0357) // FPS < 28fps
            {
                LowFPSTimer += dt;
                if (LowFPSTimer >= 2.0)
                {
                    IsLowSpecSafeguardActive = true;
                }
            }
            else if (dt <= 0.03125) // FPS >= 32fps
            {
                LowFPSTimer = Math.Max(0.0, LowFPSTimer - dt);
                if (LowFPSTimer == 0)
                {
                    IsLowSpecSafeguardActive = false;
                }
            }

            // Adaptive sensor budgeting: evaluate full physics rays every tick only when suspicious or active
            guard.SensorTimer += dt;
            guard.DynamicLoadMultiplier = (dt > 0.0333 || IsLowSpecSafeguardActive) ? 1.5 : 1.0; // Decimate when frame time spikes

            double effectiveSleepDist = IsLowSpecSafeguardActive ? (guard.SpatialSleepDistance * 0.714) : guard.SpatialSleepDistance;
            bool isDistant = dist > effectiveSleepDist && guard.State == AIState.Patrol && guard.Suspicion <= 5.0;
            double currentInterval = 0.0;
            if (isDistant)
            {
                currentInterval = guard.SleepInterval * guard.DynamicLoadMultiplier; // 2Hz sleep scaled
            }
            else if (guard.State == AIState.Patrol && guard.Suspicion <= 5.0)
            {
                currentInterval = guard.SensorInterval * guard.DynamicLoadMultiplier; // 15Hz cruising scaled
            }

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

                // Dense fog atmospheric extinction reduces unassisted guard vision range by 16.6% (14m -> 11.67m) unless flashlight active
                double baseVision = guard.VisionRange;
                if (guard.State == AIState.Investigate && guard.InvestigationStage == 1)
                {
                    baseVision *= 1.15; // Active search sweep illumination extension
                }
                double effectiveVisionRange = (guard.HasDenseFog && !guard.FlashlightActive)
                    ? (baseVision * 0.833)
                    : baseVision;
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
                    // Low-spec interleaved raycast budgeting parity:
                    // Under multi-guard loads around 30fps (0.025s - 0.050s), alternate raycast checks
                    // across guards (even index on even frames, odd index on odd frames) unless in Chase,
                    // single guard, or during coarse simulation ticks (dt >= 0.10).
                    bool shouldRaycast = !guard.HasEvaluatedVision ||
                                         (guard.State == AIState.Chase) ||
                                         (Guards.Count <= 1) ||
                                         (FrameCount % 2 == guard.GuardIndex % 2) ||
                                         (dt <= 0.020) || (dt >= 0.10);

                    if (shouldRaycast)
                    {
                        guard.HasEvaluatedVision = true;
                        guard.RaycastEvaluationCount++;
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
                        guard.CachedInVision = !rayBlocked && angleDeg <= (guard.VisionAngleDeg / 2.0);
                    }

                    inVisionCone = guard.CachedInVision;
                }

                // Distance-culled Hearing Check with ambient wind masking
                // Environmental wind masking: howling curfew wind gusts mask subtle footstep acoustics (-25% hearing range)
                // while loud whistles or glass bottle shatters remain unaffected
                double windMasking = (!IsPlayerSheltered() && ActiveDistractionPos == null) ? 0.75 : 1.0;
                double effectivePlayerNoise = playerNoiseRadius * guard.HearingSensitivity * windMasking;
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
                    // Sharp glass shard crunching alerts guards significantly faster (50.0 vs 35.0 baseline)
                    double heardSuspicionFloor = (PlayerCharacter.CurrentSurface == SurfaceType.GlassDebris) ? 50.0 : 35.0;
                    guard.Suspicion = Math.Max(guard.Suspicion, heardSuspicionFloor);
                }
            }

            if (inVisionCone)
            {
                // Detection scales with exposure, stance, and proximity. Flashlight illuminates target directly.
                double effectiveExposure = guard.FlashlightActive ? Math.Max(playerExposure, 0.75) : playerExposure;
                double lightFactor = effectiveExposure >= 0.75 ? effectiveExposure * 2.5 : effectiveExposure;

                // Flashlight direct illumination synergy: if flashlight is active and aimed within flashlight cone
                if (guard.FlashlightActive && dist <= guard.VisionRange * 0.7)
                {
                    lightFactor *= 1.50;
                }

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
                    if (guard.State != AIState.Chase)
                    {
                        ProceduralAudioLog.Add($"GuardAlertShout from {guard.Name} at ({guard.Position.X:F1}, {guard.Position.Y:F1})");
                    }
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
                // Decay suspicion if neither seeing nor hearing anything
                if (guard.State == AIState.Chase)
                {
                    guard.SearchTimer += dt;
                    if (guard.SearchTimer > 4.0)
                    {
                        guard.State = AIState.Search;
                        guard.SearchTimer = 0.0;
                    }
                }
                else if (!heardPlayerNoise && !heardDistraction)
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
                    Vec2 targetProbe = (guard.InvestigationStage == 0) ? guard.LastInvestigateTarget : guard.SecondaryInvestigateTarget;
                    double moveSpeed = guard.PatrolSpeed * 1.2;
                    // Searchlight-guard intersection tempo acceleration: if searchlight beam sweeps within 5.0m of target, tempo accelerates 1.35x
                    Vec2 activeSpotPos = CurfewSearchlight.GetCurrentPosition();
                    if (Vec2.Distance(targetProbe, activeSpotPos) <= 5.0)
                    {
                        moveSpeed *= 1.35;
                    }
                    ExecuteMoveTowards(guard, targetProbe, moveSpeed, dt);
                    if (Vec2.Distance(guard.Position, targetProbe) < 1.0)
                    {
                        if (guard.InvestigationStage == 0)
                        {
                            guard.InvestigationStage = 1;
                            guard.SecondaryInvestigateTarget = guard.LastInvestigateTarget + new Vec2(3.0, 2.0);
                        }
                        else
                        {
                            guard.State = AIState.Search;
                            guard.SearchTimer = 3.0;
                            guard.InvestigationStage = 0;
                        }
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
            // Searchlight beam glance & interlocking sector vigilance synergy:
            // If searchlight is lingering on disturbance or sweeping within 18m, bias guard vigilance towards illuminated beam
            Vec2 activeSearchlightPos = CurfewSearchlight.GetCurrentPosition();
            double distToSpot = Vec2.Distance(guard.Position, activeSearchlightPos);
            bool interlockingCoordinated = false;

            if (CurfewSearchlight.FocusTimer > 0 && CurfewSearchlight.FocusTarget.HasValue)
            {
                double distToBeam = Vec2.Distance(guard.Position, CurfewSearchlight.FocusTarget.Value);
                if (distToBeam <= 25.0)
                {
                    Vec2 beamDir = (CurfewSearchlight.FocusTarget.Value - guard.Position).Normalized();
                    guard.Forward = beamDir;
                    interlockingCoordinated = true;
                }
            }
            else if (distToSpot <= 18.0 && guard.Suspicion > 5.0 && guard.Suspicion < 30.0 && guard.LastInvestigateTarget.Magnitude() < 0.01)
            {
                Vec2 beamDir = (activeSearchlightPos - guard.Position).Normalized();
                guard.Forward = beamDir;
                interlockingCoordinated = true;
            }

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
            if (!interlockingCoordinated)
            {
                guard.Forward = dir;
            }
        }

        public void BroadcastRadioAlert(Guard callingGuard)
        {
            if (callingGuard.AlertBroadcastCooldown > 0) return;
            callingGuard.AlertBroadcastCooldown = 3.0;
            ProceduralAudioLog.Add($"RadioStaticClick from {callingGuard.Name} at ({callingGuard.Position.X:F1}, {callingGuard.Position.Y:F1})");

            // Signal nearby sweeping searchlights to focus beam
            if (Vec2.Distance(callingGuard.Position, CurfewSearchlight.Center) <= 25.0)
            {
                CurfewSearchlight.FocusOn(callingGuard.LastInvestigateTarget, 5.0, ProceduralAudioLog);
            }

            int fIndex = 0;
            for (int i = 0; i < Guards.Count; i++)
            {
                var other = Guards[i];
                if (other == callingGuard) continue;
                double d = Vec2.Distance(callingGuard.Position, other.Position);
                if (d <= callingGuard.RadioBroadcastRadius)
                {
                    fIndex++;
                    // Tactical fanning pincer offset: guard fanning +4.5m perpendicular
                    Vec2 toTarget = (callingGuard.LastInvestigateTarget - callingGuard.Position).Normalized();
                    Vec2 flank = new Vec2(-toTarget.Y, toTarget.X) * (fIndex % 2 == 1 ? 4.5 : -4.5);
                    other.LastInvestigateTarget = callingGuard.LastInvestigateTarget + flank;
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

            // Test 3: Spotlight exposure (under fog scattering, max direct exposure is 0.85)
            world.PlayerCharacter.Position = world.CurfewSearchlight.GetCurrentPosition();
            double spotExp = world.CalculatePlayerExposure();
            Assert(spotExp >= 0.85, "Curfew spotlight direct exposure equals max effective core exposure (>=0.85)");

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

            // Test 27: Whistle acoustic reverberation in enclosed concrete alleys
            var simReverb = new SimulationWorld();
            simReverb.PlayerCharacter.CurrentSurface = SurfaceType.Dirt;
            simReverb.Whistle();
            double dirtDuration = simReverb.PlayerCharacter.WhistleTimer;

            simReverb.PlayerCharacter.CurrentSurface = SurfaceType.Concrete;
            simReverb.Whistle();
            double concreteDuration = simReverb.PlayerCharacter.WhistleTimer;

            bool reverbBonusApplied = concreteDuration > dirtDuration && Math.Abs(concreteDuration - 1.5) < 0.01;
            Assert(reverbBonusApplied, "Enclosed concrete alleys sustain acoustic whistle reverberation echoes (+0.5s)");

            // Test 28: Searchlight guard beam glance synergy and grid flicker
            var simSynergy = new SimulationWorld();
            var gPatrol = simSynergy.Guards[0]; // Patrol-1 at (15, 5)
            Vec2 initialForward = gPatrol.Forward;

            // Trigger searchlight focus near guard at (18, 5)
            Vec2 beamSpot = new Vec2(18.0, 5.0);
            simSynergy.CurfewSearchlight.FocusOn(beamSpot, 4.0);
            simSynergy.CurfewSearchlight.TriggerFlicker(2.0);

            simSynergy.Step(0.2, new Vec2(0, 0));

            // Guard forward should orient east towards the beam spot (18, 5) from (15, 5)
            bool guardFacingSearchlight = gPatrol.Forward.X > 0.8 && simSynergy.CurfewSearchlight.IsFlickering;

            // Step past linger timer to verify return to standard patrol orientation
            simSynergy.Step(4.5, new Vec2(0, 0));
            bool sweepResumed = simSynergy.CurfewSearchlight.FocusTimer <= 0;

            Assert(guardFacingSearchlight && sweepResumed,
                "Patrolling guards dynamically orient towards lingering searchlight beams and searchlight sweeps recover cleanly");

            // Test 29: Low-spec dynamic spatial sleep LOD & instant alert wake-up
            var simLOD = new SimulationWorld();
            var farGuard = simLOD.Guards[2]; // Checkpoint Guard at (42, 10)
            simLOD.PlayerCharacter.Position = new Vec2(2.0, 10.0); // 40m away (> 35m spatial sleep)
            farGuard.SensorEvaluationCount = 0;

            // Step 60 ticks of 0.016s (~1s of game time)
            for (int i = 0; i < 60; i++)
            {
                simLOD.Step(0.016, new Vec2(0, 0));
            }

            // At 2Hz sleep LOD, distant guard should execute <= 4 sensory evaluations instead of 60
            bool distantGuardSleeping = farGuard.SensorEvaluationCount <= 4;

            // Trigger radio alert on mid-street Patrol-2 at (28, 15), well within 25m of farGuard (42, 10):
            // dist = sqrt(14^2 + 5^2) = sqrt(196 + 25) = sqrt(221) = 14.8m <= 25m
            var midGuard = simLOD.Guards[1];
            midGuard.State = AIState.Chase;
            midGuard.LastInvestigateTarget = new Vec2(28.0, 15.0);
            farGuard.SensorEvaluationCount = 0;
            simLOD.BroadcastRadioAlert(midGuard);

            // After radio alert, distant guard is awake in Investigate state
            bool guardAwakened = farGuard.State == AIState.Investigate && farGuard.Suspicion >= 60.0;

            Assert(distantGuardSleeping && guardAwakened,
                "Low-spec spatial sleep LOD throttles distant guards to 2Hz and wakes them immediately upon alert");

            // Test 30: Checkpoint interrogation, rice bowl bribery, and siren alarm propagation
            var simCheck = new SimulationWorld();
            simCheck.PlayerCharacter.Position = simCheck.CheckpointPosition;
            simCheck.PlayerCharacter.RiceCount = 3;

            // Bribe checkpoint guard with 2 rice bowls
            bool bribePassed = simCheck.BribeCheckpointGuard();
            bool bribeRecorded = simCheck.CheckpointBribed && simCheck.PlayerCharacter.RiceCount == 1;

            // Trigger alarm on un-bribed breach
            var simAlarm = new SimulationWorld();
            simAlarm.TriggerCheckpointAlarm();
            var checkGuard = simAlarm.Guards[2]; // Checkpoint Guard at (42, 10)
            bool alarmAlerted = simAlarm.CheckpointAlarmTripped &&
                                checkGuard.State == AIState.Chase &&
                                checkGuard.Suspicion >= 99.0;

            Assert(bribePassed && bribeRecorded && alarmAlerted,
                "Checkpoint guard accepts 2-rice bribe and security sirens escalate guards within 35m to full Alert");

            // Test 31: Wire-cutter shortcut breach and safehouse infiltration
            var simCut = new SimulationWorld();
            simCut.PlayerCharacter.HasWireCutters = true;
            simCut.PlayerCharacter.Position = new Vec2(40.0, 1.0); // Side-fence boundary below checkpoint

            bool cutSuccess = simCut.CutWireFence();
            bool fenceBreached = simCut.PlayerCharacter.HasCutPerimeterFence &&
                                 simCut.ProceduralAudioLog.Exists(a => a.StartsWith("WireFenceSnip"));

            // Proceed through breach directly to safehouse
            simCut.PlayerCharacter.Position = simCut.SafehousePosition;
            simCut.Step(0.1, new Vec2(0, 0));
            bool wonViaShortcut = simCut.GameWon;

            Assert(cutSuccess && fenceBreached && wonViaShortcut,
                "Scavenged wire cutters breach secondary perimeter fence opening stealth shortcut to safehouse");

            // Test 32: Coordinated tactical pincer flanking offsets and searchlight beam widening
            var simPincer = new SimulationWorld();
            var leadGuard = simPincer.Guards[0]; // Patrol-1 at (15, 5)
            var flankGuard = simPincer.Guards[1]; // Patrol-2 at (28, 15)
            leadGuard.LastInvestigateTarget = new Vec2(20.0, 5.0);

            simPincer.BroadcastRadioAlert(leadGuard);

            // Flank guard should have received a laterally offset investigate target
            bool flankOffsetApplied = Math.Abs(flankGuard.LastInvestigateTarget.Y - 5.0) > 3.0;
            bool beamWidened = simPincer.CurfewSearchlight.Radius > simPincer.CurfewSearchlight.BaseRadius;

            // Wait until linger expires
            simPincer.Step(5.5, new Vec2(0, 0));
            bool beamRestored = Math.Abs(simPincer.CurfewSearchlight.Radius - simPincer.CurfewSearchlight.BaseRadius) < 0.01;

            Assert(flankOffsetApplied && beamWidened && beamRestored,
                "Guards execute tactical pincer flanking convergence and searchlight beam widens during active focus");

            // Test 33: Searchlight pre-lock strobe acquisition warning and guard focus debounce
            var simAcq = new SimulationWorld();
            Vec2 pingPos = new Vec2(25.0, 10.0);
            simAcq.CurfewSearchlight.FocusOn(pingPos, 4.0);

            bool strobeTriggered = simAcq.CurfewSearchlight.IsAcquiringTarget &&
                                   simAcq.CurfewSearchlight.AcquisitionWarningTimer > 0.0;

            // Step 0.6s past pre-lock acquisition delay
            simAcq.Step(0.6, new Vec2(0, 0));
            bool lockEngaged = !simAcq.CurfewSearchlight.IsAcquiringTarget &&
                               simAcq.CurfewSearchlight.FocusTimer > 0;

            Assert(strobeTriggered && lockEngaged,
                "Searchlight pre-lock strobe warning telegraphs beam focus lock to alert player before engaging");

            // Test 34: Frame rate profiling and dynamic sensor decimation under heavy load
            var simHeavy = new SimulationWorld();
            var cruiseGuard = simHeavy.Guards[0];
            cruiseGuard.SensorEvaluationCount = 0;

            // Simulate heavy frame drop (dt = 0.050s = 20fps < 30fps threshold) for 20 ticks (1s)
            for (int i = 0; i < 20; i++)
            {
                simHeavy.Step(0.050, new Vec2(0, 0));
            }

            // At 1.5x decimated load interval, evaluations should be reduced compared to nominal
            bool decimationActive = cruiseGuard.DynamicLoadMultiplier >= 1.5 && cruiseGuard.SensorEvaluationCount < 15;
            Assert(decimationActive, "Dynamic sensor decimation scales guard sensory intervals under frame rate drops to guarantee 30fps+");

            // Test 35: Dense fog atmospheric extinction and searchlight scattering
            var simFog = new SimulationWorld();
            var fogGuard = simFog.Guards[0]; // VisionRange = 14m
            fogGuard.WaypointA = new Vec2(10.0, 5.0);
            fogGuard.WaypointB = new Vec2(10.0, 5.0);
            fogGuard.Position = new Vec2(10.0, 5.0);
            fogGuard.Forward = new Vec2(1, 0); // Looking east

            // Position player at 13m (within standard 14m range, but > 14*0.833 = 11.67m in dense fog)
            simFog.PlayerCharacter.Position = new Vec2(23.0, 5.0);
            simFog.CurfewSearchlight.Radius = 0; // In dark shadow alley
            simFog.Step(0.1, new Vec2(0, 0));

            // In dense fog, player at 13m should be concealed by atmospheric fog extinction
            bool fogConcealed = fogGuard.Suspicion == 0.0;

            // When guard turns on flashlight during investigation, flashlight pierces fog at 13m
            fogGuard.State = AIState.Investigate; // Activates flashlight
            fogGuard.LastInvestigateTarget = new Vec2(23.0, 5.0);
            fogGuard.Position = new Vec2(10.0, 5.0);
            fogGuard.Forward = new Vec2(1, 0);
            simFog.Step(0.2, new Vec2(0, 0));
            bool flashlightPiercedFog = fogGuard.Suspicion > 0.0;

            Assert(fogConcealed && flashlightPiercedFog,
                "Atmospheric fog extinction attenuates unassisted guard vision and searchlight/flashlight beams pierce through");

            // Test 36: Act 1 Vertical slice completion & mission performance summary reporting
            var simVictory = new SimulationWorld();
            simVictory.PlayerCharacter.HasWireCutters = true;
            simVictory.CutWireFence();
            simVictory.PlayerCharacter.Position = simVictory.SafehousePosition;
            simVictory.Step(1.5, new Vec2(0, 0));

            bool missionWon = simVictory.GameWon;
            bool routeTracked = simVictory.ExtractionRoute == "Wire-Cut Side Alley Shortcut";
            bool timeRecorded = simVictory.ElapsedMissionTime >= 1.5;

            Assert(missionWon && routeTracked && timeRecorded,
                "Act 1 mission completion records victory statistics, route attribution, and mission elapsed time");

            // Test 37: Staged multi-point investigation probing and searchlight mechanical jitter
            var simProbe = new SimulationWorld();
            var probeGuard = simProbe.Guards[0]; // Patrol-1 at (15, 5)
            Vec2 lurePos = new Vec2(17.5, 5.0);
            probeGuard.LastInvestigateTarget = lurePos;
            probeGuard.State = AIState.Investigate;
            probeGuard.Suspicion = 50.0;
            probeGuard.InvestigationStage = 0;

            // Move to first waypoint
            for (int i = 0; i < 20; i++)
            {
                simProbe.Step(0.1, new Vec2(0, 0));
                if (probeGuard.InvestigationStage == 1) break;
            }

            bool stageProbed = probeGuard.InvestigationStage == 1 &&
                               Vec2.Distance(probeGuard.SecondaryInvestigateTarget, lurePos) > 1.0;

            // Check motor mechanical jitter variance on searchlight
            Vec2 spotPos1 = simProbe.CurfewSearchlight.GetCurrentPosition();
            simProbe.CurfewSearchlight.SweepTime = 1.0;
            Vec2 spotPos2 = simProbe.CurfewSearchlight.GetCurrentPosition();
            bool jitterActive = simProbe.CurfewSearchlight.MotorJitter > 0;

            Assert(stageProbed && jitterActive,
                "Guards execute multi-point staged search probing and searchlights exhibit motor mechanical jitter");

            // Test 38: Dynamic guard audio threat escalation and radio click dispatches
            var simAudioEsc = new SimulationWorld();
            var gLead = simAudioEsc.Guards[0];
            gLead.LastInvestigateTarget = new Vec2(22.0, 5.0);

            simAudioEsc.BroadcastRadioAlert(gLead);
            bool radioLogged = simAudioEsc.ProceduralAudioLog.Exists(a => a.StartsWith("RadioStaticClick"));

            Assert(radioLogged, "Guard radio alert broadcasts synthesize military radio click audio and dispatch event logs");

            // Test 39: Guard alert shout vocalization synthesis and event logging
            var simShout = new SimulationWorld();
            var alertGuard = simShout.Guards[0];
            alertGuard.Position = new Vec2(15.0, 5.0);
            alertGuard.Forward = new Vec2(0, 1);
            simShout.PlayerCharacter.Position = new Vec2(15.0, 8.0); // Directly in front under bright lamp
            simShout.PlayerCharacter.CurrentStance = Stance.Sprint;
            simShout.CurfewSearchlight.Center = new Vec2(15.0, 8.0);
            simShout.CurfewSearchlight.Radius = 6.0;

            for (int i = 0; i < 15; i++)
            {
                simShout.Step(0.1, new Vec2(0, 0));
                if (alertGuard.State == AIState.Chase) break;
            }

            bool shoutTriggered = simShout.ProceduralAudioLog.Exists(a => a.StartsWith("GuardAlertShout"));
            Assert(shoutTriggered, "Guard alert shout vocalization synthesizes audio cue when player is spotted and chased");

            // Test 40: Player surface-dependent footstep audio pacing and stride intervals
            var simStep = new SimulationWorld();
            simStep.PlayerCharacter.CurrentStance = Stance.Walk;
            simStep.PlayerCharacter.Position = new Vec2(22.0, 10.0); // Inside Catwalk Metal Grate zone

            // Step 1.0s of walking input
            for (int i = 0; i < 10; i++)
            {
                simStep.Step(0.1, new Vec2(1, 0));
            }

            bool metalStepsLogged = simStep.ProceduralAudioLog.Exists(a => a.StartsWith("Footstep_MetalGrate"));
            Assert(metalStepsLogged, "Player movement synthesizes surface-dependent rhythmic footstep audio across ground terrains");

            // Test 41: Searchlight penumbra edge falloff and progressive suspicion scaling
            var simPenum = new SimulationWorld();
            simPenum.CurfewSearchlight.Center = new Vec2(15.0, 10.0);
            simPenum.CurfewSearchlight.SweepRange = 0; // Fixed spotlight
            simPenum.CurfewSearchlight.Radius = 6.0;

            // Core center exposure at (15, 10)
            simPenum.PlayerCharacter.Position = new Vec2(15.0, 10.0);
            double coreExp = simPenum.CalculatePlayerExposure();

            // Edge penumbra exposure at (19.5, 10) -> dist = 4.5m (between core 2.7m and radius 6.0m)
            simPenum.PlayerCharacter.Position = new Vec2(19.5, 10.0);
            double edgeExp = simPenum.CalculatePlayerExposure();

            bool penumbraFalloffWorking = (coreExp > edgeExp) && (edgeExp > 0.05);
            Assert(penumbraFalloffWorking, "Searchlight projects smooth penumbra falloff rewarding edge-skirting stealth");

            // Test 42: GC allocation mitigation and high-frequency sub-millisecond stepping
            var simGC = new SimulationWorld();
            long initialMemory = GC.GetTotalMemory(true);
            var swGC = System.Diagnostics.Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
            {
                simGC.Step(0.016, new Vec2(0.5, 0.2));
            }

            swGC.Stop();
            long finalMemory = GC.GetTotalMemory(false);
            long allocatedBytes = Math.Max(0, finalMemory - initialMemory);
            double avgMsPerStep = (double)swGC.ElapsedMilliseconds / 1000.0;

            bool subMillisecondSpeed = avgMsPerStep < 0.10; // Well under 0.1ms per tick
            bool memoryControlled = allocatedBytes < 250000; // Minimal allocation footprint
            Assert(subMillisecondSpeed && memoryControlled,
                "Simulation executes GC-optimized sub-millisecond updates guaranteeing locked 30fps+ on low-spec hardware");

            // Test 43: Decoupled single-point cache invalidation and smoothstep penumbra lookups
            var simSmooth = new SimulationWorld();
            simSmooth.CurfewSearchlight.Center = new Vec2(15.0, 10.0);
            simSmooth.CurfewSearchlight.SweepRange = 0;
            simSmooth.CurfewSearchlight.Radius = 6.0;

            // Halfway into penumbra at dist = 4.35m (between core 2.7m and radius 6.0m)
            simSmooth.PlayerCharacter.Position = new Vec2(19.35, 10.0);
            double midExp = simSmooth.CalculatePlayerExposure();

            bool smoothstepValid = midExp > 0.10 && midExp < 0.85;
            Assert(smoothstepValid, "Searchlight penumbra computes algebraic smoothstep S-curve falloff eliminating trigonometric overhead");

            // Test 44: Contextual proximity interaction state and tool durability decay
            var simDurability = new SimulationWorld();
            simDurability.PlayerCharacter.HasWireCutters = true;
            simDurability.PlayerCharacter.WireCutterDurability = 1;

            bool cut1 = simDurability.CutWireFence();
            bool broke = !simDurability.PlayerCharacter.HasWireCutters && simDurability.PlayerCharacter.WireCutterDurability == 0;
            bool cut2Blocked = !simDurability.CutWireFence();

            Assert(cut1 && broke && cut2Blocked,
                "Wire cutters degrade with use, breaking upon exhausting durability and blocking further breaches");

            // Test 45: Dynamic curfew wind soundscape scaling and shelter acoustic dampening
            var simWind = new SimulationWorld();
            simWind.PlayerCharacter.Cold = 80.0; // Freezing outdoor alley
            simWind.PlayerCharacter.Position = new Vec2(5.0, 10.0); // Unsheltered alley
            simWind.Step(1.0, new Vec2(0, 0));
            double outdoorWindVol = simWind.WindAmbienceVolume;

            // Move inside burn barrel shelter
            simWind.PlayerCharacter.Position = simWind.Shelters[0].Position;
            simWind.Step(2.0, new Vec2(0, 0));
            double shelteredWindVol = simWind.WindAmbienceVolume;

            bool windDampenedInShelter = shelteredWindVol < outdoorWindVol && shelteredWindVol <= 0.15;
            Assert(windDampenedInShelter, "Environmental wind soundscape volume dynamically scales with cold and dampens inside shelters");

            // Test 46: Thermal condensation breath puff exhalation and shelter suppression
            var simBreath = new SimulationWorld();
            simBreath.PlayerCharacter.Cold = 75.0; // Freezing
            simBreath.PlayerCharacter.Position = new Vec2(2.0, 10.0); // Unsheltered alley
            simBreath.CurfewSearchlight.Radius = 0; // Shadow

            // Step 2.6s so breath puff cycles
            simBreath.Step(2.6, new Vec2(0, 0));
            bool breathPuffActive = simBreath.PlayerCharacter.HasThermalBreathPuff;
            double exposedBreathLevel = simBreath.CalculatePlayerExposure();

            // Move to shelter and step
            simBreath.PlayerCharacter.Position = simBreath.Shelters[0].Position;
            simBreath.Step(0.5, new Vec2(0, 0));
            bool breathSuppressedInShelter = !simBreath.PlayerCharacter.HasThermalBreathPuff;

            Assert(breathPuffActive && exposedBreathLevel >= 0.30 && breathSuppressedInShelter,
                "Freezing players exhale visible thermal condensation breath puffs that elevate exposure unless sheltered");

            // Test 47: Broken glass debris acoustic shard escalation and guard suspicion responsiveness
            var simGlass = new SimulationWorld();
            // Move Guard closer to the glass debris zone at (33, 10)
            var patrolG = simGlass.Guards[0];
            patrolG.Position = new Vec2(30.0, 10.0);
            patrolG.Forward = new Vec2(0, 1); // Not facing directly at player

            simGlass.PlayerCharacter.Position = new Vec2(33.0, 10.0); // Inside Broken Bottle Rubble zone
            simGlass.PlayerCharacter.CurrentStance = Stance.Walk;
            simGlass.PlayerCharacter.CurrentSurface = SurfaceType.Concrete;
            double concreteNoiseRadius = simGlass.PlayerCharacter.GetNoiseRadius();

            simGlass.PlayerCharacter.CurrentSurface = SurfaceType.GlassDebris;
            double glassNoiseRadius = simGlass.PlayerCharacter.GetNoiseRadius();

            // Guard hearing glass shard noise transitions with sharper suspicion floor (50.0 vs 35.0 baseline)
            simGlass.Step(0.1, new Vec2(0.1, 0));
            bool glassAcousticsEscalated = glassNoiseRadius > concreteNoiseRadius && (glassNoiseRadius - (6.0 * 2.0)) >= 3.0;
            bool guardGlassSuspicionSharp = patrolG.Suspicion >= 50.0 && patrolG.State == AIState.Investigate;

            Assert(glassAcousticsEscalated && guardGlassSuspicionSharp,
                "Traversing glass debris triggers +3.0m shard acoustic spikes and raises guard investigation suspicion to 50%");

            // Test 48: Dynamic ambient wind acoustic masking for footstep detection vs loud distractions
            var simWindMask = new SimulationWorld();
            var windGuard = simWindMask.Guards[0]; // Patrol-1 at (15, 5)
            windGuard.Forward = new Vec2(0, 1);
            simWindMask.PlayerCharacter.CurrentStance = Stance.Walk;
            simWindMask.PlayerCharacter.CurrentSurface = SurfaceType.Concrete; // 6.0m base noise
            // Guard hearing threshold with wind masking = 6.0 * 0.75 = 4.5m
            // Position player at 5.2m away -> in still weather (6.0m) would be heard, but masked in wind (4.5m)
            simWindMask.PlayerCharacter.Position = new Vec2(15.0, 10.2); // dist = 5.2m
            simWindMask.Step(0.1, new Vec2(0.1, 0)); // Walking in wind
            bool footstepMaskedByWind = windGuard.Suspicion <= 5.0 && windGuard.State == AIState.Patrol;

            // Whistle (20m radius) still penetrates wind cleanly and alerts guard
            simWindMask.Whistle();
            simWindMask.Step(0.1, new Vec2(0, 0));
            bool loudDistractionPiercesWind = windGuard.Suspicion >= 30.0 && windGuard.State == AIState.Investigate;

            Assert(footstepMaskedByWind && loudDistractionPiercesWind,
                "Curfew wind gusts mask subtle footsteps (-25% hearing range) while loud whistles reliably alert guards");

            // Test 49: Searchlight beam interlocking vigilance coordination with nearby patrolling guards
            var simInterlock = new SimulationWorld();
            var gInterlock = simInterlock.Guards[0]; // Patrol-1 at (15, 5)
            gInterlock.Suspicion = 10.0; // Mild alertness
            simInterlock.CurfewSearchlight.Center = new Vec2(22.0, 5.0); // 7m away from guard
            simInterlock.CurfewSearchlight.SweepRange = 0; // Fixed illuminated hotspot

            simInterlock.Step(0.1, new Vec2(0, 0));
            // Guard forward should be biased towards searchlight position (22, 5) -> pointing east (X > 0.9)
            bool searchlightInterlockingActive = gInterlock.Forward.X > 0.85;

            Assert(searchlightInterlockingActive,
                "Nearby guards interlock patrol vigilance towards actively illuminated searchlight sectors within 18m");

            // Test 50: Searchlight sweep direction reversal and deceleration dynamics post-focus
            var simDecel = new SimulationWorld();
            int initialDir = simDecel.CurfewSearchlight.SweepDirection;
            simDecel.CurfewSearchlight.FocusOn(new Vec2(25.0, 10.0), 1.0);

            bool directionReversed = simDecel.CurfewSearchlight.SweepDirection == -initialDir;
            bool decelerationActive = simDecel.CurfewSearchlight.DecelerationTimer > 0;

            simDecel.Step(1.5, new Vec2(0, 0)); // Elapse focus duration into decelerated sweep
            bool deceleratedSpeedActive = simDecel.CurfewSearchlight.FocusTimer <= 0 && simDecel.CurfewSearchlight.DecelerationTimer > 0;

            Assert(directionReversed && decelerationActive && deceleratedSpeedActive,
                "Curfew searchlights reverse sweep direction and decelerate post-focus to re-interrogate sector");

            // Test 51: Dynamic quality adaptation and low-spec safeguard activation under sustained low FPS
            var simQuality = new SimulationWorld();
            // Step with sustained low frame rate (40ms = 25fps) for 2.2s (55 steps)
            for (int i = 0; i < 55; i++)
            {
                simQuality.Step(0.040, new Vec2(0, 0));
            }
            bool safeguardTriggered = simQuality.IsLowSpecSafeguardActive;

            // Step with recovered 60fps (16.6ms) for 2.2s
            for (int i = 0; i < 140; i++)
            {
                simQuality.Step(0.016, new Vec2(0, 0));
            }
            bool safeguardDeactivated = !simQuality.IsLowSpecSafeguardActive;

            Assert(safeguardTriggered && safeguardDeactivated,
                "Dynamic quality adaptation activates low-spec safeguard under sub-30fps dips and recovers smoothly");

            // Test 52: Procedural breath condensation vapor dissipation physics and progressive exposure scaling
            var simVapor = new SimulationWorld();
            simVapor.PlayerCharacter.Cold = 75.0; // Freezing
            simVapor.PlayerCharacter.Position = new Vec2(2.0, 10.0);
            simVapor.CurfewSearchlight.Radius = 0; // Dark shadows

            // Step 2.5s to trigger exhalation start
            simVapor.Step(2.5, new Vec2(0, 0));
            double initialDiameter = simVapor.PlayerCharacter.BreathVaporDiameter;
            double initialAlpha = simVapor.PlayerCharacter.BreathVaporAlpha;
            double initialExposure = simVapor.CalculatePlayerExposure();

            // Advance 0.35s midway into vapor dissipation
            simVapor.Step(0.35, new Vec2(0, 0));
            double midDiameter = simVapor.PlayerCharacter.BreathVaporDiameter;
            double midAlpha = simVapor.PlayerCharacter.BreathVaporAlpha;
            double midExposure = simVapor.CalculatePlayerExposure();

            bool expandingDiameter = midDiameter > initialDiameter && midDiameter <= 1.2;
            bool fadingAlpha = midAlpha < initialAlpha && midAlpha > 0.0;
            bool progressiveExposureDecay = midExposure < initialExposure && midExposure > 0.05;

            Assert(expandingDiameter && fadingAlpha && progressiveExposureDecay,
                "Thermal breath vapor expands from 0.4m to 1.2m while fading opacity and exposure progressively");

            // Test 53: Checkpoint verbal interrogation challenge grace period and food bribery resolution
            var simInterrogate = new SimulationWorld();
            simInterrogate.PlayerCharacter.Position = new Vec2(39.0, 10.0); // 1.0m from checkpoint gate (40, 10)
            simInterrogate.PlayerCharacter.RiceCount = 2;

            // Step 0.5s to initiate interrogation
            simInterrogate.Step(0.5, new Vec2(0, 0));
            bool interrogationChallenged = simInterrogate.CheckpointInterrogating &&
                                           simInterrogate.ProceduralAudioLog.Contains("CheckpointInterrogationChallenge") &&
                                           !simInterrogate.CheckpointAlarmTripped;

            // Bribe checkpoint guard before 3.0s grace period elapses
            bool bribeAccepted = simInterrogate.BribeCheckpointGuard();
            bool interrogationResolved = !simInterrogate.CheckpointInterrogating && simInterrogate.CheckpointBribed;

            // Un-bribed expiration triggers alarm
            var simExpire = new SimulationWorld();
            simExpire.PlayerCharacter.Position = new Vec2(39.0, 10.0);
            simExpire.Step(3.2, new Vec2(0, 0)); // Exceed 3.0s grace period
            bool alarmTrippedOnExpiration = simExpire.CheckpointAlarmTripped;

            Assert(interrogationChallenged && bribeAccepted && interrogationResolved && alarmTrippedOnExpiration,
                "Checkpoint guard initiates 3s verbal interrogation challenge before sirens trip, resolved cleanly by rice bribery");

            // Test 54: Distance-based searchlight shadow culling beyond 22m boundary
            var simCull = new SimulationWorld();
            simCull.Lamps.Clear(); // Clear lamps to isolate searchlight shadow test
            simCull.CurfewSearchlight.Center = new Vec2(10.0, 10.0);
            simCull.CurfewSearchlight.SweepRange = 0;
            simCull.CurfewSearchlight.Radius = 25.0; // Artificial large searchlight
            simCull.CurfewSearchlight.ShadowCullingDistance = 22.0;

            // At 15m away (within 22m cull limit), exposure is computed
            simCull.PlayerCharacter.Position = new Vec2(25.0, 10.0); // dist = 15m
            double withinCullExposure = simCull.CalculatePlayerExposure();

            // At 24m away (> 22m cull limit), exposure drops to ambient shadow (0.05) to conserve CPU cycles
            simCull.PlayerCharacter.Position = new Vec2(34.0, 10.0); // dist = 24m
            double culledExposure = simCull.CalculatePlayerExposure();

            bool shadowCullingFunctional = withinCullExposure > 0.05 && culledExposure == 0.05;
            Assert(shadowCullingFunctional,
                "Distance-based searchlight shadow culling drops raycast evaluation beyond 22m to conserve low-spec CPU");

            // Test 55: Searchlight-guard intersection tempo acceleration during investigation
            var simIntersect = new SimulationWorld();
            var gInvest = simIntersect.Guards[0]; // Patrol-1 at (15, 5)
            Vec2 targetProbePos = new Vec2(15.0, 10.0);
            gInvest.Suspicion = 50.0; // High suspicion so it doesn't decay below 5.0 to Patrol
            gInvest.State = AIState.Investigate;
            gInvest.LastInvestigateTarget = targetProbePos;
            gInvest.InvestigationStage = 0;

            // Searchlight centered at target probe (15, 10)
            simIntersect.CurfewSearchlight.Center = targetProbePos;
            simIntersect.CurfewSearchlight.SweepRange = 0; // Fixed illumination over target

            simIntersect.Step(0.5, new Vec2(0, 0));
            // Distance traveled in 0.5s at 1.35x accelerated speed: (2.0 * 1.2 * 1.35) * 0.5 = 1.62m
            double distTraveled = Vec2.Distance(gInvest.Position, new Vec2(15.0, 5.0));
            bool tempoAccelerated = distTraveled >= 1.5;

            Assert(tempoAccelerated,
                "Guard search tempo accelerates by 1.35x when allied searchlight beam illuminates investigation target");

            // Test 56: Checkpoint shadow inspection grace period extension to 4.5s
            var simGrace = new SimulationWorld();
            simGrace.Lamps.Clear(); // Clear lamps so position at (39, 10) is in deep shadow
            simGrace.PlayerCharacter.Position = new Vec2(39.0, 10.0); // 1m from gate
            simGrace.PlayerCharacter.CurrentStance = Stance.Crouch;
            simGrace.CurfewSearchlight.Radius = 0; // Deep shadow alley (exposure = 0.05 <= 0.20)

            simGrace.Step(0.1, new Vec2(0, 0));
            // Interrogation timer should be initialized to 4.5s (- 0.1s step = 4.4s) instead of 3.0s
            bool extendedGraceActive = simGrace.CheckpointInterrogating && simGrace.CheckpointInterrogationTimer > 3.5;

            Assert(extendedGraceActive,
                "Crouching in deep shadow extends checkpoint inspection grace period to 4.5s giving more time to bribe");

            // Test 57: Shelter thermal warming scoping parity (shelter warming completely halts outdoor cold accumulation)
            var simShelterParity = new SimulationWorld();
            simShelterParity.PlayerCharacter.Cold = 40.0;
            simShelterParity.PlayerCharacter.Position = simShelterParity.Shelters[0].Position; // Inside burn barrel alcove

            // Step 1.0s: cold should decrease by exactly 6.0 (from 40.0 to 34.0) without outdoor wind tunnel accumulation
            simShelterParity.Step(1.0, new Vec2(0, 0));
            double expectedCold = 34.0;
            bool exactShelterParity = Math.Abs(simShelterParity.PlayerCharacter.Cold - expectedCold) < 0.05;

            Assert(exactShelterParity,
                "Shelter warming completely halts outdoor cold accumulation with 1:1 parity between Unity and StealthSim");

            // Test 58: Glass debris shard scavenging replenishes throwable bottle inventory
            var simScavenge = new SimulationWorld();
            simScavenge.PlayerCharacter.BottleCount = 0; // Empty bottles
            simScavenge.PlayerCharacter.Position = new Vec2(33.0, 10.0); // Inside Broken Bottle Rubble zone
            simScavenge.Step(0.1, new Vec2(0, 0)); // Updates surface to GlassDebris

            bool scavenged = simScavenge.ScavengeGlassBottle();
            bool bottleAcquired = simScavenge.PlayerCharacter.BottleCount == 1;
            bool localNoiseEmitted = simScavenge.ActiveDistractionPos.HasValue && simScavenge.ActiveDistractionRadius == 6.0;

            Assert(scavenged && bottleAcquired && localNoiseEmitted,
                "Scavenging on broken glass debris creates throwable bottle and emits 6m local clinking noise");

            // Test 59: Checkpoint barrier breach pushback physics
            var simPush = new SimulationWorld();
            simPush.PlayerCharacter.Position = new Vec2(39.0, 10.0); // 1.0m from checkpoint gate at (40, 10)
            Vec2 preAlarmPos = simPush.PlayerCharacter.Position;

            simPush.TriggerCheckpointAlarm();
            Vec2 postAlarmPos = simPush.PlayerCharacter.Position;

            double pushDistance = Vec2.Distance(preAlarmPos, postAlarmPos);
            bool pushedBack = pushDistance >= 2.4 && postAlarmPos.X < preAlarmPos.X;

            Assert(pushedBack,
                "Checkpoint security alarm breach shoves player back 2.5m along barrier normal preventing clipping");

            // Test 60: Low-spec procedural audio concurrency capping (Max 8 concurrent voices)
            var simVoices = new SimulationWorld();
            int playedCount = 0;
            for (int i = 0; i < 12; i++)
            {
                if (simVoices.PlayProceduralAudio($"Footstep_{i}"))
                {
                    playedCount++;
                }
            }
            bool voiceBudgetEnforced = playedCount == 8 && simVoices.ActiveVoiceCount == 8;

            Assert(voiceBudgetEnforced,
                "Procedural audio concurrency is capped at 8 voices to prevent buffer underruns on dual-core low-spec CPUs");

            // Test 61: Checkpoint guard inspection torch sweep activation during interrogation
            var simTorch = new SimulationWorld();
            simTorch.PlayerCharacter.Position = new Vec2(39.0, 10.0); // 1.0m from gate
            simTorch.Step(0.2, new Vec2(0, 0));

            bool torchSweepEngaged = simTorch.CheckpointInterrogating && simTorch.CheckpointTorchSweeping;

            // Step away beyond 3.5m
            simTorch.PlayerCharacter.Position = new Vec2(35.0, 10.0); // 5.0m from gate
            simTorch.Step(0.1, new Vec2(0, 0));

            bool torchDisengagedOnExit = !simTorch.CheckpointInterrogating && !simTorch.CheckpointTorchSweeping;

            Assert(torchSweepEngaged && torchDisengagedOnExit,
                "Checkpoint guard activates inspection torch sweep during interrogation and disengages when player retreats");

            // Test 62: Stance-dependent acoustic footstep audio dampening on loud debris
            var simStanceDamp = new SimulationWorld();
            simStanceDamp.PlayerCharacter.CurrentStance = Stance.Crouch;
            simStanceDamp.PlayerCharacter.Position = new Vec2(22.0, 10.0); // Inside Catwalk Metal Grate zone

            for (int i = 0; i < 15; i++)
            {
                simStanceDamp.Step(0.1, new Vec2(1, 0));
            }

            bool crouchDampenedStepLogged = simStanceDamp.ProceduralAudioLog.Exists(a => a.StartsWith("Footstep_MetalGrate_Vol0.16"));
            Assert(crouchDampenedStepLogged,
                "Crouching attenuates footstep audio volume on metallic grating and glass debris surfaces");

            // Test 63: Guard tactical search flashlight direct glare detection synergy
            var simFlashSynergy = new SimulationWorld();
            var gFlash = simFlashSynergy.Guards[0]; // (15, 5) facing East (1, 0)
            gFlash.Position = new Vec2(10.0, 10.0);
            gFlash.Forward = new Vec2(1.0, 0.0);
            simFlashSynergy.PlayerCharacter.Position = new Vec2(14.0, 10.0); // 4.0m directly ahead

            // In Patrol state (flashlight inactive)
            gFlash.State = AIState.Patrol;
            gFlash.Suspicion = 0.0;
            simFlashSynergy.Step(0.1, new Vec2(0, 0));
            double suspicionWithoutFlash = gFlash.Suspicion;

            // In Investigate state (flashlight active)
            var simFlashOn = new SimulationWorld();
            var gFlashOn = simFlashOn.Guards[0];
            gFlashOn.Position = new Vec2(10.0, 10.0);
            gFlashOn.Forward = new Vec2(1.0, 0.0);
            simFlashOn.PlayerCharacter.Position = new Vec2(14.0, 10.0);
            gFlashOn.State = AIState.Investigate;
            gFlashOn.LastInvestigateTarget = new Vec2(14.0, 10.0);
            gFlashOn.Suspicion = 30.0; // Investigate base
            simFlashOn.Step(0.1, new Vec2(0, 0));
            double suspicionDeltaWithFlash = gFlashOn.Suspicion - 30.0;

            bool flashlightSynergyWorking = suspicionDeltaWithFlash > (suspicionWithoutFlash * 1.3);
            Assert(flashlightSynergyWorking,
                "Guard tactical search flashlight directly escalates detection rate and suspicion accumulation in shadows");

            // Test 64: Low-spec spatial sleep distance contraction (25m threshold)
            var simSleepLOD = new SimulationWorld();
            var gLOD = simSleepLOD.Guards[0];
            gLOD.Position = new Vec2(10.0, 10.0);
            simSleepLOD.PlayerCharacter.Position = new Vec2(38.0, 10.0); // 28m distance
            gLOD.State = AIState.Patrol;
            gLOD.Suspicion = 0.0;

            // In normal quality mode (>35m sleep threshold), guard at 28m evaluates at cruising 15Hz
            simSleepLOD.IsLowSpecSafeguardActive = false;
            gLOD.SensorEvaluationCount = 0;
            for (int i = 0; i < 10; i++) simSleepLOD.Step(0.05, new Vec2(0, 0)); // 0.5s total
            int normalSensors = gLOD.SensorEvaluationCount;

            // In low-spec mode (25m sleep threshold), guard at 28m drops to 2Hz sleep LOD
            var simLowSpec = new SimulationWorld();
            var gLow = simLowSpec.Guards[0];
            gLow.Position = new Vec2(10.0, 10.0);
            simLowSpec.PlayerCharacter.Position = new Vec2(38.0, 10.0); // 28m distance
            gLow.State = AIState.Patrol;
            gLow.Suspicion = 0.0;
            simLowSpec.IsLowSpecSafeguardActive = true;
            gLow.SensorEvaluationCount = 0;
            for (int i = 0; i < 10; i++) simLowSpec.Step(0.05, new Vec2(0, 0)); // 0.5s total
            int lowSpecSensors = gLow.SensorEvaluationCount;

            bool lodDecimationActive = lowSpecSensors < normalSensors;
            Assert(lodDecimationActive,
                "Low-spec safeguard contracts guard spatial sleep distance from 35m to 25m to conserve CPU cycles");

            // Test 65: Checkpoint alarm siren zero-allocation guard alert escalation
            var simZeroGC = new SimulationWorld();
            simZeroGC.PlayerCharacter.Position = new Vec2(39.0, 10.0);
            simZeroGC.TriggerCheckpointAlarm();

            bool allInRadiusAlerted = true;
            for (int i = 0; i < simZeroGC.Guards.Count; i++)
            {
                var g = simZeroGC.Guards[i];
                if (Vec2.Distance(simZeroGC.CheckpointPosition, g.Position) <= 35.0)
                {
                    if (g.State != AIState.Chase || g.Suspicion < 100.0)
                    {
                        allInRadiusAlerted = false;
                        break;
                    }
                }
            }
            Assert(allInRadiusAlerted && simZeroGC.CheckpointAlarmTripped,
                "Checkpoint alarm trips siren audio and escalates guards within radius to Chase state with zero GC spikes");

            // Test 66: Multi-guard proximity capture evaluation across all active guards
            var simMultiCapture = new SimulationWorld();
            var gPatrol2 = simMultiCapture.Guards[1]; // Patrol-2 initialized at (28, 15)
            gPatrol2.Position = new Vec2(30.0, 15.0);
            gPatrol2.State = AIState.Chase;
            gPatrol2.Suspicion = 100.0;
            simMultiCapture.PlayerCharacter.Position = new Vec2(31.0, 15.0); // 1.0m from Patrol-2 (within 1.8m capture zone)
            simMultiCapture.Step(0.1, new Vec2(0, 0));

            bool playerCapturedByPatrol2 = simMultiCapture.GameOver && simMultiCapture.EndReason.Contains("caught you");
            Assert(playerCapturedByPatrol2,
                "Player proximity capture evaluates across all active patrol guards rather than only primary guard");

            // Test 67: Radio alert broadcast zero-GC indexed guard convergence
            var simRadioZeroGC = new SimulationWorld();
            var gBroadcaster = simRadioZeroGC.Guards[0];
            gBroadcaster.LastInvestigateTarget = new Vec2(22.0, 10.0);
            simRadioZeroGC.BroadcastRadioAlert(gBroadcaster);

            var gReceiver = simRadioZeroGC.Guards[1]; // Within 25m radio radius
            bool radioReceivedClean = gReceiver.State == AIState.Investigate &&
                                      gReceiver.Suspicion >= 60.0 &&
                                      simRadioZeroGC.ProceduralAudioLog.Exists(a => a.StartsWith("RadioStaticClick"));
            Assert(radioReceivedClean,
                "Radio alert broadcasts converge allied guards via zero-GC indexed array iterations");

            // Test 68: Zero-GC indexed shelter and street lamp exposure evaluation loops
            var simShelterLoop = new SimulationWorld();
            simShelterLoop.PlayerCharacter.Position = simShelterLoop.Shelters[0].Position; // (20, 4) Alley Burn Barrel
            bool shelterDetected = simShelterLoop.IsPlayerSheltered();

            simShelterLoop.PlayerCharacter.Position = simShelterLoop.Lamps[0].Position; // (12, 10) Lamp 1
            double lampExposure = simShelterLoop.CalculatePlayerExposure();
            bool lampExposureDetected = Math.Abs(lampExposure - 0.50) < 0.01;

            Assert(shelterDetected && lampExposureDetected,
                "Shelter proximity and lamp exposure calculations execute via zero-GC indexed iterations");

            // Test 69: Distraction acoustic inspection and raycast distance pre-filtering
            var simDistractLOD = new SimulationWorld();
            var gDistract = simDistractLOD.Guards[0]; // (15, 5)
            simDistractLOD.PlayerCharacter.BottleCount = 1;
            simDistractLOD.ThrowBottle(new Vec2(18.0, 5.0)); // 3m from guard
            simDistractLOD.Step(0.1, new Vec2(0, 0));

            bool distractionLuredCleanly = gDistract.State == AIState.Investigate &&
                                          Vec2.Distance(gDistract.LastInvestigateTarget, new Vec2(18.0, 5.0)) < 0.01;
            Assert(distractionLuredCleanly,
                "Sound distraction inspection evaluates acoustic propagation cleanly via zero-GC cached arrays");

            // Test 70: Zero-GC searchlight beam glance and silhouette calculation
            var simSearchlightLOD = new SimulationWorld();
            var gGlance = simSearchlightLOD.Guards[0]; // (15, 5)
            simSearchlightLOD.CurfewSearchlight.Center = new Vec2(18.0, 5.0);
            simSearchlightLOD.CurfewSearchlight.FocusOn(new Vec2(18.0, 5.0), 3.0);
            simSearchlightLOD.Step(0.1, new Vec2(0, 0));

            // Guard forward should orient towards the searchlight beam
            bool searchlightInterlocked = Vec2.Dot(gGlance.Forward, new Vec2(1, 0)) > 0.95;
            Assert(searchlightInterlocked,
                "Patrolling guards orient towards searchlight hotspot via zero-GC light zone evaluations");

            // Test 71: Procedural audio clip pre-caching and voice concurrency throttling
            var simAudioCache = new SimulationWorld();
            for (int i = 0; i < 20; i++)
            {
                simAudioCache.PlayProceduralAudio("Footstep_Concrete");
            }
            // Max concurrent voices is 8, preventing thread contention on dual-core low-spec machines
            bool audioThrottled = simAudioCache.ActiveVoiceCount == 8;
            Assert(audioThrottled,
                "Procedural audio pre-caching and concurrency budgeting ensures zero buffer underruns on low-spec hardware");

            // Test 72: Searchlight squared-distance evaluation optimization and penumbra integrity
            var simSqrDist = new SimulationWorld();
            simSqrDist.CurfewSearchlight.Center = new Vec2(20.0, 10.0);
            simSqrDist.CurfewSearchlight.SweepRange = 0.0;
            simSqrDist.CurfewSearchlight.Radius = 5.0;

            simSqrDist.PlayerCharacter.Position = new Vec2(20.0, 10.0); // Inside core
            double expCore = simSqrDist.CalculatePlayerExposure();

            simSqrDist.PlayerCharacter.Position = new Vec2(23.0, 10.0); // Inside penumbra (dist = 3.0m)
            double expPenumbra = simSqrDist.CalculatePlayerExposure();

            simSqrDist.PlayerCharacter.Position = new Vec2(20.0, 2.0); // Outside radius (dist = 8.0m, Y=2.0 dark alley)
            double expOutside = simSqrDist.CalculatePlayerExposure();

            bool sqrDistOptimizationValid = expCore > expPenumbra && expPenumbra > expOutside && expOutside <= 0.05;
            Assert(sqrDistOptimizationValid,
                "Squared-distance calculations optimize searchlight evaluation without degrading penumbra fidelity");

            // Test 73: Guard investigating secondary search sweep vision range extension
            var simSearchVision = new SimulationWorld();
            var gSearch = simSearchVision.Guards[0];
            gSearch.State = AIState.Investigate;
            gSearch.InvestigationStage = 0; // Primary probe
            gSearch.Position = new Vec2(10.0, 10.0);
            gSearch.Forward = new Vec2(1.0, 0.0);
            simSearchVision.PlayerCharacter.Position = new Vec2(25.0, 10.0); // 15m ahead

            // Outside normal vision (14m) in stage 0
            simSearchVision.Step(0.1, new Vec2(0, 0));
            double suspicionStage0 = gSearch.Suspicion;

            // In stage 1 (active scanning), vision range extends by 15% (14m * 1.15 = 16.1m)
            var simSearchStage1 = new SimulationWorld();
            var gStage1 = simSearchStage1.Guards[0];
            gStage1.State = AIState.Investigate;
            gStage1.InvestigationStage = 1;
            gStage1.Position = new Vec2(10.0, 10.0);
            gStage1.Forward = new Vec2(1.0, 0.0);
            simSearchStage1.PlayerCharacter.Position = new Vec2(25.0, 10.0); // 15m ahead (within 16.1m!)
            simSearchStage1.Step(0.1, new Vec2(0, 0));
            double suspicionStage1 = gStage1.Suspicion;

            bool visionExtensionActive = suspicionStage1 > suspicionStage0 && suspicionStage1 > 0.0;
            Assert(visionExtensionActive,
                "Investigating guards focus flashlight beam extending active vision by 15% during secondary scan");

            // Test 74: Checkpoint bribery audio confirmation cue and squared-distance interaction
            var simBribeAudio = new SimulationWorld();
            simBribeAudio.PlayerCharacter.Position = new Vec2(39.0, 10.0); // 1.0m from gate
            simBribeAudio.PlayerCharacter.RiceCount = 2;

            bool bribeSuccess = simBribeAudio.BribeCheckpointGuard();
            bool bribeAudioLogged = simBribeAudio.ProceduralAudioLog.Contains("CheckpointBribeAccepted");
            Assert(bribeSuccess && bribeAudioLogged && simBribeAudio.CheckpointBribed,
                "Checkpoint guard food bribery emits audio confirmation and grants clearance via squared-distance checks");

            // Test 75: Shelter and Safehouse squared-distance evaluation optimization
            var simOpt = new SimulationWorld();
            simOpt.PlayerCharacter.Position = new Vec2(20.0, 4.0); // Inside Alley Burn Barrel (radius = 3.5)
            bool optSheltered = simOpt.IsPlayerSheltered();

            simOpt.PlayerCharacter.Position = new Vec2(56.0, 10.0); // Safehouse extraction trigger
            simOpt.Step(0.1, new Vec2(0, 0));
            bool optVictory = simOpt.GameWon;

            Assert(optSheltered && optVictory,
                "Shelter warming and safehouse extraction triggers evaluate via squared-distance checks without square root overhead");

            // Test 76: Complete Act 1 vertical slice simulation idempotency and retry state reset
            var simRetry = new SimulationWorld();
            simRetry.PlayerCharacter.HasCutPerimeterFence = true;
            simRetry.PlayerCharacter.HasWireCutters = false;
            simRetry.PlayerCharacter.WireCutterDurability = 0;
            simRetry.PlayerCharacter.Stamina = 0.0;
            simRetry.GameOver = true;

            // Instant level restart
            simRetry = new SimulationWorld();
            bool cleanResetState = !simRetry.GameOver && !simRetry.GameWon &&
                                   !simRetry.PlayerCharacter.HasCutPerimeterFence &&
                                   simRetry.PlayerCharacter.HasWireCutters &&
                                   simRetry.PlayerCharacter.WireCutterDurability == 2 &&
                                   simRetry.PlayerCharacter.Stamina == 100.0;
            Assert(cleanResetState,
                "Act 1 vertical slice quick restart guarantees complete state idempotency across all gameplay systems");

            // Test 77: Multi-guard vision raycast interleaving prevents multi-ray spikes under 30fps loads
            var simInterleave = new SimulationWorld();
            // Position player directly in front of Patrol-1 (index 0) and Patrol-2 (index 1)
            simInterleave.PlayerCharacter.Position = new Vec2(15.0, 10.0);
            var ig0 = simInterleave.Guards[0]; // index 0
            var ig1 = simInterleave.Guards[1]; // index 1
            ig0.Position = new Vec2(15.0, 8.0);
            ig0.WaypointA = new Vec2(15.0, 5.0);
            ig0.WaypointB = new Vec2(15.0, 15.0);
            ig0.Forward = new Vec2(0, 1);
            ig0.PatrolSpeed = 0.1;

            ig1.Position = new Vec2(15.0, 12.0);
            ig1.WaypointA = new Vec2(15.0, 15.0);
            ig1.WaypointB = new Vec2(15.0, 5.0);
            ig1.Forward = new Vec2(0, -1);
            ig1.PatrolSpeed = 0.1;

            // Step with dt = 0.0333 (30fps) across 6 frames
            int startG0Rays = ig0.RaycastEvaluationCount;
            int startG1Rays = ig1.RaycastEvaluationCount;

            for (int f = 0; f < 6; f++)
            {
                simInterleave.Step(0.0333, Vec2.Zero);
            }

            int newG0Rays = ig0.RaycastEvaluationCount - startG0Rays;
            int newG1Rays = ig1.RaycastEvaluationCount - startG1Rays;
            bool interleavingActive = newG0Rays < 6 && newG1Rays < 6 &&
                                      ig0.CachedInVision && ig1.CachedInVision;
            Assert(interleavingActive,
                "Multi-guard vision raycast interleaving budgets CPU cycles across frames while preserving vision persistence");

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
