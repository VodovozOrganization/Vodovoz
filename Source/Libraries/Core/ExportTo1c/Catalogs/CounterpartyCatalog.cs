using System.Collections.Generic;
using ExportTo1c.Library.ExportNodes;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.EntityRepositories.Orders;

namespace ExportTo1c.Library.Catalogs
{
	/// <summary>
	/// Контрагент
	/// </summary>
	public class CounterpartyCatalog : GenericCatalog<Counterparty>
	{
		public CounterpartyCatalog(ExportData exportData)
			: base(exportData)
		{
		}

		protected override string Name
		{
			get { return "Контрагенты"; }
		}

		public override ReferenceNode CreateReferenceTo(Counterparty counterparty)
		{
			if((exportData.ExportMode == Export1cMode.BuhgalteriaOOO || exportData.ExportMode == Export1cMode.ComplexAutomation)
			   && string.IsNullOrWhiteSpace(counterparty.INN))
				exportData.Errors.Add($"Для контрагента {counterparty.Id} - '{counterparty.Name}' не заполнен ИНН.");

			int id = GetReferenceId(counterparty);
			var referenceNode = new ReferenceNode(id,
				new PropertyNode("ИНН",
					Common1cTypes.String,
					counterparty.INN)
			);

			if(exportData.ExportMode == Export1cMode.ComplexAutomation)
			{
				referenceNode.Properties.Add(
					new PropertyNode("КПП",
						Common1cTypes.String,
						counterparty.KPP));

				referenceNode.Properties.Add(
					new PropertyNode("Наименование",
						Common1cTypes.String,
						counterparty.Name));

				referenceNode.Properties.Add(
					new PropertyNode("НаименованиеПолное",
						Common1cTypes.String,
						counterparty.FullName
					));
			}

			return referenceNode;
		}

		protected override PropertyNode[] GetProperties(Counterparty counterparty)
		{
			var properties = new List<PropertyNode>();
			properties.Add(
				new PropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);

			if(exportData.ExportMode != Export1cMode.ComplexAutomation)
			{
				properties.Add(
					new PropertyNode("Наименование",
						Common1cTypes.String,
						counterparty.Name
					)
				);
			}

			if(exportData.ExportMode == Export1cMode.ComplexAutomation)
			{
				properties.Add(
					new PropertyNode("ДополнительнаяИнформация",
						Common1cTypes.String,
						counterparty.Comment
					)
				);
			}
			else
			{
				properties.Add(
					new PropertyNode("Код",
						Common1cTypes.String,
						counterparty.Code1c
					)
				);
				properties.Add(
					new PropertyNode("Родитель",
						Common1cTypes.ReferenceContract
					)
				);

				var account = counterparty.DefaultAccount;
				if(account == null)
				{
					properties.Add(
						new PropertyNode("ОсновнойБанковскийСчет",
							Common1cTypes.ReferenceAccount(exportData.ExportMode)
						)
					);
				}
				else
				{
					properties.Add(
						new PropertyNode("ОсновнойБанковскийСчет",
							Common1cTypes.ReferenceAccount(exportData.ExportMode),
							exportData.AccountCatalog.CreateReferenceTo(counterparty.DefaultAccount, counterparty)
						)
					);
				}

				properties.Add(
					new PropertyNode("Комментарий",
						Common1cTypes.String,
						counterparty.Comment));
			}

			if(exportData.ExportMode == Export1cMode.ComplexAutomation)
			{
				var counterpartyType = counterparty.PersonType == PersonType.legal && counterparty.TypeOfOwnership != "ИП"
					? "ЮридическоеЛицо"
					: "ФизическоеЛицо";

				properties.Add(
					new PropertyNode("ЮридическоеФизическоеЛицо",
						Common1cTypes.EnumNaturalOrLegal,
						counterpartyType
					)
				);
			}
			else
			{
				var counterpartyType = counterparty.PersonType == PersonType.legal
					? "ЮридическоеЛицо"
					: "ФизическоеЛицо";

				properties.Add(
					new PropertyNode("ЮридическоеФизическоеЛицо",
						Common1cTypes.String,
						counterpartyType
					)
				);
			}

			if(exportData.ExportMode != Export1cMode.ComplexAutomation)
			{
				properties.Add(
					new PropertyNode("НаименованиеПолное",
						Common1cTypes.String,
						counterparty.FullName
					)
				);

				properties.Add(
					new PropertyNode("КПП",
						Common1cTypes.String,
						counterparty.KPP
					)
				);
			}

			if(counterparty.MainCounterparty != null && counterparty.MainCounterparty.PersonType != PersonType.natural)
			{
				properties.Add(
					new PropertyNode(
						"ГоловнойКонтрагент",
						Common1cTypes.ReferenceCounterparty,
						exportData.CounterpartyCatalog.CreateReferenceTo(counterparty.MainCounterparty)
					)
				);
			}
			else
			{
				properties.Add(
					new PropertyNode(
						"ГоловнойКонтрагент",
						Common1cTypes.ReferenceCounterparty
					)
				);
			}

			if(exportData.ExportMode == Export1cMode.ComplexAutomation)
			{
				properties.Add(
					new PropertyValueParameterNode("ЮрАдрес",
						Common1cTypes.String,
						counterparty.JurAddress
					)
				);
			}

			return properties.ToArray();
		}
	}
}
