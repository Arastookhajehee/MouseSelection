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

namespace MouseSelection
{
    public class MouseSelection : GH_Component
    {
        int counter = 0;
        MouseInteraction mouse;

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
            DA.GetData(0, ref run);
            DA.GetDataList(1, branches);

            if (!run) 
            {
                counter = 0;
                if (mouse != null) mouse.DisableInteractions();
                return;
            }
            if (counter == 0) 
            {
                mouse = new MouseInteraction(this.OnPingDocument(),this, branches);
                mouse.EnableInteraction();
            }

            mouse.branches = branches;
            DA.SetData(0, mouse.selectedBranch);
            DA.SetData(1, mouse.secondarayBranch);


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

    public class MouseInteraction : Rhino.UI.MouseCallback
    {
        public GH_Component gh_comp  { get; set; }
        public GH_Document gh_doc { get; set; }
        public List<TimberBranch> branches { get; set; }

        public TimberBranch selectedBranch { get; set; }
        public TimberBranch secondarayBranch { get; set; }

        public Point3d downPnt { get; set; }
        public Point3d upPnt { get; set; }
        public double dragDistance { get; set; }
        public double dragAngle { get; set; }

        //public System.Timers.Timer timer { get; set; }


        public MouseInteraction(GH_Document gh_doc, GH_Component gh_comp, List<TimberBranch> branches)
        {
            this.gh_doc = gh_doc;
            this.gh_comp = gh_comp;
            this.branches = branches;
            this.selectedBranch = null;
            this.secondarayBranch = null;
        }

        public void EnableInteraction()
        {
            this.Enabled = true;
            //this.timer = new System.Timers.Timer(100);
            //this.timer.Elapsed += Timer_Elapsed;
            //this.timer.AutoReset = true;
            //this.timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            this.gh_doc.ScheduleSolution(1, doc => 
            {
                this.gh_comp.ExpireSolution(false);
            });
        }

        public void DisableInteractions()
        {
            this.Enabled = false;
            //this.timer.Stop();
            //this.timer.Elapsed -= Timer_Elapsed;
            //this.timer.AutoReset = false;
            //this.timer.Dispose();
        }

        protected override void OnMouseDown(MouseCallbackEventArgs e)
        {

            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            if (this.selectedBranch == null) return;

            bool controlKeyDown = e.CtrlKeyDown;

            if (!controlKeyDown) this.downPnt = GetCPlaneSelectionPoint(e, this.selectedBranch.buildOnPlane);
        }

        private static Point3d GetCPlaneSelectionPoint(MouseCallbackEventArgs e, Plane cPlane)
        {
            // get the intersection of the view's cplane with the view's frustum line
            var rhinoDoc = Rhino.RhinoDoc.ActiveDoc;
            var view = rhinoDoc.Views.ActiveView;

            view.ActiveViewport.GetFrustumLine(e.ViewportPoint.X, e.ViewportPoint.Y, out Line line);

            Intersection.LinePlane(line, cPlane, out double param);

            return line.PointAt(param);
        }

        // mouse up
        protected override void OnMouseUp(MouseCallbackEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            if (this.selectedBranch == null) return;
            if (e.CtrlKeyDown) return;

            Plane cPlane = this.selectedBranch.buildOnPlane;

            this.upPnt = GetCPlaneSelectionPoint(e, cPlane);

            // calculate the distance and angle between the two points
            this.dragDistance = this.downPnt.DistanceTo(this.upPnt);
            this.dragAngle = Vector3d.VectorAngle(this.upPnt - cPlane.Origin, this.downPnt - cPlane.Origin, cPlane);

            if (dragDistance < 1) return;
            // if the down point is close to the plane origin, translate the plane ins the direction of the drag
            double dragThreshold = 20;

            double distanceToPlane = this.downPnt.DistanceTo(cPlane.Origin);

            if (distanceToPlane < dragThreshold)
            {
                Plane tempPlane = new Plane(cPlane);

                tempPlane.Translate(this.upPnt - this.downPnt);

                this.selectedBranch.buildOnPlane = tempPlane;
            }
            else
            {
                // if the down point is far from the plane origin, rotate the plane around the plane origin
                Plane tempPlane = new Plane(cPlane);
                tempPlane.Rotate(-this.dragAngle, tempPlane.ZAxis, tempPlane.Origin);
                this.selectedBranch.buildOnPlane = tempPlane;   
            }

            this.gh_doc.ScheduleSolution(1, doc =>
            {
                this.gh_comp.ExpireSolution(false);
            });

        }

        // double click
        protected override void OnMouseDoubleClick(MouseCallbackEventArgs e)
        {
            
            // get the frustum line
            var rhinoDoc = Rhino.RhinoDoc.ActiveDoc;
            var view = rhinoDoc.Views.ActiveView;

            bool controlDown = e.CtrlKeyDown;

            view.ActiveViewport.GetFrustumLine(e.ViewportPoint.X, e.ViewportPoint.Y, out Line line);

            TimberBranch nearestBranch = null;
            Plane nearestPlane = Plane.Unset;
            double minDistance = double.MaxValue;
            foreach (var branch in this.branches)
            {
                Mesh meshItem = branch.meshBox;
                Point3d[] points = Intersection.MeshLine(meshItem, line, out int[] faceIndices);
                if (points != null && points.Length > 0)
                {
                    for (int i = 0; i < points.Length; i++)
                    {
                        Point3d pnt = points[i];
                        int faceIndex = faceIndices[i];

                        double distance = pnt.DistanceTo(line.To);
                        if (distance < minDistance)
                        {
                            minDistance = distance;


                            // get the plane from the orientation planes that has the largest z local value for the point
                            Plane pl = branch.orientablePlanes.OrderBy(p => TimberBranch.PlanePointZValue(p,pnt)).Last();
                            nearestPlane = new Plane(pl);
                            nearestPlane.Origin = pnt + nearestPlane.ZAxis * branch.thickness /2.0;
                            branch.buildOnPlane = nearestPlane;
                            nearestBranch = branch;

                        }

                    }
                }
            }

            if (nearestBranch == null) return;
            if (controlDown)
            {
                if (this.secondarayBranch == null)
                {
                    this.secondarayBranch = nearestBranch;
                }
                else if (this.secondarayBranch.ID == nearestBranch.ID)
                {
                    this.secondarayBranch = null;
                }
                else
                {
                    this.secondarayBranch = nearestBranch;
                }
            }

            else this.selectedBranch = nearestBranch;

            if (!controlDown) this.downPnt = GetCPlaneSelectionPoint(e, nearestBranch.buildOnPlane);

            this.gh_doc.ScheduleSolution(1, doc =>
            {
                this.gh_comp.ExpireSolution(false);
            });

        }
    }

}