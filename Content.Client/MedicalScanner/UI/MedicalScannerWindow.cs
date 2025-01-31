using System.Text;
using Content.Shared.Damage;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using static Content.Shared.MedicalScanner.SharedMedicalScannerComponent;

namespace Content.Client.MedicalScanner.UI
{
    public class MedicalScannerWindow : SS14Window
    {
        public readonly Button ScanButton;
        private readonly Label _diagnostics;
        public MedicalScannerWindow()
        {
            SetSize = (250, 100);

            Contents.AddChild(new VBoxContainer
            {
                Children =
                {
                    (ScanButton = new Button
                    {
                        Text = Loc.GetString("Scan and Save DNA")
                    }),
                    (_diagnostics = new Label
                    {
                        Text = ""
                    })
                }
            });
        }

        public void Populate(MedicalScannerBoundUserInterfaceState state)
        {
            var text = new StringBuilder();

            if (!state.Entity.HasValue ||
                !state.HasDamage() ||
                !IoCManager.Resolve<IEntityManager>().TryGetEntity(state.Entity.Value, out var entity))
            {
                _diagnostics.Text = Loc.GetString("No patient data.");
                ScanButton.Disabled = true;
                SetSize = (250, 100);
            }
            else
            {
                text.Append($"{entity.Name}{Loc.GetString("'s health:")}\n");

                foreach (var (@class, classAmount) in state.DamageClasses)
                {
                    text.Append($"\n{Loc.GetString("{0}: {1}", @class, classAmount)}");

                    foreach (var type in @class.ToTypes())
                    {
                        if (!state.DamageTypes.TryGetValue(type, out var typeAmount))
                        {
                            continue;
                        }

                        text.Append($"\n- {Loc.GetString("{0}: {1}", type, typeAmount)}");
                    }

                    text.Append("\n");
                }

                _diagnostics.Text = text.ToString();
                ScanButton.Disabled = state.IsScanned;

                SetSize = (250, 575);
            }
        }
    }
}
