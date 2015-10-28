﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using TerminologyLauncher.GUI.Annotations;

namespace TerminologyLauncher.GUI.ToolkitWindows.ConfigWindow.ConfigObjects
{
    public abstract class ConfigObject : INotifyPropertyChanged
    {
        protected ConfigObject(String name, String key)
        {
            this.Name = name;
            this.Key = key;
        }
        private string RealKey;
        private string RealName;

        public String Key
        {
            get { return this.RealKey; }
            protected set
            {
                this.RealKey = value;
                this.OnPropertyChanged();
            }
        }

        public String Name
        {
            get { return this.RealName; }
            set
            {
                this.RealName = value;
                this.OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
