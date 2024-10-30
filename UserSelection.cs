using System;
using System.Collections.Generic;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MouseSelection
{
    public class UserSelection : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the UserSelection class.
        /// </summary>
        public UserSelection()
          : base("UserSelection", "Nickname",
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
            pManager.AddTextParameter("user", "user", "user", GH_ParamAccess.item);
            pManager.AddColourParameter("color", "color", "color", GH_ParamAccess.item);
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

            List<TimberBranch> list = new List<TimberBranch>();
            string user = "";
            System.Drawing.Color color = System.Drawing.Color.Black;

            DA.GetDataList(0, list);
            DA.GetData(1, ref user);
            DA.GetData(2, ref color);

            List<TimberBranch> selected = new List<TimberBranch>();
            foreach (var item in list)
            {
                // new TimberBranch with new user name
                // make the color 30% lighter
                color = System.Drawing.Color.FromArgb(
                    (int)(color.R * 1.3) > 255 ? 255 : (int)(color.R * 1.3),
                    (int)(color.G * 1.3) > 255 ? 255 : (int)(color.G * 1.3),
                    (int)(color.B * 1.3) > 255 ? 255 : (int)(color.B * 1.3));
                TimberBranch newBranch = new TimberBranch(item.placementPlane, user, color, "selected");
                selected.Add(newBranch);
            }

            DA.SetData(0, TimberBranch.ListToJson(selected));
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
            get { return new Guid("1A214FD1-5DC8-4D21-BBA6-81222FFBCE5F"); }
        }
    }
}