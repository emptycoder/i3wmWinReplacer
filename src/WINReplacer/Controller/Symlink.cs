﻿using IWshRuntimeLibrary;

namespace WINReplacer
{
    class Symlink
    {
        public static IWshShortcut GetRealPath(string path)
        {
            if (System.IO.File.Exists(path))
            {
                WshShell shell = new WshShell(); //Create a new WshShell Interface
                IWshShortcut link = (IWshShortcut)shell.CreateShortcut(path); //Link the interface to our shortcut
                return link;
            }
            return null;
        }
    }
}
