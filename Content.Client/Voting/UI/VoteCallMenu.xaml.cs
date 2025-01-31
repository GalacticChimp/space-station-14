﻿#nullable enable
using Content.Client.Stylesheets;
using JetBrains.Annotations;
using Robust.Client.AutoGenerated;
using Robust.Client.Console;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Console;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using Robust.Shared.Maths;

namespace Content.Client.Voting.UI
{
    [GenerateTypedNameReferences]
    public partial class VoteCallMenu : BaseWindow
    {
        [Dependency] private readonly IClientConsoleHost _consoleHost = default!;

        public static readonly (string name, string id, (string name, string id)[]? secondaries)[] AvailableVoteTypes =
        {
            ("ui-vote-type-restart", "restart", null),
            ("ui-vote-type-gamemode", "preset", null)
        };

        public VoteCallMenu()
        {
            IoCManager.InjectDependencies(this);
            RobustXamlLoader.Load(this);

            Stylesheet = IoCManager.Resolve<IStylesheetManager>().SheetSpace;
            CloseButton.OnPressed += _ => Close();

            for (var i = 0; i < AvailableVoteTypes.Length; i++)
            {
                var (text, _, _) = AvailableVoteTypes[i];
                VoteTypeButton.AddItem(Loc.GetString(text), i);
            }

            VoteTypeButton.OnItemSelected += VoteTypeSelected;
            VoteSecondButton.OnItemSelected += VoteSecondSelected;
            CreateButton.OnPressed += CreatePressed;
        }

        private void CreatePressed(BaseButton.ButtonEventArgs obj)
        {
            var typeId = VoteTypeButton.SelectedId;
            var (_, typeKey, secondaries) = AvailableVoteTypes[typeId];

            if (secondaries != null)
            {
                var secondaryId = VoteSecondButton.SelectedId;
                var (_, secondKey) = secondaries[secondaryId];

                _consoleHost.LocalShell.RemoteExecuteCommand($"createvote {typeKey} {secondKey}");
            }
            else
            {
                _consoleHost.LocalShell.RemoteExecuteCommand($"createvote {typeKey}");
            }

            Close();
        }

        private static void VoteSecondSelected(OptionButton.ItemSelectedEventArgs obj)
        {
            obj.Button.SelectId(obj.Id);
        }

        private void VoteTypeSelected(OptionButton.ItemSelectedEventArgs obj)
        {
            VoteTypeButton.SelectId(obj.Id);

            var (_, _, options) = AvailableVoteTypes[obj.Id];
            if (options == null)
            {
                VoteSecondButton.Visible = false;
            }
            else
            {
                VoteSecondButton.Visible = true;
                VoteSecondButton.Clear();

                for (var i = 0; i < options.Length; i++)
                {
                    var (text, _) = options[i];
                    VoteSecondButton.AddItem(Loc.GetString(text), i);
                }
            }
        }

        protected override DragMode GetDragModeFor(Vector2 relativeMousePos)
        {
            return DragMode.Move;
        }
    }

    [UsedImplicitly]
    public sealed class VoteMenuCommand : IConsoleCommand
    {
        public string Command => "votemenu";
        public string Description => "Opens the voting menu";
        public string Help => "Usage: votemenu";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            new VoteCallMenu().OpenCentered();
        }
    }
}
