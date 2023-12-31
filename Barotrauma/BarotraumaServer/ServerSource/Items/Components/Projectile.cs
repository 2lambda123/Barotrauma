﻿using Barotrauma.Networking;
using System;

namespace Barotrauma.Items.Components
{
    partial class Projectile : ItemComponent
    {
        private readonly struct EventData : IEventData
        {
            public readonly bool Launch;
            public readonly byte SpreadCounter;
            
            public EventData(bool launch, byte spreadCounter = 0)
            {
                Launch = launch;
                SpreadCounter = spreadCounter;
            }
        }
        
        private float launchRot;

        public override bool ValidateEventData(NetEntityEvent.IData data)
            => TryExtractEventData<EventData>(data, out _);

        public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
        {
            var eventData = ExtractEventData<EventData>(extraData);
            bool launch = eventData.Launch;
            
            msg.WriteBoolean(launch);
            if (launch)
            {
                msg.WriteUInt16(User?.ID ?? Entity.NullEntityID);
                msg.WriteSingle(launchPos.X);
                msg.WriteSingle(launchPos.Y);
                msg.WriteSingle(launchRot);
                msg.WriteByte(eventData.SpreadCounter);
                msg.WriteUInt16(LaunchSub?.ID ?? Entity.NullEntityID);
            }

            bool stuck = StickTarget != null && !item.Removed && !StickTargetRemoved();
            msg.WriteBoolean(stuck);
            if (stuck)
            {
                msg.WriteUInt16(item.Submarine?.ID ?? Entity.NullEntityID);
                msg.WriteUInt16(item.CurrentHull?.ID ?? Entity.NullEntityID);
                msg.WriteSingle(item.SimPosition.X);
                msg.WriteSingle(item.SimPosition.Y);
                msg.WriteSingle(jointAxis.X);
                msg.WriteSingle(jointAxis.Y);
                if (StickTarget.UserData is Structure structure)
                {
                    msg.WriteByte((byte)StickTargetType.Structure);
                    msg.WriteUInt16(structure.ID);
                    int bodyIndex = structure.Bodies.IndexOf(StickTarget);
                    msg.WriteByte((byte)(bodyIndex == -1 ? 0 : bodyIndex));
                }
                else if (StickTarget.UserData is Item item)
                {
                    msg.WriteByte((byte)StickTargetType.Item);
                    msg.WriteUInt16(item.ID);
                }
                else if (StickTarget.UserData is Submarine sub)
                {
                    msg.WriteByte((byte)StickTargetType.Submarine);
                    msg.WriteUInt16(sub.ID);
                }
                else if (StickTarget.UserData is Limb limb)
                {
                    msg.WriteByte((byte)StickTargetType.Limb);
                    msg.WriteUInt16(limb.character.ID);
                    msg.WriteByte((byte)Array.IndexOf(limb.character.AnimController.Limbs, limb));
                }
                else if (StickTarget.UserData is Voronoi2.VoronoiCell cell)
                {
                    msg.WriteByte((byte)StickTargetType.LevelWall);
                    msg.WriteInt32(Level.Loaded.GetAllCells().IndexOf(cell));
                }
                else
                {
                    msg.WriteByte((byte)StickTargetType.Unknown);
                    throw new NotImplementedException(StickTarget.UserData?.ToString() ?? "null" + " is not a valid projectile stick target.");
                }
            }
        }
    }
}
