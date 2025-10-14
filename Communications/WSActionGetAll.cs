using BonsaiInstallation;
using System.Collections.Generic;

namespace MouseSelection.Communications
{
    class WSActionGetAll : WSAction
    {

        public WSActionGetAll() 
        {
            action = action.GetAll;
        }

        //public WSActionGetAll(List<TimberBranch> branches)
        //{
        //    action = action.GetAll;
        //    payload = branches;
        //}

        public bool Execute(List<TimberBranch> memoryBranchList)
        {
            memoryBranchList.Clear();
            memoryBranchList.AddRange(this.payload);
            return true;
        }


    }
}
