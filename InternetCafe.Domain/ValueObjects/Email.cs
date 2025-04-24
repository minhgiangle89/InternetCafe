using System;
using System.Collections.Generic;

namespace InternetCafe.Domain.ValueObjects
{
    public class Email : ValueObject
    {
        public string Value { get; private set; }

        private Email() { }

        public Email(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty", nameof(value));

            if (!IsValidEmail(value))
                throw new ArgumentException("Invalid email format", nameof(value));

            Value = value;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
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