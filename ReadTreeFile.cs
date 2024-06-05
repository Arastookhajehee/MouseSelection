using System;
using System.Collections.Generic;
using BonsaiInstallation;
using Grasshopper.Kernel;
using Rhino.Geometry;
using System.Drawing;
using System.Linq;

namespace MouseSelection
{
    public class ReadTreeFile : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the ReadTreeFile class.
        /// </summary>
        public ReadTreeFile()
          : base("ReadTreeFile", "RTF",
              "reads tree json file",
              "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("json", "json", "json", GH_ParamAccess.item);
            pManager.AddPlaneParameter("resetPlane","rstPlane","rstPlane",GH_ParamAccess.item);
            pManager.AddNumberParameter("resetZShift","resetZShift","resetZShift",GH_ParamAccess.item);
            pManager.AddBooleanParameter("RESET","RESET","RESET",GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("BonsaiTree","BTree","BonsaiTree",GH_ParamAccess.list);
            pManager.AddTextParameter("json","json","json",GH_ParamAccess.item);
            pManager.AddBrepParameter("breps","breps","breps",GH_ParamAccess.list);
            pManager.AddColourParameter("colors","colors","colors",GH_ParamAccess.list);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string json = "";
            Plane plane = Plane.Unset;
            double resetZShift = 0;
            bool RESET = false;
            
            DA.GetData(0, ref json);
            DA.GetData(1, ref plane);
            DA.GetData(2, ref resetZShift);
            DA.GetData(3, ref RESET);


            if (RESET)
            {
                Plane ogPlane = plane != Plane.Unset ? plane : Plane.WorldXY;
                ogPlane.Translate(ogPlane.ZAxis * (-9 - resetZShift));
                List<TimberBranch> resetList = new List<TimberBranch> { new TimberBranch(ogPlane, "BASE", Color.Tan, "VIRTUALBASE") };
                DA.SetDataList(0, resetList);
                DA.SetData(1, TimberBranch.ListToJson(resetList));
                return;
            }

            List<TimberBranch> list = TimberBranch.FromJsonToList(json);
            DA.SetDataList(0, list);
            DA.SetData(1,json);
            DA.SetDataList(2, list.Select(o => o.brep));
            DA.SetDataList(3, list.Select(o => o.color));

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
            get { return new Guid("10949853-3925-4178-8D81-B3A29FEBE830"); }
        }
    }
}