using System;
using System.Collections.Generic;

namespace InternetCafe.Domain.ValueObjects
{
    public class Money : ValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        private Money() { }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        public static Money FromVND(decimal amount)
        {
            return new Money(amount, "VND");
        }

        public static Money Zero(string currency = "VND")
        {
            return new Money(0, currency);
        }

        public Money Add(Money money)
        {
            if (Currency != money.Currency)
                throw new InvalidOperationException("Cannot add money with different currencies");

            return new Money(Amount + money.Amount, Currency);
        }

        public Money Subtract(Money money)
        {
            if (Currency != money.Currency)
                throw new InvalidOperationException("Cannot subtract money with different currencies");

            return new Money(Amount - money.Amount, Currency);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
            yield return Currency;
        }

        public override string ToString()
        {
            return $"{Amount:N0} {Currency}";
        }
    }
}