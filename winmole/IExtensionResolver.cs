using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace winmole
{
    public interface IExtensionResolver
    {
        Prompt BuildPrompt(string path);
    }


    public class DirectoryResolver : IExtensionResolver
    {

        public Prompt BuildPrompt(string path)
        {
            return new Prompt(path, path);
        }
    }


    public class SystemLinkResolver : IExtensionResolver
    {


        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

        public Prompt BuildPrompt(string path)
        {

            FileInfo file = new FileInfo(path);

            if (file.Extension.ToLower() != ".lnk")
                throw new ArgumentException("specified file isn't link file");

            IWshRuntimeLibrary.WshShortcut link = shell.CreateShortcut(file.FullName) as IWshRuntimeLibrary.WshShortcut;
            return new Prompt()
            {
                Title = file.Name.Substring(0, file.Name.Length - 4),
                ExecutePath = link.TargetPath,
                FullPath = file.FullName
            };



        }
    }
}
