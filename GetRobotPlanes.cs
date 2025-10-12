using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MouseSelection
{
    public class GetRobotPlanes : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the GetRobotPlanes class.
        /// </summary>
        public GetRobotPlanes()
          : base("GetRobotPlanes", "Nickname",
              "Description",
              "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Branch","branch","branch",GH_ParamAccess.item);
            pManager.AddIntegerParameter("index","index","index",GH_ParamAccess.item);
            pManager.AddNumberParameter("shift","shift","shift",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("plane","plane","plane",GH_ParamAccess.item);
            pManager.AddBooleanParameter("endPlane","endPlane","endPlane",GH_ParamAccess.item);
            pManager.AddNumberParameter("shift","shift","shift",GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            TimberBranch branch = null;
            int index = 0;
            double shift = 0.0;

            DA.GetData(0, ref branch);
            DA.GetData(1, ref index);
            DA.GetData(2, ref shift);

            var orientables = branch.orientable_planes.Select(p => new Plane(p)).ToList();

            

            

            Plane extraPlane = new Plane(branch.placement_plane);
            Plane extraPlane2 = new Plane(branch.placement_plane);

            extraPlane.Rotate(Math.PI / 2, branch.placement_plane.XAxis, branch.placement_plane.Origin);
            extraPlane2.Rotate(-Math.PI / 2, branch.placement_plane.XAxis, branch.placement_plane.Origin);
            extraPlane.Translate(extraPlane.ZAxis * (branch.stick_length / 2.0 - branch.stick_thickness/2.0));
            extraPlane2.Translate(extraPlane2.ZAxis * (branch.stick_length / 2.0 - branch.stick_thickness/2.0));

            for (int i = 0; i < 4; i++)
            {
                Plane tempPlane = new Plane(extraPlane);
                tempPlane.Rotate(i * Math.PI / 2, tempPlane.ZAxis, tempPlane.Origin);
                orientables.Add(tempPlane);
            }
            for (int i = 0; i < 4; i++)
            {
                Plane tempPlane = new Plane(extraPlane2);
                tempPlane.Rotate(i * Math.PI / 2, tempPlane.ZAxis, tempPlane.Origin);
                orientables.Add(tempPlane);
            }

            Plane selectedPlane = orientables[index];

            bool isEndPlane = index > 3;
            if (!isEndPlane) selectedPlane.Translate(selectedPlane.YAxis * shift);


            DA.SetData(0,selectedPlane);
            DA.SetData(1, isEndPlane);
            DA.SetData(2, shift);
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
            get { return new Guid("26CCBF71-D29B-4043-A78A-07A783162A59"); }
        }
    }
}