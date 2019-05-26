using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace ToDoApp
{
    public class ToDoItemRoot: ToDoItem
    {
        public ToDoItemRoot()
            : base("Root")
        {
        }

        public void save(string filePath)
        {
            setIsModified();
            File.WriteAllText(filePath, JsonConvert.SerializeObject(this, jsonSettings));
        }

        public static ToDoItemRoot open(string filePath)
        {
            string json = File.ReadAllText(filePath);
            ToDoItemRoot serializedItem = JsonConvert.DeserializeObject<ToDoItemRoot>(json, jsonSettings);
            return serializedItem;
        }

        public static JsonSerializerSettings jsonSettings
        {
            get
            {
                if (_settings != null)
                    return _settings;
                _settings = new JsonSerializerSettings();
                _settings.Formatting = Formatting.Indented;
                return _settings;
            }
        }
        private static JsonSerializerSettings _settings;

        public bool needsToBeSaved
        {
            get
            {
                return isTreeModified(this);
            }
        }

        public static bool canBeCutPastedHere(ToDoItem itemToBeCut, ToDoItem pasteParent)
        {
            // check for cyclic dependencies.
            return itemToBeCut != null && pasteParent != null && !itemToBeCut.containsInTree(pasteParent);
        }

        public static bool cutPaste(ToDoItem itemToBeCut, ToDoItem parentOfItemToBeCut, ToDoItem pasteParent)
        {
            if (!canBeCutPastedHere(itemToBeCut, pasteParent))
                return false;
            
            parentOfItemToBeCut.deleteChild(itemToBeCut);
            pasteParent.addChild(itemToBeCut);
            return true;
        }
    }
}
