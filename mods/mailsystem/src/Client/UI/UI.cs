//using Vintagestory.API.Client;
//using Vintagestory.API.Common;

//namespace mailsystem.src.UI ------POSTPONED-------
//{
//    public class GuiDialogSelectAction : GuiDialog
//    {
//        public override string ToggleKeyCombinationCode => "annoyingtextgui";

//        ElementBounds dialogBounds;
//        ElementBounds textBounds;
//        ElementBounds bgBounds;

//        public class SelectTextSystem : ModSystem
//        {
//            ICoreClientAPI capi;
//            GuiDialog dialog;

//            public override bool ShouldLoad(EnumAppSide forSide)
//            {
//                return forSide == EnumAppSide.Client;
//            }

//            public override void StartClientSide(ICoreClientAPI api)
//            {
//                base.StartClientSide(api);

//                dialog = new GuiDialogSelectAction(api);

//                capi = api;
//                capi.Input.RegisterHotKey("mailui", "Opens the mail UI", GlKeys.U, HotkeyType.GUIOrOtherControls);
//                capi.Input.SetHotKeyHandler("mailui", ToggleGui);
//            }

//            private bool ToggleGui(KeyCombination comb)
//            {
//                if (dialog.IsOpened()) dialog.TryClose();
//                else dialog.TryOpen();

//                return true;
//            }
//        }



//        public GuiDialogSelectAction(ICoreClientAPI capi) : base(capi)
//        {
//            SetupDialog();
//        }
//        private void SetupDialog()
//        {
//            // Auto-sized dialog at the center of the screen
//            dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

//            // Just a simple 300x300 pixel box
//            textBounds = ElementBounds.Fixed(0, 40, 300, 20);

//            // Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
//            bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
//            bgBounds.BothSizing = ElementSizing.FitToChildren;
//            bgBounds.WithChildren(textBounds);

//            bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
//            bgBounds.BothSizing = ElementSizing.FitToChildren;
//            bgBounds.WithChildren(textBounds);

//            ElementBounds sendBtnBounds = ElementBounds.Percentual(EnumDialogArea.LeftBottom, 0.5d, 0.7d);
//            ElementBounds inboxBtnBounds = ElementBounds.Percentual(EnumDialogArea.RightBottom, 0.5d, 0.7d);

//            // Lastly, create the dialog
//            ClearComposers();

//            Composers["SelectDialog"] =
//                capi.Gui.CreateCompo("mailDialog", dialogBounds)
//                .AddShadedDialogBG(bgBounds)
//                .AddDialogTitleBar("Mail interface", OnTitleBarCloseClicked)
//                .AddTextToggleButtons(new string[] { "Send", "Inbox" }, CairoFont.ButtonText(), OnBtnClick, new ElementBounds[] { sendBtnBounds, inboxBtnBounds })
//                .Compose()
//                ;
//        }

//        private void OnBtnClick(int id)
//        {

//        }

//        private void ShowSendDialog()
//        { 
        
//        }

//        private void ShowInboxDialog()
//        {

//        }
//        private void OnTitleBarCloseClicked()
//        {
//            TryClose();
//        }


//    }



//}
