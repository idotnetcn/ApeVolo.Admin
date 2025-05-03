using System.Collections.Generic;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Core.Permission.User;
using SqlSugar;

namespace Ape.Volo.Entity.Core.Permission
{
    /// <summary>
    /// 岗位
    /// </summary>
    [SugarTable("sys_job")]
    public class Job : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public string Name { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        [SugarColumn(IsNullable = true)]
        public int Sort { get; set; }

        /// <summary>
        /// 是否激活
        /// </summary>
        [SugarColumn(IsNullable = false)]
        public bool Enabled { get; set; }

        /// <summary>
        /// 用户列表
        /// </summary>
        [SugarColumn(IsIgnore = true)]
        [Navigate(typeof(UserJob), nameof(UserJob.JobId), nameof(UserJob.UserId))]
        public List<User.User> Users { get; set; }
    }
}
