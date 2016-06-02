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

namespace EditorWindow
{
    public partial class GLControlExt : GLControl
    {
        public GLControlExt()
            : base(Window.DefaultGraphics)
        {
            InitializeComponent();
        }
    }
}
