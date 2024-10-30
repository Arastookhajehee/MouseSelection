using System;
using System.Collections.Generic;
using System.Linq;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MouseSelection
{
    public class CategorizeFabricables : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the CategorizeFabricables class.
        /// </summary>
        public CategorizeFabricables()
          : base("CategorizeFabricables", "Nickname",
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
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("built", "built", "built", GH_ParamAccess.list);
            pManager.AddGenericParameter("potential", "buildable", "buildable", GH_ParamAccess.list);
            pManager.AddGenericParameter("notYet", "notYet", "notYet", GH_ParamAccess.list);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<TimberBranch> tree = new List<TimberBranch>();

            DA.GetDataList(0, tree);

            var built = tree.Where(x => x.state == "physical").ToList();
            
            List<TimberBranch> buildables = new List<TimberBranch>();
            List<TimberBranch> notYet = new List<TimberBranch>();
            foreach (var item in tree)
            {
                string state = item.state;
                if (state == "physical") continue;
                
                TimberBranch parent = tree.Where(o => o.ID == item.ID).FirstOrDefault();
                if (parent.state == "physical")
                {
                    buildables.Add(item);
                }
                else
                {
                    notYet.Add(item);
                }

            }

            DA.SetDataList(0, built);
            DA.SetDataList(1, buildables);
            DA.SetDataList(2, notYet);
        }

        bool IsBuilt(TimberBranch branch)
        {
            return branch.state == "physical";
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
            get { return new Guid("4D98B297-420A-4BC8-AEBB-3B0C387FB4D3"); }
        }
    }
}