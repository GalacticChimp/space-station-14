using System;
using Content.Shared.Solar;
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Client.Solar
{
    [UsedImplicitly]
    public class SolarControlConsoleBoundUserInterface : BoundUserInterface
    {
        [Dependency] private readonly IGameTiming _gameTiming = default!;

        private SolarControlWindow? _window;
        private SolarControlConsoleBoundInterfaceState _lastState = new(0, 0, 0, 0);

        protected override void Open()
        {
            base.Open();

            _window = new SolarControlWindow(_gameTiming);
            _window.OnClose += Close;
            _window.PanelRotation.OnTextEntered += (text) => {
                double value;
                if (double.TryParse(text.Text, out value))
                {
                    SolarControlConsoleAdjustMessage msg = new SolarControlConsoleAdjustMessage();
                    msg.Rotation = Angle.FromDegrees(value);
                    msg.AngularVelocity = _lastState.AngularVelocity;
                    SendMessage(msg);
                }
            };
            _window.PanelVelocity.OnTextEntered += (text) => {
                double value;
                if (double.TryParse(text.Text, out value))
                {
                    SolarControlConsoleAdjustMessage msg = new SolarControlConsoleAdjustMessage();
                    msg.Rotation = _lastState.Rotation;
                    msg.AngularVelocity = Angle.FromDegrees(value / 60);
                    SendMessage(msg);
                }
            };
            _window.OpenCentered();
        }

        public SolarControlConsoleBoundUserInterface(ClientUserInterfaceComponent owner, object uiKey) : base(owner, uiKey)
        {
        }

        private string FormatAngle(Angle d)
        {
            return d.Degrees.ToString("F1");
        }

        // The idea behind this is to prevent every update from the server
        //  breaking the textfield.
        private void UpdateField(LineEdit field, string newValue)
        {
            if (!field.HasKeyboardFocus())
            {
                field.Text = newValue;
            }
        }

        protected override void UpdateState(BoundUserInterfaceState state)
        {
            base.UpdateState(state);

            if (_window == null)
            {
                return;
            }

            SolarControlConsoleBoundInterfaceState scc = (SolarControlConsoleBoundInterfaceState) state;
            _lastState = scc;
            _window.NotARadar.UpdateState(scc);
            _window.OutputPower.Text = ((int) MathF.Floor(scc.OutputPower)).ToString();
            _window.SunAngle.Text = FormatAngle(scc.TowardsSun);
            UpdateField(_window.PanelRotation, FormatAngle(scc.Rotation));
            UpdateField(_window.PanelVelocity, FormatAngle(scc.AngularVelocity * 60));
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                _window?.Dispose();
            }
        }

        private sealed class SolarControlWindow : SS14Window
        {
            public readonly Label OutputPower;
            public readonly Label SunAngle;

            public readonly SolarControlNotARadar NotARadar;

            public readonly LineEdit PanelRotation;
            public readonly LineEdit PanelVelocity;

            public SolarControlWindow(IGameTiming igt)
            {
                Title = "Solar Control Window";

                var rows = new GridContainer();
                rows.Columns = 2;

                // little secret: the reason I put the values
                // in the first column is because otherwise the UI
                // layouter autoresizes the window to be too small
                rows.AddChild(new Label {Text = "Output Power:"});
                rows.AddChild(new Label {Text = ""});

                rows.AddChild(OutputPower = new Label {Text = "?"});
                rows.AddChild(new Label {Text = "W"});

                rows.AddChild(new Label {Text = "Sun Angle:"});
                rows.AddChild(new Label {Text = ""});

                rows.AddChild(SunAngle = new Label {Text = "?"});
                rows.AddChild(new Label {Text = "°"});

                rows.AddChild(new Label {Text = "Panel Angle:"});
                rows.AddChild(new Label {Text = ""});

                rows.AddChild(PanelRotation = new LineEdit());
                rows.AddChild(new Label {Text = "°"});

                rows.AddChild(new Label {Text = "Panel Angular Velocity:"});
                rows.AddChild(new Label {Text = ""});

                rows.AddChild(PanelVelocity = new LineEdit());
                rows.AddChild(new Label {Text = "°/min."});

                rows.AddChild(new Label {Text = "Press Enter to confirm."});
                rows.AddChild(new Label {Text = ""});

                PanelRotation.HorizontalExpand = true;
                PanelVelocity.HorizontalExpand = true;

                NotARadar = new SolarControlNotARadar(igt);

                var outerColumns = new HBoxContainer();
                outerColumns.AddChild(rows);
                outerColumns.AddChild(NotARadar);
                Contents.AddChild(outerColumns);
                Resizable = false;
            }
        }

        private sealed class SolarControlNotARadar : Control
        {
            // IoC doesn't apply here, so it's propagated from the parent class.
            // This is used for client-side prediction of the panel rotation.
            // This makes the display feel a lot smoother.
            private IGameTiming _gameTiming;

            private SolarControlConsoleBoundInterfaceState _lastState = new(0, 0, 0, 0);

            private TimeSpan _lastStateTime = TimeSpan.Zero;

            public const int SizeFull = 290;
            public const int RadiusCircle = 140;

            public SolarControlNotARadar(IGameTiming igt)
            {
                _gameTiming = igt;
                MinSize = (SizeFull, SizeFull);
            }

            public void UpdateState(SolarControlConsoleBoundInterfaceState ls)
            {
                _lastState = ls;
                _lastStateTime = _gameTiming.CurTime;
            }

            protected override void Draw(DrawingHandleScreen handle)
            {
                var point = SizeFull / 2;
                var fakeAA = new Color(0.08f, 0.08f, 0.08f);
                var gridLines = new Color(0.08f, 0.08f, 0.08f);
                var panelExtentCutback = 4;
                var gridLinesRadial = 8;
                var gridLinesEquatorial = 8;

                // Draw base
                handle.DrawCircle((point, point), RadiusCircle + 1, fakeAA);
                handle.DrawCircle((point, point), RadiusCircle, Color.Black);

                // Draw grid lines
                for (var i = 0; i < gridLinesEquatorial; i++)
                {
                    handle.DrawCircle((point, point), (RadiusCircle / gridLinesEquatorial) * i, gridLines, false);
                }

                for (var i = 0; i < gridLinesRadial; i++)
                {
                    Angle angle = (Math.PI / gridLinesRadial) * i;
                    var aExtent = angle.ToVec() * RadiusCircle;
                    handle.DrawLine((point, point) - aExtent, (point, point) + aExtent, gridLines);
                }

                // The rotations need to be adjusted because Y is inverted in Robust (like BYOND)
                Vector2 rotMul = (1, -1);

                Angle predictedPanelRotation = _lastState.Rotation + (_lastState.AngularVelocity * ((_gameTiming.CurTime - _lastStateTime).TotalSeconds));

                var extent = predictedPanelRotation.ToVec() * rotMul * RadiusCircle;
                Vector2 extentOrtho = (extent.Y, -extent.X);
                handle.DrawLine((point, point) - extentOrtho, (point, point) + extentOrtho, Color.White);
                handle.DrawLine((point, point) + (extent / panelExtentCutback), (point, point) + extent - (extent / panelExtentCutback), Color.DarkGray);

                var sunExtent = _lastState.TowardsSun.ToVec() * rotMul * RadiusCircle;
                handle.DrawLine((point, point) + sunExtent, (point, point), Color.Yellow);
            }
        }
    }
}
