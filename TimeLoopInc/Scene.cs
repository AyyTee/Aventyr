using Game;
using Game.Common;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class Scene
    {
        public readonly ImmutableHashSet<Vector2i> Walls;
        public readonly ImmutableList<TimePortal> Portals;
        public SceneState State = new SceneState();

        readonly Dictionary<int, SceneInstant> _cachedInstants = new Dictionary<int, SceneInstant>();

        public Scene(ISet<Vector2i> walls, IList<TimePortal> portals, Player player, IList<Block> blocks)
        {
            Walls = walls.ToImmutableHashSet();
            Portals = portals.ToImmutableList();

            State.PlayerTimeline = new Timeline<Player>();
            if (player != null)
            {
                State.PlayerTimeline.Add(player);
            }

            State.BlockTimelines.AddRange(blocks.Select(item => {
                var timeline = new Timeline<Block>();
                timeline.Add(item);
                return timeline;
            }));

            _cachedInstants[State.StartTime] = new SceneInstant();
            State.CurrentInstant = GetStateInstant(State.StartTime + 1);
        }

        public void Step(Input input)
        {
            if (State.CurrentInstant.Entities.ContainsKey(State.CurrentPlayer))
            {
                State.CurrentPlayer.Input.Add(input);
                for (int i = State.CurrentInstant.Time + 1; i < _cachedInstants.Keys.Max(); i++)
                {
                    _cachedInstants.Remove(i);
                }
            }
            State.CurrentInstant = GetStateInstant(State.CurrentInstant.Time + 1);
        }

        public SceneInstant GetStateInstant(int time)
        {
            if (time >= State.StartTime)
            {
                var key = _cachedInstants.Keys.Where(item => item <= time).Max();
                var instant = _cachedInstants[key];

                for (int i = key; i < time; i++)
                {
                    instant = _step(instant);
                    _cachedInstants[i + 1] = instant;
                }
                return instant;
            }
            return new SceneInstant() { Time = time };
        }

        SceneInstant _step(SceneInstant sceneInstant)
        {
            var nextInstant = sceneInstant.DeepClone();

            foreach (var entity in nextInstant.Entities.Keys.ToList())
            {
                if (entity.EndTime == nextInstant.Time)
                {
                    nextInstant.Entities.Remove(entity);
                }
            }

            foreach (var entity in nextInstant.Entities.Keys)
            {
                if (entity is Player player)
                {
                    var velocity = (Vector2d)(player.GetInput(nextInstant.Time).Direction?.Vector ?? new Vector2i());
                    velocity = Vector2Ex.TransformVelocity(velocity, nextInstant[entity].Transform.GetMatrix());
                    var result = Move(nextInstant[entity].Transform, (Vector2i)velocity, nextInstant.Time);
                    nextInstant[entity].PreviousVelocity = result.Velocity;
                    nextInstant[entity].Transform = result.Transform;
                }
            }

            foreach (var block in nextInstant.Entities.Values.OfType<BlockInstant>())
            {
                block.IsPushed = false;
                block.PreviousVelocity = new Vector2i();
            }

            var directions = new[] { GridAngle.Left, GridAngle.Right, GridAngle.Up, GridAngle.Down };
            for (int i = 0; i < directions.Length; i++)
            {
                foreach (var block in nextInstant.Entities.Keys.OfType<Block>())
                {
                    var adjacent = nextInstant[block].Transform.Position - directions[i].Vector;
                    var pushes = nextInstant.Entities.Values
                        .OfType<PlayerInstant>()
                        .Where(item => item.Transform.Position - item.PreviousVelocity == adjacent && item.Transform.Position == nextInstant[block].Transform.Position);

                    var blockInstant = (BlockInstant)nextInstant[block];
                    if (pushes.Count() >= block.Size && !blockInstant.IsPushed)
                    {
                        blockInstant.IsPushed = true;
                        var result = Move(blockInstant.Transform, directions[i].Vector, nextInstant.Time);
                        blockInstant.Transform = result.Transform;
                        blockInstant.PreviousVelocity = result.Velocity;
                    }
                }
            }

            //if (State.Entities.ContainsKey(State.CurrentPlayer))
            //{
            //    var player = State.Entities[State.CurrentPlayer];
            //    var portalExit = Portals.FirstOrDefault(item => item.Position == player.Transform.Position);
            //    var (prevPlayerPosition, _) = Move(player.Transform, -player.PreviousVelocity);
            //    var portalEnter = Portals.FirstOrDefault(item => item.Position == prevPlayerPosition.Position);
            //    if (portalExit != null && portalEnter != null && portalEnter.TimeOffset != 0)
            //    {
            //        var newTime = State.Time + portalEnter.TimeOffset;
            //        State.CurrentPlayer.EndTime = State.Time;
            //        State.CurrentPlayer = new Player(portalExit.Position, newTime);
            //        State.PlayerTimeline.Path.Add(State.CurrentPlayer);
            //        SetTime(newTime);
            //        return;
            //    }
            //}

            foreach (var entity in State.Timelines.SelectMany(item => item.Path))
            {
                if (entity.StartTime == nextInstant.Time)
                {
                    nextInstant.Entities.Add(entity, entity.CreateInstant());
                }
            }

            nextInstant.Time++;

            return nextInstant;
        }

        (Transform2i Transform, Vector2i Velocity, int Time) Move(Transform2i transform, Vector2i velocity, int time)
        {
            var offset = Vector2d.One / 2;
            var transform2d = transform.ToTransform2d();
            transform2d = transform2d.WithPosition(transform2d.Position + offset);
            var result = Ray.RayCast(
                (Transform2)transform2d,
                Transform2.CreateVelocity((Vector2)velocity),
                Portals,
                new Ray.Settings());

            var newTime = time + 1 + result.PortalsEntered.Sum(item => ((TimePortal)item.EnterData.EntrancePortal).TimeOffset);

            var resultTransform = (Transform2d)result.WorldTransform;
            resultTransform = resultTransform.WithPosition(resultTransform.Position - offset);
            var posNextGrid = Transform2i.RoundTransform2d(resultTransform);

            if (!Walls.Contains(posNextGrid.Position))
            {
                return ValueTuple.Create(
                    posNextGrid, 
                    (Vector2i)result.WorldVelocity.Position.Round(Vector2.One),
                    newTime);
            }
            return ValueTuple.Create(transform, new Vector2i(), newTime);
        }
    }
}
