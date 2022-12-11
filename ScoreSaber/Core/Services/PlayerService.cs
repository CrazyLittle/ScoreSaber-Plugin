﻿#region

using IPA.Utilities;
using Newtonsoft.Json;
using Oculus.Platform;
using Oculus.Platform.Models;
using ScoreSaber.Core.Data.Internal;
using ScoreSaber.Core.Data.Models;
using ScoreSaber.Core.Data.Wrappers;
using ScoreSaber.Extensions;
using Steamworks;
using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

#endregion

namespace ScoreSaber.Core.Services {
    internal class PlayerService {
        public LocalPlayerInfo LocalPlayerInfo { get; set; }
        public LoginStatus Status { get; set; }
        public event Action<LoginStatus, string> LoginStatusChanged;
        public enum LoginStatus {
            Info = 0,
            Error = 1,
            Success = 2
        }

        public PlayerService() {

            Plugin.Log.Debug("PlayerService Setup!");
        }

        public void ChangeLoginStatus(LoginStatus _loginStatus, string status) {

            Status = _loginStatus;
            LoginStatusChanged?.Invoke(Status, status);
        }

        public void GetLocalPlayerInfo() {

            if (LocalPlayerInfo == null) {
                if (File.Exists(Path.Combine(UnityGame.InstallPath, "Beat Saber_Data", "Plugins", "x86_64", "steam_api64.dll"))) {
                    GetLocalPlayerInfo1().RunTask();
                } else {
                    GetLocalPlayerInfo2();
                }
            }
        }

        private async Task GetLocalPlayerInfo1() {

            ChangeLoginStatus(LoginStatus.Info, PluginConfig.Instance.LoginInfo);

            int attempts = 1;

            while (attempts < 4) {
                var steamInfo = await GetLocalSteamInfo();
                if (steamInfo != null) {
                    bool authenticated = await AuthenticateWithScoreSaber(steamInfo);
                    if (authenticated) {
                        LocalPlayerInfo = steamInfo;
                        ChangeLoginStatus(LoginStatus.Success, PluginConfig.Instance.LoginSuccess);
                        break;
                    } else {
                        ChangeLoginStatus(LoginStatus.Error, PluginConfig.Instance.LoginRetry + $" ({attempts} of 3 tries...)");
                        attempts++;
                        await Task.Delay(4000);
                    }
                } else {
                    Plugin.Log.Error("Steamworks is not initialized!");
                    ChangeLoginStatus(LoginStatus.Error, PluginConfig.Instance.LoginFailedSteam);
                    break;
                }
            }

            if (Status != LoginStatus.Success) {
                ChangeLoginStatus(LoginStatus.Error, PluginConfig.Instance.LoginFailed);
            }
        }

        private void GetLocalPlayerInfo2() {

            ChangeLoginStatus(LoginStatus.Info, PluginConfig.Instance.LoginInfo);

            Users.GetLoggedInUser().OnComplete(delegate (Message<User> loggedInMessage) {
                if (!loggedInMessage.IsError) {
                    Users.GetLoggedInUserFriends().OnComplete(delegate (Message<UserList> friendsMessage) {
                        if (!friendsMessage.IsError) {
                            Users.GetUserProof().OnComplete(delegate (Message<UserProof> userProofMessage) {
                                if (!userProofMessage.IsError) {
                                    Users.GetAccessToken().OnComplete(async delegate (Message<string> authTokenMessage) {
                                        string playerId = loggedInMessage.Data.ID.ToString();
                                        string playerName = loggedInMessage.Data.OculusID;
                                        string friends = playerId + ",";
                                        string nonce = userProofMessage.Data.Value + "," + authTokenMessage.Data;
                                        var oculusInfo = new LocalPlayerInfo(playerId, playerName, friends, "1", nonce);
                                        bool authenticated = await AuthenticateWithScoreSaber(oculusInfo);
                                        if (authenticated) {
                                            LocalPlayerInfo = oculusInfo;
                                            ChangeLoginStatus(LoginStatus.Success, PluginConfig.Instance.LoginSuccess);
                                        } else {
                                            ChangeLoginStatus(LoginStatus.Error, PluginConfig.Instance.LoginFailed);
                                        }
                                    });

                                } else {
                                    ChangeLoginStatus(LoginStatus.Error, PluginConfig.Instance.LoginFailedSteam);
                                }
                            });
                        } else {
                            ChangeLoginStatus(LoginStatus.Error, PluginConfig.Instance.LoginFailedSteam);
                        }
                    });
                } else {
                    ChangeLoginStatus(LoginStatus.Error, PluginConfig.Instance.LoginFailedSteam);
                }
            });
        }

        private async Task<LocalPlayerInfo> GetLocalSteamInfo() {

            await TaskEx.WaitUntil(() => SteamManager.Initialized);

            string authToken = (await new SteamPlatformUserModel().GetUserAuthToken()).token;

            var steamInfo = await Task.Run(() => {
                var steamID = SteamUser.GetSteamID();
                string playerId = steamID.m_SteamID.ToString();
                string playerName = SteamFriends.GetPersonaName();
                string friends = playerId + ",";
                for (int i = 0; i < SteamFriends.GetFriendCount(EFriendFlags.k_EFriendFlagAll); i++) {
                    var friendSteamId = SteamFriends.GetFriendByIndex(i, EFriendFlags.k_EFriendFlagImmediate);
                    if (friendSteamId.m_SteamID.ToString() != "0") {
                        friends = friends + friendSteamId.m_SteamID.ToString() + ",";
                    }
                }
                friends = friends.Remove(friends.Length - 1);
                return new LocalPlayerInfo(playerId, playerName, friends, "0", authToken);
            });


            return steamInfo;
        }

        private async Task<bool> AuthenticateWithScoreSaber(LocalPlayerInfo playerInfo) {


            if (Plugin.HttpInstance.PersistentRequestHeaders.ContainsKey("Cookies")) {
                Plugin.HttpInstance.PersistentRequestHeaders.Remove("Cookies");
            }

            var form = new WWWForm();
            form.AddField("at", playerInfo.AuthType);
            form.AddField("playerId", playerInfo.PlayerId);
            form.AddField("nonce", playerInfo.PlayerNonce);
            form.AddField("friends", playerInfo.PlayerFriends);
            form.AddField("name", playerInfo.PlayerName);

            try {
                string response = await Plugin.HttpInstance.PostAsync("/game/auth", form);
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(response);
                playerInfo.PlayerKey = authResponse.PlayerKey;
                playerInfo.ServerKey = authResponse.ServerKey;

                Plugin.HttpInstance.PersistentRequestHeaders.Add("Cookies", $"connect.sid={playerInfo.ServerKey}");
                return true;
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed user authentication: {ex.Message}");
                return false;
            }
        }

        public async Task<PlayerInfo> GetPlayerInfo(string playerId, bool full) {

            string url = $"/player/{playerId}";

            if (full) {
                url += "/full";
            } else {
                url += "/basic";
            }

            var response = await Plugin.HttpInstance.GetAsync(url);
            var playerStats = JsonConvert.DeserializeObject<PlayerInfo>(response);
            return playerStats;
        }

        public async Task<byte[]> GetReplayData(IDifficultyBeatmap level, int leaderboardId, ScoreMap scoreMap) {

            if (scoreMap.HasLocalReplay) {
                string replayPath = GetReplayPath(scoreMap.Parent.SongHash, level.difficulty.SerializedName(), level.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName, scoreMap.Score.LeaderboardPlayerInfo.Id, level.level.songName);
                if (replayPath != null) {
                    return File.ReadAllBytes(replayPath);
                }
            }

            byte[] response = await Plugin.HttpInstance.DownloadAsync($"/game/telemetry/downloadReplay?playerId={scoreMap.Score.LeaderboardPlayerInfo.Id}&leaderboardId={leaderboardId}");

            if (response != null) {
                return response;
            } else {
                throw new Exception("Failed to download replay");
            }
        }

        private string GetReplayPath(string levelId, string difficultyName, string characteristic, string playerId, string songName) {

            songName = songName.ReplaceInvalidChars().Truncate(155);

            string path = $@"{Settings.ReplayPath}\{playerId}-{songName}-{difficultyName}-{characteristic}-{levelId}.dat";
            if (File.Exists(path)) {
                return path;
            }

            string legacyPath = $@"{Settings.ReplayPath}\{playerId}-{songName}-{levelId}.dat";
            if (File.Exists(legacyPath)) {
                return legacyPath;
            }

            return null;
        }

    }
}