using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MouseSelection
{
    public class SetShiftSliderValues : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the SetShiftSliderValues class.
        /// </summary>
        public SetShiftSliderValues()
          : base("SetShiftSliderValues", "Nickname",
              "Description",
              "MouseSelection", "MouseSelection")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {

            pManager.AddBooleanParameter("modify", "modify", "modify", GH_ParamAccess.item);
            pManager.AddNumberParameter("shifts", "shifts", "shifts", GH_ParamAccess.list);
            pManager.AddNumberParameter("glueShifts", "glueShifts", "glueShifts", GH_ParamAccess.list);
            pManager.AddGenericParameter("shiftSliderX", "shiftSliderX", "shiftSliderX", GH_ParamAccess.item);
            pManager.AddGenericParameter("shiftSliderY", "shiftSliderY", "shiftSliderY", GH_ParamAccess.item);
            pManager.AddGenericParameter("shiftSliderZ", "shiftSliderZ", "shiftSliderZ", GH_ParamAccess.item);
            pManager.AddGenericParameter("glueShiftSliderX", "glueShiftSliderX", "glueShiftSliderX", GH_ParamAccess.item);
            pManager.AddGenericParameter("glueShiftSliderY", "glueShiftSliderY", "glueShiftSliderY", GH_ParamAccess.item);
            pManager.AddGenericParameter("glueShiftSliderZ", "glueShiftSliderZ", "glueShiftSliderZ", GH_ParamAccess.item);
            pManager.AddGenericParameter("pickShift", "pickShift", "pickShift", GH_ParamAccess.item);
            pManager.AddGenericParameter("orientPlane","orientPlane", "orientPlane", GH_ParamAccess.item);

        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddBooleanParameter("modify", "modify", "modify", GH_ParamAccess.item);
            //pManager.AddNumberParameter("shifts", "shifts", "shifts", GH_ParamAccess.list);
            //pManager.AddNumberParameter("glueShifts", "glueShifts", "glueShifts", GH_ParamAccess.list);
            pManager.AddNumberParameter("shiftSliderX", "shiftSliderX", "shiftSliderX", GH_ParamAccess.item);
            pManager.AddNumberParameter("shiftSliderY", "shiftSliderY", "shiftSliderY", GH_ParamAccess.item);
            pManager.AddNumberParameter("shiftSliderZ", "shiftSliderZ", "shiftSliderZ", GH_ParamAccess.item);
            pManager.AddNumberParameter("glueShiftSliderX", "glueShiftSliderX", "glueShiftSliderX", GH_ParamAccess.item);
            pManager.AddNumberParameter("glueShiftSliderY", "glueShiftSliderY", "glueShiftSliderY", GH_ParamAccess.item);
            pManager.AddNumberParameter("glueShiftSliderZ", "glueShiftSliderZ", "glueShiftSliderZ", GH_ParamAccess.item);
            pManager.AddNumberParameter("pickShift", "pickShift", "pickShift", GH_ParamAccess.item);
            pManager.AddNumberParameter("orientPlane", "orientPlane", "orientPlane", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            bool modify = false;
            List<double> shifts = new List<double>();
            List<double> glueShifts = new List<double>();

            DA.GetData(0, ref modify);
            DA.GetDataList(1, shifts);
            DA.GetDataList(2, glueShifts);

            DA.SetData(0, shifts[0]);
            DA.SetData(1, shifts[1]);
            DA.SetData(2, shifts[2]);
            DA.SetData(3, glueShifts[0]);
            DA.SetData(4, glueShifts[1]);
            DA.SetData(5, glueShifts[2]);
            DA.SetData(6, shifts[3]);
            DA.SetData(7, shifts[4]);


            if (!modify) return;

            this.OnPingDocument().ScheduleSolution(1, doc => {

                var shiftSliderX = (Grasshopper.Kernel.Special.GH_NumberSlider)this.Params.Input[3].Sources[0];
                var shiftSliderY = (Grasshopper.Kernel.Special.GH_NumberSlider)this.Params.Input[4].Sources[0];
                var shiftSliderZ = (Grasshopper.Kernel.Special.GH_NumberSlider)this.Params.Input[5].Sources[0];
                var glueShiftSliderX = (Grasshopper.Kernel.Special.GH_NumberSlider)this.Params.Input[6].Sources[0];
                var glueShiftSliderY = (Grasshopper.Kernel.Special.GH_NumberSlider)this.Params.Input[7].Sources[0];
                var glueShiftSliderZ = (Grasshopper.Kernel.Special.GH_NumberSlider)this.Params.Input[8].Sources[0];
                var pickShift = (Grasshopper.Kernel.Special.GH_NumberSlider)this.Params.Input[9].Sources[0];
                var orientPlane = (Grasshopper.Kernel.Special.GH_NumberSlider)this.Params.Input[10].Sources[0];

                shiftSliderX.SetSliderValue((decimal) shifts[0]);
                shiftSliderY.SetSliderValue((decimal) shifts[1]);
                shiftSliderZ.SetSliderValue((decimal)shifts[2]);
                pickShift.SetSliderValue((decimal)shifts[3]);
                orientPlane.SetSliderValue((decimal)shifts[4]);

                glueShiftSliderX.SetSliderValue((decimal)glueShifts[0]);
                glueShiftSliderY.SetSliderValue((decimal)glueShifts[1]);
                glueShiftSliderZ.SetSliderValue((decimal)glueShifts[2]);


            });

            
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
            get { return new Guid("3DE86241-C4F5-4D26-8A01-F86270BD5D73"); }
        }
    }
}