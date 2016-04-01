using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using QSOrmProject;
using Vodovoz.Domain.Orders;
using Vodovoz.Domain;
using Vodovoz.Repository;
using Vodovoz.ExportTo1c.Catalogs;
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
			
		public List<ObjectNode> Objects{ get; set;}
		public RulesNode ExchangeRules{ get; set; }

		public int objectCounter;

		public IUnitOfWork UoW;

		public AccountCatalog AccountDirectory;
		public BankCatalog BankDirectory;
		public ContractCatalog ContractDirectory;
		public CounterpartyCatalog CounterpartyDirectory;
		public CurrencyCatalog CurrencyDirectory;
		public MeasurementUnitsCatalog MeasurementUnitsDirectory;
		public NomenclatureCatalog NomenclatureDirectory;
		public OrganizationCatalog OrganizationDirectory;
		public WarehouseCatalog WarehouseDirectory;

		public Dictionary<NomenclatureCategory, Nomenclature> CategoryToNomenclatureMap;
		public Organization CashlessOrganization{ get; private set;}

		public ExportData(IUnitOfWork uow)
		{			
			this.Objects = new List<ObjectNode>();
			this.UoW = uow;
			this.AccountDirectory = new AccountCatalog(this);
			this.BankDirectory = new BankCatalog(this);
			this.ContractDirectory = new ContractCatalog(this);
			this.CounterpartyDirectory = new CounterpartyCatalog(this);
			this.CurrencyDirectory = new CurrencyCatalog(this);
			this.MeasurementUnitsDirectory = new MeasurementUnitsCatalog(this);
			this.NomenclatureDirectory = new NomenclatureCatalog(this);
			this.OrganizationDirectory = new OrganizationCatalog(this);
			this.WarehouseDirectory = new WarehouseCatalog(this);
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
			this.ExchangeRules = new RulesNode();
		}

		public void AddOrder(Order order)
		{
			var exportSalesDocument = GetSalesDocument(order);
			var exportInvoiceDocument = new InvoiceDocumentNode();
			exportInvoiceDocument.Id = ++objectCounter;
			exportInvoiceDocument.Reference = new ReferenceNode(exportInvoiceDocument.Id,
				new PropertyNode("Номер", Common1cTypes.String, order.Id),
				new PropertyNode("Дата", Common1cTypes.Date, order.DeliveryDate.ToString("s"))
			);

			exportInvoiceDocument.Properties.Add(
				new PropertyNode("Организация",
					Common1cTypes.ReferenceOrganization,
					OrganizationDirectory.GetReferenceTo(CashlessOrganization)
				)
			);

			exportInvoiceDocument.Properties.Add(
				new PropertyNode("Комментарий",
					Common1cTypes.String
				)
			);

			var contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoW,order.Client,order.PaymentType);
			if (contract != null)
			{
				exportInvoiceDocument.Properties.Add(
					new PropertyNode("ДоговорКонтрагента",
						Common1cTypes.ReferenceContract,
						ContractDirectory.GetReferenceTo(contract)
					)
				);
			}
			else
			{
				exportInvoiceDocument.Properties.Add(
					new PropertyNode("ДоговорКонтрагента",
						Common1cTypes.ReferenceContract
					)
				);
			}

			exportInvoiceDocument.Properties.Add(
				new PropertyNode("ДокументОснование",
					"ДокументСсылка.РеализацияТоваровУслуг",
					exportSalesDocument.Reference
				)
			);

			exportInvoiceDocument.Properties.Add(
				new PropertyNode("ВидСчетаФактуры",
					Common1cTypes.EnumInvoiceType,
					"НаРеализацию"
				)
			);
			exportInvoiceDocument.Properties.Add(
				new PropertyNode("НомерПлатежноРасчетногоДокумента",
					Common1cTypes.String
				)
			);
			exportInvoiceDocument.Properties.Add(
				new PropertyNode("ДатаПлатежноРасчетногоДокумента",
					Common1cTypes.Date
				)
			);
			exportInvoiceDocument.Properties.Add(
				new PropertyNode("ВалютаДокумента",
					Common1cTypes.ReferenceCurrency,
					CurrencyDirectory.GetReferenceTo(Currency.Default)
				)
			);
			exportInvoiceDocument.Properties.Add(
				new PropertyNode("СтавкаНДС",
					Common1cTypes.EnumVAT
				)
			);
			exportInvoiceDocument.Properties.Add(
				new PropertyNode("Сумма",
					Common1cTypes.Numeric
				)
			);
			exportInvoiceDocument.Properties.Add(
				new PropertyNode("СуммаНДС",
					Common1cTypes.Numeric
				)
			);
			exportInvoiceDocument.Properties.Add(
				new PropertyNode("Контрагент",
					Common1cTypes.ReferenceCounterparty,
					CounterpartyDirectory.GetReferenceTo(order.Client)
				)
			);
			exportInvoiceDocument.Properties.Add(
				new PropertyNode("ПометкаУдаления",
					Common1cTypes.Boolean
				)
			);
			exportInvoiceDocument.Properties.Add(
				new PropertyNode("Проведен",
					Common1cTypes.Boolean,
					"true"
				)
			);

			Objects.Add(exportSalesDocument);
			Objects.Add(exportInvoiceDocument);
		}

		public SalesDocumentNode GetSalesDocument(Order order)
		{
			var goods = order.OrderItems.Where(item => Nomenclature.GetCategoriesForGoods().Contains(item.Nomenclature.Category));
			var exportSaleDocument = new SalesDocumentNode();
			exportSaleDocument.Id = ++objectCounter;
			exportSaleDocument.Reference = new ReferenceNode(exportSaleDocument.Id,
				new PropertyNode("Номер", Common1cTypes.String, order.Id),
				new PropertyNode("Дата", Common1cTypes.Date, order.DeliveryDate.ToString("s"))
			);

			var exportGoodsTable = new TableNode{
				Name="Товары",
			};

			foreach (var orderItem in goods)
			{
				var record = GetRecord(orderItem);
				exportGoodsTable.Records.Add(record);
				exportSaleDocument.Comission.Comissions.Add(0);
			}

			exportSaleDocument.Properties.Add(
				new PropertyNode("Организация",
					Common1cTypes.ReferenceOrganization,
					OrganizationDirectory.GetReferenceTo(CashlessOrganization)
				)
			);
			exportSaleDocument.Properties.Add(
				new PropertyNode("Комментарий",
					Common1cTypes.String,
					order.Comment
				)
			);
			exportSaleDocument.Properties.Add(
				new PropertyNode("Склад",
					Common1cTypes.ReferenceWarehouse,
					WarehouseDirectory.GetReferenceTo(Warehouse1c.Default)
				)
			);
			exportSaleDocument.Properties.Add(
				new PropertyNode("Контрагент",
					Common1cTypes.ReferenceCounterparty,
					CounterpartyDirectory.GetReferenceTo(order.Client)
				)
			);
			var contract = CounterpartyContractRepository.GetCounterpartyContractByPaymentType(UoW,order.Client,order.PaymentType);
			if (contract != null)
			{
				exportSaleDocument.Properties.Add(
					new PropertyNode("ДоговорКонтрагента",
						Common1cTypes.ReferenceContract,
						ContractDirectory.GetReferenceTo(contract)
					)
				);
			}
			else
			{
				exportSaleDocument.Properties.Add(
					new PropertyNode("ДоговорКонтрагента",
						Common1cTypes.ReferenceContract
					)
				);
			}
			exportSaleDocument.Properties.Add(
				new PropertyNode("ВалютаДокумента",
					Common1cTypes.ReferenceCurrency,
					CurrencyDirectory.GetReferenceTo(ExportTo1c.Currency.Default)
				)
			);

			exportSaleDocument.Properties.Add(
				new PropertyNode("УчитыватьНДС",
					Common1cTypes.Boolean,
					"true"
				)
			);

			exportSaleDocument.Properties.Add(
				new PropertyNode("СуммаВключаетНДС",
					Common1cTypes.Boolean,
					"true"
				)
			);
			exportSaleDocument.Properties.Add(
				new PropertyNode("ВидОперации",
					"ПеречислениеСсылка.ВидыОперацийРеализацияТоваров",
					"ПродажаКомиссия"
				)
			);
			exportSaleDocument.Properties.Add(
				new PropertyNode("КурсВзаиморасчетов",
					Common1cTypes.Numeric,
					1
				)
			);
			exportSaleDocument.Properties.Add(
				new PropertyNode("КратностьВзаиморасчетов",
					Common1cTypes.Numeric,
					1
				)
			);

			var exportServicesTable = new TableNode
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
			return exportSaleDocument;
		}			

		public TableRecordNode GetRecord(OrderItem orderItem)
		{
			var record = new TableRecordNode();
			var nomenclatureReference = NomenclatureDirectory.GetReferenceTo(orderItem.Nomenclature);
			record.Properties.Add(
				new PropertyNode("Номенклатура",
					Common1cTypes.ReferenceNomenclature,
					nomenclatureReference
				)
			);
			if(orderItem.Nomenclature.Category==NomenclatureCategory.service)
				record.Properties.Add(
					new PropertyNode("Содержание",
						Common1cTypes.String,
						orderItem.Nomenclature.Name
					)
				);
			record.Properties.Add(
				new PropertyNode("Количество",
					Common1cTypes.Numeric,
					orderItem.ActualCount
				)
			);
			record.Properties.Add(
				new PropertyNode("Цена",
					Common1cTypes.Numeric,
					orderItem.Price.ToString()
				)
			);
			record.Properties.Add(
				new PropertyNode("Сумма",
					Common1cTypes.Numeric,
					orderItem.ActualCount*orderItem.Price
				)
			);

			var vat = orderItem.Nomenclature.VAT.GetAttribute<Value1c>().Value;
			record.Properties.Add(
				new PropertyNode("СтавкаНДС",
					Common1cTypes.EnumVAT,
					vat
				)
			);
			record.Properties.Add(
				new PropertyNode("СуммаНДС",
					Common1cTypes.Numeric,
					orderItem.ActualCount*orderItem.Price*0.18M
				)
			);
			record.Properties.Add(
				new PropertyNode("НомерГТД",
					"СправочникСсылка.НомераГТД"
				)
			);
			record.Properties.Add(
				new PropertyNode("СтранаПроисхождения",
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

