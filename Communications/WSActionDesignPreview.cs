using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using BonsaiInstallation;

namespace MouseSelection.Communications
{
    public class WSActionDesignPreview : WSAction
    {

        public List<Guid> parent_ids;
        public string designer_name;
        public Color color;
        public List<Plane> preview_planes;

        public WSActionDesignPreview()
        {
            
        }

        public WSActionDesignPreview(List<Guid> parent_ids, string designer_name, Color color, List<Plane> preview_planes)
        {
            this.action = action.Preview;
            this.parent_ids = parent_ids;
            this.designer_name = designer_name;
            this.color = color;
            this.preview_planes = preview_planes;
        }

        public bool Execute(PoolConnection pool) 
        {
            try
            {
                var designPreviews = pool.designPreviews;
                if (designPreviews.ContainsKey(this.designer_name))
                {
                    designPreviews[designer_name] = this;
                }
                else
                {
                    designPreviews.Add(this.designer_name, this);
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }




    }
}
