using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Util;
using Newtonsoft.Json;

namespace mailsystem.src.UI
{
    public class MailGuiDialog : GuiDialog
    {
        public override string ToggleKeyCombinationCode => "mailsystemgui";

        private ElementBounds dialogBounds;
        private ElementBounds tabButtonsBounds;
        private ElementBounds contentBounds;

        // Send mail elements
        private ElementBounds receiverInputBounds;
        private ElementBounds messageInputBounds;
        private ElementBounds sendButtonBounds;

        // Inbox elements
        private ElementBounds mailListBounds;
        private ElementBounds mailContentBounds;
        private ElementBounds deleteButtonBounds;

        private EnumDialogSection currentSection = EnumDialogSection.Send;
        private int selectedMailId = -1;
        private List<Mail> inboxMails = new List<Mail>();

        private readonly ICoreClientAPI capi;

        private enum EnumDialogSection
        {
            Send,
            Inbox
        }

        // Method to update inbox content
        public void UpdateInboxContent(List<Mail> mails)
        {
            this.inboxMails = mails;

            // If inbox section is active, refresh it
            if (currentSection == EnumDialogSection.Inbox && IsOpened())
            {
                ClearComposers();
                LoadComposers();
            }
        }

        public MailGuiDialog(ICoreClientAPI capi) : base(capi)
        {
            this.capi = capi;
            SetupDialog();
        }

        private void SetupDialog()
        {
            // Main dialog bounds - centered and sized appropriately
            dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterMiddle, 0, 0, 600, 400);

            // Tab buttons at the top
            tabButtonsBounds = ElementBounds.Fixed(0, 30, 600, 40);

            // Content area below tabs
            contentBounds = ElementBounds.Fixed(0, 70, 600, 330);

            // Send mail elements
            receiverInputBounds = ElementBounds.Fixed(20, 20, 560, 30).FixedUnder(tabButtonsBounds, 20);
            messageInputBounds = ElementBounds.Fixed(20, 60, 560, 180).FixedUnder(receiverInputBounds, 10);
            sendButtonBounds = ElementBounds.Fixed(460, 250, 120, 30).FixedUnder(messageInputBounds, 10);

            // Inbox elements
            mailListBounds = ElementBounds.Fixed(20, 20, 220, 270).FixedUnder(tabButtonsBounds, 20);
            mailContentBounds = ElementBounds.Fixed(260, 20, 320, 230).FixedUnder(tabButtonsBounds, 20);
            deleteButtonBounds = ElementBounds.Fixed(460, 260, 120, 30).FixedUnder(mailContentBounds, 10);

            // Setup GUI composers
            ClearComposers();
            LoadComposers();
        }

        private void LoadComposers()
        {
            // Create main dialog composer
            var mainComposer = capi.Gui
                .CreateCompo("mailSystemGui", dialogBounds)
                .AddShadedDialogBG(ElementBounds.Fill)
                .AddDialogTitleBar(Lang.Get("mailsystem:ui-title"), OnTitleBarCloseClicked);

            // Simple tab implementation with buttons instead of AddHorizontalTabs
            mainComposer.AddButton(Lang.Get("mailsystem:ui-send"), () => {
                currentSection = EnumDialogSection.Send;
                ClearComposers();
                LoadComposers();
                return true;
            }, ElementBounds.Fixed(0, 30, 300, 40))
            .AddButton(Lang.Get("mailsystem:ui-inbox"), () => {
                currentSection = EnumDialogSection.Inbox;
                ClearComposers();
                LoadComposers();
                return true;
            }, ElementBounds.Fixed(300, 30, 300, 40));

            // Add content based on current section
            if (currentSection == EnumDialogSection.Send)
            {
                LoadSendMailSection(mainComposer);
            }
            else
            {
                LoadInboxSection(mainComposer);
            }

            // Compose the dialog
            Composers["mailDialog"] = mainComposer.Compose();
        }

        private void LoadSendMailSection(GuiComposer composer)
        {
            // Add receiver label and input
            ElementBounds receiverLabelBounds = receiverInputBounds.FlatCopy().WithFixedHeight(20);
            composer.AddStaticText(Lang.Get("mailsystem:ui-receiver"), CairoFont.WhiteSmallText(), receiverLabelBounds);

            composer.AddTextInput(
                receiverInputBounds.FlatCopy().WithFixedOffset(0, 20),
                OnReceiverChanged,
                CairoFont.TextInput(),
                "receiverInput"  // Add ID for the text input
            );

            // Add message label and text area
            ElementBounds messageLabelBounds = messageInputBounds.FlatCopy().WithFixedHeight(20);
            composer.AddStaticText(Lang.Get("mailsystem:ui-message"), CairoFont.WhiteSmallText(), messageLabelBounds);

            // Use TextArea which should exist in this version
            composer.AddTextArea(
                messageInputBounds.FlatCopy().WithFixedOffset(0, 20),
                OnMessageChanged,
                CairoFont.TextInput(),
                "messageInput"
            );

            // Add send button
            composer.AddSmallButton(
                Lang.Get("mailsystem:ui-send-btn"),
                OnSendButtonClicked,
                sendButtonBounds
            );
        }

        private void LoadInboxSection(GuiComposer composer)
        {
            // Fetch inbox mails when opening the inbox tab
            FetchInboxMails();

            // Create mail list items
            string[] mailListItems = new string[inboxMails.Count];
            for (int i = 0; i < inboxMails.Count; i++)
            {
                Mail mail = inboxMails[i];
                string unread = mail.IsRead ? "" : "* ";

                // Properly format using the show-data format string
                string display = string.Format(Lang.Get("mailsystem:show-data"),
                    i,                // Index {0}
                    mail.Sender,      // Sender {1}
                    mail.IsRead,      // Read status {2}
                    mail.SentDate     // Date {3}
                );

                mailListItems[i] = unread + display;
            }

            // Add inbox title
            ElementBounds inboxTitleBounds = mailListBounds.FlatCopy().WithFixedHeight(20);
            composer.AddStaticText(Lang.Get("mailsystem:ui-inbox-title"), CairoFont.WhiteSmallText(), inboxTitleBounds);

            // Create a simple list using buttons since AddSelectionList and AddItemSelectionList don't exist
            if (mailListItems.Length > 0)
            {
                for (int i = 0; i < Math.Min(mailListItems.Length, 8); i++)
                {
                    int index = i; // Capture for lambda
                    ElementBounds itemBounds = ElementBounds.Fixed(20, 100 + i * 30, 220, 25);
                    composer.AddSmallButton(mailListItems[i], () => {
                        SelectMailItem(mailListItems[index], index);
                        return true;
                    }, itemBounds);
                }
            }
            else
            {
                // Show no mail message
                ElementBounds noMailBounds = ElementBounds.Fixed(20, 120, 220, 30);
                composer.AddStaticText(Lang.Get("mailsystem:no-mails-found"), CairoFont.WhiteDetailText(), noMailBounds);
            }

            // Add mail content title
            ElementBounds contentTitleBounds = mailContentBounds.FlatCopy().WithFixedHeight(20);
            composer.AddStaticText(Lang.Get("mailsystem:ui-mail-content"), CairoFont.WhiteSmallText(), contentTitleBounds);

            // Add mail content text area
            var textArea = composer.AddTextArea(
                mailContentBounds.FlatCopy().WithFixedOffset(0, 20),
                null,
                CairoFont.TextInput(),
                "mailContent"
            );

            // Make content read-only (by disabling the text area)
            textArea.Enabled = false;

            // Add delete button
            composer.AddSmallButton(
                Lang.Get("mailsystem:ui-delete-btn"),
                OnDeleteButtonClicked,
                deleteButtonBounds
            );

            // Set initial mail content
            if (selectedMailId >= 0 && selectedMailId < inboxMails.Count)
            {
                UpdateMailContent();
            }
            else
            {
                composer.GetTextArea("mailContent").SetValue("");
            }
        }

        private void OnReceiverChanged(string receiver)
        {
            // Store the receiver name if needed
        }

        private void OnMessageChanged(string message)
        {
            // Store the message if needed
        }

        // Button handler with bool return type as expected
        private bool OnSendButtonClicked()
        {
            string receiver = Composers["mailDialog"].GetTextInput("receiverInput").GetText();
            string message = Composers["mailDialog"].GetTextArea("messageInput").GetText();

            if (string.IsNullOrWhiteSpace(receiver))
            {
                capi.ShowChatMessage(Lang.Get("mailsystem:ui-error-no-receiver"));
                return true;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                capi.ShowChatMessage(Lang.Get("mailsystem:ui-error-no-message"));
                return true;
            }

            // Send command to server to send the mail
            capi.SendChatMessage($"/mail send {receiver} {message}");

            // Clear inputs
            Composers["mailDialog"].GetTextInput("receiverInput").SetValue("");
            Composers["mailDialog"].GetTextArea("messageInput").SetValue("");

            capi.ShowChatMessage(Lang.Get("mailsystem:ui-mail-sent"));
            return true;
        }

        private void SelectMailItem(string value, int index)
        {
            selectedMailId = index;

            if (selectedMailId >= 0 && selectedMailId < inboxMails.Count)
            {
                // Mark mail as read on server
                capi.SendChatMessage($"/mail read {selectedMailId}");

                // Update the mail as read locally too
                inboxMails[selectedMailId].IsRead = true;

                // Update the mail content
                UpdateMailContent();
            }
        }

        private void UpdateMailContent()
        {
            if (selectedMailId >= 0 && selectedMailId < inboxMails.Count)
            {
                Mail mail = inboxMails[selectedMailId];

                // Use string.Format with the show-mail format string, but properly include all arguments
                string mailFrom = mail.Sender + "\n"; // Add newline to match format
                string mailDate = mail.SentDate.ToString() + "\n"; // Add newline to match format
                string mailMsg = mail.Message;

                string content = string.Format(Lang.Get("mailsystem:show-mail"), mailFrom, mailDate, mailMsg);
                Composers["mailDialog"].GetTextArea("mailContent").SetValue(content);
            }
        }

        // Button handler with bool return type as expected
        private bool OnDeleteButtonClicked()
        {
            if (selectedMailId >= 0 && selectedMailId < inboxMails.Count)
            {
                capi.SendChatMessage($"/mail delete {selectedMailId}");

                // Remove from local list
                inboxMails.RemoveAt(selectedMailId);
                selectedMailId = -1;

                // Refresh the mail list
                ClearComposers();
                LoadComposers();
            }

            return true;
        }

        private void FetchInboxMails()
        {
            // Request mail list from server using the network system
            var networkSystem = capi.ModLoader.GetModSystem<Network.NetworkMailSystem>();
            if (networkSystem != null)
            {
                networkSystem.ClientRequestMails();
            }
        }

        // Title bar handler with void return type as expected
        private void OnTitleBarCloseClicked()
        {
            TryClose();
        }
    }

    public class MailGuiSystem : ModSystem
    {
        private ICoreClientAPI capi;
        private MailGuiDialog mailDialog;
        private bool registered = false;

        // Store mails for the inbox
        private List<Mail> inboxMails = new List<Mail>();

        public override bool ShouldLoad(EnumAppSide forSide)
        {
            return forSide == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);

            capi = api;
            mailDialog = new MailGuiDialog(capi);

            // Register command for opening mail GUI using the ChatCommands subapi
            capi.ChatCommands
                .Create("mailgui")
                .WithDescription("Opens the mail system GUI")
                .HandleWith(args => {
                    OpenMailGui();
                    return TextCommandResult.Success();
                });

            // Register hotkey for convenience
            capi.Input.RegisterHotKey("mailsystemgui", "Opens the mail system GUI", GlKeys.M, HotkeyType.GUIOrOtherControls);
            capi.Input.SetHotKeyHandler("mailsystemgui", OnMailHotkeyPressed);

            // Register GUI
            capi.Event.BlockTexturesLoaded += OnBlockTexturesLoaded;

            registered = true;
        }

        private void OnBlockTexturesLoaded()
        {
            // Register the dialog with the GUI system
            capi.Gui.RegisterDialog(mailDialog);
        }

        public void OpenMailGui()
        {
            if (mailDialog != null)
            {
                if (mailDialog.IsOpened())
                {
                    mailDialog.TryClose();
                }
                else
                {
                    mailDialog.TryOpen();
                }
            }
        }

        // Method to update inbox mails from network system
        public void UpdateInboxMails(List<Mail> mails)
        {
            inboxMails = mails;

            // If mail dialog is open, refresh it
            if (mailDialog != null && mailDialog.IsOpened())
            {
                mailDialog.UpdateInboxContent(inboxMails);
            }
        }

        private bool OnMailHotkeyPressed(KeyCombination comb)
        {
            OpenMailGui();
            return true;
        }

        public override void Dispose()
        {
            if (registered)
            {
                capi.Input.SetHotKeyHandler("mailsystemgui", null);
                registered = false;
            }
        }
    }
}