using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Gtk;
using QS.DomainModel.UoW;
using QSBanks;
using QSContacts;
using QSProjectsLib;
using QS.Tdi;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.LoadFrom1c;
using QSBanks.Repositories;

namespace Vodovoz.ServiceDialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class LoadFrom1cClientsAndDeliveryPoints : QS.Dialog.Gtk.TdiTabBase
	{
		private IUnitOfWork UoW = UnitOfWorkFactory.CreateWithoutRoot();

		List<Counterparty> VodovozCounterparties = new List<Counterparty>();
		List<Counterparty> CounterpatiesList = new List<Counterparty>();
		List<Counterparty> CounterpatiesInternalDuplicateList = new List<Counterparty>();
		List<DeliveryPoint> DeliveryPointsList = new List<DeliveryPoint>();
		List<Account1c> AccountsList = new List<Account1c>();
		List<Bank1c> Banks1cList = new List<Bank1c>();

		Nomenclature nomStroika = null;
		Nomenclature nomRuchki = null;

		Dictionary<int, int> internalNumbers = new Dictionary<int, int>();

		List<string> errorLog = new List<string>();

		public LoadFrom1cClientsAndDeliveryPoints()
		{
			if(!QSMain.User.Permissions["database_maintenance"]) {
				MessageDialogWorks.RunWarningDialog("Доступ запрещён!", "У вас недостаточно прав для доступа к этой вкладке. Обратитесь к своему руководителю.", Gtk.ButtonsType.Ok);
				FailInitialize = true;
				return;
			}

			throw new ApplicationException("Сюда сейчас нельзя.");//Counterparty.VodInternalId генерируется тригером на стороне БД. Исправить соответствующий код ниже.

			this.Build();

			TabName = "Загрузка из 1с";

			FileFilter Filter = new FileFilter();
			Filter.Name = "XML выгрузка";
			Filter.AddMimeType("application/xml");
			Filter.AddPattern("*.xml");
			filechooser.Filter = Filter;

			nomStroika = UoW.GetById<Nomenclature>(15);
			nomRuchki = UoW.GetById<Nomenclature>(7);

			errorLog.Add(string.Format("Статус;Код1с контрагента;Код1с точки доставки;Причина"));

		}

		void ErrorLog(string status, string counterpartyCode1c, string deliveryPointCode1c, string reason)
		{
				errorLog.Add(string.Format("{0};{1};{2};{3}",
				                           status,
				                           counterpartyCode1c,
				                           deliveryPointCode1c,
				                           reason));			
		}

		#region Свойства
		private IList<Bank> banks;

		private IList<Bank> Banks {
			get {
				if(banks == null) {
					banks = BankRepository.ActiveBanks(UoW);
				}
				return banks;
			}
		}
		#endregion

		#region Label свойства

		int totalCounterparty;
		public int TotalCounterparty {
			get { return totalCounterparty; }
			set {
				totalCounterparty = value;
				labelCounterpartyTotalValue.LabelProp = totalCounterparty.ToString();
				QSMain.WaitRedraw();
			}
		}

		int skipedCounterparty;
		public int SkipedCounterparty {
			get { return skipedCounterparty; }
			set {
				skipedCounterparty = value;
				labelCounterpartyFailsValue.LabelProp = skipedCounterparty.ToString();
				QSMain.WaitRedraw();
			}
		}

		int errorsCounterparty;
		public int ErrorsCounterparty {
			get { return errorsCounterparty; }
			set {
				errorsCounterparty = value;
				labelCounterpartyErrorsValue.LabelProp = errorsCounterparty.ToString();
				QSMain.WaitRedraw();
			}
		}

		int successCounterparty;
		public int SuccessCounterparty {
			get { return successCounterparty; }
			set {
				successCounterparty = value;
				labelCounterpartySuccessValue.LabelProp = successCounterparty.ToString();
				QSMain.WaitRedraw();
			}
		}

		int totalDP;
		public int TotalDP {
			get { return totalDP; }
			set {
				totalDP = value;
				labelDPTotalValue.LabelProp = totalDP.ToString();
				QSMain.WaitRedraw();
			}
		}

		int skipedDP;
		public int SkipedDP {
			get { return skipedDP; }
			set {
				skipedDP = value;
				labelDPFailsValue.LabelProp = skipedDP.ToString();
				QSMain.WaitRedraw();
			}
		}

		int errorsDP;
		public int ErrorsDP {
			get { return errorsDP; }
			set {
				errorsDP = value;
				labelDPErrorsValue.LabelProp = errorsDP.ToString();
				QSMain.WaitRedraw();
			}
		}

		int successDP;
		public int SuccessDP {
			get { return successDP; }
			set {
				successDP = value;
				labelDPSuccessValue.LabelProp = successDP.ToString();
				QSMain.WaitRedraw();
			}
		}

		#endregion


		private string GetINN(string innkpp)
		{
			if(innkpp.Contains('\\')) {
				return innkpp.Split('\\').FirstOrDefault();
			}
			if(innkpp.Contains('/')) {
				return innkpp.Split('/').FirstOrDefault();
			}
			return "";
		}

		private string GetKPP(string innkpp)
		{
			if(innkpp.Contains('\\')) {
				return innkpp.Split('\\').LastOrDefault();
			}
			if(innkpp.Contains('/')) {
				return innkpp.Split('/').LastOrDefault();
			}
			return "";
		}

		string GetAttributeValue(XmlNode node, string attributeName)
		{
			var attr = node.Attributes[attributeName];
			if(attr != null) {
				return attr.Value;
			}
			return null;
		}

		protected string TryGetOrganizationType(string name)
		{
			/*foreach(var pair in InformationHandbook.OrganizationTypes) {
				string pattern = String.Format(@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Key);
				string fullPattern = String.Format(@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Value);
				Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
				if(regex.IsMatch(name))
					return pair.Key;
				regex = new Regex(fullPattern, RegexOptions.IgnoreCase);
				if(regex.IsMatch(name))
					return pair.Key;
			}*/
			string pattern = String.Format(@".*(^|\(|\s|\W|['""])ИП($|\)|\s|\W|['""]).*");
			Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
			if(regex.IsMatch(name))
				return "ИП";
			
			return null;
		}

		void ParseCounterparty(XmlNode node)
		{
			bool error = false;
			bool isVodInternalDuplicate = false;
			Counterparty counterparty = null;
			var codeAttr = node.Attributes["Код"];
			if(codeAttr == null) {
				return;
			}

			var code1c = codeAttr.Value;

			counterparty = VodovozCounterparties.FirstOrDefault(x => x.Code1c == code1c);
			
			if(counterparty == null) {

				ErrorLog("Не загружен", code1c, "", "Не найден контрагент");
				SkipedCounterparty++;
				return;
			}

			var internalNumberAttr = node.Attributes["ВремНомер"];
			int internalNumber = 0;
			if(internalNumberAttr != null && int.TryParse(internalNumberAttr.Value, out internalNumber)) {
				//Убираем из словаря внутренний номер для нахождения дубликатов
				int buffer = 0;
				bool restoreDictionary = false;
				if(internalNumbers.ContainsKey(counterparty.Id)) {
					buffer = internalNumbers[counterparty.Id];
					internalNumbers.Remove(counterparty.Id);
					restoreDictionary = true;
				}

				//Нахождение дубликата
				bool internalNumberExists = internalNumbers.ContainsValue(internalNumber);

				//Восстановление словаря
				if(restoreDictionary) {
					internalNumbers.Add(counterparty.Id, buffer);
				}

				//Отметка дубликата для испраления в дальнейшем
				if(internalNumberExists) {
					isVodInternalDuplicate = true;
					//Записываем номер для того чтобы отобразить его в логе
					counterparty.VodovozInternalId = internalNumber;
				}else {
					if(!internalNumbers.ContainsKey(counterparty.Id)) {
						internalNumbers.Add(counterparty.Id, internalNumber);
					}
					counterparty.VodovozInternalId = internalNumber;
				}
			}

			var nameAttr = node.Attributes["Наименование"];
			if(nameAttr != null) {
				counterparty.Name = nameAttr.Value;
				string orgType = TryGetOrganizationType(counterparty.Name);
				if(!string.IsNullOrEmpty(orgType)) {
					counterparty.TypeOfOwnership = orgType;
				}
			}

			var fullNameAttr = node.Attributes["ОфициальноеНаименование"];
			if(fullNameAttr != null) {
				counterparty.FullName = fullNameAttr.Value;
			}

			var innkppAttr = node.Attributes["ИНН"];
			if(innkppAttr != null) {
				
				string innStr = GetINN(innkppAttr.Value);
				if(counterparty.TypeOfOwnership == "ИП" ? Regex.IsMatch(innStr, "^[0-9]{12,12}$") : Regex.IsMatch(innStr, "^[0-9]{10,10}$")) {
					counterparty.INN = innStr;
				} else if(!string.IsNullOrEmpty(innStr)) {
					ErrorLog("Загружен с ошибками", code1c, "", string.Format("Не корректный ИНН: {0}", innStr));
					error = true;
				}

				string kppStr = GetKPP(innkppAttr.Value);
				if(Regex.IsMatch(kppStr, "^[0-9]{9,9}$")) {
					counterparty.KPP = kppStr;
				} else if(!string.IsNullOrEmpty(kppStr)) {
					ErrorLog("Загружен с ошибками", code1c, "", string.Format("Не корректный КПП: {0}", kppStr));
					error = true;
				}
			}

			var personTypeAttr = node.Attributes["ВидКонтрагента"];
			if(personTypeAttr != null) {
				var isJur = personTypeAttr.Value == "ЮрЛица" || personTypeAttr.Value == "СвоиЮрЛица";
				counterparty.PersonType = isJur ? PersonType.legal : PersonType.natural;
				counterparty.PaymentMethod = isJur ? PaymentType.cashless : PaymentType.cash;
			}

			var addressAttr = node.Attributes["ФактАдрес"];
			if(addressAttr != null) {
				counterparty.Address = addressAttr.Value;
			}

			var JurAddressAttr = node.Attributes["ЮрАдрес"];
			if(JurAddressAttr != null) {
				counterparty.RawJurAddress = JurAddressAttr.Value;
			}

			var ringupPhoneAttr = node.Attributes["ТелДляОбзвона"];
			if(ringupPhoneAttr != null) {
				counterparty.RingUpPhone = ringupPhoneAttr.Value;
			}

			var mailsAttr = node.Attributes["Почта"];
			if(mailsAttr != null) {
				var mails = Regex.Matches(mailsAttr.Value.ToLower(), @"[a-z0-9-]{1,30}@[a-z0-9-]{1,65}.[a-z]{1,}");
				for(int i = 0; i < mails.Count; i++) {
					if(!counterparty.Emails.Any(x => x.Address == mails[i].Value)) {
						counterparty.Emails.Add(new Email { Address = mails[i].Value });
					}
				}
			}
			string comment = null;
			var commentAttr = node.Attributes["Комментариий"];
			if(commentAttr != null) {
				if(string.IsNullOrEmpty(comment)){
					comment = string.Format("\n{0}", commentAttr.Value);
				} else {
					comment += string.Format("\n{0}", commentAttr.Value);
				}
			}

			var phonesAttr = node.Attributes["Телефоны"];
			if(phonesAttr != null) {
				if(string.IsNullOrEmpty(comment)){
					comment = string.Format("\n{0}", phonesAttr.Value);
				}else {
					comment += string.Format("\n{0}", phonesAttr.Value);
				}
			}

			if(!string.IsNullOrEmpty(comment)){
				counterparty.Comment = comment;
			}

			var accountAttr = node.Attributes["НомерСчета"];
			var bikAttr = node.Attributes["БИК"];
			if(accountAttr != null && bikAttr != null) {
				string accountNumber = accountAttr.Value;
				string bik = bikAttr.Value;
				Account account = counterparty.Accounts.FirstOrDefault(x => x.Number == accountNumber);
				if(account == null) {
					if(bik.Length == 8) {
						bik = string.Format("0{0}", bik);
					}
					if(!Regex.IsMatch(bik, "^[0-9]{9,9}$")) {
						ErrorLog("Загружен с ошибками", code1c, "", string.Format("Не корректный БИК: {0}", bik));
						error = true;
					}
					if(!Regex.IsMatch(accountNumber, "^[0-9]{20,25}$") && !string.IsNullOrEmpty(accountNumber)) {
						ErrorLog("Загружен с ошибками", code1c, "", string.Format("Не корректный счет: {0}", accountNumber));
						error = true;
					} else {
						Bank bank = Banks.FirstOrDefault(b => b.Bik == bik);
						if(bank != null) {
							account = new Account {
								Number = accountNumber,
								Owner = counterparty,
								Name = "Основной",
								InBank = bank,
								Inactive = false
							};
							counterparty.Accounts.Add(account);
							counterparty.DefaultAccount = account;
						}else {
							ErrorLog("Загружен с ошибками", code1c, "", string.Format("Для счета : {0} не найден банк БИК: {1}", accountNumber, bik));
							error = true;
						}
					}
				}
			}

			if(error) {
				ErrorsCounterparty++;
			}else {
				SuccessCounterparty++;
			}

			if(isVodInternalDuplicate) {
				CounterpatiesInternalDuplicateList.Add(counterparty);
				return;
			}

			CounterpatiesList.Add(counterparty);
		}

		void ParseDeliveryPoint(XmlNode node)
		{
			bool error = false;
			var codeAttr = node.Attributes["Код"];
			if(codeAttr == null) {
				return;
			}

			var code = codeAttr.Value;

			Counterparty counterparty = null;

			var codeClientAttr = node.Attributes["КодКонтрагента"];
			if(codeClientAttr == null) {
				ErrorLog("Не загружена ТД", "", code, "Точка доставки без контрагента");
				SkipedDP++;
				return;
			}

			var counterpartyCode = codeClientAttr.Value;
			counterparty = CounterpatiesList.FirstOrDefault(x => x.Code1c == counterpartyCode);
			if(counterparty == null) {
				ErrorLog("Не загружена ТД", counterpartyCode, code, "Не найден контрагент для точки доставки");
				SkipedDP++;
				return;
			}


			var deliveryPoint = counterparty.DeliveryPoints.FirstOrDefault(x => x.Code1c == code);
			if(deliveryPoint == null) {
				ErrorLog("Не загружена ТД", counterpartyCode, code, "У контрагента не существует этой точки доставки");
				SkipedDP++;
				return;
			}

			var addrAttr = node.Attributes["Наименование"];
			if(addrAttr != null) {
				deliveryPoint.Address1c = addrAttr.Value;
			}

			var defWaterAttr = node.Attributes["Вид"];
			if(defWaterAttr != null) {
				if(defWaterAttr.Value == "Ручка") {
					deliveryPoint.DefaultWaterNomenclature = nomStroika;
				} else if(defWaterAttr.Value == "Стройка") {
					deliveryPoint.DefaultWaterNomenclature = nomRuchki;
				}
			}

			var commentAttr = node.Attributes["Комментариий"];
			if(commentAttr != null) {
				deliveryPoint.АddressAddition = commentAttr.Value;
			}


			decimal price;
			//Семиозерье id 1
			var price1 = node.Attributes["Цена1"];
			if(price1 != null) {
				price = 0m;
				if(decimal.TryParse(price1.Value, out price) && price > 0) {
					deliveryPoint.FixPrice1 = price;
				}
			}

			//Кислородная id 12
			var price2 = node.Attributes["Цена2"];
			if(price2 != null) {
				price = 0m;
				if(decimal.TryParse(price2.Value, out price) && price > 0) {
					deliveryPoint.FixPrice2 = price;
				}
			}

			//Снятогорская id 2
			var price3 = node.Attributes["Цена3"];
			if(price3 != null) {
				price = 0m;
				if(decimal.TryParse(price3.Value, out price) && price > 0) {
					deliveryPoint.FixPrice3 = price;
				}
			}

			//Стройка id 7
			var price4 = node.Attributes["Цена4"];
			if(price4 != null) {
				price = 0m;
				if(decimal.TryParse(price4.Value, out price) && price > 0) {
					deliveryPoint.FixPrice4 = price;
				}
			}

			//Ручки id 15
			var price5 = node.Attributes["Цена5"];
			if(price5 != null) {
				price = 0m;
				if(decimal.TryParse(price5.Value, out price) && price > 0) {
					deliveryPoint.FixPrice5 = price;
				}
			}

			if(error) {
				ErrorsDP++;
			} else {
				SuccessDP++;
			}
		}

		private void GenerateNewVodInternalNumbers()
		{
			int maxInternal = internalNumbers.Max(x => x.Value);

			foreach(var item in CounterpatiesInternalDuplicateList) {
				var oldNumber = item.VodovozInternalId;
				var newNumber = maxInternal + 1;
				ErrorLog("Дубликат", item.Code1c, "", string.Format("Контрагент с дублирующимся временным номером: старый номер: {0}, новый номер: {1}",
										   oldNumber, newNumber));
				item.VodovozInternalId = newNumber;
				CounterpatiesList.Add(item);
				maxInternal = newNumber;
			}
		}


		protected void OnFilechooserXMLSelectionChanged(object sender, EventArgs e)
		{
			buttonLoad.Sensitive = !String.IsNullOrWhiteSpace(filechooser.Filename);
		}

		protected void OnButtonLoadClicked(object sender, EventArgs e)
		{
			LoadFromXML();
		}

		void LoadFromXML()
		{
			Clear();

			XmlDocument xmlDoc = new XmlDocument();
			xmlDoc.Load(filechooser.Filename);

			var counterPartyNodes = xmlDoc.SelectNodes("/root/Контрагент");
			TotalCounterparty = counterPartyNodes.Count;

			var deliveryPointsNodes = xmlDoc.SelectNodes("/root/Адрес");
			TotalDP = deliveryPointsNodes.Count;
			var progressCounter = 0;
			var totalCount = counterPartyNodes.Count + deliveryPointsNodes.Count;
			progressbar.Adjustment.Upper = totalCount;

			foreach(XmlNode node in counterPartyNodes) {
				ParseCounterparty(node);
				progressCounter++;

				progressbar.Text = string.Format("Элемент: {0} из {1}", progressCounter, totalCount);
				progressbar.Adjustment.Value = progressCounter;
				QSMain.WaitRedraw();
			}

			GenerateNewVodInternalNumbers();

			foreach(XmlNode node in deliveryPointsNodes) {
				ParseDeliveryPoint(node);
				progressCounter++;

				progressbar.Text = string.Format("Элемент: {0} из {1}", progressCounter, totalCount);
				progressbar.Adjustment.Value = progressCounter;
				QSMain.WaitRedraw();
			}
			File.WriteAllLines("ImportLog.csv", errorLog);
			progressbar.Text = "Выполнено. Лог ошибок в файле ImportLog.txt в каталоге с программой";
		}

		private void Clear()
		{
			TotalCounterparty = 0;
			ErrorsCounterparty = 0;
			SkipedCounterparty = 0;
			SuccessCounterparty = 0;
			TotalDP = 0;
			ErrorsDP = 0;
			SkipedDP = 0;
			SuccessDP = 0;
		}

		protected void OnButton1Clicked(object sender, EventArgs e)
		{
			PreLoadEntities();
		}

		void PreLoadEntities()
		{
			progressbar.Text = string.Format("Загрузка контрагентов и счетов");

			var counterpartyQuery = UoW.Session.QueryOver<Counterparty>().Future<Counterparty>();
			var counterpartyQuery2 = UoW.Session.QueryOver<Counterparty>()
										.Fetch(x => x.DeliveryPoints).Eager
										.Future<Counterparty>();
			var counterpartyQuery3 = UoW.Session.QueryOver<Counterparty>()
										.Fetch(x => x.Emails).Eager
										.Future<Counterparty>();
			var counterpartyQuery4 = UoW.Session.QueryOver<Counterparty>()
										.Fetch(x => x.Accounts).Eager
										.Future<Counterparty>();

			VodovozCounterparties = counterpartyQuery4.ToList();

			internalNumbers = VodovozCounterparties.Distinct()
												   .ToDictionary(x => x.Id, x => x.VodovozInternalId);
		}

		protected void OnButtonSaveClicked(object sender, EventArgs e)
		{
			Save();
		}

		void Save()
		{
			List<string> ErrorsCounterparties = new List<string>();
			int counter = 0;
			int batchCounter = 0;
			UoW.Session.SetBatchSize(500);
			foreach(var item in CounterpatiesList) {
				try {
					UoW.Save<Counterparty>(item);
				} catch(Exception ex) {
					ErrorLog("Ошибка сохранения", item.Code1c, string.Format("{0}", item.Id), string.Format("{0}", ex.Message));
					ErrorsCounterparties.Add(string.Format("{0}", item.Id));
				}
				progressbar.Text = string.Format("Сохранение: {0} из {1}", counter, CounterpatiesList.Count);
				QSMain.WaitRedraw();
				if(batchCounter == 500) {
					UoW.Commit();
					batchCounter = 0;
				}
				counter++;
				batchCounter++;
			}
			UoW.Commit();

			if(ErrorsCounterparties.Any()) {
				File.WriteAllLines("ErrorsCounterparties.txt", errorLog);
			}

			progressbar.Text = "Сохранено.";
		}

		protected void OnButton2Clicked(object sender, EventArgs e)
		{
			PreLoadEntities();
			LoadFromXML();
			Save();
			progressbar.Text =  string.Format("Завершено в {0}", DateTime.Now);
		}
	}
}
