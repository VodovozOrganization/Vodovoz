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
		List<DeliveryPoint> DeliveryPointsList = new List<DeliveryPoint>();
		List<Account1c> AccountsList = new List<Account1c>();
		List<Bank1c> Banks1cList = new List<Bank1c>();

		List<string> missingCounterparties = new List<string>();
		List<string> missingDeliveryPoints = new List<string>();

		List<string> errorLog = new List<string>();

		public LoadFrom1cClientsAndDeliveryPoints()
		{
			this.Build();

			FileFilter Filter = new FileFilter();
			Filter.Name = "XML выгрузка";
			Filter.AddMimeType("application/xml");
			Filter.AddPattern("*.xml");
			filechooser.Filter = Filter;


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

		int totalCounterparty = 0;

		public int TotalCounterparty {
			get {
				return totalCounterparty;
			}
			set {
				totalCounterparty = value;
				//labelTotalCounterparty.LabelProp = totalCounterparty.ToString();
				QSMain.WaitRedraw();
			}
		}

		int skipedCounterparty = 0;

		public int SkipedCounterparty {
			get {
				return skipedCounterparty;
			}
			set {
				skipedCounterparty = value;
				//labelSkipedCounterparty.LabelProp = SkipedCounterparty.ToString();
				QSMain.WaitRedraw();
			}
		}

		#endregion

		private string GetINN(string innkpp)
		{
			if(innkpp.Contains('\\'))
			{
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
				missingCounterparties.Add(code1c);
				errorLog.Add(string.Format("Не найден контрагент Код1с: {0}", code1c));
				return;
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
				}else {
					errorLog.Add(string.Format("Не корректный ИНН: {0} [Контрагент: {1}]", innStr, code1c));
				}

				string kppStr = GetKPP(innkppAttr.Value);
				if(Regex.IsMatch(kppStr, "^[0-9]{9,9}$")) {
					counterparty.KPP = kppStr;
				} else {
					errorLog.Add(string.Format("Не корректный КПП: {0} [Контрагент: {1}]", kppStr, code1c));
				}
			}

			var personTypeAttr = node.Attributes["ВидКонтрагента"];
			if(personTypeAttr != null) {
				counterparty.PersonType = personTypeAttr.Value == "ЮрЛица" ? PersonType.legal : PersonType.natural;
				counterparty.PaymentMethod = personTypeAttr.Value == "ЮрЛица" ? PaymentType.cashless : PaymentType.cash;
			}

			var phonesAttr = node.Attributes["Телефоны"];
			if(phonesAttr != null) {
				counterparty.Comment += string.Format("{0}\n", phonesAttr.Value);;
			}

			var addressAttr = node.Attributes["ФактАдрес"];
			if(addressAttr != null) {
				counterparty.Address = addressAttr.Value;
			}

			var JurAddressAttr = node.Attributes["ЮрАдрес"];
			if(JurAddressAttr != null) {
				counterparty.JurAddress = JurAddressAttr.Value;
			}

			var internalNumberAttr = node.Attributes["ВремНомер"];
			int internalNumber = 0;
			if(internalNumberAttr != null && int.TryParse(internalNumberAttr.Value, out internalNumber)) {
				counterparty.VodovozInternalId = internalNumber;
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

			var commentAttr = node.Attributes["Комментариий"];
			if(commentAttr != null) {
				counterparty.Comment += string.Format("{0}\n", commentAttr.Value);
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
						//БИК не корректный
					}
					if(!Regex.IsMatch(accountNumber, "^[0-9]{20,25}$")) {
						errorLog.Add(string.Format("Не корректный счет: {0} [Контрагент: {1}]", accountNumber, code1c));
						//Номер счета не корректный
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
			CounterpatiesList.Add(counterparty);
		}

		void ParseDeliveryPoint(XmlNode node)
		{
			var codeAttr = node.Attributes["Код"];
			if(codeAttr == null) {
				return;
			}

			var code = codeAttr.Value;

			Counterparty counterparty = null;

			var codeClientAttr = node.Attributes["КодКонтрагента"];
			if(codeClientAttr == null) {
				errorLog.Add(string.Format("Точка доставки без контрагента. Точка доставки: {0}", code));
				return;
			}

			var counterpartyCode = codeClientAttr.Value;
			counterparty = CounterpatiesList.FirstOrDefault(x => x.Code1c == counterpartyCode);
			if(counterparty == null) {
				errorLog.Add(string.Format("Не найден контрагент для точки доставки: {0} [Контрагент: {1}]", code, counterpartyCode));
				return;
			}


			var deliveryPoint = counterparty.DeliveryPoints.FirstOrDefault(x => x.Code1c == code);
			if(deliveryPoint == null) {
				errorLog.Add(string.Format("У контрагента: VodovozId:{0} не существует точки доставки с кодом 1с: {1}", counterparty.Id, code));
				return;
			}

			var addrAttr = node.Attributes["Наименование"];
			if(addrAttr != null) {
				deliveryPoint.Address1c = addrAttr.Value;
			}

			var defWaterAttr = node.Attributes["Вид"];
			if(defWaterAttr != null) { 
				Nomenclature defaultWater = null;
				if(defWaterAttr.Value == "Ручка") {
					defaultWater = UoW.GetById<Nomenclature>(15);
				} else if(defWaterAttr.Value == "Стройка") {
					defaultWater = UoW.GetById<Nomenclature>(7);
				}
				deliveryPoint.DefaultWaterNomenclature = defaultWater;
			}

			var commentAttr = node.Attributes["Комментариий"];
			if(commentAttr != null) {
				deliveryPoint.Comment = commentAttr.Value;
			}

			var waterAgreements = UoW.Session.QueryOver<WaterSalesAgreement>()
								   .Where(Restrictions.Or
										  (
											  Restrictions.IsNull(Projections.Property<WaterSalesAgreement>(x => x.DeliveryPoint)),
											  Restrictions.Where<WaterSalesAgreement>(x => x.DeliveryPoint.Id == deliveryPoint.Id)
										  )
										 ).List();

			var waterAgreement = waterAgreements.FirstOrDefault(x => x.DeliveryPoint == deliveryPoint);
			if(waterAgreement == null) {
				waterAgreement = waterAgreements.FirstOrDefault(x => x.DeliveryPoint == null);
			}
			decimal price;
			if(waterAgreement != null) {
				//Семиозерье id 1
				var price1 = node.Attributes["Цена1"];
				if(price1 == null) {
					price = 0m;
					Nomenclature nomenclature1 = UoW.GetById<Nomenclature>(1);
					if(decimal.TryParse(price1.Value, out price) && nomenclature1 != null) {
						var fixPrice = waterAgreement.FixedPrices.FirstOrDefault(x => x.Nomenclature.Id == nomenclature1.Id);
						if(fixPrice == null) {
							waterAgreement.AddFixedPrice(nomenclature1, price);
						} else if(price > 0 && fixPrice.Price != price) {
							fixPrice.Price = price;
						}
					}
				}

				//Кислородная id 12
				var price2 = node.Attributes["Цена2"];
				if(price2 != null) {
					price = 0m;
					Nomenclature nomenclature2 = UoW.GetById<Nomenclature>(12);
					if(decimal.TryParse(price2.Value, out price) && nomenclature2 != null) {
						var fixPrice = waterAgreement.FixedPrices.FirstOrDefault(x => x.Nomenclature.Id == nomenclature2.Id);
						if(fixPrice == null) {
							waterAgreement.AddFixedPrice(nomenclature2, price);
						} else if(price > 0 && fixPrice.Price != price) {
							fixPrice.Price = price;
						}
					}
				}

				//Снятогорская id 2
				var price3 = node.Attributes["Цена3"];
				if(price3 != null) {
					price = 0m;
					Nomenclature nomenclature3 = UoW.GetById<Nomenclature>(2);
					if(decimal.TryParse(price3.Value, out price) && nomenclature3 != null) {
						var fixPrice = waterAgreement.FixedPrices.FirstOrDefault(x => x.Nomenclature.Id == nomenclature3.Id);
						if(fixPrice == null) {
							waterAgreement.AddFixedPrice(nomenclature3, price);
						} else if(price > 0 && fixPrice.Price != price) {
							fixPrice.Price = price;
						}
					}
				}

				//Стройка id 7
				var price4 = node.Attributes["Цена4"];
				if(price4 != null) {
					price = 0m;
					Nomenclature nomenclature4 = UoW.GetById<Nomenclature>(7);
					if(decimal.TryParse(price4.Value, out price) && nomenclature4 != null) {
						var fixPrice = waterAgreement.FixedPrices.FirstOrDefault(x => x.Nomenclature.Id == nomenclature4.Id);
						if(fixPrice == null) {
							waterAgreement.AddFixedPrice(nomenclature4, price);
						} else if(price > 0 && fixPrice.Price != price) {
							fixPrice.Price = price;
						}
					}
				}

				//Ручки id 15
				var price5 = node.Attributes["Цена5"];
				if(price5 != null) {
					price = 0m;
					Nomenclature nomenclature5 = UoW.GetById<Nomenclature>(15);
					if(decimal.TryParse(price5.Value, out price) && nomenclature5 != null) {
						var fixPrice = waterAgreement.FixedPrices.FirstOrDefault(x => x.Nomenclature.Id == nomenclature5.Id);
						if(fixPrice == null) {
							waterAgreement.AddFixedPrice(nomenclature5, price);
						} else if(price > 0 && fixPrice.Price != price) {
							fixPrice.Price = price;
						}
					}
				}
			} else {
				errorLog.Add(string.Format("Нет доп соглашения на воду для установки фиксы: Контрагент VodovozId:{0}, Точка доставки с кодом 1с: {1}", counterparty.Id, code));
				return;
			}
			//UoW.Save<DeliveryPoint>(deliveryPoint);
			//DeliveryPointsList.Add(deliveryPoint);
		}


		protected void OnFilechooserXMLSelectionChanged(object sender, EventArgs e)
		{
			buttonLoad.Sensitive = !String.IsNullOrWhiteSpace(filechooser.Filename);
		}

		protected void OnButtonLoadClicked(object sender, EventArgs e)
		{
			XmlDocument content = new XmlDocument();
			content.Load(filechooser.Filename);
			var counterPartyNodes = content.SelectNodes("/root/Контрагент");
			var counterpartyCounter = 0;
			progressbar.Adjustment.Upper = counterPartyNodes.Count;
			foreach(XmlNode node in counterPartyNodes) {
				ParseCounterparty(node);
				progressbar.Text = string.Format("Контрагенты: {0} из {1}", counterpartyCounter, counterPartyNodes.Count);
				progressbar.Adjustment.Value = counterpartyCounter;
				QSMain.WaitRedraw();
				counterpartyCounter++;
			}

			foreach(XmlNode node in content.SelectNodes("/root/Адрес")) {
				ParseDeliveryPoint(node);
			}

			File.WriteAllLines("ImportLog.txt", errorLog);
			progressbar.Text = "Выполнено. Лог ошибок в файле ImportLog.txt в каталоге с программой";
		}

		protected void OnButton1Clicked(object sender, EventArgs e)
		{
			foreach(var item in CounterpatiesList) {
				UoW.Save<Counterparty>(item);
			}
		}
	}
}
