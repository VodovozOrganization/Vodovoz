﻿using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Orders;

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

		public override ReferenceNode CreateReferenceTo(CounterpartyContract contract)
		{
			int id = GetReferenceId(contract);

			if(exportData.ExportMode == Export1cMode.BuhgalteriaOOO && contract.Organization.INN != "7816453294")
				exportData.Errors.Add($"Выгрузка в 1с возможна только для организации ООО \"Весёлый водовоз\" (ИНН 7816453294). Договор {contract.Title} оформлен на дугую организацию.");

			return new ReferenceNode(id,
				new PropertyNode("Наименование",
					Common1cTypes.String,
				                 contract.TitleIn1c
				),
				new PropertyNode("ЭтоГруппа",
					Common1cTypes.Boolean
				),
				new PropertyNode("Владелец",
					Common1cTypes.ReferenceCounterparty,
					exportData.CounterpartyCatalog.CreateReferenceTo(contract.Counterparty)
				),
				new PropertyNode("Организация",
					Common1cTypes.ReferenceOrganization,
				                 exportData.OrganizationCatalog.CreateReferenceTo(contract.Organization)
				),
				new PropertyNode("ВидДоговора",
					Common1cTypes.EnumContractType,
					"СПокупателем"
				)
			);
		}

		public ReferenceNode CreateReferenceToContract(Domain.Orders.Order order)
		{
			if(order.Contract != null)
				return CreateReferenceTo(order.Contract);

			var contract = new VirtualContract(
				order.Client,
				order.Contract.Organization,
				order.Contract.Number
			);

			return CreateReferenceTo(contract);
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
					exportData.CurrencyCatalog.CreateReferenceTo(Currency.Default)
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
				                 contract.Number
				)
			);
			return properties.ToArray();
		}
	}

	public class VirtualContract : CounterpartyContract
	{
		string title;

		public override string TitleIn1c => title;

		public VirtualContract(Counterparty counterparty, Organization organization, string title)
		{
			//В договор создаем виртуальный номер id, по номеру контрагента но отрицательный, чтобы не пересекалось с настоящими id договоров.
			//Поиск в GetReferenceId ищет по id.
			Id = -counterparty.Id;
			ContractSubNumber = 1;
			Counterparty = counterparty;
			Organization = organization;
			this.title = title;
		}
	}
}

