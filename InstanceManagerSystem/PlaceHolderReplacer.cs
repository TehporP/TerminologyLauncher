﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TerminologyLauncher.InstanceManagerSystem
{
    public class PlaceHolderReplacer
    {
        private Dictionary<String, String> Dictionary { get; set; }

        public PlaceHolderReplacer()
        {
            this.Dictionary=new Dictionary<string, string>();
        }

        public void AddToDictionary(String key, String value)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("Key should not be null or empty!");
            }
            this.Dictionary.Add(key, value);
        }

        public void RemoveFromDictionary(string key)
        {
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException("Key or value should not be null or empty!");
            }
            this.Dictionary.Remove(key);
        }

        public String ReplaceArgument(String argument)
        {
            return this.Dictionary.Aggregate(argument, (current, kp) => current.Replace(kp.Key, kp.Value));
        }
    }
}