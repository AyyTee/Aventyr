using Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Common;
using Game.Rendering;

namespace TankGame
{
    public class TankCamera : IStep, ISceneObject
    {
        public Camera2 Camera { get; private set; }
        public Tank Tank { get; private set; }
        public string Name { get; set; } = nameof(TankCamera);

        public TankCamera(Camera2 camera, Tank tank)
        {
            Camera = camera;
            SetTank(tank);
        }

        public void SetTank(Tank tank)
        {
            Tank?.Scene.SceneObjects.Remove(this);
            Tank = tank;
            Tank?.Scene.Add(this);
        }

        public void StepBegin(IScene scene, float stepSize)
        {
            if (Tank != null)
            {
                //Camera.ViewOffset = Camera.ScreenToClip(Controller.InputExt.MousePos, Vector2Ext.ToOtk(Controller.CanvasSize)) * 0.4f;
                Transform2 t = new Transform2(Tank.WorldTransform.Position, Camera.WorldTransform.Size, Camera.WorldTransform.Rotation,  Camera.WorldTransform.MirrorX);
                Camera.WorldTransform = t;
            }
            else
            {
                Camera.ViewOffset = new OpenTK.Vector2();
            }
        }

        public void StepEnd(IScene scene, float stepSize)
        {
        }

        public void Remove()
        {
        }
    }
}
