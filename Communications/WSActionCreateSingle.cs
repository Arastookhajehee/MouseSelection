using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BonsaiInstallation;

namespace MouseSelection.Communications
{
    class WSActionCreateSingle : WSAction
    {
        public WSActionCreateSingle() { }

        public WSActionCreateSingle(List<TimberBranch> branches)
        {
            action = action.CreateSingle;
            payload = branches;
        }

        public bool Execute(List<TimberBranch> memoryBranchList)
        {
            // TO DO: add the branch to the database
            memoryBranchList.AddRange(payload);
            return true;
        }
    }
}
