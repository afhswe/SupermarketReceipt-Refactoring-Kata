using System;

namespace SupermarketReceipt
{
    public class InvalidProductException : Exception
    {
        public InvalidProductException(string message) : base(message)
        {

        }
    }
}