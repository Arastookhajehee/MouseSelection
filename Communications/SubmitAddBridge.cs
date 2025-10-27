using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MouseSelection.Communications
{
    public class SubmitAddBridge : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SubmitAddBridge class.
        /// </summary>
        public SubmitAddBridge()
          : base("SubmitAddBridge", "Nickname",
              "Description",
              "MouseSelection", "Pool")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("ws", "ws", "ws", GH_ParamAccess.item);
            pManager.AddGenericParameter("branch", "branch", "branch", GH_ParamAccess.list);
            pManager.AddTextParameter("robot_path_1", "rp1", "robot_path_1", GH_ParamAccess.list);
            pManager.AddTextParameter("robot_path_2", "rp2", "robot_path_2", GH_ParamAccess.list);
            pManager.AddIntegerParameter("order", "order", "order", GH_ParamAccess.item);
            pManager.AddBooleanParameter("result", "res", "result", GH_ParamAccess.item);
            pManager.AddBooleanParameter("override", "ovr", "override", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("submit", "sbt", "submit", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("status", "status", "status", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            PoolConnection poolConn = null;
            List<TimberBranch> branches = new List<TimberBranch>();
            List<string> robotPath1 = new List<string>();
            List<string> robotPath2 = new List<string>();
            int order = 0;
            bool result = false;
            bool overrideResults = false;
            bool submit = false;

            DA.GetData(0, ref poolConn);
            DA.GetDataList(1, branches);
            DA.GetDataList(2, robotPath1);
            DA.GetDataList(3, robotPath2);
            DA.GetData(4, ref order);
            DA.GetData(5, ref result);
            DA.GetData(6, ref overrideResults);
            DA.GetData(7, ref submit);

            if (!submit) return;


            try
            {
                if (poolConn.ws == null || branches.Count != 2) return;
                if (!result) if (!overrideResults) return;

                branches[0].robot_path = robotPath1.Select(o => o.Split(',').Select(p => Convert.ToDouble(p)).ToArray()).ToList();
                branches[1].robot_path = robotPath2.Select(o => o.Split(',').Select(p => Convert.ToDouble(p)).ToArray()).ToList();

                if (order == 1)
                {
                    SendToServer(poolConn, branches[0]);
                    Thread.Sleep(200);
                    SendToServer(poolConn, branches[1]);
                }
                else
                {
                    SendToServer(poolConn, branches[1]);
                    Thread.Sleep(200);
                    SendToServer(poolConn, branches[0]);
                }
                DA.SetData(0, "sent");
            }
            catch (Exception e)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                DA.SetData(0, e.Message);
                throw;
            }
        }

        private static void SendToServer(PoolConnection poolConn, TimberBranch branch)
        {
            WSActionCreateSingle action = new WSActionCreateSingle(new List<TimberBranch> { branch });
            string message = action.ToJson();
            poolConn.ws.Send(message);
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
            get { return new Guid("B7FEEFB6-A8B5-4878-8762-A9B98ED17EF9"); }
        }
    }
}