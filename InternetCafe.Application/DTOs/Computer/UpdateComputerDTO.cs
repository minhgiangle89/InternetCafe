using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Computer
{
    public class UpdateComputerDTO
    {
        public string Name { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public decimal HourlyRate { get; set; }
    }
}
