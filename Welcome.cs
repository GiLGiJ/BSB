using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BSB
{
    public partial class Welcome : Form
    {
        BSBData data = new BSBData();
        
        public Welcome()
        {
            InitializeComponent();
        }
    }
}
