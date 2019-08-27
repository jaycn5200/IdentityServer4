using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace QuickstartIdentityServer.Web
{
    public class TestAuthorizeAttribute : AuthorizeAttribute
    {
        public string Permission { get; set; }

        public TestAuthorizeAttribute(string permission)
        {
            Permission = permission;
        }
    }
}
