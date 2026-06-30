using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class UpdateUserPasswordHashDto
    {
        public string PasswordHash { get; set; } = string.Empty;
    }
}
