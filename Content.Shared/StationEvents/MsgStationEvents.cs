﻿#nullable enable
using System;
using System.IO;
using Lidgren.Network;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.StationEvents
{
        public class MsgStationEvents : NetMessage
        {
            #region REQUIRED

            public const MsgGroups GROUP = MsgGroups.Command;
            public const string NAME = nameof(MsgStationEvents);
            public MsgStationEvents(INetChannel channel) : base(NAME, GROUP) { }

            #endregion

            public string[] Events = Array.Empty<string>();

            public override void ReadFromBuffer(NetIncomingMessage buffer)
            {
                var serializer = IoCManager.Resolve<IRobustSerializer>();
                var length = buffer.ReadVariableInt32();
                using var stream = buffer.ReadAlignedMemory(length);
                serializer.DeserializeDirect(stream, out Events);
            }

            public override void WriteToBuffer(NetOutgoingMessage buffer)
            {
                var serializer = IoCManager.Resolve<IRobustSerializer>();
                using (var stream = new MemoryStream())
                {
                    serializer.SerializeDirect(stream, Events);
                    buffer.WriteVariableInt32((int)stream.Length);
                    stream.TryGetBuffer(out var segment);
                    buffer.Write(segment);
                }
            }
        }
}
