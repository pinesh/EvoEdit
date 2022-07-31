using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace EvoEditApp
{
    /// <summary>
    /// Derived from guide: https://www.codeproject.com/articles/28306/working-with-checkboxes-in-the-wpf-treeview
    /// Added recursion and event functions.
    /// </summary>
    public class FooViewModel : INotifyPropertyChanged
    {
        bool? _isChecked = false;
        public string path;
        FooViewModel _parent;
        public static ObservableCollection<FooViewModel> CreateFoos(string name = "Select All",string path = "")
        {
            FooViewModel root = new FooViewModel(name,path)
            {
                IsInitiallySelected = false,
            };

            root.Initialize();
            return new ObservableCollection<FooViewModel> { root };
        }

        public List<string> GetPathsRecur(ObservableCollection<FooViewModel> childlist, string path, bool? ck)
        {
           
            var l = new List<string>();
            if (ck != null && childlist.Count == 0 && ck.Value)
            {
                return new List<string>(){path};
            }
            foreach (var child in childlist)
            {
                
                l.AddRange((GetPathsRecur(child.Children,child.path,child.IsChecked)));
            
            }

            return l;
        }

        public int CountChildrenRecur(ObservableCollection<FooViewModel> childlist)
        {
            int i = 0;
            foreach (var child in childlist)
            {
                if (child.Name.Contains("DATA") && child.IsChecked != null && child.IsChecked.Value)
                {
                    i++;
                }
                
                i += CountChildrenRecur(child.Children);
            }
            return i;
        }
        public FooViewModel(string name,string path)
        {
            this.Name = name;

            if (path.Length >0)
            {
                string[] files = System.IO.Directory.GetFiles(path, "*.smd3");
                if (files.Length == 0)
                    files = System.IO.Directory.GetFiles(path, "*.smd2");
                if (files.Length != 0)
                {
                    this.Name = name + $" (approx {(int)(new FileInfo(files[0]).Length / 34)} blocks)";
                }
                  
            }
            this.path = path;
            this.Children = new ObservableCollection<FooViewModel>();
        }

        public void Initialize()
        {
            foreach (FooViewModel child in this.Children)
            {
                child._parent = this;
                child.Initialize();
            }
        }


        public ObservableCollection<FooViewModel> Children { get; private set; }

        public bool IsInitiallySelected { get; private set; }

        public string Name { get; private set; }

        public bool? IsChecked
        {
            get => _isChecked;
            set => this.SetIsChecked(value, true, true);
        }

        
        public void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                foreach (var m in Children)
                {
                    m.SetIsChecked(_isChecked, true, false);
                }
            }

            if (updateParent && _parent != null)
            {
                _parent.VerifyCheckState();
            }
            if(_parent != null)
                notifyup(_parent);
            this.OnPropertyChanged("IsChecked");
        }

        public void notifyup(FooViewModel p)
        {
            if (p._parent != null)
            {
                notifyup(p._parent);
            }
            else
            {
                p.OnPropertyChanged("IsChecked");
            }
           
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

  
        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

    }
}