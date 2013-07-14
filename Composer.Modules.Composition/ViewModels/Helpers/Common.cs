using System;

namespace Composer.Modules.Composition.ViewModels.Helpers
{
    public static class Common
    {

        public static Repository.DataService.Audit GetAudit()
        {
            Repository.DataService.Audit Audit = new Repository.DataService.Audit
                                                     {
                                                         CreateDate = DateTime.Now,
                                                         ModifyDate = DateTime.Now,
                                                         Author_Id = Current.User.Id,
                                                         CollaboratorIndex = 0
                                                     };
            return Audit;
        }
    }
}
