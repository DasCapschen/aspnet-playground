using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using src.Models;

namespace src.Policies 
{
    public class OneDayEditRequirement : IAuthorizationRequirement { }

    public class OneDayEditPolicyHandler : AuthorizationHandler<OneDayEditRequirement, ActivityProtocol>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OneDayEditRequirement requirement, ActivityProtocol resource)
        {
            // fail if user not authenticated
            if(!context.User.Identity?.IsAuthenticated ?? false) {
                return Task.CompletedTask;
            }

            // succeed if creation date is less than 24 hours ago
            if(DateTimeOffset.UtcNow < resource.Date.AddHours(24)) {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

}