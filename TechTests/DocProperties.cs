using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TechTests
{
    class DocProperties
    {
        public string Name { get; set; }
        public string TargetPath { get; set; }
        public string OriginalPath { get; set; }



        public DocProperties(string lnk, string targetPath, string file)
        {
            // TODO: Complete member initialization
            this.Name = lnk;
            this.TargetPath = targetPath;
            this.OriginalPath = file;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(200);
            sb.Append(Name);
            sb.Append("->");
            sb.Append(TargetPath);


            return sb.ToString();
        }
    }
}
