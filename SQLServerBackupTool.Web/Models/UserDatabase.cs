using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SQLServerBackupTool.Web.Models
{
    public class UserDatabase
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50), Required]
        public string Username { get; set; }

        [MaxLength(50), Required]
        public string DatabaseName { get; set; }
    }
}