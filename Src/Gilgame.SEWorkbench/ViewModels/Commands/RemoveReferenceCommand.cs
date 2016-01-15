﻿using System;
using System.Windows.Input;

namespace Gilgame.SEWorkbench.ViewModels.Commands
{
    public class RemoveReferenceCommand : ICommand
    {
        private readonly ProjectViewModel _Project;

        public RemoveReferenceCommand(ProjectViewModel project)
        {
            _Project = project;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }

        public void Execute(object parameter)
        {
            _Project.PerformRemoveReference();
        }
    }
}
