using System;
using System.Collections.Generic;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MouseSelection
{
    public class SerializeTBranch : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SingleSerialize class.
        /// </summary>
        public SerializeTBranch()
          : base("SerializeTBranch", "SerializeTB",
              "Serializes TBranches to Json ",
              "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("TBranches", "TBranches", "TBranches", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("json", "json", "json", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<TimberBranch> tBranches = new List<TimberBranch>();
            DA.GetDataList(0, tBranches);

            if (tBranches == null) return;

            string json = TimberBranch.ListToJson(tBranches);
            DA.SetData(0, json);
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
            get { return new Guid("B3523724-572C-46F5-8F95-3B0168BB9C0F"); }
        }
    }
}