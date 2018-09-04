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
		public Guid DefaultPriceGuid = Guid.Parse("beae9b72-cbd3-46ec-bddf-10104d5dd3e6");

		public Groups ProductGroups { get; set; }

		public Classifier Classifier { get; set; }
		public Catalog Catalog { get; set; }

		private Root rootCatalog;
		private Root rootOffers;

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

		public void OnProgressTextOnly(string text)
		{
			CurrentTaskText = text;
			ProgressUpdated?.Invoke(this, EventArgs.Empty);
		}


#endregion

		public Export(IUnitOfWork uow )
		{
			UOW = uow;
		}

		public void RunToDirectory(string dir)
		{
			Errors.Clear();

			CreateObjects();

			OnProgressPlusOneTask("Сохраняем import.xml");
			using(XmlWriter writer = XmlWriter.Create(Path.Combine(dir, "import.xml"), Export.WriterSettings)) {
				rootCatalog.ToXml().WriteTo(writer);
			}

			OnProgressPlusOneTask("Сохраняем Изображения");
			var exportedImages = Catalog.Goods.Nomenclatures.SelectMany(x => x.Images);
			var imageDir = Path.Combine(dir, "import_files");
			Directory.CreateDirectory(imageDir);

			foreach(var img in exportedImages)
			{
				var imgFileName = Path.Combine(imageDir, $"img_{img.Id:0000000}.jpg");
				OnProgressTextOnly("Сохраняем " + imgFileName);
				File.WriteAllBytes(imgFileName, img.Image);
			}

			OnProgressPlusOneTask("Сохраняем offers.xml");
			using(XmlWriter writer = XmlWriter.Create(Path.Combine(dir, "offers.xml"), Export.WriterSettings)) {
				rootOffers.ToXml().WriteTo(writer);
			}

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

			OnProgressPlusOneTask("Выгружаем изображения");
			var exportedImages = Catalog.Goods.Nomenclatures.SelectMany(x => x.Images);

			foreach(var img in exportedImages) {
				var imgFileName = $"img_{img.Id:0000000}.jpg";
				var dirImgFileName = $"import_files/" + imgFileName;
				OnProgressTextOnly("Отправляем " + imgFileName);

				request = new RestRequest("1c_exchange.php?type=catalog&mode=file&filename=" + dirImgFileName, Method.POST);
				request.AddFile(imgFileName, img.Image, dirImgFileName);
				response = client.Execute(request);
				DebugResponse(response);
			}

			OnProgressPlusOneTask("Выгружаем наличие");

			request = new RestRequest("1c_exchange.php?type=catalog&mode=file&filename=offers.xml", Method.POST);
			//request.AddParameter("filename", "import.xml");
			//request.AddFile("import.xml", s => rootCatalog.WriteToStream(s), "import.xml");
			using(MemoryStream stream = new MemoryStream()) {
				rootOffers.WriteToStream(stream);
				stream.Position = 0;
				var file = stream.ToArray();

				request.AddFile("offers.xml", file, "offers.xml");
			}
			response = client.Execute(request);
			DebugResponse(response);

			SendImportCommand(client, "import.xml");
			SendImportCommand(client, "offers.xml");

		}

		private void SendImportCommand(RestClient client, string filename)
		{
			OnProgressPlusOneTask("Ожидаем обработки данных на сайте");

			var request = new RestRequest("1c_exchange.php?type=catalog&mode=import&filename=" + filename, Method.GET);
			IRestResponse response;

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

			rootCatalog = new Root(this, RootContents.Catalog);

			rootOffers = new Root(this, RootContents.Offers);

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
