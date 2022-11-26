#if RELEASE

#region

using IPA.Utilities;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace ScoreSaber.Core.AC {
    internal class AntiCheat {
        internal string AC() {

            //string localPath = Path.Combine(UnityGame.InstallPath, "Plugins", "Other_Stuff", "ScoreSaber.dll");
            string dataPath = Path.Combine(UnityGame.InstallPath, "Beat Saber_Data");
            string oculusPlatform = Path.Combine(dataPath, "Managed", "Oculus.Platform.dll");
            string steamWorks = Path.Combine(dataPath, "Managed", "Steamworks.NET.dll");

            using (var md5 = MD5.Create()) {

                string localHash = string.Empty;
                string oculusHash = string.Empty;
                string steamHash = string.Empty;
                string mainHash = string.Empty;

                //using (var stream = File.OpenRead(localPath)) {
                //var hasher = md5.ComputeHash(stream);
                //localHash = BitConverter.ToString(hasher).Replace("-", "").ToLowerInvariant();
                //Plugin.Log.Debug("localHash: " + localHash);
                //localHash = "4cc522a09516405225fcda489196fea2".Replace("-", "").ToLowerInvariant();
                //}

                localHash = "759240f5486342c585445776c6004dcd";

                using (var stream = File.OpenRead(oculusPlatform)) {
                    var hasher = md5.ComputeHash(stream);
                    oculusHash = BitConverter.ToString(hasher).Replace("-", "").ToLowerInvariant();
                }

                using (var stream = File.OpenRead(steamWorks)) {
                    var hasher = md5.ComputeHash(stream);
                    steamHash = BitConverter.ToString(hasher).Replace("-", "").ToLowerInvariant();
                }

                string hash = BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(
                    $"{localHash}{oculusHash}{steamHash}"))).Replace("-", "").ToLowerInvariant();

                return hash;
            }
        }
    }
}

#endif