using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Linq;
using BonsaiInstallation;

namespace MouseSelection
{
    public class FilterByState : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FilterByState class.
        /// </summary>
        public FilterByState()
          : base("FilterByState", "Nickname",
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
            pManager.AddTextParameter("state", "state", "state", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("branches", "branches", "branches",GH_ParamAccess.list);

            pManager.AddGenericParameter("built", "built", "built", GH_ParamAccess.list);
            pManager.AddGenericParameter("potential", "buildable", "buildable", GH_ParamAccess.list);
            pManager.AddGenericParameter("virtual", "virtual", "virtual", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<TimberBranch> tree = new List<TimberBranch>();
            string state = "";

            DA.GetDataList(0, tree);
            DA.GetData(1, ref state);

            var filtered = tree.Where(x => x.state == state).ToList();
            DA.SetDataList(0, filtered);


        }

        // a function to find buildable TimberBrances
        // given a selected TimberBranch, look for its children if they virtual they are buildable
        // if they are physical look in their children for virtuals
        List<TimberBranch> GetBuildable(List<TimberBranch> tree, TimberBranch item)
        {
            return null;
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
            get { return new Guid("CBC69BC3-C14C-4024-87B1-6C896961EF85"); }
        }
    }
}