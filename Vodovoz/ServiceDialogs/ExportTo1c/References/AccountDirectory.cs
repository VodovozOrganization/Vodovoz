using System;
using QSBanks;
using System.Collections.Generic;
using Vodovoz.Domain;

namespace Vodovoz.ExportTo1c.References
{
	public class AccountDirectory : GenericDirectory<Account>
	{
		public AccountDirectory(ExportData exportData)
			:base(exportData)
		{			
		}

		protected override string Name
		{
			get{return "БанковскиеСчета";}
		}

		public override ExportReferenceNode GetReferenceTo(Account account)
		{
			int id = GetReferenceId(account);
			return new ExportReferenceNode(id,
				new ExportPropertyNode("Код",
					Common1cTypes.String,
					account.Code1c
				),
				new ExportPropertyNode("Владелец",
					Common1cTypes.ReferenceCounterparty
				)
			);

		}

		public ExportReferenceNode GetReferenceTo(Account account, Counterparty owner)
		{
			int id = GetReferenceId(account, owner);
			var code1c = account.Code1c ?? account.Id.ToString();
			return new ExportReferenceNode(id,
				new ExportPropertyNode("Код",
					Common1cTypes.String,
					code1c
				),
				new ExportPropertyNode("Владелец",
					Common1cTypes.ReferenceCounterparty,
					exportData.CounterpartyDirectory.GetReferenceTo(owner)
				)
			);

		}
		protected override ExportPropertyNode[] GetProperties(Account account)
		{
			var properties = new List<ExportPropertyNode>();
			properties.Add(
				new ExportPropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new ExportPropertyNode("Наименование",
					Common1cTypes.String,
					account.Name
				)
			);
			properties.Add(
				new ExportPropertyNode("Банк",
					Common1cTypes.ReferenceBank,
					exportData.BankDirectory.GetReferenceTo(account.InBank)
				)
			);
			properties.Add(
				new ExportPropertyNode("БанкДляРасчетов",
					Common1cTypes.ReferenceBank
				)
			);
			properties.Add(
				new ExportPropertyNode("ВалютаДенежныхСредств",
					Common1cTypes.ReferenceCurrency,
					exportData.CurrencyDirectory.GetReferenceTo(ExportTo1c.Currency.Default)
				)
			);
			properties.Add(
				new ExportPropertyNode("ВидСчета",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("ДатаЗакрытия",
					Common1cTypes.Date
				)
			);
			properties.Add(
				new ExportPropertyNode("ДатаОткрытия",
					Common1cTypes.Date
				)
			);
			properties.Add(
				new ExportPropertyNode("МесяцПрописью",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new ExportPropertyNode("ТекстНазначения",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("НомерИДатаРазрешения",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("НомерСчета",
					Common1cTypes.String,
					account.Number
				)
			);
			properties.Add(
				new ExportPropertyNode("ТекстКорреспондента",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("СуммаБезКопеек",
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
			var item = new ExchangeCatalogueObject
				{				
					Id = GetReferenceId(account,owner),
					CatalogueType = this.Name
				};
			item.Reference = GetReferenceTo(account,owner);
			item.Properties.AddRange(GetProperties(account,owner));
			exportData.Objects.Add(item);
		}


		public ExportPropertyNode[] GetProperties(Account account, Counterparty owner)
		{
			var properties = new List<ExportPropertyNode>();
			properties.Add(
				new ExportPropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			var name = String.IsNullOrWhiteSpace(account.Name) ? account.Number : account.Name;
			properties.Add(
				new ExportPropertyNode("Наименование",
					Common1cTypes.String,
					name
				)
			);
			properties.Add(
				new ExportPropertyNode("Банк",
					Common1cTypes.ReferenceBank,
					exportData.BankDirectory.GetReferenceTo(account.InBank)
				)
			);
			properties.Add(
				new ExportPropertyNode("БанкДляРасчетов",
					Common1cTypes.ReferenceBank
				)
			);
			properties.Add(
				new ExportPropertyNode("ВалютаДенежныхСредств",
					Common1cTypes.ReferenceCurrency,
					exportData.CurrencyDirectory.GetReferenceTo(ExportTo1c.Currency.Default)
				)
			);
			properties.Add(
				new ExportPropertyNode("ВидСчета",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("ДатаЗакрытия",
					Common1cTypes.Date
				)
			);
			properties.Add(
				new ExportPropertyNode("ДатаОткрытия",
					Common1cTypes.Date
				)
			);
			properties.Add(
				new ExportPropertyNode("МесяцПрописью",
					Common1cTypes.Boolean
				)
			);
			properties.Add(
				new ExportPropertyNode("ТекстНазначения",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("НомерИДатаРазрешения",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("НомерСчета",
					Common1cTypes.String,
					account.Number
				)
			);
			properties.Add(
				new ExportPropertyNode("ТекстКорреспондента",
					Common1cTypes.String
				)
			);
			properties.Add(
				new ExportPropertyNode("СуммаБезКопеек",
					Common1cTypes.Boolean
				)
			);
			return properties.ToArray();
		}
	}
}

