using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Serialization;

namespace ToDoApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void applySettings()
        {
            this.TopMost = Properties.Settings.Default.isAlwaysOnTop;
            if (Properties.Settings.Default.location.X != -1)
                this.Location = Properties.Settings.Default.location;
            if (Properties.Settings.Default.size.Width != -1)
                this.Size = Properties.Settings.Default.size;
        }

        public void showAppContextMenu(Point whereInWindow)
        {
            contextMenuStrip1.Show(PointToScreen(whereInWindow));
        }

        public void showAppContextMenu(MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                showAppContextMenu(e.Location);
        }

        public void saveSize()
        {
            Properties.Settings.Default.location = this.Location;
            Properties.Settings.Default.size = this.Size;
            Properties.Settings.Default.Save();
        }

        #region Draggable
        private const int WM_NCHITTEST = 0x84;
        
        private const int HTTRANSPARENT = -0x1;
        private const int HTCLIENT = 0x1;
        private const int HTCAPTION = 0x2;

        ///
        /// Handling the window messages
        ///
        protected override void WndProc(ref Message message)
        {
            //base.WndProc(ref message);
            if (message.Msg == WM_NCHITTEST && (int)message.Result == HTCLIENT)
            {
                message.Result = (IntPtr)HTCAPTION;
                return;
            }
            base.WndProc(ref message);
        } 
        #endregion

        #region Events
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            showAppContextMenu(e);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            //if (e.Button == System.Windows.Forms.MouseButtons.Right)
            //    contextMenuStrip1.Show(PointToScreen(e.Location));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            applySettings();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            saveSize();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            saveSize();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            alwaysOnTopToolStripMenuItem.Checked = Properties.Settings.Default.isAlwaysOnTop;
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.isAlwaysOnTop = !Properties.Settings.Default.isAlwaysOnTop;
            Properties.Settings.Default.Save();
            applySettings();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void treeView1_MouseUp(object sender, MouseEventArgs e)
        {
            showAppContextMenu(e);
        }
        #endregion
    }
}
