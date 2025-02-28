﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Gamma.Utilities;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Attributes;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Orders;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.ExportTo1c.Catalogs;

namespace Vodovoz.ExportTo1c
{
	public class ExportData : IXmlConvertable
	{
		public string Version { get; set; }
		public DateTime ExportDate { get; set; }
		public DateTime StartPeriodDate { get; set; }
		public DateTime EndPeriodDate { get; set; }
		public string SourceName { get; set; }
		public string DestinationName { get; set; }
		public string ConversionRulesId { get; set; }
		public string Comment { get; set; }
		public List<string> Errors = new List<string>();

		public decimal OrdersTotalSum;
		public decimal ExportedTotalSum;

		public List<ObjectNode> Objects { get; private set; }

		public Dictionary<DateTime, RetailDocumentNode> RetailDocumentsList;
		
		public RulesNode ExchangeRules { get; set; }

		public int objectCounter;

		public readonly IUnitOfWork UoW;

		public AccountCatalog AccountCatalog { get; private set; }
		public BankCatalog BankCatalog { get; private set; }
		public ContractCatalog ContractCatalog { get; private set; }
		public CounterpartyCatalog CounterpartyCatalog { get; private set; }
		public CurrencyCatalog CurrencyCatalog { get; private set; }
		public MeasurementUnitsCatalog MeasurementUnitCatalog { get; private set; }
		public NomenclatureCatalog NomenclatureCatalog { get; private set; }
		public NomenclatureType1cTypeCatalog NomenclatureTypeCatalog { get; private set; }
		public NomenclatureGroupCatalog NomenclatureGroupCatalog { get; private set; }
		public OrganizationCatalog OrganizationCatalog { get; private set; }
		public WarehouseCatalog WarehouseCatalog { get; private set; }

		public Export1cMode ExportMode { get; private set; }

		public ExportData(IUnitOfWork uow, Export1cMode mode, DateTime dateStart, DateTime dateEnd)
		{
			this.Objects = new List<ObjectNode>();
			this.UoW = uow;
			this.ExportMode = mode;

			this.Version = "2.0";
			this.ExportDate = DateTime.Now;
			this.StartPeriodDate = dateStart;
			this.EndPeriodDate = dateEnd;
			this.SourceName = "Торговля+Склад, редакция 9.2";
			this.DestinationName = "БухгалтерияПредприятия";
			this.ConversionRulesId = "70e9dbac-59df-44bb-82c6-7d4f30d37c74";
			this.Comment = "";

			this.AccountCatalog = new AccountCatalog(this);
			this.BankCatalog = new BankCatalog(this);
			this.ContractCatalog = new ContractCatalog(this);
			this.CounterpartyCatalog = new CounterpartyCatalog(this);
			this.CurrencyCatalog = new CurrencyCatalog(this);
			this.MeasurementUnitCatalog = new MeasurementUnitsCatalog(this);
			this.NomenclatureCatalog = new NomenclatureCatalog(this);
			this.NomenclatureTypeCatalog = new NomenclatureType1cTypeCatalog(this);
			this.NomenclatureGroupCatalog = new NomenclatureGroupCatalog(this);
			this.OrganizationCatalog = new OrganizationCatalog(this);
			this.WarehouseCatalog = new WarehouseCatalog(this);
			this.ExchangeRules = new RulesNode();
			this.RetailDocumentsList = new Dictionary<DateTime, RetailDocumentNode>();
		}

		public void AddOrder(Order order)
		{
			OrdersTotalSum += order.OrderSum;
			if(order.PaymentType == PaymentType.PaidOnline || order.PaymentType == PaymentType.Cash || order.PaymentType == PaymentType.Terminal) {
				CreateRetailDocument(order);
			}
			else
			{
				var exportSalesDocument = CreateSalesDocument(order);
				var exportInvoiceDocument = new InvoiceDocumentNode {
					Id = ++objectCounter
				};
				exportInvoiceDocument.Reference = new ReferenceNode(exportInvoiceDocument.Id,
					new PropertyNode("Номер", Common1cTypes.String, ExportMode == Export1cMode.IPForTinkoff ? order.OnlineOrder.Value : order.Id),
					new PropertyNode("Дата", Common1cTypes.Date, order.DeliveryDate.Value.ToString("s"))
				);

				exportInvoiceDocument.Properties.Add(
					new PropertyNode("Организация",
						Common1cTypes.ReferenceOrganization,
						OrganizationCatalog.CreateReferenceTo(order.Contract.Organization)
					)
				);

				exportInvoiceDocument.Properties.Add(
					new PropertyNode("Комментарий",
						Common1cTypes.String
					)
				);

				exportInvoiceDocument.Properties.Add(
					new PropertyNode("ДоговорКонтрагента",
						Common1cTypes.ReferenceContract,
									 ContractCatalog.CreateReferenceToContract(order)
					)
				);

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
						CurrencyCatalog.CreateReferenceTo(Currency.Default)
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
						CounterpartyCatalog.CreateReferenceTo(order.Client)
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
		}

		public SalesDocumentNode CreateSalesDocument(Order order)
		{
			var exportSaleDocument = new SalesDocumentNode {
				Id = ++objectCounter
			};
			exportSaleDocument.Reference = new ReferenceNode(exportSaleDocument.Id,
				new PropertyNode("Номер", Common1cTypes.String, ExportMode == Export1cMode.IPForTinkoff ? order.OnlineOrder.Value : order.Id),
				new PropertyNode("Дата", Common1cTypes.Date, order.DeliveryDate.Value.ToString("s"))
			);

			var exportGoodsTable = new TableNode {
				Name = "Товары",
			};

			var exportServicesTable = new TableNode {
				Name = "Услуги",
			};

			foreach(var orderItem in order.OrderItems) {
				var record = CreateRecord(orderItem);
				if(Nomenclature.GetCategoriesForGoods().Contains(orderItem.Nomenclature.Category)) {
					exportGoodsTable.Records.Add(record);
					exportSaleDocument.Comission.Comissions.Add(0);
				} else
					exportServicesTable.Records.Add(record);
			}

			exportSaleDocument.Properties.Add(
				new PropertyNode("Организация",
					Common1cTypes.ReferenceOrganization,
					OrganizationCatalog.CreateReferenceTo(order.Contract.Organization)
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
					WarehouseCatalog.CreateReferenceTo(Warehouse1c.Default)
				)
			);
			exportSaleDocument.Properties.Add(
				new PropertyNode("Контрагент",
					Common1cTypes.ReferenceCounterparty,
					CounterpartyCatalog.CreateReferenceTo(order.Client)
				)
			);

			exportSaleDocument.Properties.Add(
				new PropertyNode("ДоговорКонтрагента",
					Common1cTypes.ReferenceContract,
								 ContractCatalog.CreateReferenceToContract(order)
				)
			);

			exportSaleDocument.Properties.Add(
				new PropertyNode("ВалютаДокумента",
					Common1cTypes.ReferenceCurrency,
					CurrencyCatalog.CreateReferenceTo(ExportTo1c.Currency.Default)
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

			exportSaleDocument.Tables.Add(exportGoodsTable);
			exportSaleDocument.Tables.Add(exportServicesTable);
			return exportSaleDocument;
		}

		public void CreateRetailDocument(Order order)
		{
			if(!order.DeliveryDate.HasValue) {
				throw new ArgumentNullException(nameof(order.DeliveryDate));
			}
			
			if (!RetailDocumentsList.TryGetValue(order.DeliveryDate.Value.Date, out RetailDocumentNode exportRetailDocument)) {
				exportRetailDocument = new RetailDocumentNode { Id = ++objectCounter };

				exportRetailDocument.Reference = new ReferenceNode(
					exportRetailDocument.Id,
					new PropertyNode(
						"Номер",
						Common1cTypes.String,
						ExportMode == Export1cMode.IPForTinkoff ? (order.OnlineOrder ?? throw new ArgumentNullException(nameof(order.OnlineOrder), $@"(OrderId: {order.Id})")) : order.Id),
					new PropertyNode(
						"Дата", 
						Common1cTypes.Date,
						order.DeliveryDate.Value.Date.ToString("s"))
				);

				var exportGoodsTable = new TableNode
				{
					Name = "Товары",
				};

					var exportRefundGoodsTable = new TableNode
				{
					Name = "Возвраты",
				};

				var exportTerminalTable = new TableNode
				{
					Name = "Оплата",
				};

				var exportRefundTerminalTable = new TableNode
				{
					Name = "ВозвратОплаты",
				};

				exportRetailDocument.Properties.Add(
					new PropertyNode("Организация",
						Common1cTypes.ReferenceOrganization,
						OrganizationCatalog.CreateReferenceTo(order.Contract.Organization))
				);
				exportRetailDocument.Properties.Add(
					new PropertyNode("Комментарий",
						Common1cTypes.String,
						order.Comment
					)
				);
				exportRetailDocument.Properties.Add(
					new PropertyNode("Склад",
						Common1cTypes.ReferenceWarehouse,
						WarehouseCatalog.CreateReferenceTo(Warehouse1c.Default)
					)
				);

				exportRetailDocument.Properties.Add(
					new PropertyNode("ВалютаДокумента",
						Common1cTypes.ReferenceCurrency,
						CurrencyCatalog.CreateReferenceTo(ExportTo1c.Currency.Default)
					)
				);

				exportRetailDocument.Properties.Add(
					new PropertyNode("СуммаВключаетНДС",
						Common1cTypes.Boolean,
						"true"
					)
				);
				exportRetailDocument.Properties.Add(
					new PropertyNode("ВидОперации",
						"ПеречислениеСсылка.ВидыОперацийОтчетОРозничныхПродажах",
						"ОтчетККМОПродажах"
					)
				);

				exportRetailDocument.Tables.Add(exportGoodsTable);
				exportRetailDocument.Tables.Add(exportTerminalTable);
				exportRetailDocument.Tables.Add(exportRefundGoodsTable);
				exportRetailDocument.Tables.Add(exportRefundTerminalTable);
			}

			bool isTerminalPaid = (order.PaymentType == PaymentType.PaidOnline || order.PaymentType == PaymentType.Terminal);
			
			foreach (var orderItem in order.OrderItems)
			{				
				if(orderItem.Sum != 0){
				
					var record = CreateRetailRecord(orderItem);

					exportRetailDocument.Tables[0].Records.Add(record);
					
					if (isTerminalPaid)
					{
						var recordPayment = new TableRecordNode();
						recordPayment.Properties.Add(
							new PropertyNode("СуммаОплаты",
								Common1cTypes.Numeric,
								orderItem.Sum
							)
						);

						exportRetailDocument.Tables[1].Records.Add(recordPayment);
					}
					
				}
			}

			if(!RetailDocumentsList.ContainsKey(order.DeliveryDate.Value.Date)) {
				RetailDocumentsList.Add(order.DeliveryDate.Value.Date, exportRetailDocument);
			}
		}

		public TableRecordNode CreateRecord(OrderItem orderItem)
		{
			var record = new TableRecordNode();
			bool isService = !Nomenclature.GetCategoriesForGoods().Contains(orderItem.Nomenclature.Category);
			var nomenclatureReference = NomenclatureCatalog.CreateReferenceTo(orderItem.Nomenclature);
			record.Properties.Add(
				new PropertyNode("Номенклатура",
					Common1cTypes.ReferenceNomenclature,
					nomenclatureReference
				)
			);
			if(isService)
				record.Properties.Add(
					new PropertyNode("Содержание",
						Common1cTypes.String,
									 orderItem.Nomenclature.OfficialName
					)
				);
			record.Properties.Add(
				new PropertyNode(
					"Количество",
					Common1cTypes.Numeric,
					orderItem.CurrentCount
				)
			);
			if(!isService) {
				record.Properties.Add(
					new PropertyNode("Коэффициент",
						Common1cTypes.Numeric,
									 1
					)
				);
			}

			record.Properties.Add(
				new PropertyNode("Цена",
					Common1cTypes.Numeric,
					orderItem.Price));

			ExportedTotalSum += orderItem.ActualSum;

			record.Properties.Add(
				new PropertyNode(
					"Сумма",
					Common1cTypes.Numeric,
					orderItem.ActualSum
				)
			);

			var vat = orderItem.Nomenclature.VAT.GetAttribute<Value1c>().Value;
			record.Properties.Add(
				new PropertyNode("СтавкаНДС",
					Common1cTypes.EnumVAT,
					vat
				)
			);

			if(orderItem.Nomenclature.VAT != VAT.No) {
				record.Properties.Add(
					new PropertyNode(
						"СуммаНДС",
						Common1cTypes.Numeric,
						orderItem.IncludeNDS ?? 0 //FIXME Нужно будет сделать что бы всегда соответствало количетству.
					)
				);
			} else {
				record.Properties.Add(
					new PropertyNode("СуммаНДС",
						Common1cTypes.Numeric
					)
				);
			}

			if(!isService) {
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
			}
			return record;
		}

		public TableRecordNode CreateRetailRecord(OrderItem orderItem)
		{
			var record = new TableRecordNode();
			var nomenclatureReference = NomenclatureCatalog.CreateReferenceTo(orderItem.Nomenclature);
			record.Properties.Add(
				new PropertyNode("Номенклатура",
					Common1cTypes.ReferenceNomenclature,
					nomenclatureReference
				)
			);
			record.Properties.Add(
				new PropertyNode(
					"Количество",
					Common1cTypes.Numeric,
					orderItem.CurrentCount
				)
			);

			record.Properties.Add(
				new PropertyNode("Цена",
					Common1cTypes.Numeric,
					orderItem.Price));

			ExportedTotalSum += orderItem.Sum;

			record.Properties.Add(
				new PropertyNode(
					"Сумма",
					Common1cTypes.Numeric,
					orderItem.Sum
				)
			);

			var vat = VAT.No.GetAttribute<Value1c>().Value;
			record.Properties.Add(
				new PropertyNode("СтавкаНДС",
					Common1cTypes.EnumVAT,
					vat
				)
			);

			record.Properties.Add(
				new PropertyNode("СуммаНДС",
					Common1cTypes.Numeric
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
			foreach(var obj in Objects.OrderBy(x => x.Id)) {
				xml.Add(obj.ToXml());
			}
			return xml;
		}

		public void  FinishRetailDocuments()
		{
			foreach (var RetailDocument in RetailDocumentsList)
			{
				Objects.Add(RetailDocument.Value);	
			}
		}
	
	}
}

