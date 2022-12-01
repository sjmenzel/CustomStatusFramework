using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Text;

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
