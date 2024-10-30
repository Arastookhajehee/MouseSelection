using System;
using System.Collections.Generic;
using System.Linq;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MouseSelection
{
    public class FilterByID : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FilterByID class.
        /// </summary>
        public FilterByID()
          : base("FilterByID", "Nickname",
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
            pManager.AddTextParameter("id", "id", "id", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
                pManager.AddGenericParameter("branches", "branches", "branches", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<TimberBranch> tree = new List<TimberBranch>();
            string id = "";

            DA.GetDataList(0, tree);
            DA.GetData(1, ref id);

            Guid guid = Guid.Parse(id);

            var filtered = tree.Where(x => x.ID == guid).ToList();

            DA.SetDataList(0, filtered);
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
            get { return new Guid("2264DAD8-EFC3-4D4B-9840-A189EFA80D8D"); }
        }
    }
}