using System;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using UnityEngine;
using System.Text;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

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
        //void OnEntityBuilt(Planner plan, GameObject go)
        //{
        //    if (go.ToBaseEntity() is BuildingBlock)
        //    {
        //        IncrementPlayerItemChangeNotificationCount(plan?.GetOwnerPlayer(), $"removed {BuildingGrade.Enum.Wood}");
        //    }
        //}
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

namespace Oxide.Plugins
{
    internal partial class CustomStatusFramework : CovalencePlugin
    {
        private void CreateStatus(string id, string text, string subText, string color, string imageLibraryIconId, Func<BasePlayer, bool> condition)
        {
            CustomStatuses[id] = new CustomStatus
            {
                Id = id,
                LeftText = text,
                RightText = subText,
                Color = color,
                Icon = imageLibraryIconId,
                OnCondition = condition
            };
        }

        private void CreateDynamicStatus(string id, string text, string color, string imageLibaryIconId, Func<BasePlayer, bool> condition, Func<BasePlayer, string> dynamicValue)
        {
            CustomStatuses[id] = new CustomStatus
            {
                Id = id,
                LeftText = text,
                Color = color,
                Icon = imageLibaryIconId,
                OnCondition = condition,
                DynamicText = dynamicValue
            };
        }
    }
}

namespace Oxide.Plugins
{
    internal partial class CustomStatusFramework : CovalencePlugin
    {
        public class CustomStatus
        {
            public string Id { get; set; }
            public string Color { get; set; } = "0.9 0.9 0.9 1";
            public string Icon { get; set; }
            public string LeftText { get; set; } = string.Empty;
            public string RightText { get; set; } = string.Empty;
            public Func<BasePlayer, string> DynamicText { get; set; } = null;
            public bool IsDynamic
            {
                get
                {
                    return DynamicText != null;
                }
            }
            public Func<BasePlayer, bool> OnCondition { get; set; } = (x) => { return true; };
            public bool IsTriggered(BasePlayer player)
            {
                return OnCondition.Invoke(player);
            }
        }
    }
}

namespace Oxide.Plugins
{
    internal partial class CustomStatusFramework : CovalencePlugin
    {
        public class DynamicElement
        {
            public CuiElement Element { get; set; }
            public CustomStatus CustomStatus { get; set; }
        }
    }
}

namespace Oxide.Plugins
{
    internal partial class CustomStatusFramework : CovalencePlugin
    {

        private readonly Dictionary<string, int> PlayerStatusCounts = new Dictionary<string, int>();

        private readonly Dictionary<string, List<DynamicElement>> DynamicElements = new Dictionary<string, List<DynamicElement>>();

        private readonly Dictionary<string, CustomStatus> CustomStatuses = new Dictionary<string, CustomStatus>();

        private string GetIcon(string icon)
        {
            return ImageLibrary?.Call<string>("GetImage", $"{icon}");
        }

        private List<CustomStatus> GetCustomStatuses(BasePlayer basePlayer)
        {
            return CustomStatuses.Values.Where(x => x.IsTriggered(basePlayer)).ToList();
        }

        private List<string> GetStatuses(BasePlayer basePlayer)
        {
            var statuses = new List<string>();
            if (basePlayer.metabolism.bleeding.value > 0)
            {
                statuses.Add("bleeding");
            }
            if (basePlayer.metabolism.temperature.value < 5)
            {
                statuses.Add("toocold");
            }
            if (basePlayer.metabolism.temperature.value > 40)
            {
                statuses.Add("toohot");
            }
            if (basePlayer.currentComfort > 0)
            {
                statuses.Add("comfort");
            }
            if (basePlayer.metabolism.calories.value < 40)
            {
                statuses.Add("starving");
            }
            if (basePlayer.metabolism.hydration.value < 25)
            {
                statuses.Add("dehydrated");
            }
            if (basePlayer.metabolism.radiation_poison.value > 0)
            {
                statuses.Add("radiation");
            }
            if (basePlayer.metabolism.wetness.value >= 0.02)
            {
                statuses.Add("wet");
            }
            if (basePlayer.metabolism.oxygen.value < 1f)
            {
                statuses.Add($"drowning {basePlayer.metabolism.oxygen.value}");
            }
            if (basePlayer.currentCraftLevel > 0)
            {
                statuses.Add($"workbench");
            }
            if (basePlayer.inventory.crafting.queue.Count > 0)
            {
                statuses.Add($"crafting");
            }
            var priv = basePlayer.GetBuildingPrivilege();
            if (priv != null && priv.IsAuthed(basePlayer))
            {
                statuses.Add($"buildpriv");
                statuses.Add($"upkeep");
            }
            if (PlayerItemPickupNotifications.ContainsKey(basePlayer.UserIDString))
            {
                for(int i = 0; i< PlayerItemPickupNotifications[basePlayer.UserIDString].Distinct().Count(); i++)
                {
                    statuses.Add($"itemchange");
                }
            }
            return statuses;
        }

        private void DestroyAllStatusHuds()
        {
            foreach(var basePlayer in BasePlayer.activePlayerList)
            {
                CuiHelper.DestroyUi(basePlayer, STATUS_HUD_ID);
            }
        }

        private readonly string STATUS_HUD_ID = "status";

        private void UpdateStatusHUD(BasePlayer basePlayer, List<string> statuses, List<CustomStatus> customs)
        {
            if (basePlayer == null)
            {
                return;
            }
            if (statuses.Count == 0)
            {
                if (customs.Count == 0)
                {
                    DynamicElements[basePlayer.UserIDString] = new List<DynamicElement>();
                }
                CuiHelper.DestroyUi(basePlayer, STATUS_HUD_ID);
            }
            string parent = STATUS_HUD_ID;
            var container = new CuiElementContainer();
            var idx = statuses.Count;
            var eh = 26;
            var eg = 2;
            var startY = (eh + eg) * idx;
            var numEntrees = 12;
            var x = 1072;
            var y = 100 + startY;
            var w = 192;
            var h = (eh + eg) * numEntrees - startY;
            // Base
            container.Add(new CuiElement
            {
                Name = parent,
                Parent = "Hud",
                Components =
                {
                    new CuiImageComponent
                    {
                        Color = "1 0 0 0"
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = "0 0",
                        AnchorMax = "0 0",
                        OffsetMin = $"{x} {y}",
                        OffsetMax = $"{x+w} {y+h}"
                    }
                }
            });
            // Custom Statuses
            idx++;
            var ey = 0;
            var fontSize = 13;
            var fontColor = "0.78 0.78 0.78 1";
            var left = 26;
            var padding = 8;
            var imgP = 5;
            var imgS = 14;
            var dynamics = new List<DynamicElement>();
            foreach (var custom in customs)
            {
                var id = $"{parent}.{idx}";
                container.Add(new CuiElement
                {
                    Name = id,
                    Parent = parent,
                    Components =
                    {
                        new CuiImageComponent
                        {
                            Color = custom.Color,
                            Material = "assets/scenes/test/waterlevelterrain/watertexture.png"
                        },
                        new CuiRectTransformComponent
                        {
                            AnchorMin = "0 0",
                            AnchorMax = "1 0",
                            OffsetMin = $"{0} {ey}",
                            OffsetMax = $"{0} {ey+eh}"
                        }
                    }
                });
                var iconElement = new CuiElement
                {
                    Parent = id,
                    Components =
                    {
                        new CuiRectTransformComponent
                        {
                            AnchorMin = "0 0.5",
                            AnchorMax = "0 0.5",
                            OffsetMin = $"{imgP} {-imgS/2}",
                            OffsetMax = $"{imgP+imgS} {imgS/2}"
                        }
                    }
                };
                if (!string.IsNullOrEmpty(custom.Icon))
                {
                    iconElement.Components.Add(new CuiImageComponent
                    {
                        Color = "0 0 0 0.8",
                        Png = GetIcon(custom.Icon)
                    });
                }
                container.Add(iconElement);
                container.Add(new CuiElement
                {
                    Parent = id,
                    Components =
                    {
                        new CuiTextComponent
                        {
                            Text = custom.LeftText.ToUpper(),
                            FontSize = fontSize,
                            Color = fontColor,
                            Align = TextAnchor.MiddleLeft
                        },
                        new CuiRectTransformComponent
                        {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1",
                            OffsetMin = $"{left} {0}",
                            OffsetMax = $"{0} {0}"
                        }
                    }
                });
                var rightText = new CuiElement
                {
                    Name = $"{id}.value",
                    Parent = id,
                    Components =
                    {
                        new CuiTextComponent
                        {
                            Text = !custom.IsDynamic ? custom.RightText : custom.DynamicText(basePlayer),
                            FontSize = fontSize,
                            Color = fontColor,
                            Align = TextAnchor.MiddleRight
                        },
                        new CuiRectTransformComponent
                        {
                            AnchorMin = "0 0",
                            AnchorMax = "1 1",
                            OffsetMin = $"{0} {0}",
                            OffsetMax = $"{-padding} {0}"
                        }
                    }
                };
                container.Add(rightText);
                if (custom.IsDynamic)
                {
                    dynamics.Add(new DynamicElement { Element = rightText, CustomStatus = custom });
                }
                idx++;
                ey += eh + eg;
            }
            DynamicElements[basePlayer.UserIDString] = dynamics;
            CuiHelper.DestroyUi(basePlayer, parent);
            CuiHelper.AddUi(basePlayer, container);
        }
    }
}

namespace Oxide.Plugins
{
    internal partial class CustomStatusFramework : CovalencePlugin
    {
        public class NullSafeDictionary<K, V> : Dictionary<K, V> where V : new()
        {
            new public V this[K key]
            {
                get
                {
                    try
                    {
                        return base[key];
                    }
                    catch
                    {
                        base[key] = new V();
                        return base[key];
                    }
                }
            }
        }
    }
}
