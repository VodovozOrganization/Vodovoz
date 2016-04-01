using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain;
using Vodovoz.Repository;
using Vodovoz.ExportTo1c.References;
using QSBusinessCommon.Domain;
using System.ComponentModel.DataAnnotations;

namespace Vodovoz.ExportTo1c
{
	public class ExportData : IXmlConvertable
	{		
		public string Version{ get; set;}
		public DateTime ExportDate{ get; set;} 
		public DateTime StartPeriodDate{get;set;}
		public DateTime EndPeriodDate{get;set;}
		public string SourceName{ get; set;}
		public string DestinationName{get;set;}
		public string ConversionRulesId{get;set;}
		public string Comment{ get; set;}
			
		public List<ExchangeObject> Objects{ get; set;}
		public ExportRulesNode ExchangeRules{ get; set; }

		public int objectCounter;

		public IUnitOfWork UoW;

		public AccountDirectory AccountDirectory;
		public BankDirectory BankDirectory;
		public ContractDirectory ContractDirectory;
		public CounterpartyDirectory CounterpartyDirectory;
		public CurrencyDirectory CurrencyDirectory;
		public MeasurementUnitsDirectory MeasurementUnitsDirectory;
		public NomenclatureDirectory NomenclatureDirectory;
		public OrganizationDirectory OrganizationDirectory;
		public WarehouseDirectory WarehouseDirectory;

		public Dictionary<NomenclatureCategory, Nomenclature> CategoryToNomenclatureMap;
		public Organization CashlessOrganization{ get; private set;}

		public ExportData(IUnitOfWork uow)
		{
			this.objectCounter = 0;
			this.Objects = new List<ExchangeObject>();
			this.UoW = uow;
			this.AccountDirectory = new AccountDirectory(this);
			this.BankDirectory = new BankDirectory(this);
			this.ContractDirectory = new ContractDirectory(this);
			this.CounterpartyDirectory = new CounterpartyDirectory(this);
			this.CurrencyDirectory = new CurrencyDirectory(this);
			this.MeasurementUnitsDirectory = new MeasurementUnitsDirectory(this);
			this.NomenclatureDirectory = new NomenclatureDirectory(this);
			this.OrganizationDirectory = new OrganizationDirectory(this);
			this.WarehouseDirectory = new WarehouseDirectory(this);
			this.CashlessOrganization = OrganizationRepository.GetOrganizationByPaymentType(uow, PaymentType.cashless);
			this.CategoryToNomenclatureMap = new Dictionary<NomenclatureCategory, Nomenclature>();
			int i = 0;
			foreach (NomenclatureCategory category in Enum.GetValues(typeof(NomenclatureCategory)).OfType<NomenclatureCategory>())
			{
				var nomenclature = new Nomenclature();
				nomenclature.Id = --i;
				nomenclature.Name = category.GetAttribute<DisplayAttribute>().Name;
				nomenclature.Code1c = category.GetAttribute<Code1c>().Code;
				this.CategoryToNomenclatureMap.Add(category, nomenclature);
			}
			this.ExchangeRules = new ExportRulesNode();
		}				

		public void AddOrder(Order order)
		{
			var goods = order.OrderItems.Where(item => Nomenclature.GetCategoriesForGoods().Contains(item.Nomenclature.Category));
			var exportSaleDocument = new ExchangeDocumentSale{				
				DocumentType="РеализацияТоваровУслуг",
				RuleName = "РеализацияТоваровУслуг",
			};
			exportSaleDocument.Id = ++objectCounter;
			exportSaleDocument.Reference = new ExportReferenceNode(exportSaleDocument.Id,
				new ExportPropertyNode("Номер", Common1cTypes.String, Exports.VodovozTo1cID(order.Id)),
				new ExportPropertyNode("Дата", Common1cTypes.Date, order.DeliveryDate.ToString("s"))
			);

			var exportGoodsTable = new ExportTableNode{
				Name="Товары",
			};

			foreach (var orderItem in goods)
			{
				var record = GetRecord(orderItem);
				exportGoodsTable.Records.Add(record);
				exportSaleDocument.Comission.Comissions.Add(0);
			}

			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("Организация",
					Common1cTypes.ReferenceOrganization,
					OrganizationDirectory.GetReferenceTo(CashlessOrganization)
				)
			);
			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("Комментарий",
					Common1cTypes.String,
					order.Comment
				)
			);
			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("Склад",
					Common1cTypes.ReferenceWarehouse,
					WarehouseDirectory.GetReferenceTo(Warehouse1c.Default)
				)
			);
			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("Контрагент",
					Common1cTypes.ReferenceCounterparty,
					CounterpartyDirectory.GetReferenceTo(order.Client)
				)
			);
			var contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoW,order.Client,order.PaymentType);
			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("ДоговорКонтрагента",
					Common1cTypes.ReferenceContract,
					ContractDirectory.GetReferenceTo(contract)
				)
			);
			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("ВалютаДокумента",
					Common1cTypes.ReferenceCurrency,
					CurrencyDirectory.GetReferenceTo(ExportTo1c.Currency.Default)
				)
			);

			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("УчитыватьНДС",
					Common1cTypes.Boolean,
					"true"
				)
			);

			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("СуммаВключаетНДС",
					Common1cTypes.Boolean,
					"true"
				)
			);
			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("ВидОперации",
					"ПеречислениеСсылка.ВидыОперацийРеализацияТоваров",
					"ПродажаКомиссия"
				)
			);
			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("КурсВзаиморасчетов",
					Common1cTypes.Numeric,
					1
				)
			);
			exportSaleDocument.Properties.Add(
				new ExportPropertyNode("КратностьВзаиморасчетов",
					Common1cTypes.Numeric,
					1
				)
			);

			var exportServicesTable = new ExportTableNode
			{
				Name = "Услуги",
			};
			var services = order.OrderItems.Where(item => item.Nomenclature.Category == NomenclatureCategory.service);
			foreach (var serviceItem in services)
			{
				var record = GetRecord(serviceItem);
				exportServicesTable.Records.Add(record);
			}
			exportSaleDocument.Tables.Add(exportGoodsTable);
			exportSaleDocument.Tables.Add(exportServicesTable);		
	
			Objects.Add(exportSaleDocument);
		}			

		public ExportTableRecordNode GetRecord(OrderItem orderItem)
		{
			var record = new ExportTableRecordNode();
			var nomenclatureReference = NomenclatureDirectory.GetReferenceTo(orderItem.Nomenclature);
			record.Properties.Add(
				new ExportPropertyNode("Номенклатура",
					Common1cTypes.ReferenceNomenclature,
					nomenclatureReference
				)
			);
			if(orderItem.Nomenclature.Category==NomenclatureCategory.service)
				record.Properties.Add(
					new ExportPropertyNode("Содержание",
						Common1cTypes.String,
						orderItem.Nomenclature.Name
					)
				);
			record.Properties.Add(
				new ExportPropertyNode("Количество",
					Common1cTypes.Numeric,
					orderItem.ActualCount
				)
			);
			record.Properties.Add(
				new ExportPropertyNode("Цена",
					Common1cTypes.Numeric,
					orderItem.Price.ToString()
				)
			);
			record.Properties.Add(
				new ExportPropertyNode("Сумма",
					Common1cTypes.Numeric,
					orderItem.ActualCount*orderItem.Price
				)
			);

			var vat = orderItem.Nomenclature.VAT.GetAttribute<Value1c>().Value;
			record.Properties.Add(
				new ExportPropertyNode("СтавкаНДС",
					"ПеречислениеСсылка.СтавкиНДС",
					vat
				)
			);
			record.Properties.Add(
				new ExportPropertyNode("СуммаНДС",
					Common1cTypes.Numeric,
					orderItem.ActualCount*orderItem.Price*0.18M
				)
			);
			record.Properties.Add(
				new ExportPropertyNode("НомерГТД",
					"СправочникСсылка.НомераГТД"
				)
			);
			record.Properties.Add(
				new ExportPropertyNode("СтранаПроисхождения",
					Common1cTypes.ReferenceCountry
				)
			);
			return record;
		}

		public XElement ToXml()	
		{
			var xml = new XElement("ФайлОбмена",
				          new XAttribute("ВерсияФормата", Version),
				          new XAttribute("ДатаВыгрузки", ExportDate.ToString("s")),
				          new XAttribute("НачалоПериодаВыгрузки", StartPeriodDate.ToString("s")),
				          new XAttribute("ОкончаниеПериодаВыгрузки", EndPeriodDate.ToString("s")),
				          new XAttribute("ИмяКонфигурацииИсточника", SourceName),
				          new XAttribute("ИдПравилКонвертации", ConversionRulesId),
				          new XAttribute("Комментарий", Comment)
			          );
			xml.Add(ExchangeRules.ToXml());
			Objects.ForEach(obj=>xml.Add(obj.ToXml()));
			return xml;
		}
	}
}

