using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Newtonsoft.Json;

namespace mailsystem.src.Handlers
{
    public class MailHandler : Handler
    {
        private static ICoreServerAPI _api;

        public override void Initialize(ICoreServerAPI api)
        {
            _api = api;
        }

        public static TextCommandResult Send(TextCommandCallingArgs args)
        {

            string receiverName = args[0].ToString();
            Mail workMail = MailUtil.CreateMail(args.Caller.Player.PlayerName, receiverName, args.LastArg.ToString());
            List<Mail> mails;

            string workingData = MailUtil.GetMailDataForPlayer(receiverName);

            if (workingData == null)
            {
                mails = new List<Mail>
                {
                    workMail
                };

                workingData = JsonConvert.SerializeObject(mails);

                if(!MailUtil.SetMailDataForPlayer(receiverName, workingData)) return TextCommandResult.Success(Lang.Get("mailsystem:player-not-existing"));

                MailUtil.NotifyPlayerIfOnline(receiverName);

                return TextCommandResult.Success(Lang.Get("mailsystem:sent"));
            }

            mails = JsonConvert.DeserializeObject<List<Mail>>(MailUtil.GetMailDataForPlayer(receiverName));


            if (mails.Count >= Mailsystem.Config.MaximumMailsPerPlayer) return TextCommandResult.Error(Lang.Get("mailsystem:maximum-mails-inbox"));

            mails.Add(workMail);

            if(!MailUtil.SetMailDataForPlayer(receiverName, JsonConvert.SerializeObject(mails))) return TextCommandResult.Success(Lang.Get("mailsystem:player-not-existing"));

            MailUtil.NotifyPlayerIfOnline(receiverName);

            return TextCommandResult.Success(Lang.Get("mailsystem:sent"));

        }

        public static TextCommandResult GetMails(TextCommandCallingArgs args)
        {
            string workingData = MailUtil.GetMailDataForPlayer(args.Caller.Player.PlayerName);

            if (workingData == null) { return TextCommandResult.Error(Lang.Get("mailsystem:no-mails-found")); }

            List<Mail> mails = JsonConvert.DeserializeObject<List<Mail>>(workingData);

            if (mails.Count <= 0) return TextCommandResult.Success(Lang.Get("mailsystem:you-have-no-mails"));

            string output = "";

            for (int i = 0; i < mails.Count; i++)
            {
                // Use string.Format with the Lang.Get string but provide ALL required arguments
                output += string.Format(Lang.Get("mailsystem:show-data"),
                    i,                    // {0}: Index
                    mails[i].Sender,      // {1}: Sender
                    mails[i].IsRead,      // {2}: Read status
                    mails[i].SentDate     // {3}: Sent date
                );
                output += "\n";
            }

            return TextCommandResult.Success(output);
        }

        public static TextCommandResult Read(TextCommandCallingArgs args)
        {
            string workingData = MailUtil.GetMailDataForPlayer(args.Caller.Player.PlayerName);

            if (workingData == null) { return TextCommandResult.Error(Lang.Get("mailsystem:no-mails-found")); }

            List<Mail> mails = JsonConvert.DeserializeObject<List<Mail>>(workingData);

            try
            {
                int index = int.Parse(args[0].ToString());

                // Use string.Format with the Lang.Get string but provide ALL required arguments
                string mailFrom = mails[index].Sender + "\n"; // Note the newline added to match the format
                string mailDate = mails[index].SentDate + "\n"; // Note the newline added to match the format
                string mailMessage = mails[index].Message;

                string output = string.Format(Lang.Get("mailsystem:show-mail"), mailFrom, mailDate, mailMessage);

                mails[index].IsRead = true;

                MailUtil.SetMailDataForPlayer(args.Caller.Player.PlayerName, JsonConvert.SerializeObject(mails));

                return TextCommandResult.Success(output);
            }
            catch (Exception e) { return TextCommandResult.Error(Lang.Get("mailsystem:not-found")); }
        }

        public static TextCommandResult Delete(TextCommandCallingArgs args)
        {
            string workingData = MailUtil.GetMailDataForPlayer(args.Caller.Player.PlayerName);

            if (workingData == null) { return TextCommandResult.Error(Lang.Get("mailsystem:no-mails-found")); }

            List<Mail> mails = JsonConvert.DeserializeObject<List<Mail>>(workingData);

            try
            {
                int index = int.Parse(args[0].ToString());

                mails.RemoveAt(index);

                MailUtil.SetMailDataForPlayer(args.Caller.Player.PlayerName, JsonConvert.SerializeObject(mails));

                return TextCommandResult.Success(Lang.Get("mailsystem:deleted"));
            }
            catch (Exception e) { return TextCommandResult.Error(Lang.Get("mailsystem:not-found")); }

        }

        public static TextCommandResult SendMassmail(TextCommandCallingArgs args)
        {
            _api.SendMessage(args.Caller.Player, GlobalConstants.GeneralChatGroup, Lang.Get("mailsystem:massmail-sending"), EnumChatType.Notification);

            MailUtil.SendMailToAllPlayers(args[0].ToString());

            return TextCommandResult.Success(Lang.Get("mailsystem:massmail-done"));
        }
    }
}
