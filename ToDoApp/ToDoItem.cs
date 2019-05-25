using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ToDoApp
{
    public class ToDoItem
    {
        public string id;
        public bool isChecked;
        public string text;
        public DateTime createdAt;
        public DateTime deadline;
        public DateTime checkedAt;
        public DateTime lastModifiedAt;
        public bool isCollapsed;

        public List<ToDoItem> children = new List<ToDoItem>();

        public ToDoItem(string text = "")
        {
            this.id = (new Random()).Next().ToString();
            this.text = text;
            setIsModified();
            createdAt = lastModifiedAt;
        }

        public void setIsChecked(bool isChecked)
        {
            this.isChecked = isChecked;
            checkedAt = DateTime.Now;
            setIsModified();
        }

        public void setText(string text)
        {
            this.text = text;
            setIsModified();
        }

        public void setIsCollapsed(bool isCollapsed)
        {
            this.isCollapsed = isCollapsed;
            setIsModified();
        }

        public void setIsModified()
        {
            lastModifiedAt = DateTime.Now;
        }

        public bool isTreeModified(ToDoItemRoot root)
        {
            return children.Find(child => child.lastModifiedAt > root.lastModifiedAt) != null ||
                children.Any(child => child.isTreeModified(root));
        }

        public bool isEmpty
        {
            get
            {
                return children.Count == 0;
            }
        }

        public void deleteChild(ToDoItem child)
        {
            children.Remove(child);
            setIsModified();
        }
    }
}
