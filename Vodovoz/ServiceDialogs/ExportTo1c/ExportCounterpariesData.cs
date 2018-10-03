using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Gamma.Utilities;
using QSOrmProject;
using Vodovoz.ExportTo1c;
using Vodovoz.Repository;

namespace Vodovoz.ServiceDialogs.ExportTo1c
{
	public class ExportCounterpariesData : IXmlConvertable
	{
		public string Version { get; set; }
		public DateTime ExportDate { get; set; }
		public List<string> Errors = new List<string>();

		public List<ObjectNode> Objects { get; private set; }

		public readonly IUnitOfWork UoW;

		XElement Xml;

		public XElement ToXml() => Xml;

		public ExportCounterpariesData(IUnitOfWork uow)
		{
			this.Objects = new List<ObjectNode>();
			this.UoW = uow;

			this.Version = "1.0";
			this.ExportDate = DateTime.Now;
			Xml	= new XElement(
				"ExchangeFile",
				new XAttribute("FormatVersion", Version),
				new XAttribute("Generated", ExportDate.ToString("s"))
			);
		}

		public void AddCounterparty(CounterpartyTo1CNode counterparty)
		{
			Xml.Add(
				new XElement(
					"Counterparty",
					new XAttribute(
						counterparty.GetPropertyName(x => x.Id),
						counterparty.Id
					),
					new XElement(
						counterparty.GetPropertyName(x => x.Name),
						counterparty.Name
					),
					new XElement(
						counterparty.GetPropertyName(x => x.Inn),
						counterparty.Inn
					),
					new XElement(
						counterparty.GetPropertyName(x => x.Phones),
						counterparty.Phones
					),
					new XElement(
						counterparty.GetPropertyName(x => x.EMails),
						counterparty.EMails
					)
				)
			);
		}
	}
}
