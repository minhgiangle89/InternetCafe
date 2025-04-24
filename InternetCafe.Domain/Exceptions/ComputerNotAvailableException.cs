using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Exceptions
{
    public class ComputerNotAvailableException : DomainException
    {
        public ComputerNotAvailableException(int computerId)
            : base($"Computer with ID {computerId} is not available.")
        {
            ComputerId = computerId;
        }

        public int ComputerId { get; }
    }
}
