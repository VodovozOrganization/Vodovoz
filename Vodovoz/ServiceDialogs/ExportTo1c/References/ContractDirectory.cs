using System;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.References
{
	public class ContractDirectory : GenericDirectory<CounterpartyContract>
	{
		public ContractDirectory(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "ДоговорыКонтрагентов";}
		}

		public override ExportReferenceNode GetReferenceTo(CounterpartyContract contract)
		{
			int id = GetReferenceId(contract);
			var organization = exportData.CashlessOrganization;
			return new ExportReferenceNode(
				new ExportPropertyNode("Наименование",
					Common1cTypes.String,
					contract.Title
				),
				new ExportPropertyNode("ЭтоГруппа",
					Common1cTypes.Boolean
				),
				new ExportPropertyNode("Владелец",
					Common1cTypes.ReferenceCounterparty,
					exportData.CounterpartyDirectory.GetReferenceTo(contract.Counterparty)
				),
				new ExportPropertyNode("Организация",
					Common1cTypes.ReferenceOrganization,
					exportData.OrganizationDirectory.GetReferenceTo(organization)
				),
				new ExportPropertyNode("ВидДоговора",
					Common1cTypes.EnumContractType,
					"СПокупателем"
				)
			);
		}
		protected override ExportPropertyNode[] GetProperties(CounterpartyContract contract)
		{
			var properties = new List<ExportPropertyNode>();
			properties.Add(
				new ExportPropertyNode("Родитель",
					Common1cTypes.ReferenceContract
				)
			);
			properties.Add(
				new ExportPropertyNode("ВалютаВзаиморасчетов",
					Common1cTypes.ReferenceCurrency,
					exportData.CurrencyDirectory.GetReferenceTo(Currency.Default)
				)
			);
			properties.Add(
				new ExportPropertyNode("ТипЦен",
					Common1cTypes.ReferencePriceType
				)
			);
			// TODO WTF!!! там 2 таких: один - null, другой true
			properties.Add(
				new ExportPropertyNode("РасчетыВУсловныхЕдиницах",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new ExportPropertyNode("Код",
					Common1cTypes.String,
					contract.Id
				)
			);
			return properties.ToArray();
		}
	}
}

