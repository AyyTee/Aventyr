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

        public Scene()
        {
            Walls = new HashSet<Vector2i>()
            {
                new Vector2i(1, 1),
                new Vector2i(1, 2),
                new Vector2i(1, 4),
            }.ToImmutableHashSet();
            State.PlayerTimeline.Path.Add(new Player(new Vector2i(), 0));

            var portal0 = new TimePortal(new Vector2i(4, 0), GridAngle.Right);
            var portal1 = new TimePortal(new Vector2i(-2, -2), GridAngle.Left);
            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(10);

            Portals = new[] 
            {
                portal0,
                portal1,
            }.ToImmutableList();

            var blockTimeline = new Timeline<Block>();
            blockTimeline.Path.Add(new Block(new Vector2i(2, 0), 0, 1));
            blockTimeline.Path.Add(new Block(new Vector2i(2, 1), 1, 1));
            blockTimeline.Path.Add(new Block(new Vector2i(2, 2), 2, 1));
            State.BlockTimelines.Add(blockTimeline);

            SetTime(State.StartTime);
        }

        public void Step(Input input)
        {
            State.CurrentPlayer.Input.Add(input);
            SetTime(State.CurrentInstant.Time);
        }

        void SetTime(int time)
        {
            State.SetTimeToStart();
            for (int i = State.StartTime; i <= time; i++)
            {
                _step(State.CurrentInstant);
            }
        }

        SceneInstant GetState(int time)
        {
            var instant = new SceneInstant();
            instant.Time = State.StartTime;
            for (int i = State.StartTime; i <= time; i++)
            {
                _step(instant);
            }
            return instant;
        }

        void _step(SceneInstant sceneInstant)
        {
            foreach (var entity in sceneInstant.Entities.Keys.ToList())
            {
                if (entity.EndTime == sceneInstant.Time)
                {
                    sceneInstant.Entities.Remove(entity);
                }
            }

            foreach (var entity in sceneInstant.Entities.Keys)
            {
                if (entity is Player player)
                {
                    var velocity = (Vector2d)(player.GetInput(sceneInstant.Time).Direction?.Vector ?? new Vector2i());
                    velocity = Vector2Ex.TransformVelocity(velocity, sceneInstant[entity].Transform.GetMatrix());
                    var result = Move(sceneInstant[entity].Transform, (Vector2i)velocity, sceneInstant.Time);
                    sceneInstant[entity].PreviousVelocity = result.Velocity;
                    sceneInstant[entity].Transform = result.Transform;
                }
            }

            foreach (var block in sceneInstant.Entities.Values.OfType<BlockInstant>())
            {
                block.IsPushed = false;
                block.PreviousVelocity = new Vector2i();
            }

            var directions = new[] { GridAngle.Left, GridAngle.Right, GridAngle.Up, GridAngle.Down };
            for (int i = 0; i < directions.Length; i++)
            {
                foreach (var block in sceneInstant.Entities.Keys.OfType<Block>())
                {
                    var adjacent = sceneInstant[block].Transform.Position - directions[i].Vector;
                    var pushes = sceneInstant.Entities.Values
                        .OfType<PlayerInstant>()
                        .Where(item => item.Transform.Position - item.PreviousVelocity == adjacent && item.Transform.Position == sceneInstant[block].Transform.Position);

                    var blockInstant = (BlockInstant)sceneInstant[block];
                    if (pushes.Count() >= block.Size && !blockInstant.IsPushed)
                    {
                        blockInstant.IsPushed = true;
                        var result = Move(blockInstant.Transform, directions[i].Vector, sceneInstant.Time);
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
                if (entity.StartTime == sceneInstant.Time)
                {
                    sceneInstant.Entities.Add(entity, entity.CreateInstant());
                }
            }

            sceneInstant.Time++;
        }

        (Transform2i Transform, Vector2i Velocity, int Time) Move(Transform2i transform, Vector2i velocity, int time)
        {
            var offset = Vector2d.One / 2;
            var transform2d = transform.ToTransform2d();
            transform2d = transform2d.SetPosition(transform2d.Position + offset);
            var result = Ray.RayCast(
                (Transform2)transform2d,
                Transform2.CreateVelocity((Vector2)velocity),
                Portals,
                new Ray.Settings());

            var newTime = time + 1 + result.PortalsEntered.Sum(item => ((TimePortal)item.EnterData.EntrancePortal).TimeOffset);

            var resultTransform = (Transform2d)result.WorldTransform;
            resultTransform = resultTransform.SetPosition(resultTransform.Position - offset);
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
