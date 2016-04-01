using System;
using QSBanks;
using System.Collections.Generic;
using Vodovoz.Domain;

namespace Vodovoz.ExportTo1c.Catalogs
{
	public class AccountCatalog : GenericCatalog<Account>
	{
		public AccountCatalog(ExportData exportData)
			:base(exportData)
		{			
		}

		protected override string Name
		{
			get{return "БанковскиеСчета";}
		}

		public override ReferenceNode GetReferenceTo(Account account)
		{
			int id = GetReferenceId(account);
			return new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					account.Code1c
				),
				new PropertyNode("Владелец",
					Common1cTypes.ReferenceCounterparty
				)
			);

		}

		public ReferenceNode GetReferenceTo(Account account, Counterparty owner)
		{
			int id = GetReferenceId(account, owner);
			var code1c = account.Code1c ?? account.Id.ToString();
			return new ReferenceNode(id,
				new PropertyNode("Код",
					Common1cTypes.String,
					code1c
				),
				new PropertyNode("Владелец",
					Common1cTypes.ReferenceCounterparty,
					exportData.CounterpartyDirectory.GetReferenceTo(owner)
				)
			);

		}
		protected override PropertyNode[] GetProperties(Account account)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					account.Name
				)
			);
			properties.Add(
				new PropertyNode("Банк",
					Common1cTypes.ReferenceBank,
					exportData.BankDirectory.GetReferenceTo(account.InBank)
				)
			);
			properties.Add(
				new PropertyNode("БанкДляРасчетов",
					Common1cTypes.ReferenceBank
				)
			);
			properties.Add(
				new PropertyNode("ВалютаДенежныхСредств",
					Common1cTypes.ReferenceCurrency,
					exportData.CurrencyDirectory.GetReferenceTo(ExportTo1c.Currency.Default)
				)
			);
			properties.Add(
				new PropertyNode("ВидСчета",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("ДатаЗакрытия",
					Common1cTypes.Date
				)
			);
			properties.Add(
				new PropertyNode("ДатаОткрытия",
					Common1cTypes.Date
				)
			);
			properties.Add(
				new PropertyNode("МесяцПрописью",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new PropertyNode("ТекстНазначения",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("НомерИДатаРазрешения",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("НомерСчета",
					Common1cTypes.String,
					account.Number
				)
			);
			properties.Add(
				new PropertyNode("ТекстКорреспондента",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("СуммаБезКопеек",
					Common1cTypes.Boolean
				)
			);
			return properties.ToArray();
		}

		public int GetReferenceId(Account account, Counterparty owner)
		{
			int id;
			if (!items.TryGetValue(account, out id))
			{
				id = ++exportData.objectCounter;
				items.Add(account, id);
				Add(account, owner);
			}
			return id;
		}

		public void Add(Account account, Counterparty owner)
		{
			var item = new CatalogObjectNode
				{				
					Id = GetReferenceId(account,owner),
					CatalogueType = this.Name
				};
			item.Reference = GetReferenceTo(account,owner);
			item.Properties.AddRange(GetProperties(account,owner));
			exportData.Objects.Add(item);
		}


		public PropertyNode[] GetProperties(Account account, Counterparty owner)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			var name = String.IsNullOrWhiteSpace(account.Name) ? account.Number : account.Name;
			properties.Add(
				new PropertyNode("Наименование",
					Common1cTypes.String,
					name
				)
			);
			properties.Add(
				new PropertyNode("Банк",
					Common1cTypes.ReferenceBank,
					exportData.BankDirectory.GetReferenceTo(account.InBank)
				)
			);
			properties.Add(
				new PropertyNode("БанкДляРасчетов",
					Common1cTypes.ReferenceBank
				)
			);
			properties.Add(
				new PropertyNode("ВалютаДенежныхСредств",
					Common1cTypes.ReferenceCurrency,
					exportData.CurrencyDirectory.GetReferenceTo(ExportTo1c.Currency.Default)
				)
			);
			properties.Add(
				new PropertyNode("ВидСчета",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("ДатаЗакрытия",
					Common1cTypes.Date
				)
			);
			properties.Add(
				new PropertyNode("ДатаОткрытия",
					Common1cTypes.Date
				)
			);
			properties.Add(
				new PropertyNode("МесяцПрописью",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new PropertyNode("ТекстНазначения",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("НомерИДатаРазрешения",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("НомерСчета",
					Common1cTypes.String,
					account.Number
				)
			);
			properties.Add(
				new PropertyNode("ТекстКорреспондента",
					Common1cTypes.String
				)
			);
			properties.Add(
				new PropertyNode("СуммаБезКопеек",
					Common1cTypes.Boolean
				)
			);
			return properties.ToArray();
		}
	}
}

