using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace ScoreSaber {
    public class PluginConfig {
        public static PluginConfig Instance { get; set; }

        public virtual string GlobalRank { get; set; } = "Rank Stinky";
        // call PluginConfig.Instance.GlobalRank to get this string

        public virtual string RankedStatus { get; set; } = "Map How Stinky";


        public virtual void Changed() {
            // this is called whenever one of the virtual properties is changed
            // can be called to signal that the content has been changed
        }

        public virtual void OnReload() {
            // this is called whenever the config file is reloaded from disk
            // use it to tell all of your systems that something has changed

            // this is called off of the main thread, and is not safe to interact
            //   with Unity in
        }
    }
}
