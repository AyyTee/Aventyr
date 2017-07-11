﻿using Equ;
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
        public readonly ImmutableList<Vector2i> Exits = new List<Vector2i>().ToImmutableList();
        public SceneInstant CurrentInstant => GetSceneInstant(CurrentTime);
        [DataMember]
        public int CurrentTime { get; private set; }
        [DataMember]
        public Timeline<Player> PlayerTimeline = new Timeline<Player>();
        public Player CurrentPlayer => (Player)PlayerTimeline.Path.Last();
        [DataMember]
        public List<Timeline<Block>> BlockTimelines = new List<Timeline<Block>>();
        public IEnumerable<ITimeline> Timelines => BlockTimelines
            .OfType<ITimeline>()
            .Concat(new[] { PlayerTimeline });
        public int StartTime => Timelines.MinOrNull(item => item.StartTime()) ?? 0;
        public int EndTime => Math.Max(Timelines.MaxOrNull(item => item.EndTime()) ?? StartTime, CurrentTime);

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
        }

        public IGridEntityInstant GetEntityInstant(IGridEntity entity)
        {
            return CurrentInstant.Entities[entity];
        }

        public ITimeline GetTimeline(IGridEntity entity)
        {
            var timelines = Timelines.Where(item => item.Path.Contains(entity));
            DebugEx.Assert(timelines.Count() == 1);
            return timelines.First();
        }

        public void Step(Input input)
        {
            if (CurrentInstant.Entities.ContainsKey(CurrentPlayer))
            {
                CurrentPlayer.Input.Add(input);
                InvalidateCache(CurrentTime + 1);
            }
            var nextInstant = GetSceneInstant(CurrentTime + 1);
            CurrentTime = nextInstant.Time;
        }

        void InvalidateCache(int time)
        {
            foreach (var key in _cachedInstants.Keys.Where(item => item >= time).ToList())
            {
                _cachedInstants.Remove(key);
            }
        }

        public SceneInstant GetSceneInstant(int time)
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
                    instant = new SceneInstant(StartTime - 1);
                }

                for (int i = key; i < time; i++)
                {
                    instant = _step(instant);
                    _cachedInstants[instant.Time] = instant;
                }
                return instant;
            }
            return new SceneInstant(time);
        }

        SceneInstant _step(SceneInstant sceneInstant)
        {
            var nextInstant = sceneInstant.WithTime(sceneInstant.Time + 1);

            foreach (var entity in nextInstant.Entities.Keys.ToList())
            {
                if (entity.EndTime == sceneInstant.Time)
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
                    var velocity = (Vector2d)(player.GetInput(sceneInstant.Time).Direction?.Vector ?? new Vector2i());
                    velocity = Vector2Ex.TransformVelocity(velocity, playerInstant.Transform.GetMatrix());
                    var result = Move(playerInstant.Transform, (Vector2i)velocity, sceneInstant.Time);
                    playerInstant.PreviousVelocity = result.Velocity;
                    playerInstant.Transform = result.Transform;
                    if (result.Time != sceneInstant.Time)
                    {
                        var timeline = PlayerTimeline;
                        if (timeline != null && PlayerTimeline.Path.Last() == player)
                        {
                            nextInstant.Entities.Remove(player);
                            player.EndTime = sceneInstant.Time;
                            timeline.Add(new Player(playerInstant.Transform, result.Time + 1, result.Velocity));
                            earliestTimeTravel = Math.Min(earliestTimeTravel, result.Time);
                            InvalidateCache(result.Time);

                            return GetSceneInstant(result.Time + 1);
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
                        .Concat(GetEntitiesCreated(nextInstant.Time).Select(item => item.CreateInstant()))
                        .OfType<PlayerInstant>()
                        .Where(item => item.Transform.Position - item.PreviousVelocity == adjacent && item.Transform.Position == nextInstant[block].Transform.Position);

                    var blockInstant = (BlockInstant)nextInstant[block];
                    if (pushes.Count() >= blockInstant.Transform.Size && !blockInstant.IsPushed)
                    {
                        blockInstant.IsPushed = true;
                        var result = Move(blockInstant.Transform, directions[i].Vector, sceneInstant.Time);
                        blockInstant.Transform = result.Transform;
                        blockInstant.PreviousVelocity = result.Velocity;
                        if (result.Time != sceneInstant.Time)
                        {
                            var timeline = BlockTimelines.FirstOrDefault(item => item.Path.Last() == block);
                            if (timeline != null)
                            {
                                nextInstant.Entities.Remove(block);
                                block.EndTime = sceneInstant.Time;
                                timeline.Add(new Block(blockInstant.Transform, result.Time + 1, result.Velocity));
                                earliestTimeTravel = Math.Min(earliestTimeTravel, result.Time);
                                InvalidateCache(result.Time);
                            }
                        }
                    }
                }
            }

            if (earliestTimeTravel < sceneInstant.Time)
            {
                return GetSceneInstant(nextInstant.Time);
            }

            foreach (var entity in GetEntitiesCreated(nextInstant.Time))
            {
                nextInstant.Entities.Add(entity, entity.CreateInstant());
            }

            return nextInstant;
        }

        public bool IsCompleted()
        {
            return Exits.Contains(GetEntityInstant(CurrentPlayer).Transform.Position);
        }

        public List<IGridEntity> GetEntitiesCreated(int time)
        {
            var list = new List<IGridEntity>();
            foreach (var entity in Timelines.SelectMany(item => item.Path))
            {
                if (entity.StartTime == time)
                {
                    list.Add(entity);
                }
            }
            return list;
        }

        public List<Paradox> GetParadoxes()
        {
            var output = new List<Paradox>();
            for (int i = StartTime; i <= EndTime; i++)
            {
                output.AddRange(GetParadoxes(i));
            }
            return output;
        }

        public List<Paradox> GetParadoxes(int time)
        {
            var entityList = GetSceneInstant(time).Entities;

            var matchingPositions = entityList
                .GroupBy(item => item.Value.Transform.Position)
                .Where(item => item.Count() > 1)
                .Select(item => new Paradox(time, new HashSet<IGridEntity>(item.Select(item2 => item2.Key))))
                .ToList();

            return matchingPositions;
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

            clone.PlayerTimeline = PlayerTimeline.DeepClone();
            clone.BlockTimelines = BlockTimelines.Select(item => item.DeepClone()).ToList();
            return clone;
        }
    }
}
