﻿using System.Linq;
using Content.Shared.Body.Components;
using Content.Shared.Body.Mechanism;
using Content.Shared.Body.Part;
using Content.Shared.Damage.Components;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Localization;
using static Robust.Client.UserInterface.Controls.ItemList;

namespace Content.Client.Body.UI
{
    public sealed class BodyScannerDisplay : SS14Window
    {
        private IEntity? _currentEntity;
        private IBodyPart? _currentBodyPart;

        private IBody? CurrentBody => _currentEntity?.GetComponentOrNull<IBody>();

        public BodyScannerDisplay(BodyScannerBoundUserInterface owner)
        {
            IoCManager.InjectDependencies(this);
            Owner = owner;
            Title = Loc.GetString("Body Scanner");

            var hSplit = new HBoxContainer
            {
                Children =
                {
                    // Left half
                    new ScrollContainer
                    {
                        HorizontalExpand = true,
                        Children =
                        {
                            (BodyPartList = new ItemList())
                        }
                    },
                    // Right half
                    new VBoxContainer
                    {
                        HorizontalExpand = true,
                        Children =
                        {
                            // Top half of the right half
                            new VBoxContainer
                            {
                                VerticalExpand = true,
                                Children =
                                {
                                    (BodyPartLabel = new Label()),
                                    new HBoxContainer
                                    {
                                        Children =
                                        {
                                            new Label
                                            {
                                                Text = "Health: "
                                            },
                                            (BodyPartHealth = new Label())
                                        }
                                    },
                                    new ScrollContainer
                                    {
                                        VerticalExpand = true,
                                        Children =
                                        {
                                            (MechanismList = new ItemList())
                                        }
                                    }
                                }
                            },
                            // Bottom half of the right half
                            (MechanismInfoLabel = new RichTextLabel
                            {
                                VerticalExpand = true
                            })
                        }
                    }
                }
            };

            Contents.AddChild(hSplit);

            BodyPartList.OnItemSelected += BodyPartOnItemSelected;
            MechanismList.OnItemSelected += MechanismOnItemSelected;
            MinSize = SetSize = (800, 600);
        }

        public BodyScannerBoundUserInterface Owner { get; }

        private ItemList BodyPartList { get; }

        private Label BodyPartLabel { get; }

        private Label BodyPartHealth { get; }

        private ItemList MechanismList { get; }

        private RichTextLabel MechanismInfoLabel { get; }

        public void UpdateDisplay(IEntity entity)
        {
            _currentEntity = entity;
            BodyPartList.Clear();

            var body = CurrentBody;

            if (body == null)
            {
                return;
            }

            foreach (var (part, _) in body.Parts)
            {
                BodyPartList.AddItem(Loc.GetString(part.Name));
            }
        }

        public void BodyPartOnItemSelected(ItemListSelectedEventArgs args)
        {
            var body = CurrentBody;

            if (body == null)
            {
                return;
            }

            var slot = body.SlotAt(args.ItemIndex);
            _currentBodyPart = body.PartAt(args.ItemIndex).Key;

            if (slot.Part != null)
            {
                UpdateBodyPartBox(slot.Part, slot.Id);
            }
        }

        private void UpdateBodyPartBox(IBodyPart part, string slotName)
        {
            BodyPartLabel.Text = $"{Loc.GetString(slotName)}: {Loc.GetString(part.Owner.Name)}";

            // TODO BODY Part damage
            if (part.Owner.TryGetComponent(out IDamageableComponent? damageable))
            {
                BodyPartHealth.Text = Loc.GetString("{0} damage", damageable.TotalDamage);
            }

            MechanismList.Clear();

            foreach (var mechanism in part.Mechanisms)
            {
                MechanismList.AddItem(mechanism.Name);
            }
        }

        // TODO BODY Guaranteed this is going to crash when a part's mechanisms change. This part is left as an exercise for the reader.
        public void MechanismOnItemSelected(ItemListSelectedEventArgs args)
        {
            UpdateMechanismBox(_currentBodyPart?.Mechanisms.ElementAt(args.ItemIndex));
        }

        private void UpdateMechanismBox(IMechanism? mechanism)
        {
            // TODO BODY Improve UI
            if (mechanism == null)
            {
                MechanismInfoLabel.SetMessage("");
                return;
            }

            // TODO BODY Mechanism description
            var message =
                Loc.GetString(
                    $"{mechanism.Name}\nHealth: {mechanism.CurrentDurability}/{mechanism.MaxDurability}");

            MechanismInfoLabel.SetMessage(message);
        }
    }
}
