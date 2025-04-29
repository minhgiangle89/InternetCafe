using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Session
{
    public class EndSessionDto
    {
        public int SessionId { get; set; }
        public string? Notes { get; set; }
    }
}
