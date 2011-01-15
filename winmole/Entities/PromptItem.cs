using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace winmole.Entities
{
    public class PromptItem
    {

        public string Name { get; set; }

        public string FullPath { get; set; }

        public string TargetPath { get; set; }


        public PromptItem(string path)
        {
            // TODO: Complete member initialization
            Name = FullPath = TargetPath = path;
        }

        /// <summary>
        /// constructs the PromptItem
        /// </summary>
        /// <param name="name">Item file name</param>
        /// <param name="target">Item pathh which points to</param>
        /// <param name="fullpath">Full path to original file</param>
        public PromptItem(string name, string target, string fullpath)
        {
            Name = name;
            TargetPath= target;
            FullPath= fullpath;
        }

        public PromptItem()
        {
            // TODO: Complete member initialization
        }
    }
}
