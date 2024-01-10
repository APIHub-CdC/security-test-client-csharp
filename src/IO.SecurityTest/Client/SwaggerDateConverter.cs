using Newtonsoft.Json.Converters;

namespace IO.SecurityTest.Client
{
    public class ApihubDateConverter : IsoDateTimeConverter
    {
        public ApihubDateConverter()
        {
            DateTimeFormat = "yyyy-MM-dd";
        }
    }
}
