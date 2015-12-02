﻿using System;
using System.IO;

namespace Gilgame.SEWorkbench.Configuration
{
    public static class Program
    {
        private static string Root
        {
            get
            {
                return Path.Combine(Services.Registry.K_ROOT, "Program");
            }
        }

        public static string SEPath
        {
            get
            {
                return Convert.ToString(Services.Registry.GetValue(Microsoft.Win32.RegistryHive.CurrentUser, Root, "SEPath", null));
            }
            set
            {
                Services.Registry.SetValue(Microsoft.Win32.RegistryHive.CurrentUser, Root, "SEPath", Convert.ToString(value));
            }
        }
    }
}
