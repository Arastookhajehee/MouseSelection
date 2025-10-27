using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using BonsaiInstallation;
using System.Drawing;
using System.Linq;

namespace MouseSelection.Communications
{
    public class SubmitPreview : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SubmitPreview class.
        /// </summary>
        public SubmitPreview()
          : base("SubmitPreview", "Nickname",
              "Description",
              "MouseSelection", "Preivew")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ws", "ws", "ws", GH_ParamAccess.item);
            pManager.AddTextParameter("user", "user", "user", GH_ParamAccess.item);
            pManager.AddColourParameter("color", "color", "color", GH_ParamAccess.item, Color.Black);
            pManager.AddGenericParameter("selecitons", "selections", "selections", GH_ParamAccess.list);
            pManager.AddPlaneParameter("previewPlanes", "previewPlanes", "previewPlanes", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PoolConnection pool = null;
            string user = "";
            Color color = Color.Black;
            List<TimberBranch> branchs = new List<TimberBranch>();
            List<Plane> previewPlanes = new List<Plane>();


            DA.GetData(0, ref pool);
            DA.GetData(1, ref user);
            DA.GetData(2, ref color);
            DA.GetDataList(3, branchs);
            DA.GetDataList(4, previewPlanes);

            var ids = branchs.Select(o => o.branch_id).ToList();

            WSActionDesignPreview preview = new WSActionDesignPreview(ids, user, color, previewPlanes);

            pool.ws.Send(preview.ToJson());
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
            get { return new Guid("D2AC1BB7-52BF-428B-85AC-9AEE60E787E1"); }
        }
    }
}