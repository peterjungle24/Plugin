#region using
using System;
using BepInEx;
using UnityEngine;
using System.IO;
using SlugBase.Features;
using static SlugBase.Features.FeatureTypes;
using System.Runtime.InteropServices;
using On;
using On.MoreSlugcats;
using ObjType = AbstractPhysicalObject.AbstractObjectType;
using UnityEngine.PlayerLoop;
using BepInEx.Logging;
using System.Collections.Generic;
using RWCustom;
using IL;
using System.Runtime.InteropServices.ComTypes;
using Random = UnityEngine.Random;
#endregion

namespace Pedro.grey // name of the space lol
{
    [BepInPlugin("Pedro.grey", "Normal Scug", "0.1.0")]
    class Plugin : BaseUnityPlugin
    {
        public const string PLUGIN_GUID = "Pedro.grey";
        public const string PLUGIN_NAME = "Scug grey";
        public const string PLUGIN_VERSION = "0.1.0";
        public static readonly SlugcatStats.Name marshaw = new SlugcatStats.Name("marshaw"); //name of my slugcat
        public static new ManualLogSource Logger { get; private set; } //for logs
        public static string crossSprite = "Sprites/Extras/CrossHair"; //the path
        public static string crossPath = Path.Combine(crossSprite); //combination of Path
        

        //Add Hooks here bro
        public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            Logger = base.Logger;
            //We need hooks here

            On.Player.CraftingResults += Player_CraftingResults; //Craft Results -- craft ------------------------------------------------------------------------
            On.RainWorld.PostModsInit += Marshaw_PostModsInit; //Mod -- craft ------------------------------------------------------------------------------------
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted; //Can Be Crafted -- craft -----------------------------------------------------------------
            On.Player.ctor += Marshaw_Babify; //pup -- pup -------------------------------------------------------------------------------------------------------
            On.RainWorld.OnModsInit += RainWorldOnOnModsInit; //idk yet ------------------------------------------------------------------------------------------
        }

        #region mods init

        private bool IsInit;

        private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;

                On.Player.ctor += Player_ctor;
                On.Player.ClassMechanicsSaint += Player_ClassMechanicsSaint;

                On.PlayerGraphics.InitiateSprites += PlayerGraphics_InitiateSprites;
                On.PlayerGraphics.DrawSprites += PlayerGraphics_DrawSprites;

                IsInit = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        #endregion
        #region ascend

        //maybe is a timer from godTimer (is a god)--------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Dictionary<PlayerGraphics, int> godPipsIndex = new();
        // Dictionary for keeping track of the indexes of the added sprites



        private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.SlugCatClass == marshaw)
            {
                // Sets the maxGodTime for the godTimer
                self.maxGodTime = (int)(200f + 40f * (float)self.Karma);
                // You won't need this if statement if your slugcat doesn't go to Rubicon
                if (self.room != null && self.room.world.name == "HR")
                {
                    self.maxGodTime = 560f;
                }

                self.godTimer = self.maxGodTime;
            }
        }

        //allow to use the Ascension (but i dont wanna this)-----------------------------------------------------------------------------------------------------------------------------------------------
        private void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player self)
        {
            orig(self);
            if (self.SlugCatClass == marshaw)
            {
                // Activate and Deactivate Ascension
                if (self.wantToJump > 0 && self.monkAscension)
                {
                    self.DeactivateAscension();
                    self.wantToJump = 0;
                }
                else if (self.wantToJump > 0 && self.input[0].pckp && self.canJump <= 0 && !self.monkAscension && self.bodyMode != Player.BodyModeIndex.Crawl && self.bodyMode != Player.BodyModeIndex.CorridorClimb && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && self.animation != Player.AnimationIndex.HangFromBeam && self.animation != Player.AnimationIndex.ClimbOnBeam && self.bodyMode != Player.BodyModeIndex.WallClimb && self.bodyMode != Player.BodyModeIndex.Swimming && self.Consious && !self.Stunned && self.godTimer > 0f && self.animation != Player.AnimationIndex.AntlerClimb && self.animation != Player.AnimationIndex.VineGrab && self.animation != Player.AnimationIndex.ZeroGPoleGrab)
                {
                    self.ActivateAscension();
                }

            }
        }

        //this will.... uhh.... maybe, initialize their sprites for work?-------------------------------------------------------------------------------------------------------------------
        private void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.player.SlugCatClass == marshaw)
            {
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + self.numGodPips + 2);
                // Resizes the sLeaser sprite array to add the amount of numGodPips (12), and 2 for the crosshair and energy burst
                // If you have already resized the sprite array then you need to combine it with above, you will also need to adjust the dictionary as well

                if (godPipsIndex.ContainsKey(self)) { godPipsIndex[self] = sLeaser.sprites.Length - self.numGodPips - 2; }
                else { godPipsIndex.Add(self, sLeaser.sprites.Length - self.numGodPips - 2); }
                // Add self as a key for godPipsIndex dictionary and store the first index of numGodPips + 2
                // godPipsIndex[self] = 13 (index of energy burst)
                // godPipsIndex[self] + 1 = 14 (index of crosshair)
                // godPipsIndex[self] + 2 = 15 (starting index of numGodPips)

                sLeaser.sprites[godPipsIndex[self]] = new FSprite("Futile_White");
                sLeaser.sprites[godPipsIndex[self]].shader = rCam.game.rainWorld.Shaders["FlatLight"];
                // Set sprite for energy burst on ascension
                sLeaser.sprites[godPipsIndex[self]].RemoveFromContainer();
                rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[godPipsIndex[self]]);
                // Add energy burst to FContainer Foreground

                sLeaser.sprites[godPipsIndex[self] + 1] = new FSprite(crossPath); //My sprite is here damn iwieofhewipgfnerklgienhoiw\FSNÇH\lwioHOEIKFNWkoneofwFKLNFKLDSMOwijf[]FJwifew´F~kfldfoeifjówefjweifhweiofnwFON
                // Set sprite for ascension crosshair
                sLeaser.sprites[godPipsIndex[self] + 1].RemoveFromContainer();
                rCam.ReturnFContainer("Foreground").AddChild(sLeaser.sprites[godPipsIndex[self] + 1]);

                // Add crosshair to FContainer ForeGround

                for (int i = 0; i < self.numGodPips; i++)
                {
                    sLeaser.sprites[godPipsIndex[self] + 2 + i] = new FSprite("WormEye");
                    // Set sprite for the godPips timer

                    sLeaser.sprites[godPipsIndex[self] + 2 + i].RemoveFromContainer();
                    rCam.ReturnFContainer("HUD2").AddChild(sLeaser.sprites[godPipsIndex[self] + 2 + i]);
                    // Add godPips to FContainer HUD2
                }
            }
        }
        //this use their sprites i guess -----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.player.room != null && self.player.SlugCatClass == marshaw)
            {
                // Taken from saint's code, handles ascension crosshair, godPips, and effects
                if (self.player.killFac > 0f || self.player.forceBurst)
                {
                    sLeaser.sprites[godPipsIndex[self]].isVisible = true; //is visible (sure)
                    sLeaser.sprites[godPipsIndex[self]].x = sLeaser.sprites[3].x + self.player.burstX; //x coordinate for sprites (maybe?)
                    sLeaser.sprites[godPipsIndex[self]].y = sLeaser.sprites[3].y + self.player.burstY + 60f; //y coordinate for sprites (maybe?)
                    float f = Mathf.Lerp(self.player.lastKillFac, self.player.killFac, timeStacker); //follow the player up (maybe?)
                    sLeaser.sprites[godPipsIndex[self]].scale = Mathf.Lerp(50f, 2f, Mathf.Pow(f, 0.5f)); //Scale of the sprites (maybe?)
                    sLeaser.sprites[godPipsIndex[self]].alpha = Mathf.Pow(f, 3f); //alpha of the sprites (maybe?)
                }
                else
                {
                    sLeaser.sprites[godPipsIndex[self]].isVisible = false;
                }

                if (self.player.killWait > self.player.lastKillWait || self.player.killWait == 1f || self.player.forceBurst)
                {
                    self.rubberMouseX += (self.player.burstX - self.rubberMouseX) * 0.3f;
                    self.rubberMouseY += (self.player.burstY - self.rubberMouseY) * 0.3f;
                }
                else
                {
                    self.rubberMouseX *= 0.15f;
                    self.rubberMouseY *= 0.25f;
                }

                if (Mathf.Sqrt(Mathf.Pow(sLeaser.sprites[3].x - self.rubberMarkX, 2f) + Mathf.Pow(sLeaser.sprites[3].y - self.rubberMarkY, 2f)) > 100f)
                {
                    self.rubberMarkX = sLeaser.sprites[3].x;
                    self.rubberMarkY = sLeaser.sprites[3].y;
                }
                else
                {
                    self.rubberMarkX += (sLeaser.sprites[3].x - self.rubberMarkX) * 0.15f;
                    self.rubberMarkY += (sLeaser.sprites[3].y - self.rubberMarkY) * 0.25f;
                }

                sLeaser.sprites[godPipsIndex[self] + 1].x = self.rubberMarkX;
                sLeaser.sprites[godPipsIndex[self] + 1].y = self.rubberMarkY + 60f;
                float num16;
                if (self.player.monkAscension)
                {
                    sLeaser.sprites[9].color = Custom.HSL2RGB(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    sLeaser.sprites[10].alpha = 0f;
                    sLeaser.sprites[11].alpha = 0f;
                    sLeaser.sprites[godPipsIndex[self] + 1].color = sLeaser.sprites[9].color;
                    num16 = 1f;
                }
                else
                {
                    num16 = 0f;
                }

                float num17;
                if ((self.player.godTimer < self.player.maxGodTime || self.player.monkAscension) && !self.player.hideGodPips)
                {
                    num17 = 1f;
                    float num18 = 15f;
                    if (!self.player.monkAscension)
                    {
                        num18 = 6f;
                    }

                    self.rubberRadius += (num18 - self.rubberRadius) * 0.045f;
                    if (self.rubberRadius < 5f)
                    {
                        self.rubberRadius = num18;
                    }

                    float num19 = self.player.maxGodTime / (float)self.numGodPips;
                    for (int m = 0; m < self.numGodPips; m++)
                    {
                        float num20 = num19 * (float)m;
                        float num21 = num19 * (float)(m + 1);
                        if (self.player.godTimer <= num20)
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].scale = 0f;
                        }
                        else if (self.player.godTimer >= num21)
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].scale = 1f;
                        }
                        else
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].scale = (self.player.godTimer - num20) / num19;
                        }

                        if (self.player.karmaCharging > 0 && self.player.monkAscension)
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].color = sLeaser.sprites[9].color;
                        }
                        else
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].color = PlayerGraphics.SlugcatColor(self.CharacterForColor);
                        }
                    }
                }
                else
                {
                    num17 = 0f;
                }

                sLeaser.sprites[godPipsIndex[self] + 1].x = self.rubberMarkX + self.rubberMouseX;
                sLeaser.sprites[godPipsIndex[self] + 1].y = self.rubberMarkY + 60f + self.rubberMouseY;
                self.rubberAlphaEmblem += (num16 - self.rubberAlphaEmblem) * 0.05f;
                self.rubberAlphaPips += (num17 - self.rubberAlphaPips) * 0.05f;
                sLeaser.sprites[godPipsIndex[self] + 1].alpha = self.rubberAlphaEmblem;
                sLeaser.sprites[10].alpha *= 1f - self.rubberAlphaPips;
                sLeaser.sprites[11].alpha *= 1f - self.rubberAlphaPips;
                for (int n = godPipsIndex[self] + 2; n < godPipsIndex[self] + 2 + self.numGodPips; n++)
                {
                    sLeaser.sprites[n].alpha = self.rubberAlphaPips;
                    Vector2 vector16 = new Vector2(sLeaser.sprites[godPipsIndex[self] + 1].x, sLeaser.sprites[godPipsIndex[self] + 1].y);
                    vector16 += Custom.rotateVectorDeg(Vector2.one * self.rubberRadius, (float)(n - 15) * (360f / (float)self.numGodPips));
                    sLeaser.sprites[n].x = vector16.x;
                    sLeaser.sprites[n].y = vector16.y;

                }
            }
        }
        #endregion //too many lines
        #region pup

        //marshaw is pup now---------------------------------------------------------------------------------------------------------------------------------------
        private void Marshaw_Babify(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if (self.SlugCatClass.value == "marshaw")
            {
                self.playerState.forceFullGrown = false;
                self.playerState.isPup = true;
            }
        }
        #endregion
        #region craft

        //Mod for add the CraftingResults from there------------------------------------------------------------------------------------------------------------------------------------------------------

        private void Marshaw_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            On.MoreSlugcats.GourmandCombos.CraftingResults += GourmandCombos_CraftingResults;

            orig(self);
        }

        //Can Be Craftted here-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            Logger.LogDebug(self.SlugCatClass);
            if (self.SlugCatClass == marshaw)
                //{
                return self.input[0].y == 1 && self.CraftingResults() != null;
            //}

            return orig(self);
        }

        //Craft Results again, im very confuse now--------------------------------------------------------------------------------------------------------------------------------------------------------
        private AbstractPhysicalObject.AbstractObjectType Player_CraftingResults(On.Player.orig_CraftingResults orig, Player self)
        {
            if (self.grasps.Length < 2 || self.SlugCatClass != marshaw) //We need to be holding at least two things
                                                                        //{
                return orig(self);
            //}


            var craftingResult = Marshaw_Craft(self, self.grasps[0], self.grasps[1]);

            return craftingResult?.type;
        }

        //GourmandCombos for Craft above (more above)-------------------------------------------------------------------------------------------------------------------------------------------------------
        private AbstractPhysicalObject GourmandCombos_CraftingResults(On.MoreSlugcats.GourmandCombos.orig_CraftingResults orig, PhysicalObject crafter, Creature.Grasp graspA, Creature.Grasp graspB)
        {
            if ((crafter as Player).SlugCatClass == marshaw)
                //{
                return Marshaw_Craft(crafter as Player, graspA, graspB);
            //}


            return orig(crafter, graspA, graspB);
        }

        //MarshawCraft, here will be the recipes-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public AbstractPhysicalObject Marshaw_Craft(Player player, Creature.Grasp graspA, Creature.Grasp graspB)
        {
            if (player == null || graspA?.grabbed == null || graspB?.grabbed == null) return null;

            //Check grasps here
            if (player.SlugCatClass == marshaw) //normal spear <-------------------------
            {
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeA = graspA.grabbed.abstractPhysicalObject.type;
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeB = graspB.grabbed.abstractPhysicalObject.type;

                if (grabbedObjectTypeA == AbstractPhysicalObject.AbstractObjectType.Rock && grabbedObjectTypeB == AbstractPhysicalObject.AbstractObjectType.Rock)
                {
                    return new AbstractSpear(player.room.world, null, player.abstractCreature.pos, player.room.game.GetNewID(), false);
                }
            }

            if (player.SlugCatClass == marshaw) //explosive spear  <-------------------------
            {
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeA = graspA.grabbed.abstractPhysicalObject.type;
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeB = graspB.grabbed.abstractPhysicalObject.type;

                if (grabbedObjectTypeA == AbstractPhysicalObject.AbstractObjectType.Spear && grabbedObjectTypeB == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
                {
                    return new AbstractSpear(player.room.world, null, player.abstractCreature.pos, player.room.game.GetNewID(), true);
                }
            }

            if (player.SlugCatClass == marshaw) //electric spear  <-------------------------
            {
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeA = graspA.grabbed.abstractPhysicalObject.type;
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeB = graspB.grabbed.abstractPhysicalObject.type;

                if (grabbedObjectTypeA == AbstractPhysicalObject.AbstractObjectType.Spear && grabbedObjectTypeB == AbstractPhysicalObject.AbstractObjectType.FlareBomb)
                {
                    return new AbstractSpear(player.room.world, null, player.abstractCreature.pos, player.room.game.GetNewID(), false, true);
                }
            }
            return null;
        }

        #endregion
        #region ascension Sprite
        //Load any resources, such as sprites or sounds ---------------------------------------------------------------------------------------------------------
        private void LoadResources(RainWorld rainWorld)
        {

            //Reminder: Do not delete try catch
            try
            {
                AssetManager.ResolveFilePath(crossPath); //CrossPath is a path for the .png
                Futile.atlasManager.LoadImage(crossPath); //CrossSource is a File
            }
            catch (Exception ex)
            {
                string assetPath = AssetManager.ResolveFilePath(crossPath + ".png");

                if (!File.Exists(assetPath))
                    Plugin.Logger.LogError("CrossHair could not be found at path " + assetPath);

                Plugin.Logger.LogError(ex);

                //Load fallback resource here
            }
        }
        #endregion

    }
}

#region Credits

//Thalber mitaclau
//NaCio
//luna ☾fallen/endspeaker☽
//Pocky(Burnout/Forager/Siren)
//Elliot (Solace's creator)
//IWannaPresents
//Alduris
//FluffBall
//Rose
//Irradiated Ravioli(Ping me)
//doppelkeks
//Tat011
//Human Resource
//@verityoffaith
//dogcat
//hootis (always ping pls)
//Tuko (bc for my region in first time)
//Ethan Barron
//Bro
//Orinaari (kiki the Scugs)

#endregion
#region reserva
#region On.Enable
/*
 public void OnEnable()
        {
            On.RainWorld.OnModsInit += Extras.WrapInit(LoadResources);
            Logger = base.Logger;
            //We need hooks here
           
            On.Player.CraftingResults += Player_CraftingResults; //Craft Results -- craft
            On.RainWorld.PostModsInit += Marshaw_PostModsInit; //Mod -- craft
            On.Player.GraspsCanBeCrafted += Player_GraspsCanBeCrafted; //Can Be Crafted -- craft
            On.Player.ctor += Marshaw_Babify; //pup -- pup
            On.RainWorld.OnModsInit += RainWorldOnOnModsInit;

        }
 */
#endregion
#region Ascend
/*

        //maybe is a timer from godTimer (is a god)--------------------------------------------------------------------------------------------------------------------------------------------------------
        public static Dictionary<PlayerGraphics, int> godPipsIndex = new();
        // Dictionary for keeping track of the indexes of the added sprites

        private void Player_ctor(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);
            if (self.SlugCatClass == marshaw)
            {
                // Sets the maxGodTime for the godTimer
                self.maxGodTime = (int)(200f + 40f * (float)self.Karma);
                // You won't need this if statement if your slugcat doesn't go to Rubicon
                if (self.room != null && self.room.world.name == "HR")
                {
                    self.maxGodTime = 560f;
                }

                self.godTimer = self.maxGodTime;
            }
        }

        //allow to use the Ascension (but i dont wanna this)-----------------------------------------------------------------------------------------------------------------------------------------------
        private void Player_ClassMechanicsSaint(On.Player.orig_ClassMechanicsSaint orig, Player self)
        {
            orig(self);
            if (self.SlugCatClass == marshaw)
            {
                // Activate and Deactivate Ascension
                if (self.wantToJump > 0 && self.monkAscension)
                {
                    self.DeactivateAscension();
                    self.wantToJump = 0;
                }
                else if (self.wantToJump > 0 && self.input[0].pckp && self.canJump <= 0 && !self.monkAscension && self.bodyMode != Player.BodyModeIndex.Crawl && self.bodyMode != Player.BodyModeIndex.CorridorClimb && self.bodyMode != Player.BodyModeIndex.ClimbIntoShortCut && self.animation != Player.AnimationIndex.HangFromBeam && self.animation != Player.AnimationIndex.ClimbOnBeam && self.bodyMode != Player.BodyModeIndex.WallClimb && self.bodyMode != Player.BodyModeIndex.Swimming && self.Consious && !self.Stunned && self.godTimer > 0f && self.animation != Player.AnimationIndex.AntlerClimb && self.animation != Player.AnimationIndex.VineGrab && self.animation != Player.AnimationIndex.ZeroGPoleGrab)
                {
                    self.ActivateAscension();
                }

            }
        }

        //this will.... uhh.... maybe, initialize their sprites for work?-------------------------------------------------------------------------------------------------------------------
        private void PlayerGraphics_InitiateSprites(On.PlayerGraphics.orig_InitiateSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
        {
            orig(self, sLeaser, rCam);
            if (self.player.SlugCatClass == marshaw)
            {
                Array.Resize(ref sLeaser.sprites, sLeaser.sprites.Length + self.numGodPips + 2);
                // Resizes the sLeaser sprite array to add the amount of numGodPips (12), and 2 for the crosshair and energy burst
                // If you have already resized the sprite array then you need to combine it with above, you will also need to adjust the dictionary as well

                if (godPipsIndex.ContainsKey(self)) { godPipsIndex[self] = sLeaser.sprites.Length - self.numGodPips - 2; }
                else { godPipsIndex.Add(self, sLeaser.sprites.Length - self.numGodPips - 2); }
                // Add self as a key for godPipsIndex dictionary and store the first index of numGodPips + 2
                // godPipsIndex[self] = 13 (index of energy burst)
                // godPipsIndex[self] + 1 = 14 (index of crosshair)
                // godPipsIndex[self] + 2 = 15 (starting index of numGodPips)

                sLeaser.sprites[godPipsIndex[self]] = new FSprite("Futile_White");
                sLeaser.sprites[godPipsIndex[self]].shader = rCam.game.rainWorld.Shaders["FlatLight"];
                // Set sprite for energy burst on ascension
                rCam.ReturnFContainer("Midground").AddChild(sLeaser.sprites[godPipsIndex[self]]);
                // Add to FContainer Midground

                sLeaser.sprites[godPipsIndex[self] + 1] = new FSprite("guardEye");
                // Set sprite for ascension crosshair

                for (int i = 0; i < self.numGodPips; i++)
                {
                    sLeaser.sprites[godPipsIndex[self] + 2 + i] = new FSprite("WormEye");
                    // Set sprite for the godPips timer

                    sLeaser.sprites[godPipsIndex[self] + 2 + i].RemoveFromContainer();
                    rCam.ReturnFContainer("HUD2").AddChild(sLeaser.sprites[godPipsIndex[self] + 2 + i]);
                    // Remove from container and add godPips to FContainer HUD2
                }
            }
        }

        //this use their sprites i guess -----------------------------------------------------------------------------------------------------------------------------------------------------------
        private void PlayerGraphics_DrawSprites(On.PlayerGraphics.orig_DrawSprites orig, PlayerGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);
            if (self.player.room != null && self.player.SlugCatClass == marshaw)
            {
                // Taken from saint's code, handles ascension crosshair, godPips, and effects
                if (self.player.killFac > 0f || self.player.forceBurst)
                {
                    sLeaser.sprites[godPipsIndex[self]].isVisible = true;
                    sLeaser.sprites[godPipsIndex[self]].x = sLeaser.sprites[3].x + self.player.burstX;
                    sLeaser.sprites[godPipsIndex[self]].y = sLeaser.sprites[3].y + self.player.burstY + 60f;
                    float f = Mathf.Lerp(self.player.lastKillFac, self.player.killFac, timeStacker);
                    sLeaser.sprites[godPipsIndex[self]].scale = Mathf.Lerp(50f, 2f, Mathf.Pow(f, 0.5f));
                    sLeaser.sprites[godPipsIndex[self]].alpha = Mathf.Pow(f, 3f);
                }
                else
                {
                    sLeaser.sprites[godPipsIndex[self]].isVisible = false;
                }

                if (self.player.killWait > self.player.lastKillWait || self.player.killWait == 1f || self.player.forceBurst)
                {
                    self.rubberMouseX += (self.player.burstX - self.rubberMouseX) * 0.3f;
                    self.rubberMouseY += (self.player.burstY - self.rubberMouseY) * 0.3f;
                }
                else
                {
                    self.rubberMouseX *= 0.15f;
                    self.rubberMouseY *= 0.25f;
                }

                if (Mathf.Sqrt(Mathf.Pow(sLeaser.sprites[3].x - self.rubberMarkX, 2f) + Mathf.Pow(sLeaser.sprites[3].y - self.rubberMarkY, 2f)) > 100f)
                {
                    self.rubberMarkX = sLeaser.sprites[3].x;
                    self.rubberMarkY = sLeaser.sprites[3].y;
                }
                else
                {
                    self.rubberMarkX += (sLeaser.sprites[3].x - self.rubberMarkX) * 0.15f;
                    self.rubberMarkY += (sLeaser.sprites[3].y - self.rubberMarkY) * 0.25f;
                }

                sLeaser.sprites[godPipsIndex[self] + 1].x = self.rubberMarkX;
                sLeaser.sprites[godPipsIndex[self] + 1].y = self.rubberMarkY + 60f;
                float num16;
                if (self.player.monkAscension)
                {
                    sLeaser.sprites[9].color = Custom.HSL2RGB(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    sLeaser.sprites[10].alpha = 0f;
                    sLeaser.sprites[11].alpha = 0f;
                    sLeaser.sprites[godPipsIndex[self] + 1].color = sLeaser.sprites[9].color;
                    num16 = 1f;
                }
                else
                {
                    num16 = 0f;
                }

                float num17;
                if ((self.player.godTimer < self.player.maxGodTime || self.player.monkAscension) && !self.player.hideGodPips)
                {
                    num17 = 1f;
                    float num18 = 15f;
                    if (!self.player.monkAscension)
                    {
                        num18 = 6f;
                    }

                    self.rubberRadius += (num18 - self.rubberRadius) * 0.045f;
                    if (self.rubberRadius < 5f)
                    {
                        self.rubberRadius = num18;
                    }

                    float num19 = self.player.maxGodTime / (float)self.numGodPips;
                    for (int m = 0; m < self.numGodPips; m++)
                    {
                        float num20 = num19 * (float)m;
                        float num21 = num19 * (float)(m + 1);
                        if (self.player.godTimer <= num20)
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].scale = 0f;
                        }
                        else if (self.player.godTimer >= num21)
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].scale = 1f;
                        }
                        else
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].scale = (self.player.godTimer - num20) / num19;
                        }

                        if (self.player.karmaCharging > 0 && self.player.monkAscension)
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].color = sLeaser.sprites[9].color;
                        }
                        else
                        {
                            sLeaser.sprites[godPipsIndex[self] + 2 + m].color = PlayerGraphics.SlugcatColor(self.CharacterForColor);
                        }
                    }
                }
                else
                {
                    num17 = 0f;
                }

                sLeaser.sprites[godPipsIndex[self] + 1].x = self.rubberMarkX + self.rubberMouseX;
                sLeaser.sprites[godPipsIndex[self] + 1].y = self.rubberMarkY + 60f + self.rubberMouseY;
                self.rubberAlphaEmblem += (num16 - self.rubberAlphaEmblem) * 0.05f;
                self.rubberAlphaPips += (num17 - self.rubberAlphaPips) * 0.05f;
                sLeaser.sprites[godPipsIndex[self] + 1].alpha = self.rubberAlphaEmblem;
                sLeaser.sprites[10].alpha *= 1f - self.rubberAlphaPips;
                sLeaser.sprites[11].alpha *= 1f - self.rubberAlphaPips;
                for (int n = godPipsIndex[self] + 2; n < godPipsIndex[self] + 2 + self.numGodPips; n++)
                {
                    sLeaser.sprites[n].alpha = self.rubberAlphaPips;
                    Vector2 vector16 = new Vector2(sLeaser.sprites[14].x, sLeaser.sprites[14].y);
                    vector16 += Custom.rotateVectorDeg(Vector2.one * self.rubberRadius, (float)(n - 15) * (360f / (float)self.numGodPips));
                    sLeaser.sprites[n].x = vector16.x;
                    sLeaser.sprites[n].y = vector16.y;
                }
            }
        } 
 */
#endregion
#region pup
/*
//marshaw is pup now-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private void Marshaw_Babify(On.Player.orig_ctor orig, Player self, AbstractCreature abstractCreature, World world)
        {
            orig(self, abstractCreature, world);

            if (self.SlugCatClass.value == "marshaw")
            {
                self.playerState.forceFullGrown = false;
                self.playerState.isPup = true;
            }
        }
 */
#endregion
#region craft
/*
//Mod for add the CraftingResults from there------------------------------------------------------------------------------------------------------------------------------------------------------

        private void Marshaw_PostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            On.MoreSlugcats.GourmandCombos.CraftingResults += GourmandCombos_CraftingResults;

            orig(self);
        }

        //Can Be Craftted here-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private bool Player_GraspsCanBeCrafted(On.Player.orig_GraspsCanBeCrafted orig, Player self)
        {
            Logger.LogDebug(self.SlugCatClass);
            if (self.SlugCatClass == marshaw)
                //{
                    return self.input[0].y == 1 && self.CraftingResults() != null;
                //}

            return orig(self);
        }

        //Craft Results again, im very confuse now--------------------------------------------------------------------------------------------------------------------------------------------------------
        private AbstractPhysicalObject.AbstractObjectType Player_CraftingResults(On.Player.orig_CraftingResults orig, Player self)
        {
            if (self.grasps.Length < 2 || self.SlugCatClass != marshaw) //We need to be holding at least two things
             //{
                return orig(self);
             //}


            var craftingResult = Marshaw_Craft(self, self.grasps[0], self.grasps[1]);

            return craftingResult?.type;
        }

        //GourmandCombos for Craft above (more above)-------------------------------------------------------------------------------------------------------------------------------------------------------
        private AbstractPhysicalObject GourmandCombos_CraftingResults(On.MoreSlugcats.GourmandCombos.orig_CraftingResults orig, PhysicalObject crafter, Creature.Grasp graspA, Creature.Grasp graspB)
        {
            if ((crafter as Player).SlugCatClass == marshaw)
            //{
                return Marshaw_Craft(crafter as Player, graspA, graspB);
            //}


            return orig(crafter, graspA, graspB);
        }

        //MarshawCraft, here will be the recipes-----------------------------------------------------------------------------------------------------------------------------------------------------------
        public AbstractPhysicalObject Marshaw_Craft(Player player, Creature.Grasp graspA, Creature.Grasp graspB)
        {
            if (player == null || graspA?.grabbed == null || graspB?.grabbed == null) return null;

            //Check grasps here
            if (player.SlugCatClass == marshaw) //normal spear <-------------------------
            {
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeA = graspA.grabbed.abstractPhysicalObject.type;
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeB = graspB.grabbed.abstractPhysicalObject.type;

                if (grabbedObjectTypeA == AbstractPhysicalObject.AbstractObjectType.Rock && grabbedObjectTypeB == AbstractPhysicalObject.AbstractObjectType.Rock)
                {
                    return new AbstractSpear(player.room.world, null, player.abstractCreature.pos, player.room.game.GetNewID(), false);
                }
            }

            if (player.SlugCatClass == marshaw) //explosive spear  <-------------------------
            {
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeA = graspA.grabbed.abstractPhysicalObject.type;
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeB = graspB.grabbed.abstractPhysicalObject.type;

                if (grabbedObjectTypeA == AbstractPhysicalObject.AbstractObjectType.Spear && grabbedObjectTypeB == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
                {
                    return new AbstractSpear(player.room.world, null, player.abstractCreature.pos, player.room.game.GetNewID(), true);
                }
            }

            if (player.SlugCatClass == marshaw) //electric spear  <-------------------------
            {
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeA = graspA.grabbed.abstractPhysicalObject.type;
                AbstractPhysicalObject.AbstractObjectType grabbedObjectTypeB = graspB.grabbed.abstractPhysicalObject.type;

                if (grabbedObjectTypeA == AbstractPhysicalObject.AbstractObjectType.Spear && grabbedObjectTypeB == AbstractPhysicalObject.AbstractObjectType.FlareBomb)
                {
                    return new AbstractSpear(player.room.world, null, player.abstractCreature.pos, player.room.game.GetNewID(), false, true);
                }
            }
            return null;
        }

 */
#endregion
#endregion
#region sommethfor invencibility
//if (slugcat.hp == 0) slugcat.hp = 1 billion;

/*
On.Player.Die += Player_Die1;

        }

        private void Player_Die1(On.Player.orig_Die orig, Player self)
        {
            orig(self);
            if (self.slugcatStats.name.value == "Kiki_slugcat")
            {
                self.glowing = false;
            }
        }
 */
#endregion 
