﻿using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.IoC;
using Robust.Shared.Localization;

namespace Content.Client.Cargo.UI
{
    public class GalacticBankSelectionMenu : SS14Window
    {
        private readonly ItemList _accounts;
        private int _accountCount;
        private string[] _accountNames = new string[] { };
        private int[] _accountIds = new int[] { };
        private int _selectedAccountId = -1;

        public GalacticBankSelectionMenu(CargoConsoleBoundUserInterface owner)
        {
            MinSize = SetSize = (300, 300);
            IoCManager.InjectDependencies(this);

            Title = Loc.GetString("Galactic Bank Selection");

            _accounts = new ItemList() { SelectMode = ItemList.ItemListSelectMode.Single };

            Contents.AddChild(_accounts);
        }

        public void Populate(int accountCount, string[] accountNames, int[] accountIds, int selectedAccountId)
        {
            _accountCount = accountCount;
            _accountNames = accountNames;
            _accountIds = accountIds;
            _selectedAccountId = selectedAccountId;

            _accounts.Clear();
            for (var i = 0; i < _accountCount; i++)
            {
                var id = _accountIds[i];
                _accounts.AddItem($"ID: {id} || {_accountNames[i]}");
                if (id == _selectedAccountId)
                    _accounts[id].Selected = true;
            }
        }
    }
}
