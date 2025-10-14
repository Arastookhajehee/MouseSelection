using System;
using System.Collections.Generic;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;
using WebSocketSharp;

namespace MouseSelection.Communications
{
    public class SubmitAddTbranch : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SubmitAddTbranch class.
        /// </summary>
        public SubmitAddTbranch()
          : base("SubmitAddTbranch", "submit",
              "Description",
              "MouseSelection", "Pool")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ws", "ws", "ws", GH_ParamAccess.item);
            pManager.AddGenericParameter("branch", "branch", "branch", GH_ParamAccess.list);
            pManager.AddBooleanParameter("submit", "sbt", "submit", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("status", "status", "status", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PoolConnection poolConn = null;
            List<TimberBranch> branches = new List<TimberBranch>();
            bool submit = false;

            DA.GetData(0, ref poolConn);
            DA.GetDataList(1, branches);
            DA.GetData(2, ref submit);

            if (!submit) return;


            try
            {
                if (poolConn.ws == null || branches.Count < 1) return;
                WSActionCreateSingle action = new WSActionCreateSingle(branches);
                string message = action.ToJson();
                poolConn.ws.Send(message);   
                DA.SetData(0, "sent");
            }
            catch (Exception e)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                DA.SetData(0, e.Message);
                throw;
            }

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
            get { return new Guid("6A0A09EE-17A3-44D4-8670-5595E9239BF3"); }
        }
    }
}