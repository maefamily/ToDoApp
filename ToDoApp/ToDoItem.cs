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

        public List<ToDoItem> children = new List<ToDoItem>();

        public ToDoItem(string text = "")
        {
            this.id = (new Random()).Next().ToString();
            this.text = text;
            setIsModified();
            createdAt = lastModifiedAt;
        }

        public void setIsChecked()
        {
            isChecked = true;
            checkedAt = DateTime.Now;
            setIsModified();
        }

        public void setText(string text)
        {
            this.text = text;
            setIsModified();
        }

        public void setIsModified()
        {
            lastModifiedAt = DateTime.Now;
        }
    }
}
