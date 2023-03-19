using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Oxide.Core.Libraries.Covalence;
using Newtonsoft.Json;

namespace Oxide.Plugins
{
    [Info("Custom Status Framework", "mr01sam", "1.0.3")]
    [Description("Allows plugins to add custom status displays for the UI")]
    internal partial class CustomStatusFramework : CovalencePlugin
    {
        [PluginReference] 
        private readonly Plugin ImageLibrary;

        void Init()
        {
            Unsubscribe(nameof(Unload));
            Unsubscribe(nameof(OnPlayerMetabolize));
            Unsubscribe(nameof(OnItemPickup));
            Unsubscribe(nameof(OnStructureUpgrade));
            Unsubscribe(nameof(CanPickupEntity));
            Unsubscribe(nameof(OnStructureRepair));
            Unsubscribe(nameof(OnDispenserGather));
        }

        void Unload()
        {
            DestroyAllStatusHuds();
        }

        void OnServerInitialized()
        {
            try
            {
                ImageLibrary.Call("isLoaded", null);
            }
            catch (Exception)
            {
                PrintWarning($"The required dependency ImageLibary is not installed, {Name} will not work properly without it.");
            }
            Subscribe(nameof(Unload));
            Subscribe(nameof(OnPlayerMetabolize));
            Subscribe(nameof(OnItemPickup));
            Subscribe(nameof(OnStructureUpgrade));
            Subscribe(nameof(CanPickupEntity));
            Subscribe(nameof(OnStructureRepair));
            Subscribe(nameof(OnDispenserGather));
        }
    }
}

namespace Oxide.Plugins
{
    internal partial class CustomStatusFramework : CovalencePlugin
    {
        private List<string> GetStatusList(BasePlayer basePlayer)
        {
            if (basePlayer == null || basePlayer.IsNpc)
            {
                return new List<string>();
            }
            return GetAllStatuses(basePlayer);
        }

        private bool HasStatus(BasePlayer basePlayer, string id)
        {
            if (basePlayer == null || basePlayer.IsNpc)
            {
                return false;
            }
            return GetStatusList(basePlayer).Exists(x => x == id);
        }

        private void ShowStatus(BasePlayer basePlayer, string id, string color = null, string text = null, string textColor = null, string subText = null, string subTextColor = null, string imageLibraryIconId = null, string iconColor = null, float seconds = 4f)
        {
            if (basePlayer == null || basePlayer.IsNpc)
            {
                return;
            }
            SetStatus(basePlayer, id, color, text, textColor, subText, subTextColor, imageLibraryIconId, iconColor);
            timer.In(seconds, () =>
            {
                if (basePlayer != null)
                {
                    StaticStatuses[basePlayer.UserIDString].Remove(StaticStatuses[basePlayer.UserIDString].FirstOrDefault(x => x.Id == id));
                }
            });
        }

        private void UpdateStatus(BasePlayer basePlayer, string id, string color, string text, string textColor, string subText, string subTextColor, string imageLibraryIconId, string iconColor)
        {
            if (basePlayer == null || basePlayer.IsNpc)
            {
                return;
            }
            ClearStatus(basePlayer, id);
            SetStatus(basePlayer, id, color, text, textColor, subText, subTextColor, imageLibraryIconId, iconColor);
            var statuses = GetStatuses(basePlayer);
            var customs = GetCustomStatuses(basePlayer);
            var statics = GetStaticStatuses(basePlayer);
            UpdateStatusHUD(basePlayer, statuses, customs.Concat(statics).ToList());
        }

        private void SetStatus(BasePlayer basePlayer, string id, string color, string text, string textColor, string subText, string subTextColor, string imageLibraryIconId, string iconColor)
        {
            if (basePlayer == null || basePlayer.IsNpc)
            {
                return;
            }
            StaticStatuses[basePlayer.UserIDString].Add(new CustomStatus
            {
                Id = id,
                LeftText = text,
                RightText = subText,
                TextColor = textColor,
                SubTextColor = subTextColor,
                Color = color,
                Icon = imageLibraryIconId,
                IconColor = iconColor
            });
        }

        private void ClearStatus(BasePlayer basePlayer, string id)
        {
            if (basePlayer == null || basePlayer.IsNpc)
            {
                return;
            }
            StaticStatuses[basePlayer.UserIDString].RemoveAll(x => x.Id == id);
        }

        private void DeleteStatus(string id)
        {
            CustomStatuses.Remove(id);
        }

        private void CreateStatus(string id, string color, string text, string subText, string textColor, string subTextColor, string imageLibraryIconId, string iconColor, Func<BasePlayer, bool> condition)
        {
            CustomStatuses[id] = new CustomStatus
            {
                Id = id,
                LeftText = text,
                RightText = subText,
                TextColor = textColor,
                Color = color,
                Icon = imageLibraryIconId,
                SubTextColor =subTextColor,
                IconColor = iconColor,
                OnCondition = condition
            };
        }

        private void CreateDynamicStatus(string id, string color, string text, string textColor, string subTextColor, string imageLibraryIconId, string iconColor, Func<BasePlayer, bool> condition, Func<BasePlayer, string> dynamicValue)
        {
            CustomStatuses[id] = new CustomStatus
            {
                Id = id,
                LeftText = text,
                TextColor = textColor,
                Color = color,
                Icon = imageLibraryIconId,
                SubTextColor = subTextColor,
                IconColor = iconColor,
                OnCondition = condition,
                DynamicText = dynamicValue
            };
        }
    }
}

namespace Oxide.Plugins
{
    partial class CustomStatusFramework
    {
        public static class RustColor
        {
            public static string Blue = "0.08627 0.25490 0.38431 1";
            public static string LightBlue = "0.25490 0.61176 0.86275 1";
            public static string Red = "0.68627 0.21569 0.14118 1";
            public static string LightRed = "0.91373 0.77647 0.75686 1";
            //public static string Green = "0.25490 0.30980 0.14510 1";
            public static string Green = "0.35490 0.40980 0.24510 1";
            public static string LightGreen = "0.76078 0.94510 0.41176 1";
            public static string Gray = "0.45490 0.43529 0.40784 1";
            public static string LightGray = "0.69804 0.66667 0.63529 1";
            public static string Orange = "1.00000 0.53333 0.18039 1";
            public static string LightOrange = "1.00000 0.82353 0.56471 1";
            public static string White = "0.87451 0.83529 0.80000 1";
            public static string LightWhite = "0.97647 0.97647 0.97647 1";
            public static string Lime = "0.64706 1.00000 0.00000 1";
            public static string LightLime = "0.69804 0.83137 0.46667 1";
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
            public string Color { get; set; } = RustColor.Gray;
            public string Icon { get; set; } = string.Empty;
            public string IconColor { get; set; } = RustColor.White;
            public string LeftText { get; set; } = string.Empty;
            public string RightText { get; set; } = string.Empty;
            public string TextColor { get; set; } = RustColor.White;
            public string SubTextColor { get; set; } = RustColor.LightGray;
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
            public override bool Equals(object obj)
            {
                if ((obj == null) || !this.GetType().Equals(obj.GetType()))
                {
                    return false;
                }
                else
                {
                    var p = obj as CustomStatus;
                    return (Id == p.Id);
                }
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
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
        object OnItemPickup(Item item, BasePlayer basePlayer)
        {
            IncrementPlayerItemChangeNotificationCount(basePlayer, item?.info?.shortname);
            return null;
        }

        object OnStructureUpgrade(BaseCombatEntity entity, BasePlayer basePlayer, BuildingGrade.Enum grade)
        {
            IncrementPlayerItemChangeNotificationCount(basePlayer, $"removed {grade}");
            return null;
        }

        bool CanPickupEntity(BasePlayer basePlayer, BaseEntity entity)
        {
            var name = entity.name;
            NextTick(() =>
            {
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
        void OnPlayerMetabolize(PlayerMetabolism metabolism, BasePlayer basePlayer, float delta)
        {
            if (basePlayer != null && delta != 0)
            {
                var statuses = GetStatuses(basePlayer);
                var customs = GetCustomStatuses(basePlayer);
                var statics = GetStaticStatuses(basePlayer);
                var combined = statuses.Count + customs.Count + statics.Count;
                if (!PlayerStatusCounts.ContainsKey(basePlayer.UserIDString) || PlayerStatusCounts[basePlayer.UserIDString] != combined)
                {
                    UpdateStatusHUD(basePlayer, statuses, customs.Concat(statics).ToList());
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

        private readonly NullSafeDictionary<string, int> PlayerStatusCounts = new NullSafeDictionary<string, int>();

        private readonly NullSafeDictionary<string, List<DynamicElement>> DynamicElements = new NullSafeDictionary<string, List<DynamicElement>>();

        private readonly NullSafeDictionary<string, CustomStatus> CustomStatuses = new NullSafeDictionary<string, CustomStatus>();

        private readonly NullSafeDictionary<string, List<CustomStatus>> StaticStatuses = new NullSafeDictionary<string, List<CustomStatus>>();

        private readonly NullSafeDictionary<string, List<string>> PlayerItemPickupNotifications = new NullSafeDictionary<string, List<string>>();

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

        private List<string> GetAllStatuses(BasePlayer basePlayer)
        {
            return GetStatuses(basePlayer)
                .Concat(GetCustomStatuses(basePlayer)
                .Select(x => x.Id))
                .Concat(GetStaticStatuses(basePlayer)
                .Select(x => x.Id))
                .ToList();
        }

        private List<CustomStatus> GetStaticStatuses(BasePlayer basePlayer)
        {
            return StaticStatuses[basePlayer.UserIDString].Distinct().ToList();
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
            if (basePlayer.modifiers.ActiveModifierCoount > 0)
            {
                statuses.Add($"modifiers");
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
            var x = -16;
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
                        Color = "0 0 0 0"
                    },
                    new CuiRectTransformComponent
                    {
                        AnchorMin = "1 0",
                        AnchorMax = "1 0",
                        OffsetMin = $"{x-w} {y}",
                        OffsetMax = $"{x} {y+h}"
                    }
                }
            });
            // Custom Statuses
            idx++;
            var ey = 0;
            var fontSize = 13;
            var left = 26;
            var padding = 8;
            var imgP = 5;
            var imgS = 14;
            var dynamics = new List<DynamicElement>();
            foreach (var custom in customs)
            {
                var fontColor = string.IsNullOrEmpty(custom.TextColor) ? RustColor.White : custom.TextColor;
                var subFontColor = string.IsNullOrEmpty(custom.SubTextColor) ? RustColor.LightGray : custom.SubTextColor;
                var bgColor = string.IsNullOrEmpty(custom.Color) ? RustColor.Gray : custom.Color;
                var iconColor = string.IsNullOrEmpty(custom.IconColor) ? RustColor.White : custom.IconColor;
                var id = $"{parent}.{idx}";
                var leftText = string.IsNullOrEmpty(custom.LeftText) ? string.Empty : custom.LeftText;
                var rightText = string.IsNullOrEmpty(custom.RightText) ? string.Empty : custom.RightText;
                container.Add(new CuiElement
                {
                    Name = id,
                    Parent = parent,
                    Components =
                    {
                        new CuiImageComponent
                        {
                            Color = bgColor,
                            Material = "assets/icons/greyout.mat"
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
                        Color = iconColor,
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
                            Text = leftText.ToUpper(),
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
                var rightTextE = new CuiElement
                {
                    Name = $"{id}.value",
                    Parent = id,
                    Components =
                    {
                        new CuiTextComponent
                        {
                            Text = !custom.IsDynamic ? rightText : custom.DynamicText(basePlayer),
                            FontSize = 11,
                            Color = subFontColor,
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
                container.Add(rightTextE);
                if (custom.IsDynamic)
                {
                    dynamics.Add(new DynamicElement { Element = rightTextE, CustomStatus = custom });
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
        private string GetIcon(string icon)
        {
            return ImageLibrary?.Call<string>("GetImage", $"{icon}");
        }

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
                set
                {
                    base[key] = value;
                }
            }
        }
    }
}
