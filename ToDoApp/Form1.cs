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
        // the data the user creates.
        public ToDoItem root = new ToDoItem("Root");

        public TreeNode targetTreeNodeUserInteraction = null;

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

        public void switchAlwaysOnTop()
        {
            Properties.Settings.Default.isAlwaysOnTop = !Properties.Settings.Default.isAlwaysOnTop;
            Properties.Settings.Default.Save();
            applySettings();
        }

        // when treeViewItems_NodeMouseClick is called, treeViewItems_MouseUp will immediately be called after that.
        // timeShowAppContextMenu is used to detect this situation.
        private DateTime timeShowAppContextMenu;
        public void showAppContextMenu(Point whereInWindow, TreeNode target)
        {
            DateTime now = DateTime.Now;
            if ((now - timeShowAppContextMenu).TotalSeconds < 1)
                return;
            timeShowAppContextMenu = now;

            targetTreeNodeUserInteraction = target;
            contextMenuStrip1.Show(PointToScreen(whereInWindow));
        }

        public void saveSizeInSettings()
        {
            Properties.Settings.Default.location = this.Location;
            Properties.Settings.Default.size = this.Size;
            Properties.Settings.Default.Save();
        }

        public void syncViewFromModel()
        {
            treeViewItems.BeginUpdate();
            synchronizeNodesWithTheToDoItems(root.children, treeViewItems.Nodes);
            treeViewItems.EndUpdate();
        }
        
        public void synchronizeNodesWithTheToDoItems(List<ToDoItem> items, TreeNodeCollection nodes)
        {
            // the index ensures the UI nodes are in the same order as the data.
            int toDoItemIndex = -1;
            foreach (ToDoItem toDoItem in items)
            {
                ++toDoItemIndex;

                int treeNodeIndex = nodes.IndexOfKey(toDoItem.id);
                TreeNode treeNode;
                if (treeNodeIndex > -1)
                {
                    // we already have a treeNode for this ToDoItem.
                    treeNode = nodes[treeNodeIndex];

                    if (treeNodeIndex != toDoItemIndex)
                    {
                        // but it's not in the right position.
                        nodes.RemoveAt(treeNodeIndex);
                        nodes.Insert(toDoItemIndex, treeNode);
                    }
                }
                else
                {
                    // this is new for me.
                    treeNode = nodes.Insert(toDoItemIndex, toDoItem.id, toDoItem.text);
                }

                treeNode.Tag = toDoItem;
                treeNode.Checked = toDoItem.isChecked;
                treeNode.Text = toDoItem.text;

                // recurse the entire tree.
                synchronizeNodesWithTheToDoItems(toDoItem.children, treeNode.Nodes);
            }

            // what if some ToDoItems have been deleted.
            while (nodes.Count > items.Count)
                nodes.RemoveAt(nodes.Count - 1);
        }

        public TreeNode getNodeForToDoItem(ToDoItem item)
        {
            TreeNode[] nodes = treeViewItems.Nodes.Find(item.id, true);
            return nodes.Length > 0 ? nodes[0] : null;
        }

        public void createNewToDoItem(ToDoItem parent)
        {
            ToDoItem newItem = new ToDoItem("text");
            parent.children.Add(newItem);
            syncViewFromModel();
            TreeNode node = getNodeForToDoItem(newItem);
            treeViewItems.SelectedNode = node;
            node.BeginEdit();
        }

        public ToDoItem getItemFromNode(TreeNode treeNode)
        {
            return treeNode.Tag as ToDoItem;
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
            //if (message.Msg == WM_NCHITTEST && (int)message.Result == HTCLIENT)
            //{
            //    message.Result = (IntPtr)HTCAPTION;
            //    return;
            //}
            base.WndProc(ref message);
        } 
        #endregion

        #region Events
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                showAppContextMenu(e.Location, null);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            applySettings();
            syncViewFromModel();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            saveSizeInSettings();
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            saveSizeInSettings();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            alwaysOnTopToolStripMenuItem.Checked = Properties.Settings.Default.isAlwaysOnTop;
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switchAlwaysOnTop();
        }

        private void createItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createNewToDoItem(targetTreeNodeUserInteraction != null ? getItemFromNode(targetTreeNodeUserInteraction) : root);
        }

        private void treeViewItems_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            // the user has changed the text of one of the tree nodes.
            getItemFromNode(e.Node).setText(e.Label);
            syncViewFromModel();
        }

        private void treeViewItems_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // the user has clicked on a tree node.
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                showAppContextMenu(e.Location, e.Node);
        }

        private void treeViewItems_MouseUp(object sender, MouseEventArgs e)
        {
            // when clicking on the background.
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
                showAppContextMenu(e.Location, null);
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion
    }
}
