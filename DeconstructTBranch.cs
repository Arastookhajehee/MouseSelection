using System;
using System.Collections.Generic;
using System.Linq;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MouseSelection
{
    public class DeconstructTBranch : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeleteTBranch class.
        /// </summary>
        public DeconstructTBranch()
          : base("DeconstructTBranch", "DeTBranch",
              "DeconstructTBranch",
              "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("TBranch", "TBranch", "TBranch", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("branch_id", "branch_id", "branch_id", GH_ParamAccess.item);
            pManager.AddPlaneParameter("placement_plane", "placement_plane", "placement_plane", GH_ParamAccess.item);
            pManager.AddPlaneParameter("orientable_planes", "orientable_planes", "orientable_planes", GH_ParamAccess.list);
            pManager.AddCurveParameter("borders","borders","borders",GH_ParamAccess.list);
            pManager.AddTextParameter("user_name", "user_name", "user_name", GH_ParamAccess.item);
            pManager.AddColourParameter("color", "color", "color", GH_ParamAccess.item);
            pManager.AddTextParameter("state", "state", "state", GH_ParamAccess.item);
            pManager.AddGenericParameter("parents","parents","parents",GH_ParamAccess.list);
            pManager.AddGenericParameter("children","children","children",GH_ParamAccess.list);
            pManager.AddBrepParameter("brep","brep","brep",GH_ParamAccess.item);
            pManager.AddBooleanParameter("selected","selected","selected",GH_ParamAccess.item);
            pManager.AddPlaneParameter("build_on_plane","build_on_plane","build_on_plane",GH_ParamAccess.item);
            pManager.AddNumberParameter("shift","shift","shift",GH_ParamAccess.list);
            pManager.AddNumberParameter("glueShift","glueShift","glueShift",GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            TimberBranch tBranch = null;
            DA.GetData(0, ref tBranch);

            if (tBranch == null) return;

            DA.SetData(0, tBranch.branch_id);
            DA.SetData(1, tBranch.placement_plane);
            DA.SetDataList(2, tBranch.orientable_planes);
            List<Curve> borders = new List<Curve>();
            for (int i = 0; i < tBranch.orientable_planes.Count; i++)
            {
                borders.Add(tBranch.GetBorderFace(i));
            }
            DA.SetDataList(3, borders);
            DA.SetData(4, tBranch.user_name);
            DA.SetData(5, tBranch.color);
            DA.SetData(6, tBranch.state);
            DA.SetDataList(7, tBranch.parent_ids);
            DA.SetDataList(8, tBranch.child_ids);
            DA.SetData(9, tBranch.brep);
            DA.SetData(10, tBranch.selected);
            DA.SetData(11, tBranch.build_on_plane);
            DA.SetDataList(12, string.IsNullOrEmpty(tBranch.placement_shift) ? null : 
                tBranch.placement_shift.Split(',').Select(o => Convert.ToDouble(o)).ToList());
            DA.SetDataList(13, string.IsNullOrEmpty(tBranch.placement_glue_shift) ? null : 
                tBranch.placement_glue_shift.Split(',').Select(o => Convert.ToDouble(o)).ToList());
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
            get { return new Guid("D26DDFC2-41BF-473D-894B-775BA8110B92"); }
        }
    }
}