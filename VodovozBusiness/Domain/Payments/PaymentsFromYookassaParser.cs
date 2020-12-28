using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vodovoz.Domain.Payments
{
    public class PaymentsFromYookassaParser
    {
	    private readonly string docPath;
        public List<PaymentByCardOnline> PaymentsFromYookassa { get; set; } = new List<PaymentByCardOnline>();

        public PaymentsFromYookassaParser(string docPath)
        {
            this.docPath = docPath;
        }

        public void Parse()
        {
	        using(var reader = new StreamReader(docPath, Encoding.GetEncoding(1251)))
	        {
		        string line;
		        while((line = reader.ReadLine()) != null)
				{
					if(line == string.Empty) continue;
					
					var data = line.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
					
					if (Guid.TryParse(data[0], out Guid result))
					{
						var payment = new PaymentByCardOnline(data, false);
						PaymentsFromYookassa.Add(payment);
					}
				}
	        }
        }
    }
}