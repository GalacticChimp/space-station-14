﻿using Content.Shared.Disposal.Components;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Localization;
using static Content.Shared.Disposal.Components.SharedDisposalRouterComponent;

namespace Content.Client.Disposal.UI
{
    /// <summary>
    /// Client-side UI used to control a <see cref="SharedDisposalRouterComponent"/>
    /// </summary>
    public class DisposalRouterWindow : SS14Window
    {
        public readonly LineEdit TagInput;
        public readonly Button Confirm;

        public DisposalRouterWindow()
        {
            MinSize = SetSize = (500, 110);
            Title = Loc.GetString("Disposal Router");

            Contents.AddChild(new VBoxContainer
            {
                Children =
                {
                    new Label {Text = Loc.GetString("Tags:")},
                    new Control {MinSize = (0, 10)},
                    new HBoxContainer
                    {
                        Children =
                        {
                            (TagInput = new LineEdit
                            {
                                HorizontalExpand = true,
                                MinSize = (320, 0),
                                ToolTip = Loc.GetString("A comma separated list of tags"),
                                IsValid = tags => TagRegex.IsMatch(tags)
                            }),
                            new Control {MinSize = (10, 0)},
                            (Confirm = new Button {Text = Loc.GetString("Confirm")})
                        }
                    }
                }
            });
        }


        public void UpdateState(DisposalRouterUserInterfaceState state)
        {
            TagInput.Text = state.Tags;
        }
    }
}
