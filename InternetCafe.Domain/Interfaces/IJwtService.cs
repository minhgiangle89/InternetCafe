using InternetCafe.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
