using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vodovoz.Domain.Payments
{
    public class PaymentsFromYookassaParser
    {
        public string DocPath { get; private set; }
        public List<PaymentFromYookassa> PaymentsFromYookassa { get; set; }

        public PaymentsFromYookassaParser(string documentPath)
        {
            DocPath = documentPath;
        }

        public void Parse()
        {
	        int i;
	        string line;
	        
            using(var reader = new StreamReader(DocPath, Encoding.GetEncoding(1251))) 
			{
				int count = 1;

				if(reader.ReadLine() != "1CClientBankExchange")
					return;
				
				while((line = reader.ReadLine()) != null) 
				{
					//Читаем документы
					/*while(!line.StartsWith(tags[3]))
					{
						if(line.StartsWith(tags[2])) 
							TransferDocuments.Add(doc);
						
						if(line.StartsWith(tags[1])) 
							doc = new TransferDocument();

						if(!string.IsNullOrWhiteSpace(line))
						{
							var data = line.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);

							if(data.Length == 2)
								FillData(doc, data, culture);
						}
						line = reader.ReadLine();
					}*/
				}
			}
        }
    }
}