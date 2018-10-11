using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using QS.DomainModel.UoW;
using QSSupportLib;
using RestSharp;
using RestSharp.Authenticators;
using Vodovoz.Repository;
using Vodovoz.Tools.CommerceML.Nodes;

namespace Vodovoz.Tools.CommerceML
{
	public class Export
	{
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        #region Глобальные настройки экспорта
        static public XmlWriterSettings WriterSettings = new XmlWriterSettings
			{
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
        public List<string> Results = new List<string>();

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

		public string CurrentStepText => $"({CurrentTask} из {TotalTasks})";

		public int CurrentTask = -1;

		public int TotalTasks = 10;

		public void OnProgressPlusOneTask(string text)
		{
			CurrentTaskText = text;
			CurrentTask++;
			ProgressUpdated?.Invoke(this, EventArgs.Empty);
            logger.Info(CurrentTaskText + CurrentStepText);
		}

		public void OnProgressTextOnly(string text)
		{
			CurrentTaskText = text;
			ProgressUpdated?.Invoke(this, EventArgs.Empty);
            logger.Info(CurrentTaskText + CurrentStepText);
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
            	rootCatalog.GetXDocument().Save(writer);
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
				rootOffers.GetXDocument().Save(writer);
			}

		}

		public void RunToSite()
		{
			Errors.Clear();
            TotalTasks = 10;

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
            SendFileXMLDoc(client, "import.xml", rootCatalog);
            
			OnProgressPlusOneTask("Выгружаем изображения");
			var exportedImages = Catalog.Goods.Nomenclatures.SelectMany(x => x.Images);

			foreach(var img in exportedImages) {
				var imgFileName = $"img_{img.Id:0000000}.jpg";
				var dirImgFileName = $"import_files/" + imgFileName;
				OnProgressTextOnly("Отправляем " + imgFileName);

                //Внимание здесь "/" после 1c_exchange.php не случаен в документации его нет, но если его не написать то на запрос без слеша,
                // приходит ответ 301 то есть переадресация на такую же строку но со слешем, но RestSharp после переадресации уже отправляет
                // не POST запрос а GET, из за чего, файл не принимается нормально сервером.
                request = new RestRequest("1c_exchange.php/?type=catalog&mode=file&filename=" + dirImgFileName, Method.POST);
                request.AddParameter("image/jpeg", img.Image, ParameterType.RequestBody);
				response = client.Execute(request);
				DebugResponse(response);
			}

			OnProgressPlusOneTask("Выгружаем склад");
            SendFileXMLDoc(client, "offers.xml", rootOffers);

            Results.Add("Выгрузка каталога товаров:");
			SendImportCommand(client, "import.xml");
            Results.Add("Выгрузка склада и цен:");
            SendImportCommand(client, "offers.xml");

            Results.Add("Выгружено изображений: " + exportedImages.Count());
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
                Results.Add(response.Content);
			} while(response.Content.StartsWith("progress"));
		}

		private void SendFileXMLDoc(RestClient client, string filename, Root root)
        {
            //Внимание здесь "/" после 1c_exchange.php не случаен в документации его нет, но если его не написать то на запрос без слеша,
			// приходит ответ 301 то есть переадресация на такую же строку но со слешем, но RestSharp после переадресации уже отправляет
			// не POST запрос а GET, из за чего, файл не принимается нормально сервером.
            var request = new RestRequest("1c_exchange.php/?type=catalog&mode=file&filename=" + filename, Method.POST);

            using (MemoryStream stream = new MemoryStream())
            {
                root.WriteToStream(stream);
                stream.Position = 0;
                var file = stream.ToArray();

                request.AddParameter("application/xml", file, ParameterType.RequestBody);
            }

            var response = client.Execute(request);
            DebugResponse(response);
        }

        void DebugResponse(IRestResponse response)
		{
            if (response == null)
            {
                logger.Error("Ответ пустой.");
                return;
            }
			logger.Debug(response.ResponseUri?.ToString());
            logger.Debug(response.StatusCode.ToString());
            logger.Debug(response.Content);
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
