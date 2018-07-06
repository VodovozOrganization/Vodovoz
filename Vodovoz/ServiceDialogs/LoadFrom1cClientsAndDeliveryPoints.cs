using System;
using System.Collections.Generic;
using Gtk;
using System.Linq;
using System.Xml;
using Vodovoz.Domain.Client;
using QSTDI;
using QSProjectsLib;
using QSBanks;
using QSOrmProject;
using QSWidgetLib;
using Vodovoz.LoadFrom1c;
using System.Text.RegularExpressions;
using QSContacts;
using Vodovoz.Domain.Goods;
using NHibernate.Criterion;
using System.IO;

namespace Vodovoz.ServiceDialogs
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class LoadFrom1cClientsAndDeliveryPoints : TdiTabBase
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
			this.Build();

			TabName = "Загрузка из 1с";

			FileFilter Filter = new FileFilter();
			Filter.Name = "XML выгрузка";
			Filter.AddMimeType("application/xml");
			Filter.AddPattern("*.xml");
			filechooser.Filter = Filter;

			nomStroika = UoW.GetById<Nomenclature>(15);
			nomRuchki = UoW.GetById<Nomenclature>(7);

			internalNumbers = UoW.Session.QueryOver<Counterparty>()
								 .WhereRestrictionOn(x => x.VodovozInternalId).IsNotNull
								 .SelectList( list => list
			                                 .Select(x => x.Id)
			                                 .Select(x => x.VodovozInternalId)			                                
			                                )
			                     .List<object[]>().ToDictionary(x => (int)x[0], x => (int)x[1]);
		}

		#region Свойства
		private IList<Bank> banks;

		private IList<Bank> Banks {
			get {
				if(banks == null) {
					banks = QSBanks.Repository.BankRepository.ActiveBanks(UoW);
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
			foreach(var pair in InformationHandbook.OrganizationTypes) {
				string pattern = String.Format(@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Key);
				string fullPattern = String.Format(@".*(^|\(|\s|\W|['""]){0}($|\)|\s|\W|['""]).*", pair.Value);
				Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
				if(regex.IsMatch(name))
					return pair.Key;
				regex = new Regex(fullPattern, RegexOptions.IgnoreCase);
				if(regex.IsMatch(name))
					return pair.Key;
			}
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
			counterparty = UoW.Session.QueryOver<Counterparty>()
							  .Where(x => x.Code1c == code1c).List()
							  .FirstOrDefault();
			if(counterparty == null) {
				errorLog.Add(string.Format("Не найден контрагент Код1с: {0}", code1c));
				SkipedCounterparty++;
				return;
			}

			var internalNumberAttr = node.Attributes["ВремНомер"];
			int internalNumber = 0;
			if(internalNumberAttr != null && int.TryParse(internalNumberAttr.Value, out internalNumber)) {
				var sdfsd = internalNumbers.Where(x => x.Key != counterparty.Id);
				var sdfgsdfdf = sdfsd.ToDictionary(x => x.Key, x => x.Value);

				if(sdfgsdfdf.ContainsValue(internalNumber)) {
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
					errorLog.Add(string.Format("Не корректный ИНН: {0} [Контрагент: {1}]", innStr, code1c));
					error = true;
				}

				string kppStr = GetKPP(innkppAttr.Value);
				if(Regex.IsMatch(kppStr, "^[0-9]{9,9}$")) {
					counterparty.KPP = kppStr;
				} else if(!string.IsNullOrEmpty(kppStr)) {
					errorLog.Add(string.Format("Не корректный КПП: {0} [Контрагент: {1}]", kppStr, code1c));
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
				counterparty.JurAddress = JurAddressAttr.Value;
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
					if(!Regex.IsMatch(bik, "^[0-9]{9,9}$")) {
						errorLog.Add(string.Format("Не корректный БИК: {0} [Контрагент: {1}]", bik, code1c));
						error = true;
					}
					if(!Regex.IsMatch(accountNumber, "^[0-9]{20,25}$") && !string.IsNullOrEmpty(accountNumber)) {
						errorLog.Add(string.Format("Не корректный счет: {0} [Контрагент: {1}]", accountNumber, code1c));
						error = true;
					} else {
						Bank bank = Banks.FirstOrDefault(b => b.Bik == bik);
						account = new Account {
							Number = accountNumber,
							Owner = counterparty,
							Name = "Основной",
							InBank = bank,
							Inactive = bank == null
						};
						counterparty.Accounts.Add(account);
						account.IsDefault = true;
						counterparty.DefaultAccount = account;
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
				errorLog.Add(string.Format("Точка доставки без контрагента. Точка доставки: {0}", code));
				SkipedDP++;
				return;
			}

			var counterpartyCode = codeClientAttr.Value;
			counterparty = CounterpatiesList.FirstOrDefault(x => x.Code1c == counterpartyCode);
			if(counterparty == null) {
				errorLog.Add(string.Format("Не найден контрагент для точки доставки: {0} [Контрагент: {1}]", code, counterpartyCode));
				SkipedDP++;
				return;
			}


			var deliveryPoint = counterparty.DeliveryPoints.FirstOrDefault(x => x.Code1c == code);
			if(deliveryPoint == null) {
				errorLog.Add(string.Format("У контрагента: VodovozId:{0} не существует точки доставки с кодом 1с: {1}", counterparty.Id, code));
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
				errorLog.Add(string.Format("Контрагент (Код1с: {0}) с дублирующимся временным номером: старый номер: {1}, новый номер: {2}", 
				                           item.Code1c, oldNumber, newNumber));
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
			File.WriteAllLines("ImportLog.txt", errorLog);
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
			foreach(var item in CounterpatiesList) {
				UoW.Save<Counterparty>(item);
				UoW.Commit();
			}

			progressbar.Text = "Сохранено.";
		}
	}
}
