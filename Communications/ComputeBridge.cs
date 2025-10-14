using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using Rhino.Geometry.Collections;
using Rhino.Geometry.Intersect;
using BonsaiInstallation;
using Grasshopper.Kernel.Types;
using Grasshopper;
using System.Threading.Tasks;
using System.Linq;

namespace MouseSelection.Communications
{
    public class ComputeBridge : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ComputeBridge class.
        /// </summary>
        public ComputeBridge()
          : base("ComputeBridge", "computeBridge",
              "Description",
              "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("run", "run", "run", GH_ParamAccess.item);
            pManager.AddNumberParameter("shift_a", "shift_a", "shift_a", GH_ParamAccess.item);
            pManager.AddNumberParameter("shift_b", "shift_b", "shift_b", GH_ParamAccess.item);
            pManager.AddNumberParameter("shift_ab", "shift_ab", "shift_ab", GH_ParamAccess.item);
            pManager.AddGenericParameter("branch_a", "branch_a", "branch_a",GH_ParamAccess.item);
            pManager.AddGenericParameter("branch_b", "branch_b", "branch_b",GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("branch_b", "branch_b", "branch_b", GH_ParamAccess.tree);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            bool run = false;
            double shift_a = 0;
            double shift_b = 0;
            double shift_ab = 0;
            TimberBranch branch_a = null;
            TimberBranch branch_b = null;

            DA.GetData(0, ref run);
            DA.GetData(1, ref shift_a);
            DA.GetData(2, ref shift_b);
            DA.GetData(3, ref shift_ab);
            DA.GetData(4, ref branch_a);
            DA.GetData(5, ref branch_b);


            if (!run)
            {
                return;
            }
            Tuple<TimberBranch, TimberBranch>[] results = BonsaiOps.RunParallelConfigs(branch_a.placement_plane, branch_b.placement_plane, shift_ab, shift_a, shift_b);
            DataTree<object> breps = new DataTree<object>();
            foreach (Tuple<TimberBranch, TimberBranch> pair in results)
            {
                if (pair != null)
                {
                    if (pair.Item1 != null)
                    {
                        breps.Add(pair.Item1, new GH_Path(breps.BranchCount));
                    }
                    if (pair.Item2 != null)
                    {
                        breps.Add(pair.Item2, new GH_Path(breps.BranchCount - 1));
                    }
                }
            }
            DA.SetDataTree(0,breps);
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
            get { return new Guid("ADC29C9E-C109-4587-9B26-62A42FEE2B6B"); }
        }

        public class BonsaiStick
        {
            public Plane PlacementPlane;

            public double Length;

            public double Width;

            public double Thickness;

            public Box Box;

            public Brep Brep;

            public Point3d Center;

            public List<Brep> Tips = new List<Brep>();

            public List<Brep> Sides = new List<Brep>();

            public BonsaiStick(Plane pl, double length = 300.0, double width = 18.0, double thickness = 18.0)
            {
                PlacementPlane = pl;
                Length = length;
                Width = width;
                Thickness = thickness;
                Box = new Box(xSize: new Interval((0.0 - width) * 0.5, width * 0.5), ySize: new Interval((0.0 - length) * 0.5, length * 0.5), zSize: new Interval((0.0 - thickness) * 0.5, thickness * 0.5), basePlane: pl);
                Brep = Box.ToBrep();
                Center = pl.Origin;
                int[] tipsIndices = new int[2] { 0, 2 };
                for (int i = 0; i < 6; i++)
                {
                    Brep sub = Brep.DuplicateSubBrep(new int[1] { i });
                    if (Array.IndexOf(tipsIndices, i) >= 0)
                    {
                        Tips.Add(sub);
                    }
                    else
                    {
                        Sides.Add(sub);
                    }
                }
            }
        }

        public static class BonsaiOps
        {
            public static Brep PlaneToStick(Plane placementPlane)
            {
                double width = 18.0;
                double length = 300.0;
                double thickness = 18.0;
                Interval x = new Interval((0.0 - width) * 0.5, width * 0.5);
                Interval y = new Interval((0.0 - length) * 0.5, length * 0.5);
                Interval z = new Interval((0.0 - thickness) * 0.5, thickness * 0.5);
                return new Box(placementPlane, x, y, z).ToBrep();
            }

            public static Plane StickToPlane(BonsaiStick stick)
            {
                List<Tuple<BrepFace, double>> sorted = SortStickFaces(stick);
                List<Brep> sides = GetSides(sorted);
                List<Brep> tips = GetTips(sorted);
                Plane stickPlane = GetStickPlaneInternal(stick, sides, tips);
                return new Plane(stickPlane.Origin, stickPlane.XAxis, stickPlane.ZAxis);
            }

            public static void ScaleLine(ref Line line, double factor)
            {
                Point3d mid = line.PointAt(0.5);
                Plane linePlane = new Plane(mid, Plane.WorldXY.ZAxis);
                Transform xform = Transform.Scale(linePlane, factor, factor, factor);
                line.Transform(xform);
            }

            public static List<Tuple<BrepFace, double>> SortStickFaces(BonsaiStick stick)
            {
                BrepFaceList faces = stick.Brep.Faces;
                List<Tuple<BrepFace, double>> result = new List<Tuple<BrepFace, double>>(faces.Count);
                for (int i = 0; i < faces.Count; i++)
                {
                    Brep fbrep = faces[i].DuplicateFace(duplicateMeshes: false);
                    double area = AreaMassProperties.Compute(fbrep)?.Area ?? 0.0;
                    result.Add(new Tuple<BrepFace, double>(faces[i], area));
                }
                result.Sort((Tuple<BrepFace, double> a, Tuple<BrepFace, double> b) => a.Item2.CompareTo(b.Item2));
                return result;
            }

            public static List<Brep> GetSides(List<Tuple<BrepFace, double>> sortedFaces)
            {
                List<Brep> list = new List<Brep>();
                for (int i = 2; i < sortedFaces.Count; i++)
                {
                    Brep fbrep = sortedFaces[i].Item1.DuplicateFace(duplicateMeshes: false);
                    list.Add(fbrep);
                }
                return list;
            }

            public static List<Brep> GetTips(List<Tuple<BrepFace, double>> sortedFaces)
            {
                List<Brep> list = new List<Brep>();
                for (int i = 0; i < 2 && i < sortedFaces.Count; i++)
                {
                    Brep fbrep = sortedFaces[i].Item1.DuplicateFace(duplicateMeshes: false);
                    list.Add(fbrep);
                }
                return list;
            }

            public static double GetThickness(List<Brep> tips)
            {
                if (tips == null || tips.Count == 0)
                {
                    return 0.0;
                }
                AreaMassProperties amp = AreaMassProperties.Compute(tips[0]);
                return (amp != null) ? Math.Sqrt(amp.Area) : 0.0;
            }

            public static double GetLength(List<Brep> tips)
            {
                if (tips == null || tips.Count < 2)
                {
                    return 0.0;
                }
                AreaMassProperties topAmp = AreaMassProperties.Compute(tips[0]);
                AreaMassProperties botAmp = AreaMassProperties.Compute(tips[1]);
                if (topAmp == null || botAmp == null)
                {
                    return 0.0;
                }
                return topAmp.Centroid.DistanceTo(botAmp.Centroid);
            }

            public static Plane GetStickPlaneInternal(BonsaiStick stick, List<Brep> sides, List<Brep> tips)
            {
                Plane plane = new Plane(stick.PlacementPlane);
                plane.Rotate(-Math.PI / 2.0, stick.PlacementPlane.ZAxis, stick.PlacementPlane.Origin);
                plane.Rotate(-Math.PI / 2.0, stick.PlacementPlane.XAxis, stick.PlacementPlane.Origin);
                return plane;
            }

            public static Plane GetSidePlane(List<Brep> sides, int sideId, Plane stickPlane)
            {
                if (sideId < 0 || sideId >= sides.Count)
                {
                    return Plane.Unset;
                }
                Brep side = sides[sideId];
                Point3d center = BrepCenter(side);
                Vector3d xAxis = stickPlane.ZAxis;
                Surface srf = side.Surfaces[0];
                double u = srf.Domain(0).Mid;
                double v = srf.Domain(1).Mid;
                Vector3d zAxis = srf.NormalAt(u, v);
                zAxis.Unitize();
                Vector3d yAxis = Vector3d.CrossProduct(zAxis, xAxis);
                yAxis.Unitize();
                return new Plane(center, xAxis, yAxis);
            }

            public static Point3d BrepCenter(Brep brep)
            {
                Point3d[] verts = brep.DuplicateVertices();
                if (verts == null || verts.Length == 0)
                {
                    return Point3d.Origin;
                }
                Point3d sum = Point3d.Origin;
                for (int i = 0; i < verts.Length; i++)
                {
                    sum += verts[i];
                }
                return sum / verts.Length;
            }

            public static Plane GetSharedPlane(Plane sidePlaneA, Plane sidePlaneB, double shiftAb)
            {
                if (!Intersection.PlanePlane(sidePlaneA, sidePlaneB, out var shared))
                {
                    return Plane.Unset;
                }
                ScaleLine(ref shared, 100.0);
                Vector3d dir = shared.Direction;
                dir.Unitize();
                NurbsCurve crv = shared.ToNurbsCurve();
                if (crv == null) return Plane.Unset;
                crv.Domain = new Interval(0.0, 1.0);
                Point3d origin = crv.PointAt(shiftAb);
                return new Plane(origin, dir);
            }

            public static Curve GetSharedLine(Plane sidePlaneA, Plane sidePlaneB, double shiftAb)
            {
                if (!Intersection.PlanePlane(sidePlaneA, sidePlaneB, out var shared))
                {
                    return null;
                }
                ScaleLine(ref shared, 100.0);
                NurbsCurve crv = shared.ToNurbsCurve();
                if (crv == null) return null;
                crv.Domain = new Interval(0.0, 1.0);
                return crv;
            }

            public static TimberBranch CreateNewStick(BonsaiStick sourceStick, Plane stickPlane, Plane sidePlane, Plane sharedPlane, Curve sharedLine, double thickness, double length, int flip, double shift)
            {
                if (!Intersection.PlanePlane(sharedPlane, sidePlane, out var line))
                {
                    return null;
                }
                ScaleLine(ref line, 100.0);
                Point3d point = 0.5 * (line.From + line.To);
                point = 0.5 * (point + sharedPlane.Origin);
                Vector3d vector = sharedLine.PointAtEnd - sharedLine.PointAtStart;
                vector.Unitize();
                Plane newStickPlane = new Plane(point, vector, sidePlane.ZAxis);
                Plane newStickPl = new Plane(sourceStick.PlacementPlane);
                Transform p2p = Transform.PlaneToPlane(stickPlane, newStickPlane);
                newStickPl.Transform(p2p);
                Transform t1 = Transform.Translation(0.5 * thickness * newStickPlane.YAxis);
                newStickPl.Transform(t1);
                Transform t2 = Transform.Translation((double)flip * 0.5 * thickness * sharedPlane.ZAxis);
                newStickPl.Transform(t2);
                Transform t3 = Transform.Translation(length * shift * newStickPlane.ZAxis);
                newStickPl.Transform(t3);
                return new TimberBranch(newStickPl, "temporary",System.Drawing.Color.White,"temporary");
            }

            public static Tuple<double, double> AdjustP1(List<Brep> sides, int sideId, Plane sharedPlane)
            {
                if (sideId < 0 || sideId >= sides.Count)
                {
                    return new Tuple<double, double>(0.0, 0.0);
                }
                Brep faceBrep = sides[sideId];
                BrepFace face = faceBrep.Faces[0];
                Curve edge = face.OuterLoop.To3dCurve();
                Curve[] segments = edge.DuplicateSegments();
                double minD = double.PositiveInfinity;
                double maxD = double.NegativeInfinity;
                for (int i = 0; i < segments.Length; i++)
                {
                    Point3d p = segments[i].PointAtStart;
                    Point3d cp = sharedPlane.ClosestPoint(p);
                    double d = p.DistanceTo(cp);
                    if (d < minD)
                    {
                        minD = d;
                    }
                    if (d > maxD)
                    {
                        maxD = d;
                    }
                }
                return new Tuple<double, double>(minD, maxD);
            }

            public static Tuple<TimberBranch, TimberBranch> GenerateLink(BonsaiStick stickA, BonsaiStick stickB, int sideIdA, int sideIdB, double shiftAb, bool flipBool, double shiftA, double shiftB)
            {
                List<Tuple<BrepFace, double>> sortedA = SortStickFaces(stickA);
                List<Brep> sidesA = stickA.Sides;
                List<Brep> tipsA = stickA.Tips;
                List<Tuple<BrepFace, double>> sortedB = SortStickFaces(stickB);
                List<Brep> sidesB = stickB.Sides;
                List<Brep> tipsB = stickB.Tips;
                double thicknessA = GetThickness(tipsA);
                double lengthA = GetLength(tipsA);
                double thicknessB = GetThickness(tipsB);
                double lengthB = GetLength(tipsB);
                Plane stickPlaneA = GetStickPlaneInternal(stickA, sidesA, tipsA);
                Plane stickPlaneB = GetStickPlaneInternal(stickB, sidesB, tipsB);
                Plane sidePlaneA = GetSidePlane(sidesA, sideIdA, stickPlaneA);
                Plane sidePlaneB = GetSidePlane(sidesB, sideIdB, stickPlaneB);
                Plane sharedPlane = GetSharedPlane(sidePlaneA, sidePlaneB, 0.0);
                Curve sharedLine = GetSharedLine(sidePlaneA, sidePlaneB, 0.0);
                if (sharedLine == null)
                {
                    return new Tuple<TimberBranch, TimberBranch>(null, null);
                }
                Tuple<double, double> abA = AdjustP1(sidesA, sideIdA, sharedPlane);
                Tuple<double, double> abB = AdjustP1(sidesB, sideIdB, sharedPlane);
                double minD = Math.Max(abA.Item1, abB.Item1);
                double maxD = Math.Min(abA.Item2, abB.Item2);
                double translation = shiftAb * minD + (1.0 - shiftAb) * maxD;
                Transform move = Transform.Translation(translation * sharedPlane.ZAxis);
                Plane shiftedSharedPlane = sharedPlane;
                shiftedSharedPlane.Transform(move);
                int flip = (flipBool ? 1 : (-1));
                TimberBranch stickC = CreateNewStick(stickA, stickPlaneA, sidePlaneA, shiftedSharedPlane, sharedLine, thicknessA, lengthA, flip, shiftA);
                TimberBranch stickD = CreateNewStick(stickB, stickPlaneB, sidePlaneB, shiftedSharedPlane, sharedLine, thicknessB, lengthB, -flip, shiftB);
                if (!CheckIntersections(stickA.Brep, stickB.Brep, stickC.brep, stickD.brep))
                {
                    return new Tuple<TimberBranch, TimberBranch>(null, null);
                }
                return new Tuple<TimberBranch, TimberBranch>(stickC, stickD);
            }

            public static bool CheckIntersections(Brep stick_a, Brep stick_b, Brep stick_c, Brep stick_d)
            {
                bool[] results = new bool[5];
                Task.WaitAll(Task.Run(delegate
                {
                    bool result3;
                    results[0] = (result3 = IntersectConfirmation(stick_c, stick_a));
                    return result3;
                }), Task.Run(() => results[1] = !IntersectConfirmation(stick_c, stick_b)), Task.Run(delegate
                {
                    bool result2;
                    results[2] = (result2 = IntersectConfirmation(stick_d, stick_b));
                    return result2;
                }), Task.Run(() => results[3] = !IntersectConfirmation(stick_d, stick_a)), Task.Run(delegate
                {
                    bool result;
                    results[4] = (result = IntersectConfirmation(stick_c, stick_d));
                    return result;
                }));
                bool allTrue = true;
                for (int i = 0; i < results.Length; i++)
                {
                    if (!results[i])
                    {
                        allTrue = false;
                        break;
                    }
                }
                return allTrue;
            }

            private static bool IntersectConfirmation(Brep x, Brep y)
            {
                Curve[] crvs;
                Point3d[] pnts;
                bool intersects = Intersection.BrepBrep(x, y, 0.001, out crvs, out pnts) && crvs != null && crvs.Length > 0;
                bool enoughIntersection = IsAreaGreaterThanThreshold(crvs);
                return intersects && enoughIntersection;
            }

            public static bool IsAreaGreaterThanThreshold(Curve[] curves)
            {
                if (curves == null || curves.Length == 0)
                {
                    return false;
                }
                Curve crv = Curve.JoinCurves(curves, 0.001)[0];
                AreaMassProperties compute = AreaMassProperties.Compute(crv, 0.001);
                if (compute == null)
                {
                    return false;
                }
                return compute.Area > 324.0;
            }

            public static Tuple<TimberBranch, TimberBranch>[] RunParallelConfigs(Plane branchAPlacement, Plane branchBPlacement, double shiftAb, double shiftA, double shiftB)
            {
                BonsaiStick stickA = new BonsaiStick(branchAPlacement);
                BonsaiStick stickB = new BonsaiStick(branchBPlacement);
                int total = 32;
                Tuple<TimberBranch, TimberBranch>[] configurations = new Tuple<TimberBranch, TimberBranch>[total];
                Action<int> runConfig = delegate (int idx)
                {
                    int sideIdA = idx / 8;
                    int sideIdB = idx / 2 % 4;
                    bool flipBool = idx % 2 == 0;
                    try
                    {
                        configurations[idx] = GenerateLink(stickA, stickB, sideIdA, sideIdB, shiftAb, flipBool, shiftA, shiftB);
                    }
                    catch (Exception)
                    {
                        configurations[idx] = null;
                    }
                };
                ParallelOptions parallelOptions = new ParallelOptions();
                parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;
                ParallelOptions options = parallelOptions;
                Parallel.For(0, total, options, runConfig);
                return configurations;
            }

            public static Tuple<TimberBranch, TimberBranch>[] RunParallelConfigs(Plane branchAPlacement, Plane branchBPlacement, double shiftA, double shiftB)
            {
                int nShiftAB = (int)Math.Round(10.0) + 1;
                int total = 32 * nShiftAB;
                Tuple<TimberBranch, TimberBranch>[] configurations = new Tuple<TimberBranch, TimberBranch>[total];
                double[] shiftABs = new double[nShiftAB];
                for (int s = 0; s < nShiftAB; s++)
                {
                    shiftABs[s] = -2.5 + (double)s * 0.5;
                }
                BonsaiStick stickA = new BonsaiStick(branchAPlacement);
                BonsaiStick stickB = new BonsaiStick(branchBPlacement);
                ParallelOptions parallelOptions = new ParallelOptions();
                parallelOptions.MaxDegreeOfParallelism = Environment.ProcessorCount;
                ParallelOptions options = parallelOptions;
                int batchSize = 512;
                Partitioner.Create(0, total, batchSize).AsParallel().WithDegreeOfParallelism(Environment.ProcessorCount)
                    .ForAll(delegate (Tuple<int, int> range)
                    {
                        for (int i = range.Item1; i < range.Item2; i++)
                        {
                            int num = i;
                            int num2 = num / 32;
                            num -= num2 * 32;
                            int num3 = num / 8;
                            num -= num3 * 8;
                            int sideIdB = num / 2;
                            bool flipBool = (num & 1) == 0;
                            double shiftAb = shiftABs[num2];
                            try
                            {
                                configurations[i] = GenerateLink(stickA, stickB, num3, sideIdB, shiftAb, flipBool, shiftA, shiftB);
                            }
                            catch
                            {
                                configurations[i] = null;
                            }
                        }
                    });
                return configurations;
            }
        }
    }
}