using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

using BonsaiInstallation;
using MouseSelection.Communications;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Linq;
using System.Drawing;

namespace MouseSelection
{
    public class PreviewDesign : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the PreviewDesign class.
        /// </summary>
        public PreviewDesign()
          : base("PreviewDesign", "PreviewDesign",
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
            pManager.AddNumberParameter("textScale", "textScale", "textScale", GH_ParamAccess.item, 1.0);
            pManager.AddIntegerParameter("transparency", "transparency", "transparency", GH_ParamAccess.item, 150);
            pManager.AddNumberParameter("offset", "offset", "offset", GH_ParamAccess.item, 1);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPointParameter("pt", "pt", "pt", GH_ParamAccess.tree);
            pManager.AddTextParameter("user", "user", "user", GH_ParamAccess.tree);  
            pManager.AddBoxParameter("box", "box", "box", GH_ParamAccess.tree);
            pManager.AddColourParameter("color", "color", "color", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            PoolConnection pool = null;
            double textScale = 1.0;
            int transparency = 1;
            double offset = 1.0;

            DA.GetData(0, ref pool);
            DA.GetData(1, ref textScale);
            DA.GetData(2, ref transparency);
            DA.GetData(3, ref offset);

            if (pool == null) return;

            GH_Structure<GH_Point> points = new GH_Structure<GH_Point>();
            GH_Structure<GH_Colour> colors = new GH_Structure<GH_Colour>();
            GH_Structure<GH_Box> boxes = new GH_Structure<GH_Box>();
            GH_Structure<GH_String> users = new GH_Structure<GH_String>();

            Interval xInterval, yInterval, zInterval;
            CreateIntervals(out xInterval, out yInterval, out zInterval, 0);

            Interval prtxInterval, prtyInterval, prtzInterval;
            CreateIntervals(out prtxInterval, out prtyInterval, out prtzInterval, 1);

            int index = 0;
            foreach (var item in pool.designPreviews)
            {
                string designer_name = item.Key;
                var previewPoints = item.Value.preview_planes.Select(o => o.Origin);
                var parents = pool.tree.Where(o => item.Value.parent_ids.Contains(o.branch_id))
                    .Select(o => new Box(o.placement_plane, prtxInterval, prtyInterval, prtzInterval)).ToList();
                var previewBoxes = item.Value.preview_planes.Select(o => new Box(o, xInterval, yInterval, zInterval));
                Color color = item.Value.color;

                var path = new GH_Path(index);
                points.AppendRange(previewPoints.Select(o => new GH_Point(o)), path);
                boxes.AppendRange(parents.Select(o => new GH_Box(o)), path);
                boxes.AppendRange(previewBoxes.Select(o => new GH_Box(o)), path);
                users.Append(new GH_String(designer_name), path);
                colors.Append(new GH_Colour(Color.FromArgb(transparency, color.R, color.G, color.B)), path);
                index++;
            }

            DA.SetDataTree(0, points);
            DA.SetDataTree(1, users);
            DA.SetDataTree(2, boxes);
            DA.SetDataTree(3, colors);
        }

        private static void CreateIntervals(out Interval xInterval, out Interval yInterval, out Interval zInterval, double offset)
        {
            xInterval = new Interval(-TimberBranch.StickWidth / 2 - offset/2, TimberBranch.StickWidth / 2 + offset / 2);
            yInterval = new Interval(-TimberBranch.StickLength / 2 - offset / 2, TimberBranch.StickLength / 2 + offset / 2);
            zInterval = new Interval(-TimberBranch.StickThickness / 2 - offset / 2, TimberBranch.StickThickness / 2 + offset / 2);
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
            get { return new Guid("F20BC245-0C0F-4570-A1CC-D87D5A6FBB13"); }
        }
    }
}