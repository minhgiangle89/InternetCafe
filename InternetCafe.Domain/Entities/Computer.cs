using InternetCafe.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace InternetCafe.Domain.Entities
{
    public class Computer : BaseEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string IPAddress { get; set; } = null!;
        public string Specifications { get; set; } = null!;
        public string Location { get; set; } = null!;
        public ComputerStatus Status { get; set; } = ComputerStatus.Available;
        public decimal HourlyRate { get; set; }
        public DateTime? LastMaintenanceDate { get; set; }
        public DateTime? LastUsedDate { get; set; }
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
