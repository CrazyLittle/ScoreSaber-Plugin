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
        public virtual string ScoreNotSet { get; set; } = "You haven't set a score on this leaderboard";
        public virtual string ScoreBoardEmpty { get; set; } = "No scores on this leaderboard, be the first!";
        public virtual string ScorePageEmpty { get; set; } = "No scores on this page";

        // Text related to score upload attempts
        public virtual string UploadV3Unsupported { get; set; } = "OPENSOURCE XX New note type not supported, not uploading XX";
        public virtual string UploadScoreTooLow { get; set; } = "OPENSOURCE XX Didn't beat score, not uploading. XX";
        public virtual string UploadBanned { get; set; } = "OPENSOURCE XX Failed to upload (banned) XX";
        public virtual string Uploading { get; set; } = "OPENSOURCE XX Uploading score... XX";
        public virtual string UploadSuccess { get; set; } = "OPENSOURCE XX Score uploaded! XX";
        public virtual string UploadError { get; set; } = "OPENSOURCE XX Failed to upload score. XX";
        public virtual string UploadRetrying { get; set; } = "OPENSOURCE XX Failed, attempting again";

        // Text related to logins
        public virtual string LoginInfo { get; set; } = "OPENSOURCE XX Signing into ScoreSaber... XX";
        public virtual string LoginSuccess { get; set; } = "OPENSOURCE XX Successfully signed into ScoreSaber! XX";
        public virtual string LoginRetry { get; set; } = "Failed, attempting again";
        public virtual string LoginFailedSteam { get; set; } = "Failed to authenticate! Error getting steam info";
        public virtual string LoginFailed { get; set; } = "Failed to authenticate with ScoreSaber! Please restart your game";
        public virtual string LoginFailedOculus { get; set; } = "Failed to authenticate! Error getting oculus info";


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
