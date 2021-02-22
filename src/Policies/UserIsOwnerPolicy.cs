using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using src.Models;

namespace src.Policies 
{
    public class UserIsOwnerRequirement : IAuthorizationRequirement { }

    public class UserIsOwnerPolicyHandler : AuthorizationHandler<UserIsOwnerRequirement, ActivityProtocol>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UserIsOwnerRequirement requirement, ActivityProtocol resource)
        {
            // fail if user not authenticated
            if(!context.User.Identity?.IsAuthenticated ?? false) 
            {
                return Task.CompletedTask;
            }

            //check if user id is same as owner id
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier).Value;
            if(userId == resource.OwnerId) 
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

}