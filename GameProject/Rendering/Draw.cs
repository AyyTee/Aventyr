using Game.Common;
using Game.Models;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Rendering
{
    public static class Draw
    {
        public static IRenderable Text(Font font, Vector2 position, string text, Vector2 alignment = new Vector2(), int lineSpacing = 0)
        {
            return new TextEntity(font, position, text, alignment, lineSpacing);
        }

        public static IRenderable Text(Font font, Vector2 position, string text, Color4 color, Vector2 alignment = new Vector2(), int lineSpacing = 0)
        {
            return new TextEntity(font, position, text, color, alignment, lineSpacing);
        }

        public static Renderable Rectangle(Vector2 topLeft, Vector2 bottomRight)
        {
            return Rectangle(topLeft, bottomRight, Color4.White);
        }

        public static Renderable Rectangle(Vector2 topLeft, Vector2 bottomRight, Color4 color)
        {
            var plane = new Model(
                ModelFactory.CreatePlaneMesh(
                    Vector2.ComponentMin(topLeft, bottomRight),
                    Vector2.ComponentMax(topLeft, bottomRight),
                    color));
            return GetRenderable(plane);
        }

        public static Renderable Line(LineF line)
        {
            return Line(line, Color4.White);
        }

        public static Renderable Line(LineF line, Color4 color, float thickness = 1)
        {
            var lineModel = ModelFactory.CreateLinesWidth(new[] { line }, thickness, color);
            return GetRenderable(lineModel);
        }

        public static Renderable Triangle(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            return Triangle(v0, v1, v2, Color4.White);
        }

        public static Renderable Triangle(Vector2 v0, Vector2 v1, Vector2 v2, Color4 color)
        {
            var vArray = MathEx.SetWinding(new[] { v0, v1, v2 }, false);
            var triangle = ModelFactory.CreateTriangle(
                new Vector3(vArray[0]),
                new Vector3(vArray[1]),
                new Vector3(vArray[2]),
                color);
            return GetRenderable(triangle);
        }

        static Renderable GetRenderable(Model model)
        {
            var renderable = new Renderable();
            renderable.IsPortalable = false;
            renderable.Models.Add(model);
            return renderable;
        }
    }
}
