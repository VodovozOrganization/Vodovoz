using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Vodovoz.Domain.Payments;

namespace Vodovoz.ViewModels.ViewModels.Payments
{
	public class TransferDocumentsFromBankParser
	{
		public List<TransferDocument> TransferDocuments { get; set; }

		public string DocPath { get; private set; }

		private Dictionary<string, string> documentProperties;
		private List<Dictionary<string, string>> accounts;

		private string[] tags = { "СекцияРасчСчет", "СекцияДокумент", "КонецДокумента", "КонецФайла" };
		private readonly decimal[] curVersion = { 1.02M, 1.03M };

		public TransferDocumentsFromBankParser(string docPath)
		{
			DocPath = docPath;
			documentProperties = new Dictionary<string, string>();
			accounts = new List<Dictionary<string, string>>();
		}

		public void Parse()
		{
			int i;
			string line;

			var doc = new TransferDocument();
			TransferDocuments = new List<TransferDocument>();

			var culture = CultureInfo.CreateSpecificCulture("ru-RU");
			culture.NumberFormat.NumberDecimalSeparator = ".";

			using(var reader = new StreamReader(DocPath, Encoding.GetEncoding(1251))) 
			{
				var count = 1;

				if(reader.ReadLine() != "1CClientBankExchange")
				{
					return;
				}

				var str = reader.ReadLine().Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

				var version = decimal.Parse(str[1], culture);

				if(!curVersion.Contains(version))
				{
					throw new Exception("Изменилась версия выписки! Необходимо проверить формат данных.");
				}

				while((line = reader.ReadLine()) != null) 
				{
					//Читаем свойства документа
					while(line != null && !line.StartsWith(tags[0]))
					{
						if(!string.IsNullOrWhiteSpace(line))
						{
							var data = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

							if(data.Length == 2) {
								if(data[0] == "РасчСчет") {
									documentProperties.Add(data[0] + count, data[1]);
									count++;
								} else {
									documentProperties.Add(data[0], data[1]);
								}
							}
						}
						line = reader.ReadLine();
					}

					//Читаем рассчетные счета
					i = -1;
					while(line != null && !line.StartsWith(tags[1]))
					{
						if(!string.IsNullOrWhiteSpace(line))
						{
							if(line.StartsWith(tags[0]))
							{
								i++;
							}

							if(accounts.Count <= i)
							{
								accounts.Add(new Dictionary<string, string>());
							}

							var dataArray = line.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

							if(dataArray.Length == 2) {
								accounts[i].Add(dataArray[0], dataArray[1]);
							}
						}
						line = reader.ReadLine();
					}

					//Читаем документы
					while(line != null && !line.StartsWith(tags[3]))
					{
						if(line.StartsWith(tags[2]))
						{
							TransferDocuments.Add(doc);
						}

						if(line.StartsWith(tags[1]))
						{
							doc = new TransferDocument();
						}

						if(!string.IsNullOrWhiteSpace(line))
						{
							var data = line.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);

							if(data.Length == 2)
							{
								FillData(doc, data, culture);
							}
						}
						line = reader.ReadLine();
					}
				}
			}
		}

		private void FillData(TransferDocument doc, string[] data, IFormatProvider culture)
		{
			switch(data[0])
			{
				case "СекцияДокумент":
					doc.docType = TransferDocument.GetDocTypeFromString(data[1]);
					break;
				case "Номер":
					doc.DocNum = data[1];
					break;
				case "Дата":
					doc.Date = DateTime.Parse(data[1], culture);
					break;
				case "Сумма":
					doc.Total = decimal.Parse(data[1], culture);
					break;
				case "КвитанцияДата":
					doc.ReceiptDate = string.IsNullOrWhiteSpace(data[1]) ? (DateTime?)null : DateTime.Parse(data[1], culture);
					break;
				case "ПлательщикСчет":
					doc.PayerAccount = data[1];
					break;
				case "ДатаСписано":
					doc.WriteOffDate = string.IsNullOrWhiteSpace(data[1]) ? (DateTime?)null : DateTime.Parse(data[1], culture);
					break;
				case "Плательщик1":
					if(data[1].Contains("р/с") && !string.IsNullOrWhiteSpace(data[1].Substring(0, data[1].IndexOf("р/с"))))
						doc.PayerName = data[1].Substring(0, data[1].IndexOf("р/с"));
					else
						doc.PayerName = data[1];
					break;
				case "Плательщик2":
					doc.PayerCurrentAccount = data[1];
					break;
				case "Плательщик3":
					doc.PayerBank = data[1];
					break;
				case "Плательщик4":
					doc.CityOfPayerBank = data[1];
					break;
				case "ПлательщикИНН":
					doc.PayerInn = data[1];
					break;
				case "ПлательщикКПП":
					doc.PayerKpp = data[1];
					break;
				case "ПлательщикРасчСчет":
					doc.PayerCurrentAccount = data[1];
					break;
				case "ПлательщикБанк1":
					doc.PayerBank = data[1];
					break;
				case "ПлательщикБанк2":
					doc.CityOfPayerBank = data[1];
					break;
				case "ПлательщикБИК":
					doc.PayerBik = data[1];
					break;
				case "ПлательщикКорсчет":
					doc.PayerCorrespondentAccount = data[1];
					break;
				case "ПолучательСчет":
					doc.RecipientAccount = data[1];
					break;
				case "ДатаПоступило":
					doc.ReceivedDate = string.IsNullOrWhiteSpace(data[1]) ? (DateTime?)null : DateTime.Parse(data[1], culture);
					break;
				case "Получатель1":
					if(data[1].Contains("р/с") && !string.IsNullOrWhiteSpace(data[1].Substring(0, data[1].IndexOf("р/с"))))
						doc.RecipientName = data[1].Substring(0, data[1].IndexOf("р/с"));
					else
						doc.RecipientName = data[1];
					break;
				case "Получатель2":
					doc.RecipientCurrentAccount = data[1];
					break;
				case "Получатель3":
					doc.RecipientBank = data[1];
					break;
				case "Получатель4":
					doc.CityOfRecipientBank = data[1];
					break;
				case "ПолучательИНН":
					doc.RecipientInn = data[1];
					break;
				case "ПолучательКПП":
					doc.RecipientKpp = data[1];
					break;
				case "ПолучательРасчСчет":
					doc.RecipientCurrentAccount = data[1];
					break;
				case "ПолучательБанк1":
					doc.RecipientBank = data[1];
					break;
				case "ПолучательБанк2":
					doc.CityOfRecipientBank = data[1];
					break;
				case "ПолучательБИК":
					doc.RecipientBik = data[1];
					break;
				case "ПолучательКорсчет":
					doc.RecipientCorrespondentAccount = data[1];
					break;
				case "НазначениеПлатежа":
					doc.PaymentPurpose = data[1];
					break;
				case "Плательщик":
					doc.Payer = data[1];
					break;
				case "Получатель":
					doc.Recipient = data[1];
					break;
				case "ВидПлатежа":
					doc.PaymentType = data[1];
					break;
				case "КодНазПлатежа":
					doc.PaymentCode = data[1];
					break;
				case "ВидОплаты":
					doc.OperationType = data[1];
					break;
				case "Код":
					doc.PaymentId = data[1];
					break;
				case "СтатусСоставителя":
					doc.AuthorStatus = data[1];
					break;
				case "ПоказательКБК":
					doc.BudgetСlassificationСode = data[1];
					break;
				case "ОКАТО":
					doc.Okato = data[1];
					break;
				case "ПоказательОснования":
					doc.BaseIndicator = data[1];
					break;
				case "ПоказательПериода":
					doc.PeriodIndicator = data[1];
					break;
				case "ПоказательНомера":
					doc.NumIndicator = data[1];
					break;
				case "ПоказательДаты":
					doc.DateIndicator = data[1];
					break;
				case "ПоказательТипа":
					doc.TypeIndicator = data[1];
					break;
				case "Очередность":
					doc.Priority = data[1];
					break;
				case "СрокАкцепта":
					doc.AcceptancePeriod = data[1];
					break;
				case "ВидАккредитива":
					doc.ApplicationOfCreditType = data[1];
					break;
				case "СрокПлатежа":
					doc.PaymentTerm = data[1];
					break;
				case "УсловиеОплаты1":
					doc.PaymentConditionStr1 = data[1];
					break;
				case "УсловиеОплаты2":
					doc.PaymentConditionStr2 = data[1];
					break;
				case "УсловиеОплаты3":
					doc.PaymentConditionStr3 = data[1];
					break;
				case "ПлатежПоПредст":
					doc.PaymentBySubmission = data[1];
					break;
				case "ДополнУсловия":
					doc.AdditionalTerms = data[1];
					break;
				case "НомерСчетаПоставщика":
					doc.VendorAccount = data[1];
					break;
				case "ДатаОтсылкиДок":
					doc.DocSendDate = data[1];
					break;
			}
		}
	}
}
