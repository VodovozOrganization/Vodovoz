using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using QSDocTemplates;
using QSProjectsLib;
using Vodovoz.Domain.Employees;
using Vodovoz.Domain.Orders;

namespace Vodovoz.DocTemplates
{
	public class M2ProxyDocumentParser : DocParserBase<M2ProxyDocument>
	{
		public M2ProxyDocumentParser()
		{
		}

		public override void UpdateFields()
		{
			fieldsList.Clear();

			AddField(x => x.Id, "НомерДоверенности", PatternFieldType.FString);
			AddField(x => x.Date.ToString("dd.MM.yyyy"), "ДатаДоверенности", PatternFieldType.FString);
			AddField(x => x.ExpirationDate.ToString("dd.MM.yyyy"), "ДатаОкончания", PatternFieldType.FString);
			AddField(x => x.TicketDate.Year == 1 ? "\t" : x.TicketDate.ToString("dd.MM.yyyy"), "ДатаНаряда", PatternFieldType.FString);
			AddField(x => x.TicketNumber ?? "\t", "НомерНаряда", PatternFieldType.FString);

			AddField(x => x.Organization.Name, PatternFieldType.FString);
			AddField(x => x.Organization.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.Address, PatternFieldType.FString);
			AddField(x => x.Organization.JurAddress, PatternFieldType.FString);
			AddField(x => x.Organization.INN, PatternFieldType.FString);
			AddField(x => x.Organization.KPP, PatternFieldType.FString);
			AddField(x => x.Organization.OKPO, PatternFieldType.FString);
			AddField(x => x.Organization.OKVED, PatternFieldType.FString);
			AddField(x => x.Organization.OGRN, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.Number, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.InBank.Bik, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.InBank.Name, PatternFieldType.FString);
			AddField(x => x.Organization.DefaultAccount.InBank.CorAccount, PatternFieldType.FString);
			AddField(x => x.Organization.Leader.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.Leader.ShortName, PatternFieldType.FString);
			AddField(x => x.Organization.Buhgalter.FullName, PatternFieldType.FString);
			AddField(x => x.Organization.Buhgalter.ShortName, PatternFieldType.FString);

			AddField(x => x.Supplier.Name, PatternFieldType.FString);

			AddField(x => x.Employee.FullName, PatternFieldType.FString);
			AddField(x => x.Employee.ShortName, PatternFieldType.FString);
			AddField(x => x.Employee.PassportSeria, PatternFieldType.FString);
			AddField(x => x.Employee.PassportNumber, PatternFieldType.FString);
			AddField(x => x.Employee.PassportIssuedOrg, PatternFieldType.FString);
			AddField(x => x.Employee.PassportIssuedDate.HasValue ? x.Employee.PassportIssuedDate.Value.ToString("dd.MM.yyyy") : "", "Водитель.ДатаВыдачиПаспорта", PatternFieldType.FString);
			AddField(x => x.Employee.AddressRegistration, PatternFieldType.FString);

			SortFields();
		}

		public void AddTableEquipmentFromClient(List<OrderEquipment> list)
		{
			if(list == null)
				return;
			List<M2ProxyDocumentParserNode> result = new List<M2ProxyDocumentParserNode>();
			foreach(var item in list) {
				var node = new M2ProxyDocumentParserNode();
				node.FullNameString = item.FullNameString;
				node.Units = "шт.";//оборудование только в штуках
				node.Count = item.Count.ToString();
				node.CountString = String.Format("{0} ({1})", node.Count, RusNumber.Str(item.Count, true, "", "", "").Trim());
				result.Add(node);
			}

			customTablesList.Clear();
			AddCustomTable<M2ProxyDocumentParserNode>("ОборудованиеОтКлиента", result)
				.AddColumn(x => x.FullNameString, PatternFieldType.FString)
				.AddColumn(x => x.Count, PatternFieldType.FString)
				.AddColumn(x => x.CountString, PatternFieldType.FString);
			SortFields();
		}

		public class M2ProxyDocumentParserNode
		{
			[Display(Name = "Наименование")]
			public string FullNameString { get; set; }
			[Display(Name = "Единицы")]
			public string Units { get; set; }
			[Display(Name = "Количество")]
			public string Count { get; set; }
			[Display(Name = "Пропись")]
			public string CountString { get; set; }
		}
	}
}
