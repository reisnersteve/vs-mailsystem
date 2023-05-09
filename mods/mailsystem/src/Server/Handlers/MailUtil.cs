using mailsystem.src.Handlers;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace mailsystem.src
{
    public class MailUtil : Handler
    {
        private static ICoreServerAPI _api;
        public override void Initialize(ICoreServerAPI api)
        {
            _api = api;
        }
        public static Mail CreateMail(string sender, string receiver, string message)
        {
            return new Mail(sender, receiver, message, DateTime.Now);
        }

        public static string GetMailDataForPlayer(string player)
        {
            IServerPlayerData playerData = _api.PlayerData.GetPlayerDataByLastKnownName(player);
            if (playerData == null) return null;
            return playerData.CustomPlayerData.Get("maildata");
        }

        public static bool SetMailDataForPlayer(string player, string jsonData)
        {
            IServerPlayerData playerData = _api.PlayerData.GetPlayerDataByLastKnownName(player);
            if (playerData == null) return false;
            playerData.CustomPlayerData.Remove("maildata");
            playerData.CustomPlayerData.Add("maildata", jsonData);
            return true;
        }

        public static IPlayer GetPlayerByName(string name)
        {
            try
            {
                IPlayer[] players = _api.World.AllPlayers;
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].PlayerName.ToUpper() == name.ToUpper()) return players[i];
                }
                return null;
            }
            catch { return null; }

        }

        public static void NotifyPlayerIfOnline(string player) 
        {
            try
            {
                IPlayer receiver = GetPlayerByName(player);
                if (receiver != null) _api.SendMessage(receiver, GlobalConstants.GeneralChatGroup, $"<strong>{Lang.Get("mailsystem:new-mail-notify")}</strong>", EnumChatType.Notification);
            }
            catch { _api.Logger.Log(EnumLogType.Notification, "Tried to notify player, player not online."); }

        }
    }
}
