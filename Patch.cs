﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using SPT.Reflection.Patching;
using dvize.BushNoESP;
using EFT;
using EFT.Ballistics;
using HarmonyLib;
using UnityEngine;
using BepInEx.Logging;
using System.Globalization;

#pragma warning disable IDE0007

namespace NoBushESP
{
    public class NoBushPatch : ModulePatch
    {

        private static readonly List<string> exclusionList = new List<string>
        { "filbert", "fibert", "tree", "pine", "plant", "birch", "collider", "timber", "spruce", "bush", "metal", "wood"};

        private static readonly List<MaterialType> extraMaterialList = new List<MaterialType> { MaterialType.Glass, MaterialType.GlassShattered };
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsGroup), "CalcGoalForBot");
        }

        [PatchPostfix]
        public static void PatchPostfix(BotOwner bot)
        {
            try
            {
                var goalEnemy = bot.Memory.GoalEnemy;
                if (goalEnemy == null) return;

                var person = bot.Memory.GoalEnemy.Person;
                if (person.IsAI) return;

                Vector3 headPosition = bot.MainParts[BodyPartType.head].Position;
                Vector3 enemyHeadPosition = person.MainParts[BodyPartType.head].Position;
                Vector3 direction = enemyHeadPosition - headPosition;
                float distance = direction.magnitude;

                if (!Physics.Raycast(new Ray(headPosition, direction), out RaycastHit hitInfo, distance, GetLayerMask())) return;

                string objectName = hitInfo.transform.parent?.gameObject?.name?.ToLower();

                if (objectName == null) return;

                if (Settings.DebugEnabled.Value) Plugin.LogSource.LogInfo($"Raycast check {objectName}");

                if (exclusionList.Any(exclusion => objectName.Contains(exclusion)))
                {
                    BlockShooting(bot, goalEnemy);
                    if (Settings.DebugEnabled.Value) Plugin.LogSource.LogInfo($"Shot blocked by {objectName}");
                    return;
                }

                MaterialType materialType = hitInfo.transform.gameObject.GetComponentInParent<BallisticCollider>()?.TypeOfMaterial ?? default;

                if (Settings.DebugEnabled.Value) Plugin.LogSource.LogInfo($"Raycast check material {materialType}");

                if (IsMaterialBlockingShot(materialType, hitInfo.transform.position, headPosition))
                {
                    if (Settings.DebugEnabled.Value)
                        Plugin.LogSource.LogInfo($"Shot blocked by {materialType}");

                    BlockShooting(bot, goalEnemy);
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError(ex.ToString());
            }
        }

        private static bool IsMaterialBlockingShot(MaterialType materialType, Vector3 hitPosition, Vector3 shooterPosition)
        {
            float hitDistance = Vector3.Distance(hitPosition, shooterPosition);
            if ((materialType == MaterialType.GrassHigh || materialType == MaterialType.GrassLow) && hitDistance > 25) return true;
            if (hitDistance > 50 && extraMaterialList.Contains(materialType)) return true;

            return false;
        }

        private static LayerMask GetLayerMask()
        {
            return LayerMaskClass.HighPolyWithTerrainMaskAI | 30 | 31;
        }

        private static void BlockShooting(BotOwner bot, EnemyInfo goalEnemy)
        {
            try
            {
                ReflectionHelper.SetProperty(goalEnemy, "IsVisible", false);
                bot.AimingManager.CurrentAiming.LoseTarget();
                bot.ShootData.EndShoot();

                ReflectionHelper.SetProperty(bot.ShootData, "CanShootByState", false);
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError("BlockShooting " + ex.ToString());
            }
        }
    }
}