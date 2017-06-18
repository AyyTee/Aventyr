using Equ;
using Game;
using Game.Common;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    [DataContract]
    public class Scene : IDeepClone<Scene>
    {
        [DataMember]
        public readonly ImmutableHashSet<Vector2i> Walls = new HashSet<Vector2i>().ToImmutableHashSet();
        [DataMember]
        public readonly ImmutableList<TimePortal> Portals = new List<TimePortal>().ToImmutableList();
        [DataMember]
        public SceneInstant CurrentInstant = new SceneInstant();
        [DataMember]
        public Timeline<Player> PlayerTimeline = new Timeline<Player>();
        public Player CurrentPlayer => (Player)PlayerTimeline.Path.Last();
        [DataMember]
        public List<Timeline<Block>> BlockTimelines = new List<Timeline<Block>>();
        public IEnumerable<ITimeline> Timelines => BlockTimelines
            .OfType<ITimeline>()
            .Concat(new[] { PlayerTimeline });
        public int StartTime => Timelines.Min(item => item.Path.MinOrNull(entity => entity.StartTime)) ?? 0;

        readonly Dictionary<int, SceneInstant> _cachedInstants = new Dictionary<int, SceneInstant>();

        public Scene()
        {
        }

        public Scene(ISet<Vector2i> walls, IList<TimePortal> portals, Player player, IList<Block> blocks)
        {
            Walls = walls.ToImmutableHashSet();
            Portals = portals.ToImmutableList();

            PlayerTimeline = new Timeline<Player>();
            if (player != null)
            {
                PlayerTimeline.Add(player);
            }

            BlockTimelines.AddRange(blocks.Select(item => {
                var timeline = new Timeline<Block>();
                timeline.Add(item);
                return timeline;
            }));

            CurrentInstant = GetStateInstant(StartTime);
        }

        public void Step(Input input)
        {
            if (CurrentInstant.Entities.ContainsKey(CurrentPlayer))
            {
                CurrentPlayer.Input.Add(input);
                InvalidateCache(CurrentInstant.Time + 1);
            }
            CurrentInstant = GetStateInstant(CurrentInstant.Time + 1);
        }

        void InvalidateCache(int time)
        {
            foreach (var key in _cachedInstants.Keys.Where(item => item >= time).ToList())
            {
                _cachedInstants.Remove(key);
            }
        }

        public SceneInstant GetStateInstant(int time)
        {
            if (time >= StartTime)
            {
                int key;
                SceneInstant instant;
                if (_cachedInstants.Keys.Where(item => item <= time).Any())
                {
                    key = _cachedInstants.Keys.Where(item => item <= time).Max();
                    instant = _cachedInstants[key];
                }
                else
                {
                    key = StartTime - 1;
                    instant = new SceneInstant() { Time = StartTime - 1 };
                }

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

            var earliestTimeTravel = int.MaxValue;

            foreach (var entity in nextInstant.Entities.Keys)
            {
                if (entity is Player player)
                {
                    var playerInstant = nextInstant[entity];
                    var velocity = (Vector2d)(player.GetInput(nextInstant.Time).Direction?.Vector ?? new Vector2i());
                    velocity = Vector2Ex.TransformVelocity(velocity, playerInstant.Transform.GetMatrix());
                    var result = Move(playerInstant.Transform, (Vector2i)velocity, nextInstant.Time);
                    playerInstant.PreviousVelocity = result.Velocity;
                    playerInstant.Transform = result.Transform;
                    if (result.Time != nextInstant.Time)
                    {
                        var timeline = PlayerTimeline;
                        if (timeline != null && PlayerTimeline.Path.Last() == player)
                        {
                            nextInstant.Entities.Remove(player);
                            player.EndTime = nextInstant.Time;
                            timeline.Add(new Player(playerInstant.Transform, result.Time + 1, result.Velocity));
                            earliestTimeTravel = Math.Min(earliestTimeTravel, result.Time);
                            InvalidateCache(result.Time);

                            return GetStateInstant(result.Time + 1);
                        }
                    }
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
                foreach (var block in nextInstant.Entities.Keys.OfType<Block>().ToList())
                {
                    var adjacent = nextInstant[block].Transform.Position - directions[i].Vector;
                    var pushes = nextInstant.Entities.Values
                        .OfType<PlayerInstant>()
                        .Where(item => item.Transform.Position - item.PreviousVelocity == adjacent && item.Transform.Position == nextInstant[block].Transform.Position);

                    var blockInstant = (BlockInstant)nextInstant[block];
                    if (pushes.Count() >= blockInstant.Transform.Size && !blockInstant.IsPushed)
                    {
                        blockInstant.IsPushed = true;
                        var result = Move(blockInstant.Transform, directions[i].Vector, nextInstant.Time);
                        blockInstant.Transform = result.Transform;
                        blockInstant.PreviousVelocity = result.Velocity;
                        if (result.Time != nextInstant.Time)
                        {
                            var timeline = BlockTimelines.FirstOrDefault(item => item.Path.Last() == block);
                            if (timeline != null)
                            {
                                nextInstant.Entities.Remove(block);
                                block.EndTime = nextInstant.Time;
                                timeline.Add(new Block(blockInstant.Transform, result.Time + 1, result.Velocity));
                                earliestTimeTravel = Math.Min(earliestTimeTravel, result.Time);
                                InvalidateCache(result.Time);
                            }
                        }
                    }
                }
            }

            if (earliestTimeTravel < nextInstant.Time)
            {
                return GetStateInstant(nextInstant.Time + 1);   
            }

            foreach (var entity in Timelines.SelectMany(item => item.Path))
            {
                if (entity.StartTime == nextInstant.Time + 1)
                {
                    nextInstant.Entities.Add(entity, entity.CreateInstant());
                }
            }

            nextInstant.Time++;

            return nextInstant;
        }

        public (Transform2i Transform, Vector2i Velocity, int Time) Move(Transform2i transform, Vector2i velocity, int time)
        {
            var offset = Vector2d.One / 2;
            var transform2d = transform.ToTransform2d();
            transform2d = transform2d.WithPosition(transform2d.Position + offset);
            var result = Ray.RayCast(
                (Transform2)transform2d,
                Transform2.CreateVelocity((Vector2)velocity),
                Portals,
                new Ray.Settings());

            var newTime = time + result.PortalsEntered.Sum(item => ((TimePortal)item.EnterData.EntrancePortal).TimeOffset);

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

        public Scene DeepClone()
        {
            var clone = (Scene)MemberwiseClone();

            clone.CurrentInstant = CurrentInstant.DeepClone();
            clone.PlayerTimeline = PlayerTimeline.DeepClone();
            clone.BlockTimelines = BlockTimelines.Select(item => item.DeepClone()).ToList();
            return clone;
        }
    }
}
