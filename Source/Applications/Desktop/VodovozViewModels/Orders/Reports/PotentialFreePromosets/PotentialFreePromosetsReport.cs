using QS.DomainModel.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Presentation.ViewModels.Reports;

namespace Vodovoz.ViewModels.Orders.Reports.PotentialFreePromosets
{
	[Appellative(Nominative = "Отчет по потенциальным халявщикам")]
	public class PotentialFreePromosetsReport : IClosedXmlReport
	{
		private const string _dateFormatString = "dd.MM.yyyy";
		public PotentialFreePromosetsReport(IEnumerable<PromosetReportRow> rows)
		{
			//var row1 = new PromosetReportRow
			//{
			//	SequenceNumber = 1,
			//	Address = "Адерес 1",
			//	AddressCategory = "Прочее",
			//	Phone = "(921) 234-23-23",
			//	Client = "Клиент",
			//	Order = 123456,
			//	OrderCreationDate = DateTime.Now,
			//	OrderDeliveryDate = DateTime.Now,
			//	Promoset = "Водный Бум",
			//	Author = "Автор А.Б.",
			//	IsRootRow = true
			//};

			//var row2 = new PromosetReportRow
			//{
			//	SequenceNumber = 2,
			//	Address = "Адерес 2",
			//	AddressCategory = "Квартира",
			//	Phone = "(921) 234-23-23",
			//	Client = "Клиент",
			//	Order = 123458,
			//	OrderCreationDate = DateTime.Now,
			//	OrderDeliveryDate = DateTime.Now,
			//	Promoset = "Оптимальный",
			//	Author = "Автор А.Б.",
			//	IsRootRow = true
			//};

			Rows = rows.ToList();
		}

		public string TemplatePath => @".\Reports\Orders\PotentialFreePromosetsReport.xlsx";
		public IList<PromosetReportRow> Rows { get; set; } = new List<PromosetReportRow>();
		public string StartDateString => DateTime.Now.ToString(_dateFormatString);
		public string EndDateString => DateTime.Now.ToString(_dateFormatString);

		public class PromosetReportRow
		{
			public int SequenceNumber { get; set; }
			public string Address { get; set; }
			public string AddressCategory { get; set; }
			public string Phone { get; set; }
			public string Client { get; set; }
			public int Order { get; set; }
			public DateTime? OrderCreationDate { get; set; }
			public DateTime? OrderDeliveryDate { get; set; }
			public string Promoset { get; set; }
			public string Author { get; set; }
			public bool IsRootRow { get; set; }
		}
	}
}
