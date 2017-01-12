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
        public FormPreviewSchema(Image im)
        {
            InitializeComponent();            
            pictureBox1.Image = im;
        }
    }
}
