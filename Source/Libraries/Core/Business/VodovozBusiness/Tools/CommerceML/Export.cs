using QS.DomainModel.UoW;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Vodovoz.EntityRepositories.Organizations;
using Vodovoz.Settings;
using Vodovoz.Tools.CommerceML.Nodes;

namespace Vodovoz.Tools.CommerceML
{
	/// <summary>
	/// Некоторые нюансы экспорта.
	/// Для UMI при отправке пост запроса нужно после .php добавлять \ иначе umi возвращает код переадресации 301, а RestSharp это корректно обрабатывает только для GET запроса.
	/// Для Bitrix на стороне сайта в админке /bitrix/admin/1c_admin.php надо обязательно отключить zip. Пункт "Использовать сжатие zip, если доступно" в расширенных настройках.
	/// Так как иначе сайт ждет архив.
	/// Так же для Битрикс, если на сайте стоит редакция для Малый Бизнес. Надо установить guid на сайте https://yadi.sk/i/ruFjv4Q0BFlDjg такой же как в свойстве DefaultPriceGuid. Так как Балый бизнес подерживает только один тип цен.
	/// </summary>
	public class Export
	{
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IOrganizationRepository _organizationRepository;
		private readonly ISettingsController _settingsController;

        #region Глобальные настройки экспорта
        static public XmlWriterSettings WriterSettings = new XmlWriterSettings
			{
				Indent = true,
				Encoding = System.Text.Encoding.UTF8,
				NewLineChars = "\r\n"
			};

		public const string OnlineStoreExportMode = "online_store_export_mode";
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

		public string CurrentStepText => $" ({CurrentTask} из {TotalTasks})";

		public int CurrentTask = 0;

		public int TotalTasks = 12;

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

		public Export(IUnitOfWork uow, IOrganizationRepository organizationRepository, ISettingsController settingsController)
		{
			UOW = uow;
			_organizationRepository = organizationRepository ?? throw new ArgumentNullException(nameof(organizationRepository));
			_settingsController = settingsController ?? throw new ArgumentNullException(nameof(settingsController));
		}

		public void RunToDirectory(string dir)
		{
			Errors.Clear();
			TotalTasks = 8;

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
            TotalTasks = 12;
			ExportMode mode = _settingsController.ContainsSetting(OnlineStoreExportMode) 
				? (ExportMode)Enum.Parse(typeof(ExportMode), _settingsController.GetStringValue(OnlineStoreExportMode))
				: ExportMode.Umi;

			OnProgressPlusOneTask("Соединяемся с сайтом");
			//Проверяем связь с сервером
			var configuredUrl = _settingsController.GetStringValue(OnlineStoreUrlParameterName);
			var parsedUrl = new Uri(configuredUrl);
			var path = parsedUrl.LocalPath; 
			var client = new RestClient(configuredUrl.Replace(path, ""));
			client.CookieContainer = new System.Net.CookieContainer();
			client.Authenticator = new HttpBasicAuthenticator(_settingsController.GetStringValue(OnlineStoreLoginParameterName),
				_settingsController.GetStringValue(OnlineStorePasswordParameterName));
			var request = new RestRequest(path + "?type=catalog&mode=checkauth", Method.GET);
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
			request = new RestRequest(path + "?type=catalog&mode=init", Method.GET);
			response = client.Execute(request);
			DebugResponse(response);

			OnProgressPlusOneTask("Выгружаем каталог");
			SendFileXMLDoc(client, path, "import.xml", rootCatalog, mode);
            
			OnProgressPlusOneTask("Выгружаем изображения");
			var exportedImages = Catalog.Goods.Nomenclatures.SelectMany(x => x.Images);
			//Внимание здесь "/" после 1c_exchange.php в выгрузке для umi не случаен, в документации его нет, но если его не написать то на запрос без слеша,
			// приходит ответ 301 то есть переадресация на такую же строку но со слешем, но RestSharp после переадресации уже отправляет
			// не POST запрос а GET, из за чего, файл не принимается нормально сервером.
			var beginOfUrl = path + (mode == ExportMode.Umi ? "/" : "") +"?type=catalog&mode=file&filename=";

			foreach(var img in exportedImages) {
				var imgFileName = $"img_{img.Id:0000000}.jpg";
				var dirImgFileName = $"import_files/" + imgFileName;
				OnProgressTextOnly("Отправляем " + imgFileName);
				request = new RestRequest(beginOfUrl + dirImgFileName, Method.POST);
				request.AddParameter("image/jpeg", img.Image, ParameterType.RequestBody);
				response = client.Execute(request);
				DebugResponse(response);
			}
			Results.Add("Выгружено изображений: " + exportedImages.Count());

			OnProgressPlusOneTask("Выгружаем склад");
			SendFileXMLDoc(client, path, "offers.xml", rootOffers, mode);

			Results.Add("Выгрузка каталога товаров:");
			OnProgressPlusOneTask("Импорт каталога товаров на сайте.");
			SendImportCommand(client, path, "import.xml");
			Results.Add("Выгрузка склада и цен:");
			OnProgressPlusOneTask("Импорт склада и цен на сайте.");
			SendImportCommand(client, path, "offers.xml");
        }

		private void SendImportCommand(RestClient client, string path, string filename)
		{
			var request = new RestRequest(path + "?type=catalog&mode=import&filename=" + filename, Method.GET);
			IRestResponse response = null;

			do {
				if(response != null) {
					string progress = DecodeResponseContent(response).Substring(9);//Здесь обрезаем progress
					CurrentTaskText = $"Ожидаем обработки данных на сайте [{progress}]";
				}
				ProgressUpdated?.Invoke(this, EventArgs.Empty);
				response = client.Execute(request);
				DebugResponse(response);
			} while(response.Content.StartsWith("progress"));
			Results.Add(DecodeResponseContent(response));
		}

		private void SendFileXMLDoc(RestClient client, string path, string filename, Root root, ExportMode mode)
		{
			//Внимание здесь "/" после 1c_exchange.php не случаен в документации его нет, но если его не написать то на запрос без слеша,
			// приходит ответ 301 то есть переадресация на такую же строку но со слешем, но RestSharp после переадресации уже отправляет
			// не POST запрос а GET, из за чего, файл не принимается нормально сервером.
			var beginOfUrl = path + (mode == ExportMode.Umi ? "/" : "") + "?type=catalog&mode=file&filename=";
			var request = new RestRequest(beginOfUrl + filename, Method.POST);

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
			logger.Debug("Cookies:{0}", String.Join(";", response.Cookies.Select(x => x.Name + "=" + x.Value)));
			logger.Debug(response.StatusCode.ToString());
			logger.Debug(DecodeResponseContent(response));
		}

		private string DecodeResponseContent(IRestResponse response)
		{
			if(response.ContentType != null && response.ContentType.Contains("charset=windows-1251")) {
				Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
				Encoding encoding = Encoding.GetEncoding("Windows-1251");
				return encoding.GetString(response.RawBytes);
			}
			return response.Content;
		}

		void  CreateObjects()
		{
			OnProgressPlusOneTask("Получение общих объектов");

			var org = _organizationRepository.GetOrganizationByInn(UOW, "7816453294");
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

	public enum ExportMode
	{
		Umi,
		Bitrix
	}
}
