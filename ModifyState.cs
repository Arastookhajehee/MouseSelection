using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Newtonsoft.Json;
using System.Drawing;
using System.Linq;
using BonsaiInstallation;

namespace MouseSelection
{
    public class ModifyState : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ModifyState class.
        /// </summary>
        public ModifyState()
          : base("ModifyState", "Nickname",
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
            pManager.AddGenericParameter("selection", "selection", "selection", GH_ParamAccess.item);
            pManager.AddTextParameter("state", "state", "state", GH_ParamAccess.item); 
            pManager.AddBooleanParameter("modify", "mod", "modify", GH_ParamAccess.item);
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
            TimberBranch selection = null;
            string state = "";
            bool modify = false;

            DA.GetDataList(0, list);
            DA.GetData(1, ref selection);
            DA.GetData(2, ref state);
            DA.GetData(3, ref modify);

            if (!modify) return;

            if (selection == null) return;

            // get the branch
            int index = list.FindIndex(b => b.ID == selection.ID);
            TimberBranch branch = list[index];
            if (branch == null) return;

            branch.state = state;

            list.RemoveAt(index);
            list.Insert(index, branch);

            DA.SetData(0, TimberBranch.ListToJson(list));
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
            get { return new Guid("5E60D9FE-320F-4D0D-A538-93B47041659F"); }
        }
    }
}