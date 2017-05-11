using Game;
using Game.Common;
using Game.Portals;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeLoopInc
{
    public class Scene
    {
        public HashSet<Vector2i> Walls = new HashSet<Vector2i>();
        public List<TimePortal> Portals = new List<TimePortal>();
        public SceneState State = new SceneState();

        public Scene()
        {
            Walls = new HashSet<Vector2i>()
            {
                new Vector2i(1, 1),
                new Vector2i(1, 2),
                new Vector2i(1, 4),
            };
            State.CurrentPlayer = new Player(new Vector2i(), 0);
            State.PlayerTimeline.Path.Add(State.CurrentPlayer);

            var portal0 = new TimePortal(new Vector2i(4, 0), Direction.Right);
            var portal1 = new TimePortal(new Vector2i(-2, -2), Direction.Left);
            portal0.SetLinked(portal1);
            portal0.SetTimeOffset(0);

            Portals.Add(portal0);
            Portals.Add(portal1);

            var blockTimeline = new Timeline<Block>();
            blockTimeline.Path.Add(new Block(new Vector2i(2, 0), 0, 1));
            State.BlockTimelines.Add(blockTimeline);

            SetTime(State.StartTime);
        }

        public void Step(Input input)
        {
            State.CurrentPlayer.Input.Add(input);
            SetTime(State.Time);
        }

        void SetTime(int time)
        {
            State.SetTimeToStart();
            for (int i = State.StartTime; i <= time; i++)
            {
                _step();
            }
        }

        void _step()
        {
            foreach (var entity in State.Entities.Keys.ToList())
            {
                if (entity.EndTime == State.Time)
                {
                    State.Entities.Remove(entity);
                }
            }

            foreach (var entity in State.Entities.Keys)
            {
                if (entity is Player player)
                {
                    State.Entities[entity].SetPosition(Move(State.Entities[entity].Position, player.GetInput(State.Time).Heading));
                }
            }

            foreach (var block in State.Entities.Values.OfType<BlockInstant>())
            {
                block.IsPushed = false;
                block.AddPosition(new Vector2i());
            }

            var directions = new[] { Direction.Left, Direction.Right, Direction.Up, Direction.Down };
            for (int i = 0; i < directions.Length; i++)
            {
                foreach (var block in State.Entities.Keys.OfType<Block>())
                {
                    var adjacent = State.Entities[block].Position - DirectionEx.ToVector(directions[i]);
                    var pushes = State.Entities.Values
                        .OfType<PlayerInstant>()
                        .Where(item => item.Position - item.PreviousVelocity == adjacent && item.Position == State.Entities[block].Position);

                    var blockInstant = (BlockInstant)State.Entities[block];
                    if (pushes.Count() >= block.Size && !blockInstant.IsPushed)
                    {
                        blockInstant.IsPushed = true;
                        blockInstant.SetPosition(Move(blockInstant.Position, directions[i]));
                    }
                }
            }

            if (State.Entities.ContainsKey(State.CurrentPlayer))
            {
                var portal = Portals.FirstOrDefault(item => item.Position == State.Entities[State.CurrentPlayer].Position);
                if (portal != null && portal.TimeOffset != 0)
                {
                    var newTime = State.Time + portal.TimeOffset;
                    State.CurrentPlayer.EndTime = State.Time;
                    State.CurrentPlayer = new Player(portal.Position, newTime);
                    State.PlayerTimeline.Path.Add(State.CurrentPlayer);
                    SetTime(newTime);
                    return;
                }
            }

            foreach (var entity in State.Timelines.SelectMany(item => item.Path))
            {
                if (entity.StartTime == State.Time)
                {
                    State.Entities.Add(entity, entity.CreateInstant());
                }
            }

            State.Time++;
        }

        Vector2i Move(Vector2i position, Direction? heading)
        {
            //var transform = Ray.RayCast(
            //    new Transform2((Vector2)position), 
            //    Transform2.CreateVelocity((Vector2)DirectionEx.ToVector(heading)), 
            //    Portals, 
            //    new Ray.Settings()).Transform;

            //var posNext = (Vector2i)transform.Position.SnapToGrid(Vector2.One);

            //if (!Walls.Contains(posNext)) 
            //{
            //    return posNext;
            //}
            return position;
        }
    }
}
