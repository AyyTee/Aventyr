﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using Game.Common;

namespace TimeLoopInc.Editor
{
    [DataContract]
    public class SceneBuilder
    {
        [DataMember]
        public ImmutableHashSet<Vector2i> Walls { get; private set; } = new HashSet<Vector2i>().ToImmutableHashSet();
        [DataMember]
        public ImmutableHashSet<Vector2i> Exits { get; private set; } = new HashSet<Vector2i>().ToImmutableHashSet();
        [DataMember]
        public ImmutableList<IGridEntity> Entities { get; private set; } = new List<IGridEntity>().ToImmutableList();
        [DataMember]
        public ImmutableList<PortalLink> Links { get; private set; } = new List<PortalLink>().ToImmutableList();

        public SceneBuilder With(
            ISet<Vector2i> walls = null,
            ISet<Vector2i> exits = null,
            IEnumerable<IGridEntity> entities = null,
            IEnumerable<PortalLink> links = null)
        {
            var clone = (SceneBuilder)MemberwiseClone();
            clone.Walls = walls?.ToImmutableHashSet() ?? Walls;
            clone.Exits = exits?.ToImmutableHashSet() ?? Exits;
            clone.Entities = entities?.ToImmutableList() ?? Entities;
            clone.Links = links?.ToImmutableList() ?? Links;
            return clone;
        }

        public Scene CreateScene()
        {
            var portals = new List<TimePortal>();
            foreach (var link in Links)
            {
                DebugEx.Assert(link.Portals.Any(item => item != null));

                var linkPortals = link.Portals
                    .Where(item => item != null)
                    .Select(item => new TimePortal(item.Position, item.Direction)).ToList();
                if (linkPortals.Count == 2)
                {
                    linkPortals[0].SetLinked(linkPortals[1]);
                    linkPortals[0].SetTimeOffset(link.TimeOffset);
                }
                portals.AddRange(linkPortals);
            }
            DebugEx.Assert(
                portals.GroupBy(item => (item.Position, item.Direction)).All(item => item.Count() == 1),
                "Portals should not sit on top of eachother.");

            return new Scene(Walls, portals, Entities, Exits);
        }
    }
}