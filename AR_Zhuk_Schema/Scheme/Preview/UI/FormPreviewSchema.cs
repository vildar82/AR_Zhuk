using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AR_Zhuk_Schema.Scheme.Preview.UI
{
    public partial class FormPreviewSchema : Form
    {
        static Point location;
        static Size size;
        public FormPreviewSchema(Image im)
        {
            InitializeComponent();
            SetImage(im);            
        }

        public void SetImage(Image image)
        {
            pictureBox1.Image = image;
            this.Focus();
        }

        private void FormPreviewSchema_FormClosed(object sender, FormClosedEventArgs e)
        {
            location = Location;
            size = Size;
        }

        private void FormPreviewSchema_Load(object sender, EventArgs e)
        {
            if (!location.IsEmpty)
            {
                Location = location;
                Size = size;
            }            
        }
    }
}
