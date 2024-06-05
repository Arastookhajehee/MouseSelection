using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace MouseSelection
{
    public class MouseSelectionInfo : GH_AssemblyInfo
    {
        public override string Name => "MouseSelection";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("ac65139a-d0b2-4444-8862-00ffd4f4781c");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}