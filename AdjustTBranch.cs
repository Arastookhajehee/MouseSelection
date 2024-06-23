using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BonsaiInstallation;
using Grasshopper.Kernel;
using MouseSelection.Mouse;
using Rhino.Geometry;

namespace MouseSelection
{
    public class AdjustTBranch : GH_Component
    {
        int counter = 0;
        // drag mouse object
        private ClickDrag mouse;
        Guid currentMember = Guid.Empty;
        
        /// <summary>
        /// Initializes a new instance of the AdjustTBranch class.
        /// </summary>
        public AdjustTBranch()
          : base("AdjustTBranch", "Nickname",
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
            pManager.AddBooleanParameter("modify", "mod", "modify", GH_ParamAccess.item);
            pManager.AddBooleanParameter("MouseClicks", "MouseClicks", "MouseClicks", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBrepParameter("preivew", "preivew", "preivew brep", GH_ParamAccess.item);
            pManager.AddCircleParameter("circle", "circle", "circle", GH_ParamAccess.item);
            pManager.AddTextParameter("serliazed", "serliazed", "serliazed", GH_ParamAccess.item);
            pManager.AddGenericParameter("TBranch", "TBranch", "TBranch", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<TimberBranch> list = new List<TimberBranch>();
            TimberBranch selection = null;
            bool modify = false;
            bool mouseClicks = false;

            if (!DA.GetDataList(0, list)) return;
            if (!DA.GetData(1, ref selection)) return;
            if (!DA.GetData(2, ref modify)) return;
            if (!DA.GetData(3, ref mouseClicks)) return;

            if (selection == null) return;

            

            if (!mouseClicks)
            {
                counter = 0;
                if (mouse != null) mouse.DisableInteractions();
                return;
            }
            if (counter == 0)
            {
                mouse = new ClickDrag(false,true, selection.duplicatePlane, this.OnPingDocument(), this, list, selection, null);
                mouse.EnableInteraction();
                counter++;
            }
            counter = counter > 100 ? 5 : counter + 1;

            if (selection.ID != currentMember)
            {
                currentMember = selection.ID;
                mouse.selectedBranch = selection;
                mouse.mousePlane = selection.duplicatePlane;
            }

            TimberBranch branch = new TimberBranch(selection, mouse.mousePlane);

            if (!modify)
            {
                if (selection == null) return;
                DA.SetData(0, branch.brep);
                DA.SetData(1, new Circle(mouse.mousePlane, 20));
                DA.SetData(3, branch);
                return;
            }

            int index = list.FindIndex(x => x.ID == selection.ID);
            list.RemoveAt(index);
            list.Insert(index, branch);
            DA.SetData(2, TimberBranch.ListToJson(list));

            // make a System.timers.timer to update the find the MouseSelection component and set the selection to null
            // this will allow the user to select a new branch
            // the timer is 100ms, and will only run once
            // the timer will be stopped after the first run
        }

        //private void ResetSelection()
        //{
        //    System.Timers.Timer timer = new System.Timers.Timer(100);
        //    timer.AutoReset = false;
        //    timer.Elapsed += (sender, e) =>
        //    {
        //        this.OnPingDocument().ScheduleSolution(1, doc =>
        //        {

        //            MouseSelection mouseSelectionComp = (MouseSelection)this.OnPingDocument().Objects.Where(o => o is MouseSelection).FirstOrDefault();
        //            if (mouseSelectionComp != null)
        //            {
        //                mouseSelectionComp.mouse.primarySelection = null;
        //                mouseSelectionComp.mouse.secondarySelection = null;
        //                mouseSelectionComp.ExpireSolution(false);
        //            }
        //        });
        //    };
        //    timer.Start();
        //}

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
            get { return new Guid("01ED04D4-CA55-4EEC-BEAC-FAF93A697BB7"); }
        }
    }
}