using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.User
{
    public class UpdateUserRoleDto
    {
        public UserRole Role
        {
            get; set;
        }
    }
}
