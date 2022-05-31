using System;

namespace SupermarketReceipt
{
    public class InvalidProductNameException : Exception
    {
        public InvalidProductNameException(string message) : base(message)
        {

        }
    }
}