using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Exceptions
{
    public class SessionNotFoundException : DomainException
    {
        public SessionNotFoundException(int sessionId)
            : base($"Session with ID {sessionId} was not found.")
        {
            SessionId = sessionId;
        }

        public int SessionId { get; }
    }
}
