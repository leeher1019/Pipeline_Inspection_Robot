using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bluetooth
{
    public partial class Form3 : Form
    {
        Form1 Form1_Ref = null;

        public Form3(Bitmap compose, ref bool stop ,Form1 Temp)
        {
            InitializeComponent();
            this.Text = "Full View-Z";
            stop = true;
            pbZ.Image = compose;
            Form1_Ref = Temp;
        }
    }
}
