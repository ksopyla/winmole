using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using winmole.Entities;

namespace winmole.Logic
{
    public interface IExtensionResolver
    {
        PromptItem BuildPrompt(string path);
    }


    public class DirectoryResolver : IExtensionResolver
    {

        public PromptItem BuildPrompt(string path)
        {
            return new PromptItem(path);
        }
    }

    public class FileResolver: IExtensionResolver
    {
        public PromptItem BuildPrompt(string path)
        {
            //FileInfo file = new FileInfo(path);

            string fileName=Path.GetFileName(path);
            return new PromptItem(fileName, path, path);
        }
    }


    public class SystemLinkResolver : IExtensionResolver
    {


        IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();

        public PromptItem BuildPrompt(string path)
        {

            FileInfo file = new FileInfo(path);

            if (file.Extension.ToLower() != ".lnk")
                throw new ArgumentException("specified file isn't link file");

            IWshRuntimeLibrary.WshShortcut link = shell.CreateShortcut(file.FullName) as IWshRuntimeLibrary.WshShortcut;
            return new PromptItem(
                file.Name.Substring(0, file.Name.Length - 4),
                link.TargetPath,
                file.FullName
            );



        }
    }
}
