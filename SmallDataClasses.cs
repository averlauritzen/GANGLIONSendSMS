using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GANGLIONSendSMS
{
    public class SmsReciept
    {
        public List<string> SmsIDNumbers { get; set; } = new List<string>();
        public string Valuta { get; set; } = "";
        public string Pris { get; set; } = "";
    }

    class SmsContent
    {
        public string Telefonnummer { get; set; }
        public string Landekode { get; set; }
        public string AfsenderNavn { get; set; }
        public string Besked { get; set; }
        public string ID { get; set; }
    }
}
