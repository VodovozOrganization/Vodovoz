using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Gtk;
using QSBanks;
using QSBusinessCommon.Domain;
using QSBusinessCommon.Repository;
using QSOrmProject;
using QSProjectsLib;
using QSTDI;
using QSWidgetLib;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Orders;
using Vodovoz.LoadFrom1c;
using ServiceDialogs.LoadFrom1c;
using Gamma.GtkWidgets;

namespace Vodovoz
{
	public partial class LoadFrom1cDlg : TdiTabBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot ();

		List<string> IncludeParents = new List<string>{
			"00000002",
			"00000001",
		};

		List<string> ExcludeParents = new List<string>{
			"03281439",
		};

		private string cahsString = "00002", cashlessString = "00003", barterString = "00093";

		#if SHORT
		List<string> ExcludeNomenclatures = new List<string> {
			"00000969"
		};
		#endif

		#region Свойства
		private IList<Bank> banks;

		private IList<Bank> Banks {
			get {
				if(banks == null)
				{
					banks = QSBanks.Repository.BankRepository.ActiveBanks (UoW);
				}
				return banks;
			}
		}

		int totalCounterparty = 0;

		public int TotalCounterparty {
			get {
				return totalCounterparty;
			}
			set {
				totalCounterparty = value;
				labelTotalCounterparty.LabelProp = totalCounterparty.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int skipedCounterparty = 0;

		public int SkipedCounterparty {
			get {
				return skipedCounterparty;
			}
			set {
				skipedCounterparty = value;
				labelSkipedCounterparty.LabelProp = SkipedCounterparty.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int readedAccounts = 0;

		public int ReadedAccounts {
			get {
				return readedAccounts;
			}
			set {
				readedAccounts = value;
				labelReadedAccounts.LabelProp = ReadedAccounts.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int linkAccounts = 0;

		public int LinkedAccounts {
			get {
				return linkAccounts;
			}
			set {
				linkAccounts = value;
				labelLinkedAccount.LabelProp = LinkedAccounts.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int inactiveAccounts = 0;

		public int InactiveAccounts {
			get {
				return inactiveAccounts;
			}
			set {
				inactiveAccounts = value;
				labelInactiveAccounts.LabelProp = InactiveAccounts.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int readedBanks = 0;

		public int ReadedBanks {
			get {
				return readedBanks;
			}
			set {
				readedBanks = value;
				labelTotalBanks.LabelProp = ReadedBanks.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int inactiveBanks = 0;

		public int InactiveBanks {
			get {
				return inactiveBanks;
			}
			set {
				inactiveBanks = value;
				labelInactiveBanks.LabelProp = InactiveBanks.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int readedNomenclatures = 0;

		public int ReadedNomenclatures {
			get {
				return readedNomenclatures;
			}
			set {
				readedNomenclatures = value;
				labelReadedNomenclatures.LabelProp = ReadedNomenclatures.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int readedOrders = 0;

		public int ReadedOrders {
			get {
				return readedOrders;
			}
			set {
				readedOrders = value;
				labelReadedOrders.LabelProp = ReadedOrders.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int newCounterparties = 0;

		public int NewCounterparties {
			get {
				return newCounterparties;
			}
			set {
				newCounterparties = value;
				labelNewCounterparties.LabelProp = NewCounterparties.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int newNomenclatures = 0;

		public int NewNomenclatures {
			get {
				return newNomenclatures;
			}
			set {
				newNomenclatures = value;
				labelNewNomenclatures.LabelProp = NewNomenclatures.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int newOrders = 0;

		public int NewOrders {
			get {
				return newOrders;
			}
			set {
				newOrders = value;
				labelNewOrders.LabelProp = NewOrders.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int newAddresses = 0;

		public int NewAddresses {
			get {
				return newAddresses;
			}
			set {
				newAddresses = value;
				labelNewAddresses.LabelProp = NewAddresses.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int changedCounterparties = 0;

		public int ChangedCounterparties {
			get {
				return changedCounterparties;
			}
			set {
				changedCounterparties = value;
				labelCounterpartiesChanged.LabelProp = ChangedCounterparties.ToString ();
				QSMain.WaitRedraw ();
			}
		}

		int changedOrders = 0;

		public int ChangedOrders {
			get {
				return changedOrders;
			}
			set {
				changedOrders = value;
				labelOrdersChanged.LabelProp = ChangedOrders.ToString ();
				QSMain.WaitRedraw ();
			}
		}
		#endregion

		List<Counterparty> 	CounterpatiesList = new List<Counterparty>();
//		List<Counterparty> 	ChangedCounterpartiesList = new List<Counterparty>();
		List<Account1c> 	AccountsList = new List<Account1c>();
		List<Bank1c> 		Banks1cList = new List<Bank1c>();
		List<Nomenclature> 	NomenclaturesList = new List<Nomenclature>();
		List<Order> 		OrdersList = new List<Order>();
		List<Order> 		ChangedOrdersList = new List<Order>();
		List<DateTime> 		LoadedOrderDates = new List<DateTime>();
		List<ChangedItem> 	Changes = new List<ChangedItem>();

		List<DeliveryPoint> 	DeliveryPointsList = null;
		List<DeliverySchedule> 	DeliverySchedules = null;

		MeasurementUnits unitU;
		MeasurementUnits UnitServ;


		public LoadFrom1cDlg ()
		{
			this.Build ();
			TabName = "Загрузка контрагентов из 1с 7.7";

			FileFilter Filter = new FileFilter();
			Filter.Name = "XML выгрузка";
			Filter.AddMimeType("application/xml");
			Filter.AddPattern("*.xml");
			filechooserXML.Filter = Filter;

			DeliveryPointsList = UoW.GetAll<DeliveryPoint>().ToList<DeliveryPoint>();
			DeliverySchedules  = UoW.GetAll<DeliverySchedule>().ToList();
			unitU 	 = MeasurementUnitsRepository.GetDefaultGoodsUnit(UoW);
			UnitServ = MeasurementUnitsRepository.GetDefaultGoodsService(UoW);

			vboxChanges.Visible = false;
			ytreeEntites.ColumnsConfig = ColumnsConfigFactory.Create<ChangedItem>()
				.AddColumn("Название").AddTextRenderer(x => x.Title)
				.Finish();

			ytreeEntites.Selection.Mode = SelectionMode.Browse;
			ytreeEntites.Selection.Changed += YtreeEntites_Selection_Changed;

			ytreeChanges.ColumnsConfig = ColumnsConfigFactory.Create<FieldChange>()
				.AddColumn("Поле").AddTextRenderer(x => x.Title)
				.AddColumn("Старое значение").AddTextRenderer(x => x.OldPangoText, useMarkup: true)
				.AddColumn("Новое значение").AddTextRenderer(x => x.NewPangoText, useMarkup: true)
				.Finish();
		}

		void YtreeEntites_Selection_Changed (object sender, EventArgs e)
		{
			var item = ytreeEntites.GetSelectedObject<ChangedItem>();
			ytreeChanges.ItemsDataSource = item?.Fields;
			ytreeChanges.ColumnsAutosize();
		}

		protected void OnButtonLoadClicked (object sender, EventArgs e)
		{
			checkSkipCounterparties.Sensitive = false;
			logger.Info ("Читаем XML файл...");
			progressbar.Text = "Читаем XML файл...";
			TotalCounterparty = SkipedCounterparty 		= ReadedAccounts
				= LinkedAccounts		= InactiveAccounts 		= ReadedBanks
				= InactiveBanks			= ReadedNomenclatures 	= ReadedOrders
				= NewCounterparties		= NewNomenclatures 		= NewOrders
				= NewAddresses			= ChangedCounterparties = ChangedOrders
				= 0;
			
			CounterpatiesList		 .Clear();
			AccountsList	  		 .Clear();
			Banks1cList		  		 .Clear();
			NomenclaturesList 		 .Clear();
			OrdersList		  		 .Clear();
			ChangedOrdersList		 .Clear();
			DeliveryPointsList		 .Clear();

			XmlDocument content = new XmlDocument ();
			content.Load (filechooserXML.Filename);

			progressbar.Text = "Разбор данных в файле...";
			foreach(XmlNode node in content.SelectNodes ("/ФайлОбмена/Объект"))
			{
				string ruleName = node.Attributes ["ИмяПравила"].Value;
				switch(ruleName)
				{
				case "Контрагенты":
					ParseCounterparty (node);
					break;
				case "БанковскиеСчета":
					ParseAccount (node);
					break;
				case "Банки":
					ParseBank (node);
					break;
				case "КонтактнаяИнформация":
					ParseContactInfo (node);
					break;
				case "Номенклатура":
					ParseNomenclature (node);
					break;
				case "РеализацияТоваровУслуг":
					ParseOrders (node);
					break;
				}
			}
			progressbar.Text = "Сопопоставляем расчетные счета с владельщами.";
			progressbar.Adjustment.Value = 0;
			progressbar.Adjustment.Upper = AccountsList.Count;

			foreach(var ac1c in AccountsList)
			{
				progressbar.Adjustment.Value++;
				QSMain.WaitRedraw ();

				if (ac1c.DomainAccount.Inactive)
					continue;

				var counterparty = CounterpatiesList.Find (c => c.Code1c == ac1c.OwnerCode1c);

				if(counterparty != null)
				{
					counterparty.AddAccount (ac1c.DomainAccount);
					LinkedAccounts++;
				}
			}

			progressbar.Text = "Выполнено";
			buttonCreate.Sensitive = checkRewrite.Sensitive = checkOnlyAddress.Sensitive =
				CounterpatiesList.Count > 0 || OrdersList.Count > 0 || NomenclaturesList.Count > 0 ||
				DeliveryPointsList.Count > 0;
			checkSkipCounterparties.Sensitive = true;
		}

		#region Парсеры Xml
		void ParseCounterparty(XmlNode node)
		{
			bool skipCounterparties = checkSkipCounterparties.Active;

			var parrentNode = node.SelectSingleNode ("Свойство[@Имя='Родитель']/Ссылка/Свойство[@Имя='Код']/Значение");

			if (skipCounterparties)
			{
				if((parrentNode == null || !IncludeParents.Contains (parrentNode.InnerText)))
				{
					logger.Debug ("Пропускаем... так как не входит в выборку.");
					SkipedCounterparty++;
					return;
				}
				
				if(ExcludeParents.Contains (parrentNode.InnerText))
				{
					logger.Debug ("Пропускаем... так как попадает в исключение.");
					SkipedCounterparty++;
					return;
				}
			}

			bool isGroup = false;
			var groupNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='ЭтоГруппа']/Значение");
			if (groupNode != null && groupNode.InnerText == "true") //Если не группа то isGroup == null так как нода "Значение" нет.
			{
				logger.Debug ("Это группа");
				isGroup = true;
			}

			var codeNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Код']/Значение");

			if(isGroup)
			{
				IncludeParents.Add (codeNode.InnerText);
				logger.Debug ("добавили группу {0}.", codeNode.InnerText);
			}
			else
			{
				var counterparty = new Counterparty ();

				var nameNode = node.SelectSingleNode ("Свойство[@Имя='Наименование']/Значение");
				counterparty.Name = nameNode.InnerText;
				logger.Debug ("Читаем контрагента <{0}>", counterparty.Name);

				counterparty.Code1c = codeNode.InnerText;

				var jurNode = node.SelectSingleNode ("Свойство[@Имя='ЮрФизЛицо']/Значение");
				counterparty.PersonType = jurNode.InnerText == "ЮрЛицо" ? PersonType.legal : PersonType.natural;
				counterparty.PaymentMethod = jurNode.InnerText == "ЮрЛицо" ? PaymentType.cashless : PaymentType.cash;

				var accountNode = node.SelectSingleNode ("Свойство[@Имя='ОсновнойБанковскийСчет']/Ссылка/Свойство[@Имя='Код']/Значение");
				if(accountNode != null)
				{
					var ac1c = AccountsList.Find (a => a.DomainAccount.Code1c == accountNode.InnerText);
					if(!ac1c.DomainAccount.Inactive)
						counterparty.DefaultAccount = ac1c.DomainAccount;
				}

				var commentNode = node.SelectSingleNode ("Свойство[@Имя='Комментарий']/Значение");
				if(commentNode != null)
				{
					counterparty.Comment = commentNode.InnerText;
				}

				var fullnameNode = node.SelectSingleNode ("Свойство[@Имя='НаименованиеПолное']/Значение");
				if(fullnameNode != null)
				{
					counterparty.FullName = fullnameNode.InnerText;
				}

				var INNNode = node.SelectSingleNode ("Свойство[@Имя='ИНН']/Значение");
				if(INNNode != null)
				{
					if(INNNode.InnerText.Length > 12)
						counterparty.INN = INNNode.InnerText.Substring(0, 12);
					else
						counterparty.INN = INNNode.InnerText;
				}

				var KPPNode = node.SelectSingleNode ("Свойство[@Имя='КПП']/Значение");
				if(KPPNode != null)
				{
					if(KPPNode.InnerText.Length > 9)
						counterparty.KPP = KPPNode.InnerText.Substring(0, 9);
					else
						counterparty.KPP = KPPNode.InnerText;
				}
					
				string[] InnSplited = counterparty.INN.Split ('/');
				if(InnSplited.Length > 1)
				{
					counterparty.INN = InnSplited [0];
					counterparty.KPP = InnSplited [1];
				}

				if(counterparty.PersonType == PersonType.legal)
				{
					counterparty.FullName = counterparty.FullName.TrimStart();
					var found = CommonValues.Ownerships.FirstOrDefault(x => counterparty.FullName.StartsWith(x.Key));
					if (!String.IsNullOrEmpty(found.Key))
						counterparty.TypeOfOwnership = found.Key;
				}

				var MainNode = node.SelectSingleNode ("Свойство[@Имя='ГоловнойКонтрагент']/Значение");
				if (MainNode != null)
					logger.Warn ("ГоловнойКонтрагент не пустой");

				TotalCounterparty++;
				CounterpatiesList.Add (counterparty);
			}

		}

		void ParseAccount(XmlNode node)
		{
			var account1c = new Account1c ();
			Account account = account1c.DomainAccount;
			var codeNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Код']/Значение");
			account.Code1c = codeNode.InnerText;

			var ownerNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Владелец']/Ссылка/Свойство[@Имя='Код']/Значение");
			if (ownerNode != null)
				account1c.OwnerCode1c = ownerNode.InnerText;
			else
				logger.Warn ("Счет с кодом {0}, без владельца.", codeNode.InnerText);

			var nameNode = node.SelectSingleNode ("Свойство[@Имя='Наименование']/Значение");
			account.Name = nameNode.InnerText;

			var bankNode = node.SelectSingleNode ("Свойство[@Имя='Банк']/Ссылка/Свойство[@Имя='Код']/Значение");

			if (bankNode != null)
			{
				var readedBank = Banks1cList.Find (b => b.Code1c == bankNode.InnerText);
				if(readedBank.IsDead)
				{
					account.Inactive = true;
				}
				else
				{
					account.InBank = readedBank.DomainBank;
				}
			}
			else
			{
				logger.Warn ("Счет с кодом {0}, без банка.", codeNode.InnerText);
				account.Inactive = true;
			}

			var numberNode = node.SelectSingleNode ("Свойство[@Имя='НомерСчета']/Значение");
			if (numberNode == null)
			{
				logger.Warn ("Пустой номер счета.");
				account.Inactive = true;
			}
			else
				account.Number = numberNode.InnerText;

			AccountsList.Add (account1c);
			ReadedAccounts++;
			if(account.Inactive)
				InactiveAccounts++;
		}

		void ParseBank(XmlNode node)
		{
			var bank1c = new Bank1c ();
			var codeNode = node.SelectSingleNode ("Ссылка/Свойство[@Имя='Код']/Значение");

			if (codeNode == null)
			{
				logger.Warn ("В банке нет  кода!!! Пропускаем...");
				return;
			}
			
			bank1c.Code1c = codeNode.InnerText;

			var domainBank = Banks.FirstOrDefault (b => b.Bik == bank1c.Code1c);
			bank1c.DomainBank = domainBank;
			bank1c.IsDead = domainBank == null;

			if (bank1c.IsDead)
				InactiveBanks++;

			Banks1cList.Add (bank1c);
			ReadedBanks++;
		}

		void ParseContactInfo(XmlNode node)
		{
			var typeNode = node.SelectSingleNode ("Свойство[@Имя='Вид']/Значение");
			var objectNode = node.SelectSingleNode ("Свойство[@Имя='Объект']/Ссылка/Свойство[@Имя='Код']/Значение");

			var counterparty = CounterpatiesList.Find (c => c.Code1c == objectNode.InnerText);
			if(counterparty == null)
			{
				logger.Debug ("Контактная информация видемо для пропущеного контаргента. Идем дальше...");
				return;
			}
			var presentationNode = node.SelectSingleNode ("Свойство[@Имя='Представление']/Значение");

			switch(typeNode.InnerText)
			{
			case "ЮрАдресКонтрагента":
				counterparty.JurAddress = presentationNode.InnerText;
				break;
			case "ФактАдресКонтрагента":
				counterparty.Address = presentationNode.InnerText;
				break;
			case "ТелефонКонтрагента":
				counterparty.Comment = String.Format ("Телефоны импортированные из 1с 7.7: {0}\n{1}", presentationNode.InnerText, counterparty.Comment);
				break;
			default:
				logger.Warn ("Неизвестный тип контактной информации ({0})", typeNode.InnerText);
				break;
			}
		}

		private void ParseNomenclature(XmlNode node)
		{
			logger.Debug("Парсим номенклатуру");
			var parentNode = node.SelectSingleNode("Ссылка/Свойство[@Имя='ЭтоГруппа']/Значение");
			if (parentNode != null)
				return;
			
			var code1cNode 		 = node.SelectSingleNode("Ссылка/Свойство[@Имя='Код']/Значение");
			var nameNode 		 = node.SelectSingleNode("Свойство[@Имя='Наименование']/Значение");
			var officialNameNode = node.SelectSingleNode("Свойство[@Имя='НаименованиеПолное']/Значение");
			var servicelNode 	 = node.SelectSingleNode("Свойство[@Имя='Услуга']/Значение");

			logger.Debug("Создаем номенклатуру");
			var nomenclature = new Nomenclature
			{
				Code1c = code1cNode?.InnerText,
				Name = nameNode?.InnerText,
				OfficialName = officialNameNode?.InnerText
			};
			nomenclature.Category = servicelNode?.InnerText == "true"
				? NomenclatureCategory.service
				: NomenclatureCategory.additional;
			switch (nomenclature.Category)
			{
				case NomenclatureCategory.service:
					nomenclature.Unit = UnitServ;
					break;
				case NomenclatureCategory.additional:
					nomenclature.Unit = unitU;
					break;
				default:
					nomenclature.Unit = unitU;
					break;
			}

			NomenclaturesList.Add(nomenclature);
			ReadedNomenclatures++;
		}

		private void ParseOrders(XmlNode node)
		{
			List<OrderItem> orderItems = new List<OrderItem>();

			logger.Debug("Парсим заказ");
			var code1cNode 		 	  = node.SelectSingleNode("Ссылка/Свойство[@Имя='Номер']/Значение");
			var dateNode 		 	  = node.SelectSingleNode("Ссылка/Свойство[@Имя='Дата']/Значение");
			var organisationNode 	  = node.SelectSingleNode("Свойство[@Имя='Организация']/Ссылка/Свойство[@Имя='Код']/Значение");
			var commentNode 	 	  = node.SelectSingleNode("Свойство[@Имя='Комментарий']/Значение");
			var deliverySchedulesNode = node.SelectSingleNode("Свойство[@Имя='ВремяДоставки']/Значение");;
			var counterpartyNode 	  = node.SelectSingleNode("Свойство[@Имя='Контрагент']/Ссылка/Свойство[@Имя='Код']/Значение");
			var addressNode 	 	  = node.SelectSingleNode("Свойство[@Имя='АдресДоставки']/Значение");
			var toClient 	 		  = node.SelectSingleNode("Свойство[@Имя='ОбоорудованиеКлиенту']/Значение");
			var fromClient 	 	  	  = node.SelectSingleNode("Свойство[@Имя='ОбоорудованиеОтКлиента']/Значение");
			var goodsNodes 		 	  = node.SelectNodes("ТабличнаяЧасть[@Имя='Товары']/Запись");
			var servicesNodes 	 	  = node.SelectNodes("ТабличнаяЧасть[@Имя='Услуги']/Запись");
			var nPayment 			  = node.SelectSingleNode("Свойство[@Имя='Организация']/Ссылка/Свойство[@Имя='Код']/Значение");

			//TODO Предусмотреть самовывоз в адресе
			DeliveryPoint deliveryPoint = DeliveryPointsList.FirstOrDefault(d => d.Address1c == addressNode?.InnerText);
			Counterparty client = CounterpatiesList.FirstOrDefault(c => c.Code1c == counterpartyNode?.InnerText);

			if (client == null)
				return;
			
			#if SHORT
//			if (addressNode?.InnerText != null)
//				if (addressNode.InnerText.ToLower().Contains("самовывоз"))
//					return;
			#endif

			DateTime deliveryDate = Convert.ToDateTime(dateNode?.InnerText.Split('T')[0] ?? "0001-01-01");
			if (!LoadedOrderDates.Contains(deliveryDate))
				LoadedOrderDates.Add(deliveryDate);

			var deliverySchedule = DeliverySchedules.FirstOrDefault(x => x.Name == deliverySchedulesNode?.InnerText);

			PaymentType paymentType = PaymentType.cashless;
			if (nPayment != null)
			{
				if(nPayment.InnerText.Contains(cahsString))
					paymentType = PaymentType.cash;
				if (nPayment.InnerText.Contains(cashlessString) || nPayment.InnerText.Contains(barterString))
					paymentType = PaymentType.cashless;
			}

			logger.Debug($"Создаем заказ {code1cNode?.InnerText}");
			Order order = new Order
				{
					Code1c 		  	 	= code1cNode?.InnerText,
					Comment 	  	 	= commentNode?.InnerText,
					Client 		  	 	= client,
					DeliveryDate  	 	= deliveryDate,
					DeliverySchedule 	= deliverySchedule,
					DeliverySchedule1c 	= deliverySchedulesNode?.InnerText,
					DeliveryPoint 	 	= deliveryPoint,
					Address1c 	  	 	= addressNode?.InnerText,
					PaymentType 		= paymentType,
					ToClientText 		= toClient?.InnerText,
					FromClientText 		= fromClient?.InnerText
				};
			//Заполняем товары для заказа
			logger.Debug($"Парсим товары для заказа {code1cNode?.InnerText}");
			foreach(var item in goodsNodes){
				orderItems.Add(ParseOrderItem(item as XmlNode, order));
			}
			//Заполняем услуги для заказа
			logger.Debug($"Парсим услуги {code1cNode?.InnerText}");
			foreach (var item in servicesNodes){
				orderItems.Add(ParseOrderItem(item as XmlNode, order));
			}

			order.OrderItems = orderItems;
			OrdersList.Add(order);
			ReadedOrders++;
		}

		private OrderItem ParseOrderItem(XmlNode node, Order order)
		{
			var nCode1c = node.SelectSingleNode("Свойство[@Имя='Номенклатура']/Ссылка/Свойство[@Имя='Код']/Значение");
			var nCount 	= node.SelectSingleNode("Свойство[@Имя='Количество']/Значение");
			var nPrice 	= node.SelectSingleNode("Свойство[@Имя='Цена']/Значение");
			var nSumm 	= node.SelectSingleNode("Свойство[@Имя='Сумма']/Значение");
			var nNDS 	= node.SelectSingleNode("Свойство[@Имя='СуммаНДС']/Значение");
			//NumberFormatInfo нужен для корректного перевода строки в число
			var nfi = new NumberFormatInfo();

			int count 	  = Convert.ToInt32(nCount?.InnerText ?? "0");
			decimal price = Convert.ToDecimal(nPrice?.InnerText ?? "0", nfi);
			decimal summ  = Convert.ToDecimal(nSumm?.InnerText ?? "0", nfi);
			int discount  = 0;
			//Проверяем на наличие скидки и вычисляем ее
			if (summ != price * count)
			{
				if(price * count == 0)
				{
					logger.Warn("Странные данные в строке заказа {0}: цена: {1} кол: {2} сумма: {3}", order.Code1c, price, count, summ);
				}
				else
				{
					decimal tempDiscount = summ / (price * count);
					discount = (int)(1 - tempDiscount * 100);
				}
			}

			return new OrderItem
			{
				Nomenclature = NomenclaturesList.FirstOrDefault(n => n.Code1c == nCode1c?.InnerText),
				IncludeNDS 	 = nNDS?.InnerText == null ? 0 : Convert.ToDecimal(nNDS.InnerText, nfi),
				Count 		 = count,
				Price 		 = price,
				Discount 	 = discount,
				Order 		 = order
			};
		}
		#endregion

		protected void OnFilechooserXMLSelectionChanged (object sender, EventArgs e)
		{
			buttonLoad.Sensitive = !String.IsNullOrWhiteSpace (filechooserXML.Filename);
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			progressbar.Text = "Записываем данные в базу...";
			logger.Info("Записываем данные в базу...");
			UoW.Commit ();
			progressbar.Text = "Выполнено";
			buttonSave.Sensitive = false;
		}

		protected void OnButtonCreateClicked (object sender, EventArgs e)
		{
			bool rewrite = checkRewrite.Active;
			checkRewrite.Sensitive = false;
			progressbar.Text = "Загружаем таблицу существующих контрагентов.";
			QSMain.WaitRedraw ();
			var counterpartyCodes1c = CounterpatiesList.Select(c => c.Code1c).ToArray();
			var ExistCouterpaties = Repository.CounterpartyRepository.GetCounterpartiesByCode1c (UoW, counterpartyCodes1c);

			progressbar.Text = "Сверяем контрагентов...";
			progressbar.Adjustment.Value = 0;
			progressbar.Adjustment.Upper = CounterpatiesList.Count;
			QSMain.WaitRedraw ();

			foreach(var loaded in CounterpatiesList)
			{
				progressbar.Adjustment.Value++;
				QSMain.WaitRedraw ();

				var exist = ExistCouterpaties.FirstOrDefault (c => c.Code1c == loaded.Code1c);
				//TODO подумать про проверку по ИНН если надо.
				//Проверка контрагентов
				if (exist != null)
				{
					var change = ChangedItem.CompareAndChange(exist, loaded);
					if (change != null) {
						ChangedCounterparties++;
						Changes.Add(change);
						UoW.Save(exist);
					}
					//FIXME Если понадобится тут нужно реклизовать проверку счетов на изменения.
					continue;
						
					foreach (var loadedAcc in  loaded.Accounts)
					{
						var existAcc = exist.Accounts.FirstOrDefault(a => a.Code1c == loadedAcc.Code1c);
						if (existAcc != null)
							loadedAcc.Id = existAcc.Id;
					}
				}
				else
				{
					NewCounterparties++;
					
					if (loaded.DefaultAccount != null && !loaded.Accounts.Contains (loaded.DefaultAccount))
						loaded.AddAccount (loaded.DefaultAccount);
					
					UoW.Save (loaded);
				}
					
			}
			List<Nomenclature> ExistNomenclatures = null;
			if (!checkOnlyAddress.Active)
			{
				progressbar.Text = "Загружаем таблицу существующих номенклатур.";
				ExistNomenclatures = UoW.GetAll<Nomenclature>().ToList<Nomenclature>();

				progressbar.Text = "Сверяем номенклатуры...";
				progressbar.Adjustment.Value = 0;
				progressbar.Adjustment.Upper = NomenclaturesList.Count;
				QSMain.WaitRedraw();

				//Проверка номенклатур
				foreach (var loaded in NomenclaturesList)
				{
					progressbar.Adjustment.Value++;
					QSMain.WaitRedraw();

					var exist = ExistNomenclatures.FirstOrDefault(n => n.Code1c == loaded.Code1c);

					if (exist != null)
					{
						if (!rewrite)
							continue;

						loaded.Id = exist.Id;
					}
					else
						NewNomenclatures++;
					UoW.Save(loaded);
				}
			}

			progressbar.Text = "Загружаем таблицу существующих заказов.";
			var orderCodes1c = OrdersList.Select(c => c.Code1c).ToArray();
			var ExistOrders = Repository.OrderRepository.GetOrdersByCode1c(UoW, orderCodes1c);

			progressbar.Text = "Сверяем заказы...";
			progressbar.Adjustment.Value = 0;
			progressbar.Adjustment.Upper = OrdersList.Count;

			List<Order> OrdersInDataBase = new List<Order>();
			foreach (var date in LoadedOrderDates)
				OrdersInDataBase.AddRange(Repository.OrderRepository.GetOrdersBetweenDates(UoW, date, date));
			
			QSMain.WaitRedraw ();

			//Проверка заказов
			foreach (var loaded in OrdersList)
			{
				progressbar.Adjustment.Value++;
				QSMain.WaitRedraw ();

				#if SHORT
				foreach (var item in loaded.OrderItems) {
					if(ExcludeNomenclatures.Contains(item.Nomenclature.Code1c))
						continue;
				}
				#endif

				var existCounterparty = ExistCouterpaties.FirstOrDefault(n => n.Code1c == loaded.Client.Code1c);
				if (existCounterparty != null)
					loaded.Client = existCounterparty;

				if (loaded.Client.Id > 0)
				{
					loaded.DeliveryPoint = loaded.Client.DeliveryPoints.FirstOrDefault(x => x.Address1c == loaded.Address1c);
				}

				if (checkOnlyAddress.Active)
				{
					if(loaded.DeliveryPoint == null)
					{
//						if (loaded.Address1c.ToLower().Contains("самовывоз"))
//							continue;
						var newPoint = DeliveryPoint.Create(loaded.Client);
						newPoint.Address1c = loaded.Address1c;
						UoW.Save(newPoint);
						NewAddresses++;
					}
					continue;
				}

				if (loaded.Address1c != null && loaded.Address1c.ToLower().Contains("самовывоз"))
					loaded.SelfDelivery = true;

				foreach (var item in loaded.OrderItems)
				{
					var existNom = ExistNomenclatures.FirstOrDefault(n => n.Code1c == item.Nomenclature.Code1c);
					if (existNom != null)
					{
						item.Nomenclature = existNom;
					}
				}

				loaded.SumToReceive = loaded.TotalSum;

				var exist = ExistOrders.FirstOrDefault(o => o.Code1c == loaded.Code1c
					&& o.DeliveryDate.Value.Year == loaded.DeliveryDate.Value.Year);

				if (exist != null)
				{
					var change = ChangedItem.CompareAndChange(exist, loaded);
					if (change != null) {
						ChangedOrders++;
						Changes.Add(change);
						UoW.Save(exist);
					}
				}
				else
				{
					NewOrders++;
					if (loaded.DeliveryPoint == null)
						NewAddresses++;

					if (loaded.DeliveryPoint != null && loaded.DeliverySchedule != null 
						&& !String.IsNullOrWhiteSpace(loaded.DeliveryPoint.CompiledAddress))
						loaded.ChangeStatus(OrderStatus.Accepted);

					UoW.Save (loaded);
				}
			}
			var notLoaded = GetNotLoadedOrders(OrdersInDataBase);
			if (notLoaded != null)
			{
				Changes.Add(notLoaded);
				labelLostOrders.LabelProp = notLoaded.Fields.Count.ToString();
			}
				
			progressbar.Text = "Выполнено";
			buttonSave.Sensitive = checkRewrite.Sensitive = true;
			buttonCreate.Sensitive = buttonLoad.Sensitive = false;
			vboxChanges.Visible = Changes.Count > 0;
			ytreeEntites.ItemsDataSource = Changes;
		}

		private ChangedItem GetNotLoadedOrders(List<Order> orders) 
		{
			foreach (var loadedOrder in OrdersList)
			{
				var order = orders?.FirstOrDefault(o => o.Code1c == loadedOrder.Code1c);
				if (order != null)
					orders.Remove(order);
			}
				
			var result = new List<FieldChange>();

			foreach (var order in orders)
			{
				result.Add(new FieldChange(
					string.Format("Заказ с кодом {0} и номером {1}",
						string.IsNullOrWhiteSpace(order.Code1c)
						? "(нет кода)" : order.Code1c,
						order.Id),
					string.Empty, string.Empty));
			}

			if (result.Count > 0)
				return new ChangedItem
				{
					Title = string.Format("Заказы, которые не были загружены ({0} штук(а))", orders.Count),
					Fields = result
				};
			else
				return null;
		}
	}
}

