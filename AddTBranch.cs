using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MouseSelection
{
    public class AddTBranch : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the AddTBranch class.
        /// </summary>
        public AddTBranch()
          : base("AddTBranch", "AddTBranch",
              "AddTBranch",
              "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddGenericParameter("branches", "branches", "branches", GH_ParamAccess.list);
            pManager.AddGenericParameter("selection", "selection", "selection", GH_ParamAccess.item);
            pManager.AddTextParameter("user", "user", "user", GH_ParamAccess.item);
            pManager.AddColourParameter("color", "color", "color", GH_ParamAccess.item);
            pManager.AddBooleanParameter("ADD", "ADD", "ADD", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("preivew", "preivew", "preivew brep", GH_ParamAccess.item);
            pManager.AddCircleParameter("circle", "circle", "circle", GH_ParamAccess.item);
            pManager.AddTextParameter("serliazed", "serliazed", "serliazed", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<TimberBranch> list = new List<TimberBranch>();
            TimberBranch selection = null;
            string user = "";
            System.Drawing.Color color = System.Drawing.Color.White;
            bool ADD = false;
            DA.GetDataList(0, list);
            DA.GetData(1, ref selection);
            DA.GetData(2, ref user);
            DA.GetData(3, ref color);
            DA.GetData(4, ref ADD);

            if (!ADD)
            {
                if (selection == null) return;
                TimberBranch branch = new TimberBranch(selection.buildOnPlane, user, color, "new");
                DA.SetData(0, branch.brep);
                DA.SetData(1, new Circle(selection.buildOnPlane, 20));
                return;
            }

            var newOne = selection.BuildOn(user,color,"builtOn");
            list.Add(newOne);

            DA.SetData(2, TimberBranch.ListToJson(list));
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
            get { return new Guid("E19C7C7A-F914-47B7-899D-6951DC92E95C"); }
        }
    }
}