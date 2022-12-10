using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]

namespace ScoreSaber {
    public class PluginConfig {
        public static PluginConfig Instance { get; set; }

        // call PluginConfig.Instance.GlobalRanking to get this string (etc)
        public virtual string GlobalRanking { get; set; } = "Global Ranking";
        public virtual string RankedStatus { get; set; } = "Ranked Status";
        public virtual string Loading { get; set; } = "Loading";
        public virtual string ModifiersEnabled { get; set; } = "Ranked (DA = +0.02, GN +0.04)";
        public virtual string ModifiersDisabled { get; set; } = "Ranked (modifiers disabled)";
        public virtual string Unranked { get; set; } = "Unranked";
        public virtual string V3Unsupported { get; set; } = "Maps with new note types currently not supported";
        public virtual string UploadV3Unsupported { get; set; } = "OPENSOURCE XX New note type not supported, not uploading XX";
        public virtual string UploadScoreTooLow { get; set; } = "OPENSOURCE XX Didn't beat score, not uploading. XX";


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
