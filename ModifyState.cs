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
            pManager.AddTextParameter("status", "status", "status", GH_ParamAccess.item); 
            pManager.AddBooleanParameter("modify", "mod", "modify", GH_ParamAccess.item);
            pManager.AddNumberParameter("appShiftX", "appShiftX", "appShiftX", GH_ParamAccess.item);
            pManager.AddNumberParameter("appShiftY", "appShiftY", "appShiftY", GH_ParamAccess.item);
            pManager.AddNumberParameter("appShiftZ", "appShiftZ", "appShiftZ", GH_ParamAccess.item);
            pManager.AddNumberParameter("glueShiftX", "glueShiftX", "glueShiftX", GH_ParamAccess.item);
            pManager.AddNumberParameter("glueShiftY", "glueShiftY", "glueShiftY", GH_ParamAccess.item);
            pManager.AddNumberParameter("glueShiftZ", "glueShiftZ", "glueShiftZ", GH_ParamAccess.item);
            pManager.AddNumberParameter("pickShift", "pickShift", "pickShift", GH_ParamAccess.item);
            pManager.AddNumberParameter("orientation", "orientation", "orientation", GH_ParamAccess.item);  
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
            List<TimberBranch> newList = new List<TimberBranch>();
            TimberBranch selection = null;
            string state = "";
            bool modify = false;

            double appShiftX = 0.0;
            double appShiftY = 0.0;
            double appShiftZ = 0.0;
            double glueX = 0.0;
            double glueY = 0.0;
            double glueZ = 0.0;
            double pickShift = 0.0;
            double orientation = 0;


            DA.GetDataList(0, list);
            DA.GetData(1, ref selection);
            DA.GetData(2, ref state);
            DA.GetData(3, ref modify);
            DA.GetData(4, ref appShiftX);
            DA.GetData(5, ref appShiftY);
            DA.GetData(6, ref appShiftZ);
            DA.GetData(7, ref glueX);
            DA.GetData(8, ref glueY);
            DA.GetData(9, ref glueZ);
            DA.GetData(10, ref pickShift);
            DA.GetData(11, ref orientation);

            if (!modify) return;

            if (selection == null) return;

            for (int i = list.Count - 1; i > -1; i--)
            {
                TimberBranch branch = list[i];
                if (branch.branch_id == selection.branch_id)
                {
                    string modified = branch.modification_time_stamp;
                    TimberBranch duplicate = new TimberBranch(branch, branch.placement_plane);
                    duplicate.modification_time_stamp = modified;

                    if (state.Equals("physical"))
                    {
                        string built = TimberBranch.GetTimeStamp();
                        duplicate.physical_time_stamp = built;
                    }
                    if (state.Equals("fabricated")) 
                    {
                        string fabricated = TimberBranch.GetTimeStamp();
                        duplicate.fabricated_time_stamp = fabricated;
                    }
                    if (state.Equals("fabrication_fail"))
                    {
                        string fail = TimberBranch.GetTimeStamp();
                        duplicate.fabrication_fail = fail;
                    }

                    duplicate.status = state;


                    double[] shiftValues = { appShiftX, appShiftY, appShiftZ, pickShift, orientation};
                    double[] glueValues = { glueX, glueY, glueZ };
                    // turn the values into a csv stirng
                    duplicate.placement_shift = shiftValues.Length == 0 ? "" : string.Join(",", shiftValues);
                    duplicate.placement_glue_shift = glueValues.Length == 0 ? "" : string.Join(",", glueValues);


                    newList.Add(duplicate);
                }
                else
                {
                    newList.Add(branch);
                }
                
            }


            DA.SetData(0, TimberBranch.ListToJson(newList));
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
        /// Gets the unique branch_id for this component. Do not change this branch_id after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("5E60D9FE-320F-4D0D-A538-93B47041659F"); }
        }
    }
}