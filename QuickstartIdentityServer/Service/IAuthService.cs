using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickstartIdentityServer.Service
{
    public interface IAuthService
    {
        bool Validate(string phone, string code);
    }
}
