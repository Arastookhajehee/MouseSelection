using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using BonsaiInstallation;
using System.Linq;

namespace MouseSelection
{
    public class DeleteTBranch : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the DeleteTBranch class.
        /// </summary>
        public DeleteTBranch()
          : base("DeleteTBranch", "delTBranch",
              "Description",
              "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("tree", "tree", "tree", GH_ParamAccess.list);
            pManager.AddGenericParameter("branch", "branch", "branch", GH_ParamAccess.item);
            pManager.AddBooleanParameter("deleteBranch","del", "delete Branch and all children", GH_ParamAccess.item);
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
            List<TimberBranch> tree = new List<TimberBranch>();
            TimberBranch branch = null;
            bool delete = false;

            DA.GetDataList(0, tree);
            DA.GetData(1, ref branch);
            DA.GetData(2, ref delete);

            if (!delete) return;

            // all deletable IDs
            List<Guid> childrenIDs = GetChildrenIDs(branch, tree);
            childrenIDs.Add(branch.ID);
            // make the list unique
            childrenIDs = childrenIDs.Distinct().ToList();

            //tree.Remove(tree.Where(x => x.ID == branch.ID).First());
            foreach (Guid item in childrenIDs)
            {
                tree.Remove(tree.Where(x => x.ID == item).First());
            }
            
            DA.SetData(0, TimberBranch.ListToJson(tree));
        }

        // a recursive function to get all the IDs children of a branch
        private List<Guid> GetChildrenIDs(TimberBranch branch, List<TimberBranch> tree)
        {
            List<Guid> childrenIDs = new List<Guid>();
            foreach (Guid childID in branch.childIDs)
            {
                TimberBranch child = tree.Where(x => x.ID == childID).First();
                if (child != null) childrenIDs.Add(childID);
                childrenIDs.AddRange(GetChildrenIDs(child, tree));
            }
            return childrenIDs;
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
            get { return new Guid("DEAE65DD-9E67-447C-9421-13C9D003D5D3"); }
        }
    }
}