using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InternetCafe.Domain.Enums
{
    public enum Status
    {
        Active = 1,
        Cancelled = 2
    }

    public enum UserRole
    {
        Customer = 0,
        Staff = 1,
        Admin = 2
    }

    public enum UserStatus
    {
        Active = 0,
        Inactive = 1,
        Suspended = 2
    }

    public enum ComputerStatus
    {
        Available = 0,
        InUse = 1,
        Maintenance = 2,
        OutOfOrder = 3
    }

    public enum TransactionType
    {
        Deposit = 0,
        Withdrawal = 1,
        ComputerUsage = 2,
        ServiceCharge = 3,
        Refund = 4
    }

    public enum SessionStatus
    {
        Active = 0,
        Completed = 1,
        Terminated = 2,
        TimedOut = 3
    }

    public enum PaymentMethod
    {
        Cash = 0,
        CreditCard = 1,
        DebitCard = 2,
        ElectronicWallet = 3
    }
}
