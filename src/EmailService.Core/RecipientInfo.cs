namespace EmailService.Core
{
    public class RecipientInfo
    {
        public RecipientInfo(string address, RecipientType type = RecipientType.To)
        {
            Address = address;
            Type = type;
        }

        public RecipientType Type { get; }

        public string Address { get; }
    }
}
