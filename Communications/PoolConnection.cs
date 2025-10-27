using BonsaiInstallation;
using Grasshopper.Kernel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using WebSocketSharp;

namespace MouseSelection.Communications
{
    public class PoolConnection : GH_Component
    {

        string url = "ws://localhost:8080";
        bool isConnected = false;
        public WebSocket ws;
        public List<TimberBranch> tree;
        public Dictionary<string, WSActionDesignPreview> designPreviews;
        GH_Document canvasDoc;
        bool wss = false;

        /// <summary>
        /// Initializes a new instance of the PoolConnection class.
        /// </summary>
        public PoolConnection()
          : base("PoolConnection", "Nickname",
              "Description",
              "MouseSelection", "Pool")
        {
            tree = new List<TimberBranch>();
            var canvas = Grasshopper.Instances.ActiveCanvas;
            designPreviews = new Dictionary<string, WSActionDesignPreview>();
            if (canvas != null && canvas.Document != null) this.canvasDoc = canvas.Document;
            try
            {
                ws = new WebSocket(url);
            }
            catch
            {
                Console.WriteLine("Connection to url: " + url + " was unsucsessful!");
            }
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("url", "url", "url", GH_ParamAccess.item, "ws://localhost:8080");
            pManager.AddBooleanParameter("connect", "connect", "connect", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("WSS", "Is WSS server", "", GH_ParamAccess.item, false);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("ws", "ws", "ws", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string url = "ws://localhost:8080";
            bool connect = false;
            wss = false;
            DA.GetData(0, ref url);
            DA.GetData(1, ref connect);
            DA.GetData(2, ref wss);

            if (!connect)
            {
                isConnected = false;
                ws.OnClose -= Ws_OnClose;
            }

            if (connect && !isConnected)
            {
                ws = new WebSocket(url);

                if (wss)
                {
                    // TLS fix:
                    ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                    // Still allow debugging bad certs:
                    ws.SslConfiguration.ServerCertificateValidationCallback = (sender, cert, chain, errors) => true;
                }

                ws.OnOpen += Ws_OnOpen;
                ws.OnMessage += Ws_OnMessage;
                ws.OnClose += Ws_OnClose;

                ws.Connect();
                designPreviews = new Dictionary<string, WSActionDesignPreview>();
            }
            else if (!connect)
            {
                ws.Close();

                ws.OnOpen -= Ws_OnOpen;
                ws.OnMessage -= Ws_OnMessage;
                ws.OnClose -= Ws_OnClose;
            }

            DA.SetData(0, this);
        }

        private void Ws_OnClose(object sender, CloseEventArgs e)
        {
            ws.OnOpen -= Ws_OnOpen;
            ws.OnMessage -= Ws_OnMessage;
            ws.OnClose -= Ws_OnClose;

            ws = new WebSocket(url);
            if (wss)
            {
                // TLS fix:
                ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
                // Still allow debugging bad certs:
                ws.SslConfiguration.ServerCertificateValidationCallback = (s, cert, chain, errors) => true;
            }
            ws.OnOpen += Ws_OnOpen;
            ws.OnMessage += Ws_OnMessage;
            ws.OnClose += Ws_OnClose;

            ws.Connect();
        }

        private void Ws_OnMessage(object sender, MessageEventArgs e)
        {
            string message = e.Data;
            this.FromJson(message);

            if (canvasDoc == null)
            {
                var canvas = Grasshopper.Instances.ActiveCanvas;
                if (canvas != null && canvas.Document != null) this.canvasDoc = canvas.Document;
                if (canvasDoc == null) return;
            }
            canvasDoc.ScheduleSolution(1, doc =>
            {
                var pools = canvasDoc.Objects.Where(o => o is PoolTree);
                foreach (var item in pools)
                {
                    item.ExpireSolution(true);
                }
            });
        }

        private void Ws_OnOpen(object sender, EventArgs e)
        {
            var getAll = new WSActionGetAll();
            this.isConnected = true;
            this.ws.Send(getAll.ToJson());
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
            get { return new Guid("D19FD6B5-69BE-4648-B65A-F39612EFB083"); }
        }

        public bool FromJson(string json)
        {
            // get the action value from the json string by parsing it
            var actionValue = JsonConvert.DeserializeObject<Dictionary<string, object>>(json)["action"];


            // Convert string to enum (case-insensitive)
            action actionType = (action)Enum.Parse(typeof(action), actionValue.ToString());

            // Dispatch based on the enum value
            switch (actionType)
            {
                case action.GetAll:
                    WSActionGetAll getAll = JsonConvert.DeserializeObject<WSActionGetAll>(json);
                    return getAll.Execute(this.tree);

                case action.CreateSingle:
                    WSActionCreateSingle createSingle = JsonConvert.DeserializeObject<WSActionCreateSingle>(json);
                    return createSingle.Execute(this.tree);

                case action.Preview:
                    WSActionDesignPreview designPreview = JsonConvert.DeserializeObject<WSActionDesignPreview>(json);
                    return designPreview.Execute(this);

                default:
                    return false;
            }
        }
    }
}