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
    [Description("Allows adding of custom status displays for the UI")]
    internal partial class CustomStatusFramework : CovalencePlugin
    {
        [PluginReference] private readonly Plugin ImageLibrary;

        void Init()
        {

        }

        private void OnServerInitialized()
        {
            ImageLibrary.Call<bool>("AddImage", "https://i.imgur.com/fbELboi.png", $"testicon", 0UL);
            ImageLibrary.Call<bool>("AddImage", "https://uxwing.com/wp-content/themes/uxwing/download/time-and-date/clock-icon.png", $"clock", 0UL);
            ImageLibrary.Call<bool>("AddImage", "https://uxwing.com/wp-content/themes/uxwing/download/crime-security-military-law/shield-icon.png", $"shield", 0UL);
        }

        private static Dictionary<string, int> PlayerStatusCounts = new Dictionary<string, int>();

        private static Dictionary<string, List<DynamicElement>> DynamicElements = new Dictionary<string, List<DynamicElement>>();

        private List<CustomStatus> CustomStatuses = new List<CustomStatus>
        {
            new CustomStatus
            {
                Color = "0.7 0.1 0.3 1",
                LeftText = "Online Time",
                DynamicText = (x) => { return x.secondsConnected.ToString() + "s"; },
                Icon = "clock",
                OnCondition = (x) =>
                {
                    return x.metabolism.oxygen.value >= 1f;
                }
            },
            new CustomStatus
            {
                Color = "0.2 0.5 0.6 1",
                LeftText = "Fresh Spawn",
                RightText = "Invulnerable",
                Icon = "shield",
                OnCondition = (x) =>
                {
                    return x.metabolism.oxygen.value >= 1f;
                }
            },
            new CustomStatus
            {
                Color = "0.7 0.6 0.1 1",
                LeftText = "Donated",
                RightText = "You Rock!",
                Icon = "testicon",
                OnCondition = (x) =>
                {
                    return x.metabolism.oxygen.value >= 1f;
                }
            }
        };

        private string GetIcon(string icon)
        {
            return ImageLibrary?.Call<string>("GetImage", $"{icon}");
        }

        private List<CustomStatus> GetCustomStatuses(BasePlayer basePlayer)
        {
            return CustomStatuses.Where(x => x.IsTriggered(basePlayer)).ToList();
        }

        private List<string> GetStatuses(BasePlayer basePlayer)
        {
            var statuses = new List<string>();
            int count = 0;
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
            var priv = basePlayer.GetBuildingPrivilege();
            if (priv != null && priv.IsAuthed(basePlayer))
            {
                statuses.Add($"buildpriv");
                statuses.Add($"upkeep");
            }
            return statuses;
        }

        private void UpdateStatusHUD(BasePlayer basePlayer, List<string> statuses, List<CustomStatus> customs)
        {
            string parent = $"status";
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
            //var customs = GetCustomStatuses(basePlayer);
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
                container.Add(new CuiElement
                {
                    Parent = id,
                    Components =
                    {
                        new CuiImageComponent
                        {
                            Color = "0 0 0 0.8",
                            Png = GetIcon(custom.Icon)
                        },
                        new CuiRectTransformComponent
                        {
                            AnchorMin = "0 0.5",
                            AnchorMax = "0 0.5",
                            OffsetMin = $"{imgP} {-imgS/2}",
                            OffsetMax = $"{imgP+imgS} {imgS/2}"
                        }
                    }
                });
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
                    dynamics.Add(new DynamicElement { Element = rightText, CustomStatus = custom});
                }
                idx++;
                ey += eh + eg;
            }
            DynamicElements[basePlayer.UserIDString] = dynamics;
            CuiHelper.DestroyUi(basePlayer, parent);
            CuiHelper.AddUi(basePlayer, container);

        }

        void OnPlayerMetabolize(PlayerMetabolism metabolism, BasePlayer basePlayer, float delta)
        {
            if (delta != 0)
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

        [Command("status")]
        private void Status(IPlayer player, string command, string[] args)
        {
            var basePlayer = player.Object as BasePlayer;
            var statuses = GetStatuses(basePlayer);
            player.Reply($"status effects ({statuses.Count}) = {statuses.ToSentence()}");
        }

        [Command("ss")]
        private void ShowStatus(IPlayer player, string command, string[] args)
        {
            var basePlayer = player.Object as BasePlayer;
            //UpdateStatusHUD(basePlayer);
        }
    }
}