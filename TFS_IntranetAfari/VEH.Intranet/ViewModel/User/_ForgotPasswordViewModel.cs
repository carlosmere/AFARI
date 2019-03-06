using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace VEH.Intranet.ViewModel.User
{
    public class _ForgotPasswordViewModel
    {
        [Required]
        public String Email { get; set; }
    }
}