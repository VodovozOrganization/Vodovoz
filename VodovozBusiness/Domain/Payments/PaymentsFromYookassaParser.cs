using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Vodovoz.Domain.Payments
{
    public class PaymentsFromYookassaParser
    {
        public string DocPath { get; private set; }
        public List<PaymentFromTinkoff> PaymentsFromYookassa { get; set; } = new List<PaymentFromTinkoff>();

        public PaymentsFromYookassaParser(string documentPath)
        {
            DocPath = documentPath;
        }

        public void Parse()
        {
	        string line;
	        
	        var culture = CultureInfo.CreateSpecificCulture("ru-RU");
	        culture.NumberFormat.NumberDecimalSeparator = ".";
	        
            using(var reader = new StreamReader(DocPath, Encoding.GetEncoding(1251)))
            {
	            while((line = reader.ReadLine()) != null)
				{
					if(line == string.Empty) continue;
					
					var data = line.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
					
					if (Guid.TryParse(data[0], out Guid result))
					{
						var payment = new PaymentFromTinkoff(data);
						PaymentsFromYookassa.Add(payment);
					}
				}
			}
        }
    }
}