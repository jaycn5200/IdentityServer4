using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Abp.Domain.Entities.Auditing;

namespace EFCore
{
    [Table("users")]
   public class User : Entity<long>, IHasCreationTime, IHasModificationTime
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [StringLength(50)]
        public string UserName { get; set; }

        /// <summary>
        /// 规范化用户名（UserName转为大写）
        /// </summary>
        [StringLength(50)]
        public string NormalizedUserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [StringLength(50)]
        public string Password { get; set; }

        /// <summary>
        /// 是否锁定
        /// </summary>
        public bool IsLockoutEnabled { get; set; }

        /// <summary>
        /// 解锁时间
        /// </summary>
        public DateTime? LockoutEndDateUtc { get; set; }

        /// <summary>
        /// 登录错误次数
        /// </summary>
        public int AccessFailedCount { get; set; }

        /// <summary>
        /// 注册Ip
        /// </summary>
        public string RegIp { get; set; }

        /// <summary>
        /// 登录次数
        /// </summary>
        public int LoginTimes { get; set; }

        /// <summary>
        /// 最后登录IP
        /// </summary>
        [StringLength(50)]
        public string LastLoginIp { get; set; }

        /// <summary>
        /// 最后登录时间
        /// </summary>
        public DateTime? LastLoginTime { get; set; }
        public DateTime CreationTime { get; set; }
        public DateTime? LastModificationTime { get; set; }
    }
}
