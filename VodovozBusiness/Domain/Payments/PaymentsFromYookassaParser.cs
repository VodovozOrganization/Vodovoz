using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Vodovoz.Domain.Payments
{
    public class PaymentsFromYookassaParser
    {
	    private readonly string docPath;
	    private const string ShopVodovozString = "https://shop.vodovoz-spb.ru";
	    private const string VodovozString = "https://vodovoz-spb.ru";
	    private const string ShopVodovozUberserverString = "https://shopvodovoz.uberserver.ru";
	    
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
		        var count = 0;
		        var paymentByCardFrom = PaymentByCardOnlineFrom.FromVodovozWebSite;
		        
		        while((line = reader.ReadLine()) != null)
		        {
			        count++;
					if(line == string.Empty) continue;
					
					if (count == 1) {
						var paymentFrom = line.Split(new [] {' '}, StringSplitOptions.RemoveEmptyEntries);
						TryMatchPaymentFrom(paymentFrom[3].Trim('.'), ref paymentByCardFrom);
					}
					
					var data = line.Split(new [] {';'}, StringSplitOptions.RemoveEmptyEntries);
				
					if (Guid.TryParse(data[0], out Guid result)) {
						var payment = new PaymentByCardOnline(data, paymentByCardFrom);
						PaymentsFromYookassa.Add(payment);
					}
		        }
	        }
        }

        private void TryMatchPaymentFrom(string data, ref PaymentByCardOnlineFrom paymentByCardFrom)
        {
	        if (data == VodovozString) {
		        paymentByCardFrom = PaymentByCardOnlineFrom.FromSMS;
	        }
	        else if (data == ShopVodovozString) {
		        paymentByCardFrom = PaymentByCardOnlineFrom.FromEShop;
	        }
	        else if (data == ShopVodovozUberserverString) {
		        paymentByCardFrom = PaymentByCardOnlineFrom.FromVodovozWebSite;
	        }
	        else {
		        throw new ArgumentException("Невозможно определить откуда оплата.");
	        }
        }
    }
}