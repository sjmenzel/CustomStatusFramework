using System;
using System.Collections.Generic;
using System.Text;

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
