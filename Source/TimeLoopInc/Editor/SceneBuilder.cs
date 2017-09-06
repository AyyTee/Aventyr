using System;
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
        public Vector2i? Selected { get; private set; }
        [DataMember]
        public ImmutableHashSet<Vector2i> Walls { get; private set; } = new HashSet<Vector2i>().ToImmutableHashSet();
        [DataMember]
        public ImmutableHashSet<Vector2i> Exits { get; private set; } = new HashSet<Vector2i>().ToImmutableHashSet();
        [DataMember]
        public ImmutableList<IGridEntity> Entities { get; private set; } = new List<IGridEntity>().ToImmutableList();
        [DataMember]
        public ImmutableList<PortalLink> Links { get; private set; } = new List<PortalLink>().ToImmutableList();

        public SceneBuilder With(Vector2i? selected)
        {
            if (Selected == selected)
            {
                return this;
            }
            var clone = (SceneBuilder)MemberwiseClone();
            clone.Selected = selected;
            return clone;
        }

        public SceneBuilder With(
            ISet<Vector2i> walls = null,
            ISet<Vector2i> exits = null,
            IEnumerable<IGridEntity> entities = null,
            IEnumerable<PortalLink> links = null)
        {
            // If nothing has changed, return the original scene.
            if ((ReferenceEquals(Walls, walls) || walls == null) &&
                (ReferenceEquals(Exits, exits) || exits == null) &&
                (ReferenceEquals(Entities, entities) || entities == null) &&
                (ReferenceEquals(Links, links) || links == null))
            {
                return this;
            }

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
            foreach (var link in Links.Where(item => item.Portals.Any()))
            {
                var portal0 = new TimePortal(link.Portals[0].Position, link.Portals[0].Direction);
                portals.Add(portal0);
                if (link.Portals.Length == 2)
                {
                    var portal1 = new TimePortal(link.Portals[1].Position, link.Portals[1].Direction, true);
                    portal0.SetLinked(portal1);
                    portal0.SetTimeOffset(link.TimeOffset);
                    portals.Add(portal1);
                }
            }
            DebugEx.Assert(
                portals.GroupBy(item => (item.Position, item.Direction)).All(item => item.Count() == 1),
                "Portals should not sit on top of eachother.");

            return new Scene(Walls, portals, Entities, Exits);
        }
    }
}
