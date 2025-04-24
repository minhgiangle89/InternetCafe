using System;
using System.Collections.Generic;

namespace InternetCafe.Domain.ValueObjects
{
    public class IPAddressValue : ValueObject
    {
        public string Value { get; private set; }

        private IPAddressValue() { }

        public IPAddressValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("IP Address cannot be empty", nameof(value));

            if (!IsValidIPAddress(value))
                throw new ArgumentException("Invalid IP Address format", nameof(value));

            Value = value;
        }

        private bool IsValidIPAddress(string ipAddress)
        {
            return System.Net.IPAddress.TryParse(ipAddress, out _);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }

        public override string ToString()
        {
            return Value;
        }
    }
}