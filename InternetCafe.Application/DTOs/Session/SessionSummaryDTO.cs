using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Application.DTOs.Session
{
    public class SessionSummaryDTO
    {
        public int Id { get; set; }
        public int ComputerId { get; set; }
        public string ComputerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal TotalCost { get; set; }
        public int Status { get; set; }
    }
}
