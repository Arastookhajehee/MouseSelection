using System.Windows.Documents;
using System.Collections.Generic;
using BonsaiInstallation;
using Newtonsoft.Json;

namespace MouseSelection.Communications
{

    public enum action
    {
        Create = 1,
        Update = 2,
        Delete = 3,
        Get = 4,
        GetAll = 5,
        Response = 6,
        CreateSingle = 7,
        UpdateSingle = 8,
        DeleteSingle = 9,
        GetSingle = 10,
        ResponseSingle = 11,
        Confirmation = 12,
        Error = 13,
        Preview = 14
    }


    public abstract class WSAction
    {
        public action action;
        public List<TimberBranch> payload;


        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
                
    }
}
