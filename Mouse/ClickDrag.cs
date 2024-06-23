using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry.Intersect;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Rhino.Geometry;

namespace MouseSelection.Mouse
{
    public class ClickDrag : Rhino.UI.MouseCallback
    {
        public GH_Component gh_comp { get; set; }
        public GH_Document gh_doc { get; set; }
        public List<TimberBranch> branches { get; set; }

        public TimberBranch selectedBranch { get; set; }
        public TimberBranch secondarayBranch { get; set; }

        public Plane mousePlane { get; set; } 

        public Point3d downPnt { get; set; }
        public Point3d upPnt { get; set; }
        public double dragDistance { get; set; }
        public double dragAngle { get; set; }

        public bool doubleTimber { get; set; } 
        public Transform dragTransform { get; set; }
        public Transform rotationTransform { get; set; }

        public bool modificationMode { get; set; }

        //public System.Timers.Timer timer { get; set; }


        public ClickDrag(bool doubleTimber, bool modificationMode, Plane mousePlane, GH_Document gh_doc, GH_Component gh_comp, List<TimberBranch> branches, TimberBranch selectedBranch, TimberBranch secondarayBranch)
        {
            this.doubleTimber = doubleTimber;
            this.gh_doc = gh_doc;
            this.gh_comp = gh_comp;
            this.branches = branches;
            this.selectedBranch = selectedBranch;
            this.secondarayBranch = secondarayBranch;
            this.mousePlane = mousePlane;
            this.modificationMode = modificationMode;
        }

        public void EnableInteraction()
        {
            this.Enabled = true;
        }

        public void DisableInteractions()
        {
            this.Enabled = false;
        }

        protected override void OnMouseDown(MouseCallbackEventArgs e)
        {

            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;

            if (this.doubleTimber)
            {
                if (this.selectedBranch == null || this.secondarayBranch == null) return;
            }
            else
            {
                if (this.selectedBranch == null) return;
            }

            bool controlKeyDown = e.CtrlKeyDown;

            if (!controlKeyDown) this.downPnt = GetCPlaneSelectionPoint(e, this.mousePlane);
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

        protected override void OnMouseUp(MouseCallbackEventArgs e)
        {
            
            if (e.Button != System.Windows.Forms.MouseButtons.Left) return;
            if (this.doubleTimber)
            {
                if (this.selectedBranch == null || this.secondarayBranch == null) return;
            }
            else
            {
                if (this.selectedBranch == null) return;
            }
            //if (e.CtrlKeyDown) return;

            Plane cPlane = this.mousePlane;

            this.upPnt = GetCPlaneSelectionPoint(e, cPlane);

            // calculate the distance and angle between the two points
            this.dragDistance = this.downPnt.DistanceTo(this.upPnt);
            this.dragAngle = Vector3d.VectorAngle(this.upPnt - cPlane.Origin, this.downPnt - cPlane.Origin, cPlane);

            if (dragDistance < 1) return;
            // if the down point is close to the plane origin, translate the plane ins the direction of the drag
            double dragThreshold = 20;

            double distanceToPlane = this.downPnt.DistanceTo(cPlane.Origin);

            Plane tempPlane = Plane.Unset;

            if (distanceToPlane < dragThreshold)
            {
                tempPlane = new Plane(cPlane);

                Vector3d vec = this.upPnt - this.downPnt;

                if (modificationMode)
                {
                    tempPlane.RemapToPlaneSpace(this.downPnt, out Point3d downPntOnPlane);
                    tempPlane.RemapToPlaneSpace(this.upPnt, out Point3d upPntOnPlane);

                    int direction = downPntOnPlane.Y < upPntOnPlane.Y ? 1 : -1;

                    // project the drag vector onto the plane XAxis
                    vec = tempPlane.YAxis * dragDistance * direction;
                }


                this.dragTransform = Transform.Translation(vec);

                tempPlane.Transform(this.dragTransform);


                if (!modificationMode)
                {
                    
                    Plane tempPlacementPlane = new Plane(tempPlane);
                    tempPlacementPlane.Origin = selectedBranch.placementPlane.Origin;

                    Point3d closestPnt = tempPlacementPlane.ClosestPoint(tempPlane.Origin);

                    closestPnt = closestPnt + tempPlane.ZAxis * selectedBranch.thickness;

                    tempPlane.Origin = closestPnt;


                }



                this.mousePlane = tempPlane;

                //this.selectedBranch.buildOnPlane = tempPlane;
                //this.dragAngle = 0;
            }
            else
            {
                if (modificationMode) return;
                // if the down point is far from the plane origin, rotate the plane around the plane origin
                tempPlane = new Plane(cPlane);
                tempPlane.Rotate(-this.dragAngle, tempPlane.ZAxis, tempPlane.Origin);
                this.mousePlane = tempPlane;
                //this.dragDistance = 0;
            }


            





            this.gh_doc.ScheduleSolution(1, doc =>
            {
                this.gh_comp.ExpireSolution(false);
            });

        }

    }
}
