using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Entities
{
    public abstract class BaseEntity
    {
        public DateTime Creation_Timestamp { get; set; }
        public int Creation_EmpId { get; set; }
        public DateTime LastUpdated_Timestamp { get; set; }
        public int LastUpdated_EmpId { get; set; }
        public int Status { get; set; } = 1; 
    }
}
