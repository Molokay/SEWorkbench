﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Gilgame.SEWorkbench.Models;
using Gilgame.SEWorkbench.Services;
using Gilgame.SEWorkbench.Services.IO;

namespace Gilgame.SEWorkbench.ViewModels
{
    public class ProjectItemViewModel : BaseViewModel
    {
        private Services.ObservableSortedList<ProjectItemViewModel> _Children;
        public Services.ObservableSortedList<ProjectItemViewModel> Children
        {
            get
            {
                return _Children;
            }
        }

        private ProjectItem _Model;
        public ProjectItem Model
        {
            get
            {
                return _Model;
            }
        }

        public string Name
        {
            get
            {
                return _Model.Name;
            }
            set
            {
                _Model.Name = value;
                RaisePropertyChanged("Name");
            }
        }

        public ProjectItemType Type
        {
            get
            {
                return _Model.Type;
            }
        }

        public string Path
        {
            get
            {
                return _Model.Path;
            }
            set
            {
                _Model.Path = value;
                RaisePropertyChanged("Path");
            }
        }

        private bool _FileMissing = false;
        public bool FileMissing
        {
            get
            {
                return _FileMissing;
            }
            set
            {
                _FileMissing = value;
                RaisePropertyChanged("FileMissing");
            }
        }

        public string Blueprint
        {
            get
            {
                return _Model.Blueprint;
            }
        }

        private ObservableSortedList<GridItemViewModel> _Grid;
        public ObservableSortedList<GridItemViewModel> Grid
        {
            get
            {
                return _Grid;
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _Model.IsExpanded;
            }
            set
            {
                if (value != _Model.IsExpanded)
                {
                    _Model.IsExpanded = value;
                    RaisePropertyChanged("IsExpanded");
                }

                if (_Model.IsExpanded && Parent != null)
                {
                    ProjectItemViewModel parent = (ProjectItemViewModel)Parent;
                    parent.IsExpanded = true;
                }
            }
        }

        private bool _IsVisible = true;
        public bool IsVisible
        {
            get
            {
                return _IsVisible;
            }
            set
            {
                if (value != _IsVisible)
                {
                    _IsVisible = value;
                    RaisePropertyChanged("IsVisible");
                }

                ProjectItemViewModel parent = (ProjectItemViewModel)Parent;
                if (_IsVisible && parent != null)
                {
                    parent.IsVisible = true;
                }
                if (!_IsVisible && parent != null)
                {
                    parent.IsVisible = false;
                }
            }
        }

        private bool _IsSelected = false;
        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                if (value != _IsSelected)
                {
                    _IsSelected = value;
                    RaisePropertyChanged("IsSelected");

                    if (_Model.Project != null)
                    {
                        _Model.Project.RaiseSelectionChanged();
                    }
                }
            }
        }

        private string _Unsaved = String.Empty;
        public string Unsaved
        {
            get
            {
                return _Unsaved;
            }
            set
            {
                if (value != _Unsaved)
                {
                    _Unsaved = value;
                    RaisePropertyChanged("Unsaved");
                }
            }
        }

        public ProjectItemViewModel(ProjectItem item) : this(item, null)
        {
            _FileRequestedCommand = new Commands.DelegateCommand(RaiseFileRequested);
        }

        public ProjectItemViewModel(ProjectItem item, ProjectItemViewModel parent) : base(parent)
        {
            _Model = item;

            _Children = new Services.ObservableSortedList<ProjectItemViewModel>(
                (from child in _Model.Children select new ProjectItemViewModel(child, this)).ToList<ProjectItemViewModel>(),
                new Comparers.ProjectItemComparer()
            );

            _Grid = new Services.ObservableSortedList<GridItemViewModel>(
                new GridItemViewModel[] { },
                new Comparers.GridItemComparer()
            );
        }

        public ProjectItemViewModel AddChild(ProjectItem item)
        {
            return AddChild(item, null);
        }

        public ProjectItemViewModel AddChild(ProjectItem item, Interop.Grid grid)
        {
            ProjectItemViewModel vm = new ProjectItemViewModel(item, this);
            if (grid != null)
            {
                vm.Grid.Add(CreateGridViewModel(grid));
            }

            _Children.Add(vm);
            Model.Children.Add(item);

            return vm;
        }

        public void UpdatePath(string source, string destination)
        {
            Path = Path.Replace(source, destination);
            foreach(ProjectItemViewModel child in Children)
            {
                child.UpdatePath(source, destination);
            }
        }

        public void VerifyPath()
        {
            switch (_Model.Type)
            {
                case ProjectItemType.Blueprints:
                    FileMissing = (!Directory.Exists(_Model.Path) || !File.Exists(Blueprint));
                    break;

                case ProjectItemType.Collection:
                case ProjectItemType.Folder:
                    FileMissing = !Directory.Exists(_Model.Path);
                    break;

                case ProjectItemType.File:
                case ProjectItemType.Reference:
                    FileMissing = !File.Exists(_Model.Path);
                    break;
            }
            foreach (ProjectItemViewModel child in Children)
            {
                child.VerifyPath();
            }
        }

        public void Remove()
        {
            ProjectItemViewModel parent = (ProjectItemViewModel)Parent;
            parent.RemoveChild(this);
        }

        private void RemoveChild(ProjectItemViewModel child)
        {
            Model.Children.Remove(child.Model);
            _Children.Remove(child);
        }

        public bool NameContainsText(string text)
        {
            if (String.IsNullOrEmpty(text) || String.IsNullOrEmpty(Name))
            {
                return false;
            }
            return Name.IndexOf(text, StringComparison.InvariantCultureIgnoreCase) > -1;
        }

        public void SetGrid(Interop.Grid grid)
        {
            GridItemViewModel vm = CreateGridViewModel(grid);

            _Grid.Clear();
            _Grid.Add(vm);
        }

        private GridItemViewModel CreateGridViewModel(Interop.Grid grid)
        {
            GridItemViewModel root = new GridItemViewModel(
                new GridItem()
                {
                    Name = grid.Name,
                    Type = GridItemType.Root
                },
                grid.Definitions,
                grid.Path
            );

            foreach(KeyValuePair<string, List<Interop.TerminalBlock>> pair in grid.Blocks)
            {
                GridItemViewModel node = new GridItemViewModel(
                    new GridItem()
                    {
                        Name = pair.Key,
                        Type = GridItemType.Group
                    },
                    root
                );

                foreach(Interop.TerminalBlock block in pair.Value)
                {
                    if (block.IsProgram)
                    {
                        node.AddChild(
                            new GridItemViewModel(
                                new GridItem()
                                {
                                    Name = block.Name,
                                    EntityID = block.EntityID,
                                    Type = GridItemType.Program,
                                    Program = block.Program
                                },
                                node
                            )
                        );
                    }
                    else
                    {
                        node.AddChild(
                            new GridItemViewModel(
                                new GridItem()
                                {
                                    Name = block.Name,
                                    EntityID = block.EntityID,
                                    Type = GridItemType.Block
                                },
                                node
                            )
                        );
                    }
                }
                root.AddChild(node);
            }
            return root;
        }

        #region File Requested Command

        private readonly ICommand _FileRequestedCommand;
        public ICommand FileRequestedCommand
        {
            get
            {
                return _FileRequestedCommand;
            }
        }

        public void RaiseFileRequested()
        {
            _Model.Project.RaiseFileRequested();
        }

        #endregion
    }
}
