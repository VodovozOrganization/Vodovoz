using System;
using Vodovoz.Domain;
using System.Collections.Generic;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class ContractCatalog : GenericCatalog<CounterpartyContract>
	{
		public ContractCatalog(ExportData exportData)
			:base(exportData)
		{			
		}
		protected override string Name
		{
			get{return "ДоговорыКонтрагентов";}
		}

		public override ReferenceNode GetReferenceTo(CounterpartyContract contract)
		{
			int id = GetReferenceId(contract);
			var organization = exportData.CashlessOrganization;
			return new ReferenceNode(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					contract.Title
				),
				new PropertyNode("ЭтоГруппа",
					Common1cTypes.Boolean
				),
				new PropertyNode("Владелец",
					Common1cTypes.ReferenceCounterparty,
					exportData.CounterpartyDirectory.GetReferenceTo(contract.Counterparty)
				),
				new PropertyNode("Организация",
					Common1cTypes.ReferenceOrganization,
					exportData.OrganizationDirectory.GetReferenceTo(organization)
				),
				new PropertyNode("ВидДоговора",
					Common1cTypes.EnumContractType,
					"СПокупателем"
				)
			);
		}
		protected override PropertyNode[] GetProperties(CounterpartyContract contract)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("Родитель",
					Common1cTypes.ReferenceContract
				)
			);
			properties.Add(
				new PropertyNode("ВалютаВзаиморасчетов",
					Common1cTypes.ReferenceCurrency,
					exportData.CurrencyDirectory.GetReferenceTo(Currency.Default)
				)
			);
			properties.Add(
				new PropertyNode("ТипЦен",
					Common1cTypes.ReferencePriceType
				)
			);
			// TODO WTF!!! там 2 таких: один - null, другой true
			properties.Add(
				new PropertyNode("РасчетыВУсловныхЕдиницах",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new PropertyNode("Код",
					Common1cTypes.String,
					contract.Id
				)
			);
			return properties.ToArray();
		}
	}
}

