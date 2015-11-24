﻿using System;
using System.Linq;

using Gilgame.SEWorkbench.Models;

namespace Gilgame.SEWorkbench.ViewModels
{
    public class GridItemViewModel : BaseViewModel
    {
        private Services.ObservableSortedList<GridItemViewModel> _Children;
        public Services.ObservableSortedList<GridItemViewModel> Children
        {
            get
            {
                return _Children;
            }
        }

        public string Name
        {
            get
            {
                return _Model.Name;
            }
        }

        private GridItem _Model;
        public GridItem Model
        {
            get
            {
                return _Model;
            }
        }

        private bool _IsExpanded = false;
        public bool IsExpanded
        {
            get
            {
                return _IsExpanded;
            }
            set
            {
                if (value != _IsExpanded)
                {
                    _IsExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }

                if (_IsExpanded && Parent != null)
                {
                    ((GridItemViewModel)Parent).IsExpanded = true;
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
                    OnPropertyChanged("IsVisible");
                }

                if (_IsVisible && Parent != null)
                {
                    ((GridItemViewModel)Parent).IsVisible = true;
                }
                if (!_IsVisible && Parent != null)
                {
                    ((GridItemViewModel)Parent).IsVisible = false;
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
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public GridItemViewModel(GridItem item) : this(item, null)
        {
        }

        public GridItemViewModel(GridItem item, GridItemViewModel parent) : base(parent)
        {
            _Model = item;

            _Children = new Services.ObservableSortedList<GridItemViewModel>(
                (from child in _Model.Children select new GridItemViewModel(child, this)).ToList<GridItemViewModel>(),
                new Comparers.GridItemComparer<GridItemViewModel>()
            );
        }

        public void AddChild(GridItemViewModel item)
        {
            _Children.Add(item);
            Model.Children.Add(item.Model);
        }
    }
}
