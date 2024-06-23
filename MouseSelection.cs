using Grasshopper;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Timers;
using Newtonsoft.Json;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using BonsaiInstallation;
using MouseSelection.Mouse;

namespace MouseSelection
{
    public class MouseSelection : GH_Component
    {
        int counter = 0;
        public DoubleClick mouse;

        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public MouseSelection()
          : base("MouseSelection", "MouseSelection",
            "MouseSelection",
            "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "Run", GH_ParamAccess.item);
            pManager.AddGenericParameter("branches", "branches", "branches", GH_ParamAccess.list);
            pManager.AddBooleanParameter("reset", "reset", "reset", GH_ParamAccess.item);
            pManager.AddBooleanParameter("modifyMode","modifyMode","modifyMode",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("selectedBranch", "selBranch", "selBranch", GH_ParamAccess.item);
            pManager.AddGenericParameter("SecondaryBranch","secBranch", "secBranch", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            List<TimberBranch> branches = new List<TimberBranch>();
            bool reset = false;
            bool modifyMode = false;

            DA.GetData(0, ref run);
            DA.GetDataList(1, branches);
            DA.GetData(2, ref reset);
            DA.GetData(3, ref modifyMode);

            if (reset)
            {
                if (mouse == null) return;
                mouse.selectionList = new List<TimberBranch>();
                return;
            }

            if (!run) 
            {
                counter = 0;
                if (mouse != null) mouse.DisableInteractions();
                return;
            }
            if (counter == 0) 
            {
                mouse = new DoubleClick(this.OnPingDocument(),this, branches);
                mouse.EnableInteraction();
            }

            //Guid firstSelID = mouse.selectionList. mouse.branches.Where(b => b.ID == mouse.selectionList[0].ID).Select(b => b.ID).FirstOrDefault();
            //Guid secondSelID = mouse.branches.Where(b => b.ID == mouse.selectionList[1].ID).Select(b => b.ID).FirstOrDefault();

            mouse.branches = branches;

            if (modifyMode) if (mouse.selectionList.Count > 1) mouse.selectionList.RemoveAt(0);

            var firstSelection = mouse.selectionList.Count > 0 ? mouse.selectionList[0] : null;
            var secondSelection = mouse.selectionList.Count > 1 ? mouse.selectionList[1] : null;

            DA.SetData(0, firstSelection);
            DA.SetData(1, secondSelection);


            counter = counter > 100 ? 5 : counter + 1;
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("28aeeaa0-b4b4-4e5e-8219-011adc8fa55e");
    }
   
}