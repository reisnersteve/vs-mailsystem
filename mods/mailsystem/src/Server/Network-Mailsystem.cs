using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace mailsystem.src.Network
{
    public class NetworkMailSystem : ModSystem
    {
        private const string CHANNEL_NAME = "mailsystem:network";

        // Network message types
        private const string MSG_REQUEST_MAILS = "requestmails";
        private const string MSG_SEND_MAILS = "sendmails";
        private const string MSG_GUI_OPEN = "guiopen";

        private ICoreServerAPI sapi;
        private ICoreClientAPI capi;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return true; // Load on both client and server
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;

            // Register server-side network channel
            sapi.Network.RegisterChannel(CHANNEL_NAME)
                .RegisterMessageType<RequestMailsPacket>()
                .RegisterMessageType<SendMailsPacket>()
                .RegisterMessageType<OpenGuiPacket>()
                .SetMessageHandler<RequestMailsPacket>(OnClientRequestMails);
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            capi = api;

            // Register client-side network channel
            capi.Network.RegisterChannel(CHANNEL_NAME)
                .RegisterMessageType<RequestMailsPacket>()
                .RegisterMessageType<SendMailsPacket>()
                .RegisterMessageType<OpenGuiPacket>()
                .SetMessageHandler<SendMailsPacket>(OnServerSendMails)
                .SetMessageHandler<OpenGuiPacket>(OnServerRequestGuiOpen);
        }

        // Client requests mails from server
        public void ClientRequestMails()
        {
            if (capi == null) return;

            var packet = new RequestMailsPacket();
            capi.Network.GetChannel(CHANNEL_NAME).SendPacket(packet);
        }

        // Server sends mails to client
        public void ServerSendMails(IServerPlayer player, List<Mail> mails)
        {
            if (sapi == null) return;

            var packet = new SendMailsPacket
            {
                Mails = mails
            };

            sapi.Network.GetChannel(CHANNEL_NAME).SendPacket(packet, player);
        }

        // Server requests client to open GUI
        public void ServerRequestGuiOpen(IServerPlayer player)
        {
            if (sapi == null) return;

            var packet = new OpenGuiPacket();
            sapi.Network.GetChannel(CHANNEL_NAME).SendPacket(packet, player);
        }

        // Handler for client mail request
        private void OnClientRequestMails(IServerPlayer fromPlayer, RequestMailsPacket packet)
        {
            if (sapi == null) return;

            string workingData = MailUtil.GetMailDataForPlayer(fromPlayer.PlayerName);
            List<Mail> mails = new List<Mail>();

            if (workingData != null)
            {
                try
                {
                    mails = JsonConvert.DeserializeObject<List<Mail>>(workingData);
                }
                catch (Exception ex)
                {
                    sapi.Logger.Error("Error deserializing mail data: " + ex.Message);
                }
            }

            ServerSendMails(fromPlayer, mails);
        }

        // Handler for server sending mails
        private void OnServerSendMails(SendMailsPacket packet)
        {
            if (capi == null) return;

            // Update the mail dialog with received mails
            var mailGui = capi.ModLoader.GetModSystem<UI.MailGuiSystem>();
            if (mailGui != null)
            {
                mailGui.UpdateInboxMails(packet.Mails);
            }
        }

        // Handler for server requesting GUI open
        private void OnServerRequestGuiOpen(OpenGuiPacket packet)
        {
            if (capi == null) return;

            // Open the mail GUI
            var mailGui = capi.ModLoader.GetModSystem<UI.MailGuiSystem>();
            if (mailGui != null)
            {
                mailGui.OpenMailGui();
            }
        }

        // Packet classes for network messages

        public class RequestMailsPacket
        {
            // Empty packet - just a signal to request mails
        }

        public class SendMailsPacket
        {
            public List<Mail> Mails { get; set; }
        }

        public class OpenGuiPacket
        {
            // Empty packet - just a signal to open GUI
        }
    }
}