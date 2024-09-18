using RoR2;
using System.Collections.Generic;

namespace TPVoting
{
    public static class PluginGlobals
    {
        //Lol is there better way to store/check stages?!?
        public static List<string> IgnoredStages = new List<string>
        {
            "arena", //void
            "moon",
            "moon2",
            "limbo",
            "outro",
            "voidraid"
        };
    }
}
