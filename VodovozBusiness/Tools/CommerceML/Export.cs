using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using QSOrmProject;
using QSSupportLib;
using RestSharp;
using RestSharp.Authenticators;
using Vodovoz.Repository;
using Vodovoz.Tools.CommerceML.Nodes;

namespace Vodovoz.Tools.CommerceML
{
	public class Export
	{
#region Глобальные настройки экспорта
		static public XmlWriterSettings WriterSettings = new XmlWriterSettings
			{
				OmitXmlDeclaration = true,
				Indent = true,
				Encoding = System.Text.Encoding.UTF8,
				NewLineChars = "\r\n"
			};

		public const string OnlineStoreUrlParameterName = "online_store_export_url";
		public const string OnlineStoreLoginParameterName = "online_store_export_login";
		public const string OnlineStorePasswordParameterName = "online_store_export_password";

#endregion

		public IUnitOfWork UOW { get; private set; }

		public List<string> Errors = new List<string>();

		public Owner DefaultOwner { get; private set; }

		public Groups ProductGroups { get; set; }

		private Root rootCatalog;

#region Progress

		public EventHandler ProgressUpdated;

		public string CurrentTaskText { get; set; }

		public int CurrentTask = -1;

		public int TotalTasks = 5;

		public void OnProgressPlusOneTask(string text)
		{
			CurrentTaskText = text;
			CurrentTask++;
			ProgressUpdated?.Invoke(this, EventArgs.Empty);
		}

#endregion

		public Export(IUnitOfWork uow )
		{
			UOW = uow;
		}

		public void RunToDirectory()
		{
			Errors.Clear();

			CreateObjects();
		}

		public void RunToSite()
		{
			Errors.Clear();

			OnProgressPlusOneTask("Соединяемся с сайтом");
			//Проверяем связь с сервером
			var baseUrl = MainSupport.BaseParameters.All[OnlineStoreUrlParameterName].TrimEnd('/');
			var client = new RestClient(baseUrl);
			client.Authenticator = new HttpBasicAuthenticator(MainSupport.BaseParameters.All[OnlineStoreLoginParameterName],
			                                                  MainSupport.BaseParameters.All[OnlineStorePasswordParameterName]);
			var request = new RestRequest("1c_exchange.php?type=catalog&mode=checkauth", Method.GET);
			IRestResponse response = client.Execute(request);
			DebugResponse(response);
			if(!response.Content.StartsWith("success"))
			{
				Errors.Add($"Не возможно связаться с сайтом. Ответ Сайта:{response.Content}");
				return;
			}

			CreateObjects();


			OnProgressPlusOneTask("Инициализация процесса обмена");
			//Инициализируем передачу. Ответ о сжатии и размере нас не интересует. Мы не умеем других вариантов.
			request = new RestRequest("1c_exchange.php?type=catalog&mode=init", Method.GET);
			response = client.Execute(request);
			DebugResponse(response);

			OnProgressPlusOneTask("Выгружаем каталог");
		
			request = new RestRequest("1c_exchange.php?type=catalog&mode=file&filename=import.xml", Method.POST);
			//request.AddParameter("filename", "import.xml");
			//request.AddFile("import.xml", s => rootCatalog.WriteToStream(s), "import.xml");
			using(MemoryStream stream = new MemoryStream())
			{
				rootCatalog.WriteToStream(stream);
				stream.Position = 0;
				var file = stream.ToArray();

				request.AddFile("import.xml", file, "import.xml");
			}
			response = client.Execute(request);
			DebugResponse(response);

			OnProgressPlusOneTask("Ожидаем обработки данных на сайте");

			request = new RestRequest("1c_exchange.php?type=catalog&mode=import&filename=import.xml", Method.GET);

			int i = 0;
			do {
				i++;
				CurrentTaskText = $"Ожидаем обработки данных на сайте {i}";
				ProgressUpdated?.Invoke(this, EventArgs.Empty);

				response = client.Execute(request);
				DebugResponse(response);
			} while(response.Content.StartsWith("progress"));

		}

		void DebugResponse(IRestResponse response)
		{
			Errors.Add(response.ResponseUri.ToString());
			Errors.Add(response.StatusCode.ToString());
			Errors.Add(response.Content);
		}


		void  CreateObjects()
		{
			OnProgressPlusOneTask("Получение общих объектов");

			var org = OrganizationRepository.GetOrganizationByInn(UOW, "7816453294");
			DefaultOwner = new Owner(this, org);

			rootCatalog = new Root(this);

			if(Errors.Count == 0) {
				OnProgressPlusOneTask("Сохраняем созданные Guid's в базу.");
				UOW.Commit();
			}
		}

		public XElement GetXml()
		{
			OnProgressPlusOneTask("Формируем XML");
			return rootCatalog.ToXml();
		}
	}
}
