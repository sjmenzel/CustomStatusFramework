using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("CustomStatusFramework", "mr01sam", "1.0.0")]
    [Description("Allows plugins to add custom status displays for the UI")]
    internal partial class CustomStatusFramework : CovalencePlugin
    {
        [PluginReference] private readonly Plugin ImageLibrary;
        void Init()
        {
            Unsubscribe(nameof(Unload));
            Unsubscribe(nameof(OnPlayerMetabolize));
        }

        void Unload()
        {
            DestroyAllStatusHuds();
        }

        void OnServerInitialized()
        {
            Subscribe(nameof(Unload));
            Subscribe(nameof(OnPlayerMetabolize));

            //Func<BasePlayer, bool> condition = (basePlayer) =>
            //{
            //    var itemsWorn = basePlayer.inventory.containerWear;
            //    return itemsWorn.itemList.Any(item => item.info.shortname == "santahat") && itemsWorn.itemList.Any(item => item.info.shortname == "santabeard");
            //};
            //Call("CreateStatus", "santa", "Santa", "Ho ho ho!", "0.9 0.3 0.3 1", "img_id_here", condition);
        }
        private readonly NullSafeDictionary<string, List<string>> PlayerItemPickupNotifications = new NullSafeDictionary<string, List<string>>();
        //private readonly Dictionary<string, int> PlayerItemPickupNotifications = new Dictionary<string, int>();
        object OnItemPickup(Item item, BasePlayer basePlayer)
        {
            IncrementPlayerItemChangeNotificationCount(basePlayer, item?.info?.shortname);
            return null;
        }
        void OnEntityBuilt(Planner plan, GameObject go)
        {
            if (go.ToBaseEntity() is BuildingBlock)
            {
                IncrementPlayerItemChangeNotificationCount(plan?.GetOwnerPlayer(), $"removed {BuildingGrade.Enum.Wood}");
            }
        }
        object OnStructureUpgrade(BaseCombatEntity entity, BasePlayer basePlayer, BuildingGrade.Enum grade)
        {
            IncrementPlayerItemChangeNotificationCount(basePlayer, $"removed {grade}");
            return null;
        }
        object OnCollectiblePickup(CollectibleEntity collectible, BasePlayer player)
        {
            return null;
        }
        bool CanPickupEntity(BasePlayer basePlayer, BaseEntity entity)
        {
            var name = entity.name;
            NextTick(() =>
            {
                Puts($"Entity is null {entity == null} {name}");
                if (entity == null)
                {
                    IncrementPlayerItemChangeNotificationCount(basePlayer, name);
                }
            });
            return true;
        }
        object OnStructureRepair(BuildingBlock entity, BasePlayer basePlayer)
        {
            var curHp = entity.health;
            NextTick(() =>
            {
                if (entity != null)
                {
                    var newHp = entity.health;
                    if (newHp > curHp)
                    {
                        IncrementPlayerItemChangeNotificationCount(basePlayer, $"removed {entity.grade}");
                    }
                }
            });
            return null;
        }
        object OnDispenserGather(ResourceDispenser dispenser, BasePlayer basePlayer, Item item)
        {
            IncrementPlayerItemChangeNotificationCount(basePlayer, item.info.shortname);
            return null;
        }
        void IncrementPlayerItemChangeNotificationCount(BasePlayer basePlayer, string key)
        {
            if (basePlayer == null || key == null)
            {
                return;
            }
            PlayerItemPickupNotifications[basePlayer.UserIDString].Add(key);
            timer.In(4f, () =>
            {
                if (basePlayer != null)
                {
                    PlayerItemPickupNotifications[basePlayer.UserIDString].Remove(key);
                }
            });
        }

        void OnPlayerMetabolize(PlayerMetabolism metabolism, BasePlayer basePlayer, float delta)
        {
            if (basePlayer != null && delta != 0)
            {
                var statuses = GetStatuses(basePlayer);
                var customs = GetCustomStatuses(basePlayer);
                var combined = statuses.Count + customs.Count;
                if (!PlayerStatusCounts.ContainsKey(basePlayer.UserIDString) || PlayerStatusCounts[basePlayer.UserIDString] != combined)
                {
                    UpdateStatusHUD(basePlayer, statuses, customs);
                    PlayerStatusCounts[basePlayer.UserIDString] = combined;
                }
                else if (DynamicElements.ContainsKey(basePlayer.UserIDString) && DynamicElements.Count > 0)
                {
                    foreach (var de in DynamicElements[basePlayer.UserIDString])
                    {
                        (de.Element.Components[0] as CuiTextComponent).Text = de.CustomStatus.DynamicText.Invoke(basePlayer);
                        CuiHelper.DestroyUi(basePlayer, de.Element.Name);
                        CuiHelper.AddUi(basePlayer, new CuiElementContainer { de.Element });
                    }
                }
            }
        }
    }
}