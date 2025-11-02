using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Gamma.Utilities;
using NHibernate;
using NHibernate.Criterion;
using NHibernate.Dialect.Function;
using NHibernate.Transform;
using NHibernate.Util;
using QS.BusinessCommon.Domain;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using Vodovoz.Core.Domain.Goods;
using Vodovoz.Core.Domain.Users;
using Vodovoz.Domain;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.Domain.Store;
using Vodovoz.EntityRepositories.Goods;

namespace Vodovoz.ViewModels
{
	public class ExportImportNomenclatureCatalogViewModel : DialogTabViewModelBase, ITDICloseControlTab
	{
		public ExportImportNomenclatureCatalogViewModel(
			INomenclatureRepository nomenclatureRepository,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			INavigationManager navigation)
			: base(unitOfWorkFactory, commonServices.InteractiveService, navigation)
		{
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			this.nomenclatureRepository = nomenclatureRepository ?? throw new ArgumentNullException(nameof(nomenclatureRepository));

			TabName = "Выгрузка/Загрузка каталога номенклатур";

			ProgressBarMessages = new ConcurrentQueue<ColoredMessage>();
			nomenclatureCategories = new Dictionary<string, NomenclatureCategory>();
			foreach(NomenclatureCategory cat in Enum.GetValues(typeof(NomenclatureCategory))) {
				nomenclatureCategories.Add(cat.GetEnumTitle(), cat);
			}
			tareVolumes = new Dictionary<string, TareVolume>();
			foreach(TareVolume vol in Enum.GetValues(typeof(TareVolume))) {
				tareVolumes.Add(vol.GetEnumTitle(), vol);
			}
			saleCategories = new Dictionary<string, SaleCategory>();
			foreach(SaleCategory cat in Enum.GetValues(typeof(SaleCategory))) {
				saleCategories.Add(cat.GetEnumTitle(), cat);
			}
			typeOfDepositCategories = new Dictionary<string, TypeOfDepositCategory>();
			foreach(TypeOfDepositCategory cat in Enum.GetValues(typeof(TypeOfDepositCategory))) {
				typeOfDepositCategories.Add(cat.GetEnumTitle(), cat);
			}
			CreateCommands();
		}

		#region Properties

		private ICommonServices commonServices;
		private INomenclatureRepository nomenclatureRepository;
		private bool needReload = false;
		private Dictionary<string, NomenclatureCategory> nomenclatureCategories;
		private Dictionary<string, TareVolume> tareVolumes;
		private Dictionary<string, SaleCategory> saleCategories;
		private Dictionary<string, TypeOfDepositCategory> typeOfDepositCategories;
		private readonly Encoding encoding = Encoding.GetEncoding(1251);
		private IEnumerable<NomenclatureCatalogNode> itemsToSave => Items?
				.Where(x => x.Source == Source.File && x.ConflictSolveAction != ConflictSolveAction.Ignore);

		private bool dontChangeSellPrice;
		[Display(Name = "Не заменять цены на продажу")]
		public bool DontChangeSellPrice {
			get => dontChangeSellPrice;
			set { SetField(ref dontChangeSellPrice, value, () => DontChangeSellPrice); }
		}

		private string folderPath;
		[Display(Name = "Папка выгрузки")]
		public string FolderPath {
			get => folderPath;
			set { SetField(ref folderPath, value, () => FolderPath); }
		}

		private string filePath;
		[Display(Name = "Файл загрузки")]
		public string FilePath {
			get => filePath;
			set { SetField(ref filePath, value, () => FilePath); }
		}

		private double progressBarValue;
		[Display(Name = "Текущее значение прогресс бара")]
		public double ProgressBarValue {
			get => progressBarValue;
			set { SetField(ref progressBarValue, value, () => ProgressBarValue); }
		}

		private double progressBarUpper;
		[Display(Name = "Макс. значение прогресс бара")]
		public double ProgressBarUpper {
			get => progressBarUpper;
			set { SetField(ref progressBarUpper, value, () => ProgressBarUpper); }
		}

		private List<NomenclatureCatalogNode> items;
		[Display(Name = "Номенклатуры")]
		public List<NomenclatureCatalogNode> Items {
			get => items;
			set { SetField(ref items, value, () => Items); }
		}

		private LoadAction? currentState;
		[Display(Name = "Состояние")]
		public LoadAction? CurrentState {
			get => currentState;
			set {
				if(SetField(ref currentState, value, () => CurrentState)) {
					OnPropertyChanged(() => IsConfirmLoadNewButtonVisible);
					OnPropertyChanged(() => IsConfirmUpdateDataButtonVisible);
				}
			 }
		}

		private ConcurrentQueue<ColoredMessage> progressBarMessages;
		[Display(Name = "Сообщения прогресс бара")]
		public ConcurrentQueue<ColoredMessage> ProgressBarMessages {
			get => progressBarMessages;
			set { SetField(ref progressBarMessages, value, () => ProgressBarMessages); }
		}

		public bool IsConfirmLoadNewButtonVisible => CurrentState == LoadAction.LoadNew;
		public bool IsConfirmUpdateDataButtonVisible => CurrentState == LoadAction.UpdateData;

		public EventHandler ProgressBarMessagesUpdated;
		public bool TaskIsRunning { get; private set; } = false;

		#endregion

		public override bool HasChanges {
			get => false;
			set => base.HasChanges = value;
		}

		public bool CanClose()
		{
			if(TaskIsRunning) {
				commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Дождитесь завершения работы задачи");
				return false;
			}
			return true;
		}

		private void CreateCommands()
		{
			CreateExportCommand();
			CreateLoadActionCommand();
			CreateConfirmLoadNewCommand();
			CreateConfirmUpdateDataCommand();
		}

		#region HelperMethods

		private void SetProgressBar(double value, double upper, string text = null, ConsoleColor textColor = ConsoleColor.Black)
		{
			ProgressBarValue = value;
			ProgressBarUpper = upper;
			if(text != null)
				SetProgressBar(text, textColor);
		}

		private void SetProgressBar(string text, ConsoleColor textColor = ConsoleColor.Black)
		{
			if(ProgressBarMessages != null) {
				ProgressBarMessages.Enqueue(new ColoredMessage { Text = DateTime.Now.ToString("HH:mm:ss") + " " + text, Color = textColor });
				ProgressBarMessagesUpdated?.Invoke(this, new EventArgs());
			}
		}

		private IEnumerable<NomenclatureCatalogNode> ReadNodesFromFile()
		{
			CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
			configuration.Delimiter = ";";
			configuration.TrimOptions = TrimOptions.Trim;
			configuration.RegisterClassMap<NomenclatureCatalogeNodeMap>();

			using(var reader = new StreamReader($"{FilePath}", encoding))
			using(var csv = new CsvReader(reader, configuration)) {

				IEnumerable<NomenclatureCatalogNode> records = null;
				try {
					records = csv.GetRecords<NomenclatureCatalogNode>().ToList();
				} catch(TypeConverterException conversionEx) {
					SetProgressBar($"Неверно указаны данные | Столбец: \"{conversionEx.MemberMapData.Names.First()}\"  Строка: {conversionEx.ReadingContext.RawRow}", ConsoleColor.DarkRed);
					return null;
				}
				catch(HeaderValidationException headerEx) {
					if(!headerEx.HeaderNames.Any() 
						|| headerEx.ReadingContext.NamedIndexes == null
						|| !headerEx.ReadingContext.NamedIndexes.Any()
						|| headerEx.ReadingContext.NamedIndexCache == null 
						|| !headerEx.ReadingContext.NamedIndexCache.Any()) {
						SetProgressBar($"Неизвестная ошибка в заголовке файла", ConsoleColor.DarkRed);
						return null;
					}
					string header;
					if((headerEx.ReadingContext.NamedIndexes.Count - headerEx.ReadingContext.NamedIndexCache.Count) == 0 || String.IsNullOrWhiteSpace(headerEx.ReadingContext.NamedIndexes.Keys.Last()))
						header = "(Заголовок отсутствует)";
					else
						header = headerEx.ReadingContext.HeaderRecord.Last();
					SetProgressBar($"Файл имеет неправильный заголовок: '{header}'.  Ожидался заголовок: '{headerEx.HeaderNames.Last()}'", ConsoleColor.DarkRed);
					return null;
				}
				IList<NomenclatureCatalogNode> resultRecords = new List<NomenclatureCatalogNode>();
				foreach(var record in records) {
					if(String.IsNullOrWhiteSpace(record.Name))
						continue;

					record.Source = Source.File;
					resultRecords.Add(record);
				}
				return resultRecords;
			}
		}

		private IEnumerable<NomenclatureCatalogNode> ReadNodesFromBase()
		{
			NomenclatureCatalogNode resultAlias = null;
			NomenclaturePrice nomenclaturePriceAlias = null;
			MeasurementUnits measurementUnitsAlias = null;
			Folder1c folder1CAlias = null;
			FuelType fuelTypeAlias = null;
			EquipmentKind equipmentKindAlias = null;

			var result = UoW.Session.QueryOver<Nomenclature>()
				   .Left.JoinAlias(x => x.Unit, () => measurementUnitsAlias)
				   .Left.JoinAlias(x => x.Folder1C, () => folder1CAlias)
				   .Left.JoinAlias(x => x.NomenclaturePrice, () => nomenclaturePriceAlias)
				   .Left.JoinAlias(x => x.FuelType, () => fuelTypeAlias)
				   .Left.JoinAlias(x => x.Kind, () => equipmentKindAlias)
				   .SelectList(list => list
						.SelectGroup(x => x.Id).WithAlias(() => resultAlias.Id)
						.Select(x => x.OfficialName).WithAlias(() => resultAlias.Name)
						.Select(x => x.ShipperCounterparty.Id).WithAlias(() => resultAlias.ShipperCounterpartyId)
						.Select(x => x.ProductGroup.Id).WithAlias(() => resultAlias.GroupId)
						.Select(() => equipmentKindAlias.Name).WithAlias(() => resultAlias.EquipmentKindName)
						.Select(() => folder1CAlias.Name).WithAlias(() => resultAlias.Folder1cName)
						.Select(() => measurementUnitsAlias.Name).WithAlias(() => resultAlias.MeasurementUnit)
						.Select(() => fuelTypeAlias.Name).WithAlias(() => resultAlias.FuelTypeName)
						.Select(Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.Decimal, "MAX(?1)"),
								NHibernateUtil.Decimal,
								Projections.Property(() => nomenclaturePriceAlias.Price)
								).WithAlias(() => resultAlias.SellPrice))
						.Select(Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT(?1)"),
								NHibernateUtil.String,
								Projections.Property<Nomenclature>(x => x.Category)
								).WithAlias(() => resultAlias.NomenclatureCategory))
						.Select(Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT(?1)"),
								NHibernateUtil.String,
								Projections.Property<Nomenclature>(x => x.TareVolume)
								).WithAlias(() => resultAlias.TareVolume))
						.Select(Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT(?1)"),
								NHibernateUtil.String,
								Projections.Property<Nomenclature>(x => x.SaleCategory)
								).WithAlias(() => resultAlias.SaleCategory))
						.Select(Projections.SqlFunction(
							new SQLFunctionTemplate(NHibernateUtil.String, "CONCAT(?1)"),
								NHibernateUtil.String,
								Projections.Property<Nomenclature>(x => x.TypeOfDepositCategory)
								).WithAlias(() => resultAlias.TypeOfDepositCategory))
						.Select(() => Source.Base).WithAlias(() => resultAlias.Source))
				   .TransformUsing(Transformers.AliasToBean<NomenclatureCatalogNode>()).List<NomenclatureCatalogNode>();

			foreach(var item in result) {
				item.NomenclatureCategory = ((NomenclatureCategory)Enum.Parse(typeof(NomenclatureCategory), item.NomenclatureCategory)).GetEnumTitle();
				if(!String.IsNullOrWhiteSpace(item.TareVolume))
					item.TareVolume = ((TareVolume)Enum.Parse(typeof(TareVolume), item.TareVolume)).GetEnumTitle();
				if(!String.IsNullOrWhiteSpace(item.SaleCategory))
					item.SaleCategory = ((SaleCategory)Enum.Parse(typeof(SaleCategory), item.SaleCategory)).GetEnumTitle();
				if(!String.IsNullOrWhiteSpace(item.TypeOfDepositCategory))
					item.TypeOfDepositCategory = ((TypeOfDepositCategory)Enum.Parse(typeof(TypeOfDepositCategory), item.TypeOfDepositCategory)).GetEnumTitle();
			}
			return result;
		}

		private NomenclatureCatalogNode GetDuplicateNode(IEnumerable<NomenclatureCatalogNode> collection, NomenclatureCatalogNode node)
		{
			NomenclatureCatalogNode duplicateNode;
			duplicateNode = collection.FirstOrDefault(x => x.Name.Replace(" ", "").ToLower() == node.Name.Replace(" ", "").ToLower());
			return duplicateNode;
		}

		private void ValidateAndPrepareForSave(out bool NeedReload)
		{
			try {
				SetProgressBar(0, 100, "Проверка корректности данных...");
				List<int> counterpartyIds = new List<int>();
				List<int> productGroupIds = new List<int>();

				foreach(var node in itemsToSave) {
					if(node.ShipperCounterpartyId.HasValue)
						counterpartyIds.Add(node.ShipperCounterpartyId.Value);
					if(node.GroupId.HasValue)
						productGroupIds.Add(node.GroupId.Value);
				}
				progressBarValue++;
				IEnumerable<Counterparty> counterparties = UoW.GetById<Counterparty>(counterpartyIds.Distinct());
				IEnumerable<ProductGroup> productGroups = UoW.GetById<ProductGroup>(productGroupIds.Distinct());
				IEnumerable<EquipmentKind> equipmentKinds = UoW.GetAll<EquipmentKind>();
				IEnumerable<Folder1c> folders1C = UoW.GetAll<Folder1c>();
				IEnumerable<MeasurementUnits> measurementUnits = UoW.GetAll<MeasurementUnits>();
				IEnumerable<FuelType> fuelTypes = UoW.GetAll<FuelType>();
				User currentUser = UoW.GetById<User>(commonServices.UserService.CurrentUserId);

				var cnt = itemsToSave.Count();
				SetProgressBar(cnt / 50, itemsToSave.Count());
				foreach(NomenclatureCatalogNode node in itemsToSave) {
					var newNomenclature = new Nomenclature();
					newNomenclature.VAT = VAT.Vat20;
					newNomenclature.Code1c = nomenclatureRepository.GetNextCode1c(UoW);
					newNomenclature.Name = node.Name;
					newNomenclature.OfficialName = node.Name;
					newNomenclature.CreateDate = DateTime.Now;
					newNomenclature.CreatedBy = currentUser;
					//Нужно только для того чтобы если не указана категория валидация не ругалась на неуказанную тару
					newNomenclature.Category = NomenclatureCategory.additional;
					if(!String.IsNullOrWhiteSpace(node.NomenclatureCategory))
						if(!nomenclatureCategories.Keys.Contains(node.NomenclatureCategory))
							node.AddWrongDataErrorMessage($"Не найдена категория с названием \"{node.NomenclatureCategory}\"");
						else
							newNomenclature.Category = nomenclatureCategories[node.NomenclatureCategory];
					else
						node.AddWrongDataErrorMessage("Не указана категория");
					if(!String.IsNullOrWhiteSpace(node.TareVolume))
						if(!tareVolumes.Keys.Contains(node.TareVolume))
							node.AddWrongDataErrorMessage($"Не найден объем тары с названием \"{node.TareVolume}\"");
						else
							newNomenclature.TareVolume = tareVolumes[node.TareVolume];
					if(!String.IsNullOrWhiteSpace(node.SaleCategory))
						if(!saleCategories.Keys.Contains(node.SaleCategory))
							node.AddWrongDataErrorMessage($"Не найдена доступность для продаж с названием \"{node.SaleCategory}\"");
						else
							newNomenclature.SaleCategory = saleCategories[node.SaleCategory];
					if(!String.IsNullOrWhiteSpace(node.TypeOfDepositCategory))
						if(!typeOfDepositCategories.Keys.Contains(node.TypeOfDepositCategory))
							node.AddWrongDataErrorMessage($"Не найдена подкатегория залогов с названием \"{node.TypeOfDepositCategory}\"");
						else
							newNomenclature.TypeOfDepositCategory = typeOfDepositCategories[node.TypeOfDepositCategory];

					if(!String.IsNullOrWhiteSpace(node.Folder1cName)) {
						var folder = folders1C.FirstOrDefault(x => x.Name.Trim() == node.Folder1cName);
						if(folder == null) {
							node.AddWrongDataErrorMessage($"Не найдена папка 1С с названием: \"{node.Folder1cName}\"");
							//Чтобы валидация не ругалась
							newNomenclature.Folder1C = folders1C.FirstOrDefault();
						} else
							newNomenclature.Folder1C = folder;
					}

					if(!String.IsNullOrWhiteSpace(node.FuelTypeName)) {
						var fuelType = fuelTypes.FirstOrDefault(x => x.Name.Trim() == node.FuelTypeName);
						if(fuelType == null) {
							node.AddWrongDataErrorMessage($"Не найден тип топлива с названием: \"{node.FuelTypeName}\"");
						} else
							newNomenclature.FuelType = fuelType;
					}

					if(!String.IsNullOrWhiteSpace(node.EquipmentKindName)) {
						var equipType = equipmentKinds.FirstOrDefault(x => x.Name.Trim() == node.EquipmentKindName);
						if(equipType == null) {
							node.AddWrongDataErrorMessage($"Не найден вид оборудования с названием: \"{node.EquipmentKindName}\"");
						} else
							newNomenclature.Kind = equipType;
					}

					if(!String.IsNullOrWhiteSpace(node.MeasurementUnit)) {
						var unit = measurementUnits.FirstOrDefault(n => n.Name.Trim() == node.MeasurementUnit);
						if(unit == null) {
							node.AddWrongDataErrorMessage($"Не найдена единица измерения с названием: \"{node.MeasurementUnit}\"");
							//Чтобы валидация не ругалась
							newNomenclature.Unit = measurementUnits.FirstOrDefault();
						} else
							newNomenclature.Unit = unit;
					}

					newNomenclature.NomenclaturePrice.Add(new NomenclaturePrice {
						MinCount = 1,
						Nomenclature = newNomenclature,
						Price = node.SellPrice
					});
					if(node.GroupId.HasValue) {
						var group = productGroups.FirstOrDefault(x => x.Id == node.GroupId.Value);
						if(group == null) {
							node.AddWrongDataErrorMessage($"Не найдена группа товаров с ID: {node.GroupId.Value}");
							break;
						}
						newNomenclature.ProductGroup = group;
					}
					if(node.ShipperCounterpartyId.HasValue) {
						var counterparty = counterparties.FirstOrDefault(x => x.Id == node.ShipperCounterpartyId.Value);
						if(counterparty == null) {
							node.AddWrongDataErrorMessage($"Не найден контрагент с ID: {node.ShipperCounterpartyId.Value}");
							break;
						}
						newNomenclature.ShipperCounterparty = counterparty;
					}
					if(node.Id.HasValue)
						newNomenclature.Id = node.Id.Value;
					newNomenclature.UoW = UoW;
					node.Nomenclature = newNomenclature;

					//Validation
					foreach(var errMessage in newNomenclature.Validate(new ValidationContext(newNomenclature)).Select(x => x.ErrorMessage))
						node.AddWrongDataErrorMessage(errMessage);
					ProgressBarValue++;
				}
				var errCount = Items.Where(x => x.Source == Source.File).Count(x => x.Status == NodeStatus.WrongData);
				var confCount = Items.Where(x => x.Source == Source.File).Count(x => x.Status == NodeStatus.Conflict);
				var color = (errCount > 0 || confCount > 0) ? ConsoleColor.DarkRed : ConsoleColor.DarkGreen;
				SetProgressBar($"Количество строк неверных данных: {errCount}.  Конфликтов: {confCount}", color);
				foreach(var item in itemsToSave.Union(Items.Where(x => x.ConflictSolveAction == ConflictSolveAction.Ignore && x.Source == Source.File))) {
					item.SetColors();
				}
			} catch(Exception ex) {
				SetProgressBar($"Неизвестная ошибка при проверке данных " + ex.Message, ConsoleColor.DarkRed);
				Items.Clear();
				NeedReload = true;
				return;
			}
			OnPropertyChanged(nameof(Items));
			NeedReload = false;
		}

		#endregion

		#region Export

		public DelegateCommand<ExportType> ExportCommand;
		private void CreateExportCommand()
		{
			ExportCommand = new DelegateCommand<ExportType>(
			(exportType) => {
				if(folderPath == null) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Необходимо выбрать папку выгрузки");
					return;
				}
				if(!commonServices.InteractiveService.Question($"Подтвердить выгрузку {exportType.GetEnumTitle().ToLower()}?", "Подтвердить?"))
					return;
				TaskIsRunning = true;
				Task.Run(() => Export(exportType)).ContinueWith((x) => { TaskIsRunning = false; });
			},
			(category) => true
		  );
		}

		private void Export(ExportType exportType)
		{
			try {
				SetProgressBar(0, 2, "Выгружаем данные...");
				IEnumerable<NomenclatureCatalogNode> nodes;
				try {
					nodes = ReadNodesFromBase();
				} catch(Exception ex) {
					SetProgressBar(1, 1, "Ошибка в запросе к базе" + ex.Message, ConsoleColor.DarkRed);
					return;
				}
				if(exportType == ExportType.OnlineStoreGoods) {
					var onlineStoreGroups = UoW.Session.QueryOver<ProductGroup>().Where(x => x.OnlineStore != null).Select(x => x.Id).List<int>();
					nodes = nodes.Where(x => x.GroupId.HasValue && onlineStoreGroups.Contains(x.GroupId.Value));
				}
				ProgressBarValue++;
				CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);
				configuration.Delimiter = ";";
				configuration.TrimOptions = TrimOptions.Trim;
				configuration.RegisterClassMap<NomenclatureCatalogeNodeMap>();

				using(var writer = new StreamWriter($"{FolderPath}/Nomeclatures.csv", false, encoding))
				using(var csv = new CsvWriter(writer, configuration)) {
					csv.WriteRecords(nodes);
				}
				SetProgressBar(1, 1, "Выгрузка завершена", ConsoleColor.DarkGreen);
			} catch(Exception ex) {
				SetProgressBar(1, 1, "Неизвестная ошибка при выгрузке " + ex.Message, ConsoleColor.DarkRed);
				return;
			}
		}

		#endregion
		public delegate void OutAction<T1>(out T1 a);

		public DelegateCommand<LoadAction> LoadActionCommand;
		private void CreateLoadActionCommand()
		{
			LoadActionCommand = new DelegateCommand<LoadAction>(
			(action) => {
				if(filePath == null) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, "Необходимо выбрать файл загрузки");
					return;
				}
				OutAction<bool> loadActionDelegate;
				switch(action) {
					case LoadAction.LoadNew:
						loadActionDelegate = LoadNew;
						break;
					case LoadAction.UpdateData:
						loadActionDelegate = UpdateData;
						break;
					default:
						commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Действие для кнопки \"{action.GetEnumTitle()}\" не назначено");
						return;
				}
				CurrentState = action;
				TaskIsRunning = true;
				Task.Run(() => { loadActionDelegate.Invoke(out bool NeedReload); return NeedReload; })
					.ContinueWith((task) => {
						if(!task.Result) {
							ValidateAndPrepareForSave(out bool NeedReload);
							needReload = NeedReload;
						}
						TaskIsRunning = false;
					});
			},
			(action) => !TaskIsRunning
		  );
		}

		#region LoadNew

		private void LoadNew(out bool NeedReload)
		{
			try {
				SetProgressBar(0, 3, "Загружаем данные из файла...");
				IList<NomenclatureCatalogNode> itemsToAdd = new List<NomenclatureCatalogNode>();
				IEnumerable<NomenclatureCatalogNode> baseNodes;
				try {
					baseNodes = ReadNodesFromBase();
				} catch(Exception ex) {
					SetProgressBar(1, 1, "Ошибка в запросе к базе" + ex.Message, ConsoleColor.DarkRed);
					NeedReload = true;
					return;
				}
				ProgressBarValue++;
				var fileNodes = ReadNodesFromFile();
				ProgressBarValue++;
				if(fileNodes == null) {
					SetProgressBar(1, 1);
					NeedReload = true;
					return;
				}
				if(!fileNodes.Any()) {
					SetProgressBar(1, 1, "В файле нет данных.", ConsoleColor.Yellow);
					NeedReload = true;
					return;
				}

				IList<NomenclatureCatalogNode> goodNodes = new List<NomenclatureCatalogNode>();
				int i = 1;
				foreach(var newNode in fileNodes.Where(n => n.Id == null)) {
					i++;
					var duplicateNode = GetDuplicateNode(baseNodes, newNode);
					if(duplicateNode != null) {

						duplicateNode.DuplicateOf = newNode;
						duplicateNode.Status = NodeStatus.Conflict;

						newNode.DuplicateOf = duplicateNode;
						newNode.Status = NodeStatus.Conflict;
						newNode.ErrorMessages.Add(new ColoredMessage { Text = "Номенклатура с похожим наименованием уже существует в базе", Color = ConsoleColor.Yellow });

						if(!itemsToAdd.Contains(duplicateNode))
							itemsToAdd.Add(duplicateNode);
						itemsToAdd.Add(newNode);
					} else {
						newNode.Status = NodeStatus.NewData;
						goodNodes.Add(newNode);
					}
				}
				foreach(var goodNode in goodNodes) {
					itemsToAdd.Add(goodNode);
				}
				Items = new List<NomenclatureCatalogNode>(itemsToAdd);
				ProgressBarValue++;
				if(Items.Count == 0) {
					SetProgressBar("В файле нет данных, удовлетворяющих условиям", ConsoleColor.Yellow);
					NeedReload = true;
					return;
				}
				SetProgressBar("Данные загружены.", ConsoleColor.DarkGreen);
				NeedReload = false;
			} catch(Exception ex) {
				SetProgressBar(1, 1, "Неизвестная ошибка при загрузке файлов " + ex.Message, ConsoleColor.DarkRed);
				NeedReload = true;
				return;
			}
		}

		public DelegateCommand ConfirmLoadNewCommand;
		private void CreateConfirmLoadNewCommand()
		{
			ConfirmLoadNewCommand = new DelegateCommand(
			() => {
				if(needReload) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Для продолжения работы необходимо заново загрузить данные");
					return;
				}
				if(TaskIsRunning) {
					return;
				}
				if(!itemsToSave.Any()) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Нет данных для загрузки");
					return;
				}
				if(itemsToSave.Any(i => i.Status == NodeStatus.WrongData)) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, $"Для подтверждения необходимо исправить все неверные данные в файле");
					return;
				}
				if(itemsToSave.Any(i => i.Status == NodeStatus.Conflict && i.ConflictSolveAction == ConflictSolveAction.NoAction)) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Для подтверждения необходимо решить все конфликты");
					return;
				}
				if(!commonServices.InteractiveService.Question($"Подтвердить загрузку новых номенклатур?")) {
					return;
				}
				TaskIsRunning = true;
				var task = Task.Run(() => ConfirmLoadNew()).ContinueWith((x) => { TaskIsRunning = false; needReload = true; }); 
			},
			() => {
				if(CurrentState != LoadAction.LoadNew || itemsToSave == null)
					return false;
				return true;
			}
		  );
		}

		private void ConfirmLoadNew()
		{
			try {
				ProgressBarValue++;
				SetProgressBar(0, itemsToSave.Count(), $"Сохранение данных...");
				int batchCounter = 0;
				UoW.Session.SetBatchSize(500);
				foreach(var nomenclature in itemsToSave.Where(x => !(x.Status == NodeStatus.Conflict
						&& x.ConflictSolveAction != ConflictSolveAction.CreateDuplicate)).Select(x => x.Nomenclature)) {
					UoW.Save(nomenclature);
					if(batchCounter == 500) {
						UoW.Commit();
						batchCounter = 0;
					}
					batchCounter++;
					ProgressBarValue++;
				}
				SetProgressBar(1, 120, $"Завершение сохранения...");
				//Fake Counter
				CancellationTokenSource cts = new CancellationTokenSource();
				var task = Task.Run(() => {
					do {
						Thread.Sleep(500);
						ProgressBarValue++;
					} while(!cts.IsCancellationRequested);
				}, cts.Token);
				UoW.Commit();
				cts.Cancel();
				SetProgressBar(1, 1, $"Сохранено.", ConsoleColor.DarkGreen);
			} catch(Exception ex) {
				SetProgressBar(1, 1, $"Неизвестная ошибка при сохранении. " + ex.Message, ConsoleColor.DarkRed);
				return;
			}
		}

		#endregion

		#region UpdateData

		private void UpdateData(out bool NeedReload)
		{
			try {
				SetProgressBar(0, 3, "Загружаем данные из файла...");
				IList<NomenclatureCatalogNode> itemsToAdd = new List<NomenclatureCatalogNode>();
				IList<NomenclatureCatalogNode> errorNodes = new List<NomenclatureCatalogNode>();

				IEnumerable<NomenclatureCatalogNode> baseNodes;
				try {
					baseNodes = ReadNodesFromBase();
				} catch(Exception ex) {
					SetProgressBar(1, 1, "Ошибка в запросе к базе" + ex.Message, ConsoleColor.DarkRed);
					NeedReload = true;
					return;
				}
				ProgressBarValue++;
				var fileNodes = ReadNodesFromFile();
				progressBarValue++;
				if(fileNodes == null) {
					SetProgressBar(1, 1);
					NeedReload = true;
					return;
				}
				if(!fileNodes.Any()) {
					SetProgressBar(1, 1, "В файле нет данных.", ConsoleColor.Yellow);
					NeedReload = true;
					return;
				}

				int i = 1;
				foreach(var newNode in fileNodes.Where(n => n.Id != null)) {
					if(itemsToAdd.Any(x => x.Id == newNode.Id)) {
						newNode.AddWrongDataErrorMessage($"В файле найдено больше 1 номенклатуры с таким ID: {newNode.Id}");
						newNode.ConflictSolveAction = ConflictSolveAction.Ignore;
						errorNodes.Add(newNode);
						continue;
					}
					var baseNode = baseNodes.FirstOrDefault(x => x.Id == newNode.Id && x.DuplicateOf == null);
					if(baseNode != null) {
						i++;
						baseNode.Status = NodeStatus.OldData;
						baseNode.DuplicateOf = newNode;

						newNode.Status = NodeStatus.ChangedData;
						newNode.DuplicateOf = baseNode;

						itemsToAdd.Add(baseNode);
						itemsToAdd.Add(newNode);
					} else {
						newNode.ConflictSolveAction = ConflictSolveAction.Ignore;
						newNode.AddWrongDataErrorMessage($"Номенклатура с ID: {newNode.Id} не найдена в базе");
						errorNodes.Add(newNode);
					}
				}

				foreach(var errorNode in errorNodes) {
					itemsToAdd.Add(errorNode);
				}
				ProgressBarValue++;
				Items = new List<NomenclatureCatalogNode>(itemsToAdd);
				if(Items.Count == 0) {
					SetProgressBar("В файле нет данных, удовлетворяющих условиям", ConsoleColor.Yellow);
					NeedReload = true;
					return;
				}
				SetProgressBar("Данные загружены.", ConsoleColor.DarkGreen);
				NeedReload = false;
			} catch(Exception ex) {
				SetProgressBar(1, 1, "Неизвестная ошибка при замене данных. " + ex.Message, ConsoleColor.DarkRed);
				NeedReload = true;
				return;
			}
		}

		public DelegateCommand<ConfirmUpdateAction> ConfirmUpdateDataCommand;
		private void CreateConfirmUpdateDataCommand()
		{
			ConfirmUpdateDataCommand = new DelegateCommand<ConfirmUpdateAction>(
			(action) => {
				if(needReload) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Для продолжения работы необходимо заново загрузить данные");
					return;
				}
				if(TaskIsRunning)
					return;
				if(!itemsToSave.Any()) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, $"Нет данных для обновления");
					return;
				}
				if(itemsToSave.Any(i => i.Status == NodeStatus.WrongData)) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Error, $"Для подтверждения необходимо исправить все неверные данные в файле");
					return;
				}
				if(itemsToSave.Any(i => i.Status == NodeStatus.Conflict && i.ConflictSolveAction == ConflictSolveAction.NoAction)) {
					commonServices.InteractiveService.ShowMessage(ImportanceLevel.Warning, $"Для подтверждения необходимо решить все конфликты");
					return;
				}
				if(!commonServices.InteractiveService.Question($"Вы уверены что вы хотите произвести замену {action.GetEnumTitle().ToLower()}?", "Подтвердить?")) {
					return;
				}
				TaskIsRunning = true;
				Task.Run(() => ConfirmUpdateData(action))
					.ContinueWith((x) => { needReload = true; TaskIsRunning = false; });
			},
			(action) => {
				if(CurrentState != LoadAction.UpdateData || itemsToSave == null)
					return false;

				return true;
			}
		  );
		}

		private void ConfirmUpdateData(ConfirmUpdateAction action)
		{
			try {
				SetProgressBar(0, 0, $"Замена {action.GetEnumTitle().ToLower()}...");
				if(itemsToSave == null || !itemsToSave.Any()) {
					SetProgressBar(1, 1, $"Нет данных для сохранения", ConsoleColor.DarkRed);
					return;
				}
				var nomenclatures = UoW.GetById<Nomenclature>(itemsToSave.Select(x => x.Nomenclature.Id));
				ProgressBarUpper = itemsToSave.Count();

				int batchCounter = 0;
				UoW.Session.SetBatchSize(500);
				foreach(var newNom in itemsToSave.Where(x => !(x.Status == NodeStatus.Conflict 
						&& x.ConflictSolveAction != ConflictSolveAction.CreateDuplicate)).Select(x => x.Nomenclature)) {
					var nomToUpdate = nomenclatures.First(x => x.Id == newNom.Id);
					switch(action) {
						case ConfirmUpdateAction.UpdateAllData:
							nomToUpdate.ShortName = null;
							nomToUpdate.Name = newNom.Name;
							nomToUpdate.OfficialName = newNom.Name;
							if(!DontChangeSellPrice) {
								nomToUpdate.NomenclaturePrice.Clear();
								nomToUpdate.NomenclaturePrice.Add(newNom.NomenclaturePrice.First());
							}
							nomToUpdate.ShipperCounterparty = newNom.ShipperCounterparty;
							nomToUpdate.ProductGroup = newNom.ProductGroup;
							nomToUpdate.Folder1C = newNom.Folder1C;
							nomToUpdate.Unit = newNom.Unit;
							nomToUpdate.Category = newNom.Category;
							nomToUpdate.TareVolume = newNom.TareVolume;
							nomToUpdate.Kind = newNom.Kind;
							nomToUpdate.SaleCategory = newNom.SaleCategory;
							nomToUpdate.TypeOfDepositCategory = newNom.TypeOfDepositCategory;
							nomToUpdate.FuelType = newNom.FuelType;
							break;
						case ConfirmUpdateAction.UpdateNames:
							nomToUpdate.ShortName = null;
							nomToUpdate.Name = newNom.Name;
							nomToUpdate.OfficialName = newNom.Name;
							break;
						case ConfirmUpdateAction.UpdatePrices:
							if(!DontChangeSellPrice) {
								nomToUpdate.NomenclaturePrice.Clear();
								nomToUpdate.NomenclaturePrice.Add(newNom.NomenclaturePrice.First());
							}
							break;
						case ConfirmUpdateAction.UpdateShipperCounterparties:
							nomToUpdate.ShipperCounterparty = newNom.ShipperCounterparty;
							break;
						case ConfirmUpdateAction.UpdateGroups:
							nomToUpdate.ProductGroup = newNom.ProductGroup;
							break;
						case ConfirmUpdateAction.UpdateFolders1C:
							nomToUpdate.Folder1C = newNom.Folder1C;
							break;
						case ConfirmUpdateAction.UpdateMeasurementUnits:
							nomToUpdate.Unit = newNom.Unit;
							break;
						case ConfirmUpdateAction.UpdateCategories:
							nomToUpdate.Category = newNom.Category;
							break;
						case ConfirmUpdateAction.UpdateTareVolumes:
							nomToUpdate.TareVolume = newNom.TareVolume;
							break;
						case ConfirmUpdateAction.UpdateEquipmentKinds:
							nomToUpdate.Kind = newNom.Kind;
							break;
						case ConfirmUpdateAction.UpdateSaleCategory:
							nomToUpdate.SaleCategory = newNom.SaleCategory;
							break;
						case ConfirmUpdateAction.UpdateTypeOfDepositCategory:
							nomToUpdate.TypeOfDepositCategory = newNom.TypeOfDepositCategory;
							break;
						case ConfirmUpdateAction.UpdateFuelType:
							nomToUpdate.FuelType = newNom.FuelType;
							break;
						default:
							SetProgressBar(1, 1, $"Действие для кнопки \"{action.GetEnumTitle()}\" не назначено", ConsoleColor.DarkRed);
							return;
					}
					UoW.Save(nomToUpdate);
					if(batchCounter == 500) {
						UoW.Commit();
						batchCounter = 0;
					}
					ProgressBarValue++;
				}
				UoW.Commit();
				SetProgressBar(1, 1, $"Замена {action.GetEnumTitle().ToLower()} завершена.", ConsoleColor.DarkGreen);
			} catch(Exception ex) {
				SetProgressBar(1, 1, $"Неизвестная ошибка при сохранении. " + ex.Message, ConsoleColor.DarkRed);
				return;
			}
		}
		#endregion
	}

	public class ColoredMessage : PropertyChangedBase
	{
		private ConsoleColor color;
		public ConsoleColor Color {
			get => color;
			set { SetField(ref color, value, () => Color); }
		}

		private string text;
		public string Text {
			get => text;
			set { SetField(ref text, value, () => Text); }
		}
	}

	public class NomenclatureCatalogNode
	{
		public NomenclatureCatalogNode()
		{
			ErrorMessages = new List<ColoredMessage>();
			ConflictSolveAction = ConflictSolveAction.NoAction;
			BackgroundColor = ConsoleColor.White;
		}

		public int? Id { get; set; }
		public int? GroupId { get; set; }
		public int? ShipperCounterpartyId { get; set; }
		public string EquipmentKindName { get; set; }
		public string FuelTypeName { get; set; }
		public string Folder1cName { get; set; }
		public string MeasurementUnit { get; set; }
		public string Name { get; set; }
		public string NomenclatureCategory { get; set; }
		public string TareVolume { get; set; }
		public string SaleCategory { get; set; }
		public string TypeOfDepositCategory { get; set; }
		public decimal SellPrice { get; set; }

		public Nomenclature Nomenclature { get; set; }
		public NomenclatureCatalogNode DuplicateOf { get; set; }
		public ConflictSolveAction ConflictSolveAction { get; set; }
		public Source Source { get; set; }
		public ConsoleColor BackgroundColor { get; set; }
		public ConsoleColor ForegroundColor { get; set; }
		public IList<ColoredMessage> ErrorMessages { get; set; }

		private NodeStatus status;
		public NodeStatus Status {
			get {
				if(status == NodeStatus.Conflict)
					return NodeStatus.Conflict;
				if(ErrorMessages.Any())
					return NodeStatus.WrongData;
				return status;
			}
			set { status = value; }
		}

		public void SetColors()
		{
			if(Status == NodeStatus.Conflict || Status == NodeStatus.WrongData)
				BackgroundColor = ConsoleColor.Red;
			else if(Status == NodeStatus.NewData || Status == NodeStatus.ChangedData)
				BackgroundColor = ConsoleColor.Green;
			else
				BackgroundColor = ConsoleColor.White;
		}

		public void AddWrongDataErrorMessage(string message, ConsoleColor mesColor = ConsoleColor.DarkRed)
		{
			Status = NodeStatus.WrongData;
			ErrorMessages.Add(new ColoredMessage { Text = message, Color = mesColor });
		}
	}

	public sealed class NomenclatureCatalogeNodeMap : ClassMap<NomenclatureCatalogNode>
	{
		public NomenclatureCatalogeNodeMap()
		{
			Map(x => x.Id).Index(0).Name("ID номенклатуры");
			Map(x => x.Name).Index(1).Name("Наименование");
			Map(x => x.GroupId).Index(2).Name("ID группы товаров");
			Map(x => x.ShipperCounterpartyId).Index(3).Name("ID поставщика");
			Map(x => x.SellPrice).Index(4).Name("Цена продажи").Default(0).TypeConverter<DecimalConverter>();
			Map(x => x.MeasurementUnit).Index(5).Name("Ед. измерения");
			Map(x => x.Folder1cName).Index(6).Name("Имя папки 1С");
			Map(x => x.NomenclatureCategory).Index(7).Name("Категория");
			Map(x => x.TareVolume).Index(8).Name("Объем тары");
			Map(x => x.EquipmentKindName).Index(9).Name("Вид оборудования");
			Map(x => x.SaleCategory).Index(10).Name("Доступность для продажи");
			Map(x => x.TypeOfDepositCategory).Index(11).Name("Тип залога");
			Map(x => x.FuelTypeName).Index(12).Name("Тип топлива");
		}
	}

	public class DecimalConverter : DefaultTypeConverter
	{
		public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
		{
			text = text.Replace(",", ".");
			var culture = CultureInfo.CreateSpecificCulture("ru-RU");
			culture.NumberFormat.NumberDecimalSeparator = ".";
			if(!decimal.TryParse(text, NumberStyles.AllowDecimalPoint, culture, out decimal res))
				throw new TypeConverterException(this, memberMapData, text, row.Context);
			return res;
		}

		public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
		{
			if(value is string) {
				value = (value as string).Replace(",", ".");
			}
			return base.ConvertToString(value, row, memberMapData);
		}
	}

	public enum Source
	{
		[Display(Name = "Файл")]
		File,
		[Display(Name = "База")]
		Base
	}

	public enum NodeStatus
	{
		[Display(Name = "Не определён")]
		None,
		[Display(Name = "Конфликт")]
		Conflict,
		[Display(Name = "Новые данные")]
		NewData,
		[Display(Name = "Старые данные")]
		OldData,
		[Display(Name = "Обновлённые данные")]
		ChangedData,
		[Display(Name = "Неверные данные")]
		WrongData
	}

	public enum ConflictSolveAction
	{
		[Display(Name = "Нет действия")]
		NoAction,
		[Display(Name = "Игнорировать")]
		Ignore,
		[Display(Name = "Создать дубликат")]
		CreateDuplicate
	}

	public enum LoadAction
	{
		[Display(Name = "Загрузить новые номенклатуры")]
		LoadNew,
		[Display(Name = "Заменить данные у номенклатур")]
		UpdateData
	}

	public enum ExportType
	{
		[Display(Name = "Товаров ИМ")]
		OnlineStoreGoods,
		[Display(Name = "Всех номенклатур")]
		AllNomenclatures
	}

	public enum ConfirmUpdateAction
	{
		[Display(Name = "Всех данных")]
		UpdateAllData,
		[Display(Name = "Наименований")]
		UpdateNames,
		[Display(Name = "Цен")]
		UpdatePrices,
		[Display(Name = "Поставщиков")]
		UpdateShipperCounterparties,
		[Display(Name = "Складов отгрузки")]
		UpdateWarehouses,
		[Display(Name = "Групп товаров")]
		UpdateGroups,
		[Display(Name = "Папок 1С")]
		UpdateFolders1C,
		[Display(Name = "Единиц измерения")]
		UpdateMeasurementUnits,
		[Display(Name = "Категорий")]
		UpdateCategories,
		[Display(Name = "Объема тары")]
		UpdateTareVolumes,
		[Display(Name = "Видов оборудования")]
		UpdateEquipmentKinds,
		[Display(Name = "Доступностей для продажи")]
		UpdateSaleCategory,
		[Display(Name = "Типов залога")]
		UpdateTypeOfDepositCategory,
		[Display(Name = "Типов топлива")]
		UpdateFuelType
	}
}