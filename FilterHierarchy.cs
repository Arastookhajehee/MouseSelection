using System;
using System.Collections.Generic;
using System.Linq;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MouseSelection
{
    public class FilterHierarchy : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the FilterHierarchy class.
        /// </summary>
        public FilterHierarchy()
          : base("FilterHierarchy", "Nickname",
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
            pManager.AddGenericParameter("selection", "selection", "selection", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("branches", "branches", "branches", GH_ParamAccess.list);
            pManager.AddBrepParameter("breps", "breps", "breps", GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            
            List<TimberBranch> tree = new List<TimberBranch>();
            List<TimberBranch> selection = new List<TimberBranch>();

            DA.GetDataList(0, tree);
            DA.GetDataList(1, selection);

            List<Guid> filtered = new List<Guid>();

            foreach (var item in selection)
            {
                GetChildrenIDs(item, tree, filtered);
            }

            filtered = filtered.Distinct().ToList();

            List<TimberBranch> final = new List<TimberBranch>();
            foreach (var item in filtered)
            {
                TimberBranch branch = tree.Where(x => x.ID == item).First();
                final.Add(branch);
            }

            if (final.Count == 0) return;

            DA.SetDataList(0, final);
            DA.SetDataList(1, final.Select(x => x.brep).ToList());

        }

        private bool GetChildrenIDs(TimberBranch branch, List<TimberBranch> tree, List<Guid> childrenIDs)
        {
            if (branch == null) return false;
            childrenIDs.Add(branch.ID);
            foreach (Guid childID in branch.childIDs)
            {
                try
                {
                    TimberBranch child = tree.Where(x => x.ID == childID).First();
                    if (child != null) childrenIDs.Add(childID);
                    GetChildrenIDs(child, tree, childrenIDs);
                }
                catch
                {
                }
            }
            return true;
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
            get { return new Guid("1877CDAC-2497-4E1B-81B7-B315CCE23EE1"); }
        }
    }
}