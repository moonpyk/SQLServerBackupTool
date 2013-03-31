using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace SQLServerBackupTool.Web.Lib
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class MembershipPasswordValidationAttribute : DataTypeAttribute
    {
        public MembershipPasswordValidationAttribute()
            : base(DataType.Password)
        {
        }

        public override bool IsValid(object value)
        {
            return false;
        }
    }
}
