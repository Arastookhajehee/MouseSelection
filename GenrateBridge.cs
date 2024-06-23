using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using BonsaiInstallation;

namespace MouseSelection
{
    public class GenrateBridge : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GenrateBridge class.
        /// </summary>
        public GenrateBridge()
          : base("GenrateBridge", "bridge",
              "Description",
              "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("branches", "branches", "branches", GH_ParamAccess.list);
            pManager.AddPlaneParameter("plane1", "plane1", "plane1", GH_ParamAccess.item);
            pManager.AddPlaneParameter("plane2", "plane2", "plane2", GH_ParamAccess.item);
            pManager.AddGenericParameter("selection1", "selection1", "selection1", GH_ParamAccess.item);
            pManager.AddGenericParameter("selection2", "selection2", "selection2", GH_ParamAccess.item);
            pManager.AddTextParameter("user", "user", "user", GH_ParamAccess.item);
            pManager.AddColourParameter("color", "color", "color", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ADD", "ADD", "ADD", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("preivew", "preivew", "preivew brep", GH_ParamAccess.list);
            pManager.AddTextParameter("serliazed", "serliazed", "serliazed", GH_ParamAccess.item);
            pManager.AddGenericParameter("TBranch", "TBranch", "TBranch", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<TimberBranch> branches = new List<TimberBranch>();
            Plane plane1 = new Plane();
            Plane plane2 = new Plane();
            TimberBranch selection1 = null;
            TimberBranch selection2 = null;
            string user = "";
            System.Drawing.Color color = System.Drawing.Color.Black;
            
            bool ADD = false;

            if (!DA.GetDataList(0, branches)) return;
            if (!DA.GetData(1, ref plane1)) return;
            if (!DA.GetData(2, ref plane2)) return;
            if (!DA.GetData(3, ref selection1)) return;
            if (!DA.GetData(4, ref selection2)) return;
            if (!DA.GetData(5, ref user)) return;
            if (!DA.GetData(6, ref color)) return;
            if (!DA.GetData(7, ref ADD)) return;

            


            var branch1 = selection1.BuildOn(plane1, user, color, "virtual");
            var branch2 = selection2.BuildOn(plane2, user, color, "virtual");

            branches.Add(branch1);
            branches.Add(branch2);

            Brep[] previewBreps = { branch1.brep, branch2.brep };
            TimberBranch[] previewTBrances = { branch1, branch2 };

            if (!ADD) 
            {
                DA.SetDataList(0, previewBreps);
                DA.SetDataList(2, previewTBrances);
                return; 
            }
            
            DA.SetData(1, TimberBranch.ListToJson(branches));

        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("D8BFA1F4-3A23-4560-9B29-0BA7F754B6B2"); }
        }
    }
}