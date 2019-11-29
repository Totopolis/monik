using Monik.Client;

namespace Monik.Flooder.Settings
{
    public class MonikSettings
    {
        public int Delay { get; set; } = 1000;
        public MonikSenderType SenderType { get; set; }
        public MonikSenderSettings SenderSettings { get; set; }
        public ClientSettings ClientSettings { get; set; } 
    }
}