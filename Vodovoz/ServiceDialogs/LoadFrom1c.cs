using System;
using QSTDI;
using Gtk;
using System.Xml;
using Vodovoz.Domain;

namespace Vodovoz
{
	public partial class LoadFrom1c : TdiTabBase
	{
		private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

		public LoadFrom1c ()
		{
			this.Build ();
			TabName = "Загрузка контрагентов из 1с 7.7";

			FileFilter Filter = new FileFilter();
			Filter.Name = "XML выгрузка";
			Filter.AddMimeType("application/xml");
			Filter.AddPattern("*.xml");
			filechooserXML.Filter = Filter;
		}

		protected void OnButtonLoadClicked (object sender, EventArgs e)
		{
			logger.Info ("Читаем XML файл...");
			XmlDocument content = new XmlDocument ();
			content.Load (filechooserXML.Filename);

			foreach(XmlNode node in content.SelectNodes ("/ФайлОбмена/Объект"))
			{
				string ruleName = node.Attributes ["ИмяПравила"].Value;
				switch(ruleName)
				{
				case "Контрагенты":
					ParseCounterparty (node);
					break;
				}

				/*	PatternField field = DocInfo.Fields.Find (f =>  fieldName.StartsWith (f.Name));
				if (field == null)
				{
					logger.Warn ("Поле {0} не найдено, поэтому пропущено.", fieldName);
					continue;
				}
				if (field.Type == PatternFieldType.FDate) // && node.Attributes ["office:date-value"] != null)
					node.Attributes ["office:string-value"].Value = field.value != DBNull.Value ? ((DateTime)field.value).ToLongDateString () : "";
				//node.Attributes ["office:date-value"].Value = field.value != DBNull.Value ? XmlConvert.ToString ((DateTime)field.value, XmlDateTimeSerializationMode.Unspecified) : "";
				else if (field.Type == PatternFieldType.FCurrency) 
				{
					if (fieldName.Replace (field.Name, "") == ".Число") 
					{
						((XmlElement)node).SetAttribute ("value-type", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", "currency");
						((XmlElement)node).SetAttribute ("value", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", XmlConvert.ToString ((decimal)field.value));
						string curr = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol;
						((XmlElement)node).SetAttribute ("currency", "urn:oasis:names:tc:opendocument:xmlns:office:1.0", curr);
					}
					if (fieldName.Replace (field.Name, "") == ".Пропись") 
					{
						string val = RusCurrency.Str ((int)(decimal)field.value, true, "рубль", "рубля", "рублей", "", "", "");
						node.Attributes ["office:string-value"].Value = val;
					}
				}
				else
					node.Attributes ["office:string-value"].Value = field.value.ToString (); */
								} 
		}

		void ParseCounterparty(XmlNode node)
		{
			var isGroup = node.SelectSingleNode ("Ссылка/Свойство[@Имя='ЭтоГруппа']/Значение");
			if (isGroup != null && isGroup.InnerText == "true") //Если не группа то isGroup == null так как нода "Значение" нет.
			{
				logger.Debug ("Это группа, пропускаем...");
				return;
			}

			var couterparty = new Counterparty ();

			var nameNode = node.SelectSingleNode ("Свойство[@Имя='Наименование']/Значение");
			couterparty.Name = nameNode.InnerText;
			logger.Debug ("Читаем контрагента <{0}>", couterparty.Name);
		}

		protected void OnFilechooserXMLSelectionChanged (object sender, EventArgs e)
		{
			buttonLoad.Sensitive = !String.IsNullOrWhiteSpace (filechooserXML.Filename);
		}

		protected void OnButtonSaveClicked (object sender, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	}
}

