using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using EFCore;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.EntityFrameworkCore;

namespace QuickstartIdentityServer
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        private readonly UcenterDbContext _repository;

        public ResourceOwnerPasswordValidator(UcenterDbContext repository)
        {
            _repository = repository;
        }

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            //根据context.UserName和context.Password与数据库的数据做校验，判断是否合法
            var entity =
                await _repository.Users.FirstOrDefaultAsync(a =>
                    a.UserName == context.UserName && a.Password == CalcMD5(context.Password));
            if (entity!=null)
            {
                
                context.Result = new GrantValidationResult(
                    subject: context.UserName,
                    authenticationMethod: "custom",
                    claims: GetUserClaims(entity.Id.ToString(),entity.UserName));
            }
            else
            {

                //验证失败
                //context.Result = new GrantValidationResult(TokenRequestErrors.UnauthorizedClient, "用户名或者密码错误");
                context.Result = new GrantValidationResult(
                    subject: "",
                    authenticationMethod: "custom",
                    claims: GetUserErrorClaims("账户名或者密码错误"));
            }
        }
        public static string CalcMD5(string str)
        {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            return CalcMD5(bytes);
        }
        public static string CalcMD5(byte[] bytes)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] computeBytes = md5.ComputeHash(bytes);
                string result = "";
                for (int i = 0; i < computeBytes.Length; i++)
                {
                    result += computeBytes[i].ToString("X").Length == 1 ? "0" + computeBytes[i].ToString("X") : computeBytes[i].ToString("X");
                }
                return result;
            }
        }
        //可以根据需要设置相应的Claim
        private Claim[] GetUserClaims(string userId,string name)
        {
            return new Claim[]
            {
                new Claim("UserId", userId),
                new Claim(JwtClaimTypes.Name,name),
            };
        }

        //可以根据需要设置相应的Claim
        private Claim[] GetUserErrorClaims(string error)
        {
            return new Claim[]
            {
                new Claim("Error", error),
            };
        }
    }
}
