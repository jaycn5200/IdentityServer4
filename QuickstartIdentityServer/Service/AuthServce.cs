using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickstartIdentityServer.Service
{
    public class AuthServce : IAuthService
    {
        public bool Validate(string phone, string code)
        {
            return true;
        }
    }
}
