using Utconnect.Common.Configurations.Models;

namespace Utconnect.Coffer.Models
{
    public class CofferConfig : ISiteConfig
    {
        public string Url { get; set; } = default!;
    }
}