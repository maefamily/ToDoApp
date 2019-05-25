﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json.Serialization;

namespace ToDoApp
{
    public partial class Form1 : Form
    {
        #region Properties
        // the data the user creates.
        public ToDoItemRoot root = null;

        public TreeNode targetTreeNodeUserInteraction = null;
        public bool hasTarget
        {
            get { return targetTreeNodeUserInteraction != null; }
        }
        public ToDoItem targetOrRootToDoItem
        {
            get { return hasTarget ? getItemFromNode(targetTreeNodeUserInteraction) : root; }
        }

        Properties.Settings settings
        {
            get { return Properties.Settings.Default; }
        }

        public bool isTreeLoadInProgress = false;
        #endregion Properties

        public Form1()
        {
            InitializeComponent();
        }

        public void applySettings()
        {
            this.TopMost = settings.isAlwaysOnTop;
            if (settings.location.X != -1)
                this.Location = settings.location;
            if (settings.size.Width != -1)
                this.Size = settings.size;
        }

        public void switchAlwaysOnTop()
        {
            settings.isAlwaysOnTop = !settings.isAlwaysOnTop;
            settings.Save();
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
            settings.location = this.Location;
            settings.size = this.Size;
            settings.Save();
        }

        public void syncViewFromModel()
        {
            treeViewItems.BeginUpdate();
            isTreeLoadInProgress = true;
            synchronizeNodesWithTheToDoItems(root.children, treeViewItems.Nodes);
            isTreeLoadInProgress = false;
            treeViewItems.EndUpdate();

            labelInfo.Visible = treeViewItems.Nodes.Count == 0;
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

                if (!toDoItem.isCollapsed)
                    treeNode.Expand();
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

        public bool newFile()
        {
            if (!isUserHappyWithTheSavedChanges)
                return false;
            root = new ToDoItemRoot();
            syncViewFromModel();
            return true;
        }

        public void openFile(string filePath)
        {
            root = ToDoItemRoot.open(filePath);
            settings.contentFilePath = filePath;
            settings.Save();
            syncViewFromModel();
        }

        public bool isUserHappyWithTheSavedChanges
        {
            get
            {
                if (root == null || !root.needsToBeSaved)
                    return true;

                switch (MessageBox.Show("Do you want to save the changes?", this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        return save() != DialogResult.Cancel;
                    case DialogResult.No:
                        return true;
                    case DialogResult.Cancel:
                        return false;
                    default:
                        return false;
                }
            }
        }

        public DialogResult save()
        {
            if (String.IsNullOrEmpty(settings.contentFilePath))
            {
                return saveAs();
            }
            else
            {
                root.save(settings.contentFilePath);
                return DialogResult.OK;
            }
        }

        public DialogResult saveAs()
        {
            DialogResult choice = saveFileDialog1.ShowDialog(this);
            if (choice == DialogResult.OK)
            {
                root.save(settings.contentFilePath);
                settings.contentFilePath = saveFileDialog1.FileName;
                settings.Save();
                return DialogResult.OK;
            }
            return choice;
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
            if (e.Button == MouseButtons.Right)
                showAppContextMenu(e.Location, null);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            applySettings();

            try
            {
                if (new FileInfo(settings.contentFilePath).Exists)
                {
                    openFileDialog1.FileName = settings.contentFilePath;
                    saveFileDialog1.FileName = settings.contentFilePath;

                    openFile(settings.contentFilePath);
                }
                else
                {
                    newFile();
                }
            }
            catch
            {
                newFile();
            }
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
            createItemToolStripMenuItem.Text = hasTarget ? String.Format("Create Item inside '{0}'", targetTreeNodeUserInteraction.Text) : "Create Top Level Item";
            
            deleteItemToolStripMenuItem.Visible = hasTarget;
            deleteItemToolStripMenuItem.Text = hasTarget? String.Format("Delete '{0}'...", targetTreeNodeUserInteraction.Text) : "Delete Item";

            newToolStripMenuItem.Enabled = !root.isEmpty;

            saveToolStripMenuItem.Enabled = root.needsToBeSaved;

            saveAsToolStripMenuItem.Enabled = !root.isEmpty;

            alwaysOnTopToolStripMenuItem.Checked = settings.isAlwaysOnTop;
        }

        private void alwaysOnTopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            switchAlwaysOnTop();
        }

        private void createItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createNewToDoItem(targetOrRootToDoItem);
        }

        private void deleteItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(String.Format("Are you sure you want to delete '{0}'?", targetTreeNodeUserInteraction.Text), this.Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                == DialogResult.Yes)
            {
                ToDoItem parentItem = targetTreeNodeUserInteraction.Parent != null ? getItemFromNode(targetTreeNodeUserInteraction.Parent) : root;
                parentItem.deleteChild(targetOrRootToDoItem);
                syncViewFromModel();
            }
        }

        private void treeViewItems_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            // the user has changed the text of one of the tree nodes.
            getItemFromNode(e.Node).setText(e.Label!=null?e.Label:e.Node.Text);
            syncViewFromModel();
        }

        private void treeViewItems_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // the user has clicked on a tree node.
            if (e.Button == MouseButtons.Right)
                showAppContextMenu(e.Location, e.Node);
        }

        private void treeViewItems_MouseUp(object sender, MouseEventArgs e)
        {
            // when clicking on the background.
            if (e.Button == MouseButtons.Right)
                showAppContextMenu(e.Location, null);
        }

        private void treeViewItems_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (isTreeLoadInProgress)
                return;
            getItemFromNode(e.Node).setIsCollapsed(!e.Node.IsExpanded);
        }

        private void treeViewItems_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (isTreeLoadInProgress)
                return;
            getItemFromNode(e.Node).setIsCollapsed(!e.Node.IsExpanded);
        }

        private void treeViewItems_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (isTreeLoadInProgress)
                return;
            getItemFromNode(e.Node).setIsChecked(e.Node.Checked);
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            newFile();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            save();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAs();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!isUserHappyWithTheSavedChanges)
                return;
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    openFile(openFileDialog1.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error opening", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ToDoApp\nby Mihai Maerean\nMay 2019", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isUserHappyWithTheSavedChanges)
                e.Cancel = true;
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion Events
    }
}
