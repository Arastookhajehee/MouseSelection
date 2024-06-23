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
using Rhino;
using Rhino.Geometry;

namespace MouseSelection.Mouse
{
    public class DoubleClick : Rhino.UI.MouseCallback
    {
        public GH_Component gh_comp { get; set; }
        public GH_Document gh_doc { get; set; }

        public List<TimberBranch> branches { get; set; }

        public List<TimberBranch> selectionList { get; set; }
        //public TimberBranch secondarySelection { get; set; }
        public Point3d downPnt { get; set; }
        public Point3d upPnt { get; set; }
        public double dragDistance { get; set; }
        public double dragAngle { get; set; }

        //public System.Timers.Timer timer { get; set; }

        public DoubleClick(GH_Document gh_doc, GH_Component gh_comp, List<TimberBranch> branches)
        {
            this.gh_doc = gh_doc;
            this.gh_comp = gh_comp;
            this.branches = branches;
            this.selectionList = new List<TimberBranch>();
        }

        public void EnableInteraction()
        {
            this.Enabled = true;
        }
        public void DisableInteractions()
        {
            this.Enabled = false;
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
                            Plane pl = branch.orientablePlanes.OrderBy(p => TimberBranch.PlanePointZValue(p, pnt)).Last();
                            nearestPlane = new Plane(pl);
                            nearestPlane.Origin = pnt + nearestPlane.ZAxis * branch.thickness / 2.0;
                            branch.buildOnPlane = nearestPlane;
                            nearestBranch = branch;

                        }

                    }
                }
            }

            if (nearestBranch == null) return;
            
            if (this.selectionList.Select(o => o.ID).Contains(nearestBranch.ID))
            {
                this.selectionList.Remove(selectionList.Where(o => o.ID == nearestBranch.ID).FirstOrDefault());
            }
            else
            {
                this.selectionList.Add(nearestBranch);
            } 
            while (this.selectionList.Count > 2)
            {
                this.selectionList.RemoveAt(0);
            }   

            this.gh_doc.ScheduleSolution(1, doc =>
            {
                this.gh_comp.ExpireSolution(false);
            });

        }
    }
}
