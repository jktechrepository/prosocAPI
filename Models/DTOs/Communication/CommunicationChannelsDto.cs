namespace Prosoc.Models.DTOs
{
    public class CommunicationChannelsDto
    {
        public bool Email { get; set; } = true;
        public bool SMS { get; set; } = true;
        public bool PushNotification { get; set; } = true;
        public bool WhatsApp { get; set; } = false;
        public string? PreferredChannel { get; set; }
    }
}
