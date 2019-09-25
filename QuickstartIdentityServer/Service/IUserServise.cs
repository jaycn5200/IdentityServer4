using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuickstartIdentityServer.Service
{
    public interface IUserServise
    {
        Task<long> CheckOrCreate(string phone);
    }
}
