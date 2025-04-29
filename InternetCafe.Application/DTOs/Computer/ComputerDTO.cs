using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Computer
{
    public class ComputerDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IPAddress { get; set; } = string.Empty;
        public string Specifications { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int ComputerStatus { get; set; }
        public decimal HourlyRate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? LastUsedDate { get; set; }
    }
}
