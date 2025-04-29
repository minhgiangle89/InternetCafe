using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Computer
{
    public class ComputerStatusUpdateDTO
    {
        public int ComputerId { get; set; }
        public int Status { get; set; }
        public string? Reason { get; set; }
    }
}
