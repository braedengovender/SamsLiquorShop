using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace SamsLiquorShop.Helpers
{
    /// <summary>Simulated card gateway. No money moves. Outcome depends on the last four digits of the card.</summary>
    public static class PaymentGateway
    {
        public class Result
        {
            public bool Approved;
            public string Reference;
            public string Message;   // plain text
        }

        public const decimal MaxAmount = 20000m;

        public static Result Authorise(string cardNumber, string expiry, decimal amount)
        {
            try
            {
                string digits = Regex.Replace(cardNumber ?? "", @"\D", "");
                if (digits.Length < 13) return Declined("The payment portal couldn't read that card number. Please check it and try again.");
                if (amount <= 0m) return Declined("There is nothing to pay on this order.");
                if (amount > MaxAmount) return Declined("This amount is above the single-payment limit for online card payments. Please contact us on 082 405 5932.");

                switch (digits.Substring(digits.Length - 4))
                {
                    case "0002": return Declined("Your card was declined by your bank. Please try a different card.");
                    case "9995": return Declined("Your card has insufficient funds for this payment. Please try a different card.");
                    case "0069": return Declined("Your bank reports that this card has expired. Please use a different card.");
                }

                return new Result { Approved = true, Reference = NewReference(), Message = "Approved" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError("PaymentGateway.Authorise: " + ex);
                return Declined("The payment portal is unavailable right now. You were not charged. Please try again in a moment.");
            }
        }

        private static Result Declined(string message)
        {
            return new Result { Approved = false, Message = message };
        }

        private static string NewReference()
        {
            const string alphabet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
            var bytes = new byte[8];
            using (var rng = new RNGCryptoServiceProvider()) rng.GetBytes(bytes);
            var chars = new char[8];
            for (int i = 0; i < chars.Length; i++) chars[i] = alphabet[bytes[i] % alphabet.Length];
            return "SP-" + DateTime.UtcNow.ToString("yyMMdd") + "-" + new string(chars);
        }
    }
}