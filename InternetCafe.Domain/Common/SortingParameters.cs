using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Common
{
    public class SortingParameters
    {
        public string? SortBy { get; set; }
        public SortDirection Direction { get; set; } = SortDirection.Ascending;

        public enum SortDirection
        {
            Ascending,
            Descending
        }
    }
}
