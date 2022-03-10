namespace SupermarketReceipt
{
    public interface INotificationService
    {
        void SendReceipt(Receipt receipt);
    }
}