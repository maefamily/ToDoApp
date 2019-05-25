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
    }
}
