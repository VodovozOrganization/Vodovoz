using System;
using System.Xml.Serialization;

namespace Vodovoz.Nodes
{
	[Serializable]
	public class AvangardOperation
	{
		private string _transDateString;

		[XmlElement("trans_date")]
		public string TransDateString
		{
			get => _transDateString;
			set
			{
				_transDateString = value;
				TransDate = DateTime.Parse(_transDateString);
			}
		}
		[XmlIgnore]
		public DateTime TransDate { get; private set; }
		[XmlElement("terminal")]
		public string Terminal { get; set; }
		[XmlElement("type")]
		public string Type { get; set; }
		[XmlElement("status")]
		public string Status { get; set; }
		[XmlElement("checkenum")]
		public string CheckEnum { get; set; }
		[XmlElement("authcode")]
		public string Authcode { get; set; }
		[XmlElement("rrn")]
		public string Rrn { get; set; }
		[XmlElement("amount")]
		public decimal Amount { get; set; }
		[XmlElement("bank_fee_amount")]
		public decimal BankFeeAmount { get; set; }
		[XmlElement("bank_fee_rate")]
		public decimal BankFeeRate { get; set; }
		[XmlElement("currency")]
		public string Currency { get; set; }
		[XmlElement("card")]
		public string Card { get; set; }
		[XmlElement("order_number")]
		public int OrderNumber { get; set; }
		[XmlElement("order_description")]
		public string OrderDescription { get; set; }
		[XmlElement("client_name")]
		public string ClientName { get; set; }
		[XmlElement("client_address")]
		public string ClientAddress { get; set; }
		[XmlElement("client_email")]
		public string ClientEmail { get; set; }
		[XmlElement("client_phone")]
		public string ClientPhone { get; set; }
		[XmlElement("reg_num")]
		public string RegNum { get; set; }
	}
}
