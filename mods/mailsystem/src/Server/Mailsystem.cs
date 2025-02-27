using mailsystem.src.Handlers;
using mailsystem.src.Server;
using mailsystem.src.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace mailsystem.src
{
    public class Mailsystem : ModSystem
    {
        public static MailConfig Config;

        ICoreServerAPI _api;
        NetworkMailSystem _networkSystem;

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            _api = api;

            _api.Event.PlayerNowPlaying += PlayerNowPlaying;

            CommandArgumentParsers parsers = _api.ChatCommands.Parsers;

            _api.ChatCommands.Create("mail")
                .RequiresPrivilege(Privilege.chat)
                .BeginSubCommand("send")
                    .WithDescription(Lang.Get("mailsystem:snd-to-player"))
                    .WithAdditionalInformation(Lang.Get("mailsystem:snd-to-player-additional"))
                    .HandleWith(MailHandler.Send)
                    .WithArgs(parsers.Word(Lang.Get("mailsystem:playername")), parsers.All("text"))
                .EndSubCommand()
                .BeginSubCommand("list")
                    .WithDescription(Lang.Get("mailsystem:list-all"))
                    .HandleWith(MailHandler.GetMails)
                .EndSubCommand()
                .BeginSubCommand("read")
                    .WithDescription(Lang.Get("mailsystem:read"))
                    .WithAdditionalInformation(Lang.Get("mailsystem:read-additional"))
                    .WithArgs(parsers.Int("id"))
                    .HandleWith(MailHandler.Read)
                .EndSubCommand()
                .BeginSubCommand("delete")
                    .WithDescription(Lang.Get("mailsystem:delete"))
                    .WithAdditionalInformation(Lang.Get("mailsystem:delete-additional"))
                    .WithArgs(parsers.Int("id"))
                    .HandleWith(MailHandler.Delete)
                .EndSubCommand()
                .BeginSubCommand("gui")
                    .WithDescription(Lang.Get("mailsystem:open-gui"))
                    .HandleWith(OpenMailGui)
                .EndSubCommand();

            _api.ChatCommands.Create("massmail")
                .RequiresPrivilege(Privilege.announce)
                .WithAdditionalInformation(Lang.Get("mailsystem:massmail-cmd"))
                .WithArgs(parsers.All("text"))
                .HandleWith(MailHandler.SendMassmail);

            // Get the network system
            _networkSystem = _api.ModLoader.GetModSystem<NetworkMailSystem>();

            Config = _api.LoadModConfig<MailConfig>("MailConfig.json");
            if (Config == null)
            {
                _api.StoreModConfig(new MailConfig(10), "MailConfig.json");
                Config = new MailConfig(10);
            }

            InitSystems();

            base.StartServerSide(api);
        }

        public void PlayerNowPlaying(IServerPlayer byPlayer)
        {
            try
            {
                List<Mail> mailList = JsonConvert.DeserializeObject<List<Mail>>(MailUtil.GetMailDataForPlayer(byPlayer.PlayerName));
                bool hasUnread = false;

                for (int i = 0; i < mailList.Count; i++)
                {
                    if (!mailList[i].IsRead)
                    {
                        hasUnread = true;
                        break;
                    }
                }
                if (hasUnread)
                {
                    _api.SendMessage(
                        byPlayer,
                        GlobalConstants.GeneralChatGroup,
                        $"<strong>" +
                        $"{Lang.Get("mailsystem:notify-onjoin")} " +
                        $"</strong>",
                        EnumChatType.Notification);

                    _api.World.PlaySoundFor(new AssetLocation("game", "sounds/effect/receptionbell"), byPlayer, false, 32f, 0.5f);
                }
            }
            catch (Exception e) { _api.Logger.Log(EnumLogType.Notification, string.Format(Lang.Get("mailsystem:log-nomail"), byPlayer.PlayerName)); }
        }

        private TextCommandResult OpenMailGui(TextCommandCallingArgs args)
        {
            // Use network system to tell client to open GUI
            if (_networkSystem != null && args.Caller.Player is IServerPlayer player)
            {
                _networkSystem.ServerRequestGuiOpen(player);
                return TextCommandResult.Success();
            }

            return TextCommandResult.Error("Could not open mail GUI");
        }

        private void InitSystems()
        {
            new MailHandler().Initialize(_api);
            new MailUtil().Initialize(_api);
        }
    }
}