using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SkillBridge.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequireRoleAttribute : Attribute, IPageFilter
    {
        private readonly string _role;
        public RequireRoleAttribute(string role) => _role = role;

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
        }

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString(SessionKeys.UserRole);
            if (role != _role)
            {
                var redirect = _role switch {
                    "Customer" => "/Auth/CustomerLogin",
                    "Labourer" => "/Auth/LabourerLogin",
                    "Admin"    => "/Auth/AdminLogin",
                    _ => "/"
                };
                context.Result = new RedirectResult(redirect);
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
        }
    }
}
