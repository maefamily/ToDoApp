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
        public bool isCollapsed;
        public bool isModified;

        public List<ToDoItem> children = new List<ToDoItem>();

        public ToDoItem(string text = "")
        {
            this.id = (new Random()).Next().ToString();
            this.text = text;
            setIsModified();
            createdAt = DateTime.Now;
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
            isModified = true;
        }

        public void setIsNotModified()
        {
            isModified = false;
            children.ForEach(child => child.setIsNotModified());
        }

        public bool isTreeModified
        {
            get { return isModified || children.Any(child => child.isTreeModified); }
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

        public void addChild(ToDoItem child)
        {
            children.Add(child);
            setIsModified();
            child.setIsModified();
        }

        public int getChildIndex(ToDoItem child)
        {
            return children.IndexOf(child);
        }

        public void moveUpChild(ToDoItem child)
        {
            int index = getChildIndex(child);
            if (index <= 0)
                return;
            children.RemoveAt(index);
            children.Insert(index - 1, child);
            setIsModified();
        }

        public void moveDownChild(ToDoItem child)
        {
            int index = getChildIndex(child);
            if (index >= children.Count - 1)
                return;
            children.RemoveAt(index);
            children.Insert(index + 1, child);
            setIsModified();
        }

        public bool containsInTree(ToDoItem childToLookFor)
        {
            return this == childToLookFor ||
                children.Find(child => child == childToLookFor || child.containsInTree(childToLookFor)) == childToLookFor;
        }
    }
}
