using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp1
{
    public class RegistoAssiduidade
    {
        public string Data { get; set; }
        public string E1 { get; set; }
        public string S1 { get; set; }
        public string E2 { get; set; }
        public string S2 { get; set; }
        public string E3 { get; set; }
        public string S3 { get; set; }
        public string E4 { get; set; }
        public string S4 { get; set; }
        public string TotalHoras { get; set; }
        public string Tipo { get; set; }
    

    public DateTime ParsedDate
        {
            get
            {
                if (DateTime.TryParseExact(Data, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    return date.Date;
                }
                if (DateTime.TryParseExact(Data, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
                {
                    return date.Date;
                }
                return DateTime.MinValue;
            }
        }


    }
}