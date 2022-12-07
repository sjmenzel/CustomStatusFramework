using System;
using System.Collections.Generic;
using System.Text;

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
