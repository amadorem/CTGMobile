using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace CTGMobileWebAPI.Filters
{
    public class AdalTokenAcquisitionExceptionFilter: ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is AdalSilentTokenAcquisitionException)
            {
                context.Result = new ChallengeResult();
            }
        }
    }
}
