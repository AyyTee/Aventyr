using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using Game;
using Game.Rendering;

namespace EditorWindow
{
    public partial class GlControlExt : GLControl
    {
        public GlControlExt()
            : base(Renderer.DefaultGraphics)
        {
            InitializeComponent();
        }
    }
}
