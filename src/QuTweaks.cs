using System;
using System.Security.Permissions;
using BepInEx;
using Mono.Cecil.Cil;
using UnityEngine;
//using Mono.Cecil.Cil;
using MonoMod.Cil;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace QuTweaks
{
    [BepInPlugin("qu.qu_tweaks", "QuTweaks", "0.1.0")]
    public class QuTweaks : BaseUnityPlugin
    {
        public const string ModID = "qu.qu_tweaks";
        private readonly QuTweaksOptions _options;

        public QuTweaks()
        {
            _options = new QuTweaksOptions();
        }
        
        private void OnEnable()
        {
            On.RainWorld.OnModsInit += RainWorldOnModsInitHook;
            On.LizardTongue.Update += LizardTongueUpdateHook;
            IL.DropBug.FlyingWeapon += DropBugFlyingWeaponInject;
            On.Lizard.Violence += LizardViolenceHook;
            On.TubeWorm.Tongue.Update += TubeWormTongueUpdateHook;
        }

        // For the Easier tongue break option
        private void LizardTongueUpdateHook(On.LizardTongue.orig_Update orig, LizardTongue self)
        {
            var oldDist = self.dist;
            
            // Original method call
            orig(self);
            
            if (!_options.EasierTongueBreak.Value || !self.StuckToSomething ||
                !self.lizard.AI.DoIWantToHoldThisWithMyTongue(self.attached) || self.stuckCounter <= 5) return;
            
            var dDist = self.dist - oldDist;
            //Debug.Log($"oldDist: {oldDist}, newDist: {self.dist}, dDist: {dDist}, stuckCounter: {self.stuckCounter}");
            
            if (dDist < 5f - 2f * Mathf.InverseLerp(20f, 200f, self.stuckCounter)) return;
            
            //Debug.Log("Released!");
            self.Retract();
        }

        // For less trigger happy dropwigs options
        private void DropBugFlyingWeaponInject(ILContext il)
        {
            try
            {
                var c = new ILCursor(il);
                
                c.GotoNext(
                    x => x.MatchLdfld<BodyChunk>("pos"),
                    x => x.MatchLdcR4(1),
                    x => x.MatchCallOrCallvirt<ArtificialIntelligence>("VisualContact")
                );
                c.Index += 4;
                
                c.EmitDelegate<Func<bool>>(() => !_options.LessTriggerHappyDropwigs.Value);
                
                var returnLabel = c.DefineLabel();
                c.MarkLabel(returnLabel);
                
                c.GotoNext(
                    x => x.MatchLdarg(1), 
                    x => x.MatchCallOrCallvirt<PhysicalObject>("get_firstChunk")
                );
                var pointToLabel = c.DefineLabel();
                c.MarkLabel(pointToLabel);
                
                c.GotoLabel(returnLabel);
                c.Emit(OpCodes.Brfalse_S, pointToLabel);
                
                /* Testing lines
                c.Index += 3;

                c.EmitDelegate<Action>(() =>
                {
                    Debug.Log("Dropped!");
                });
                */
                //Console.WriteLine(il);
                
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception when matching IL for DropBugFlyingWeaponInject!");
                Debug.LogException(ex);
                Debug.LogError(il);
            }
        }

        // For Heavier spears option
        private void LizardViolenceHook(
            On.Lizard.orig_Violence orig,
            Lizard self,
            BodyChunk source,
            Vector2? directionAndMomentum,
            BodyChunk hitChunk,
            PhysicalObject.Appendage.Pos onAppendagePos,
            Creature.DamageType type,
            float damage,
            float stunBonus
        )
        {
            // Condition check
            var bl = _options.HeavierSpears.Value && directionAndMomentum.HasValue &&
                     LizardHitHeadShield(self, directionAndMomentum.Value) && source?.owner is Spear && 
                     self.Template.type != CreatureTemplate.Type.RedLizard;

            // Sets the stun equal to a rock
            if (bl)
            {
                stunBonus = 45;
                if (ModManager.MSC && source?.owner != null &&
                    source.owner.room.game.IsArenaSession && source.owner.room.game.GetArenaGameSession.chMeta != null)
                    stunBonus = 90;
            }
            
            // Original method call
            orig(self, source, directionAndMomentum, hitChunk, onAppendagePos, type, damage, stunBonus);
            
            if (!bl || source == null) return;
            
            // Flip lizziboi
            self.turnedByRockDirection = (int) Mathf.Sign(source.pos.x - source.lastPos.x);
            self.turnedByRockCounter = 20;
        }

        private void TubeWormTongueUpdateHook(On.TubeWorm.Tongue.orig_Update orig, TubeWorm.Tongue self)
        {
            orig(self);

            if (!self.Attached) return;
            Player player = null;
            foreach (var grasp in self.worm.grabbedBy)
            {
                if (!(grasp.grabber is Player p)) continue;
                player = p;
                break;
            }
            if (player == null) return;
            
            var movement = 0f;
            
            if (player.input[0].y > 0) movement = -3f;
            else if (player.input[0].y < 0) movement = 1f;

            if (movement == 0f) return;
            var tongueLength = Vector2.Distance(self.pos, self.worm.mainBodyChunk.pos);
            //self.requestedRopeLength = Mathf.Clamp(self.requestedRopeLength, tongueLength - 3f, tongueLength + 1f);
            self.requestedRopeLength = Mathf.Clamp(self.requestedRopeLength + movement, 50f, self.idealRopeLength);
            self.elastic = 1f;
        }

        // For registering the options menu
        private void RainWorldOnModsInitHook(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            MachineConnector.SetRegisteredOI(ModID, _options);
        }

        // Recreation of the Lizard HitHeadShield method without the sound method involved
        private static bool LizardHitHeadShield(Lizard lizard, Vector2 direction)
        {
            var hitAngle = Vector2.Angle(direction, -lizard.bodyChunks[0].Rotation);
            var maxLizardHeadShieldAngle = lizard.lizardParams.headShieldAngle + 20.0 * lizard.JawOpen;
            return !(lizard.HitInMouth(direction) || hitAngle >= maxLizardHeadShieldAngle);
        }
    }
}