using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using QuickstartIdentityServer.Service;

namespace QuickstartIdentityServer.Authentcation
{
    public class SmsAuthCodeValidator : IExtensionGrantValidator
    {
        private readonly IAuthService _authService;
        private readonly IUserServise _userServise;

        public SmsAuthCodeValidator(IAuthService authService, IUserServise userServise)
        {
            _authService = authService;
            _userServise = userServise;
        }

        public string GrantType => "sms_auth_cod";

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var phone = context.Request.Raw["phone"];
            var code = context.Request.Raw["auth_code"];
            var  errorVadationRequest=new GrantValidationResult(TokenRequestErrors.InvalidClient);
            if (string.IsNullOrWhiteSpace(phone)||string.IsNullOrWhiteSpace(code))
            {
                context.Result = errorVadationRequest;
                return;
            }

            if (!_authService.Validate(phone,code))
            {
                context.Result = errorVadationRequest;
                return;
            }

            var userId =await  _userServise.CheckOrCreate(phone);
            if (userId<=0)
            {
                context.Result = errorVadationRequest;
                return;
            }
            context.Result=new GrantValidationResult(userId.ToString(),GrantType);
        }
    }
}
