using System;
using System.Collections.Generic;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using NLog;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Journal.EntitySelector;
using QS.Project.Services;
using QS.Validation;
using Vodovoz.Core.DataService;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Goods;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories;
using Vodovoz.EntityRepositories.CallTasks;
using Vodovoz.EntityRepositories.Employees;
using Vodovoz.EntityRepositories.Goods;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.EntityRepositories.Orders;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Filters.ViewModels;
using Vodovoz.FilterViewModels.Goods;
using Vodovoz.Infrastructure.Services;
using Vodovoz.JournalSelector;
using Vodovoz.JournalViewModels;
using Vodovoz.Parameters;
using Vodovoz.Tools;
using Vodovoz.Tools.CallTasks;

namespace Vodovoz.Dialogs.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListControlDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();

		private readonly IEmployeeService employeeService = VodovozGtkServicesConfig.EmployeeService;
		private readonly IUserRepository userRepository = UserSingletonRepository.GetInstance();
		private readonly IRouteListRepository routeListRepository = new RouteListRepository();
		private readonly ISubdivisionRepository subdivisionRepository = new SubdivisionRepository();
		
		private  INomenclatureRepository nomenclatureRepository;
		public virtual INomenclatureRepository NomenclatureRepository {
			get {
				if (nomenclatureRepository == null) {
					nomenclatureRepository = new NomenclatureRepository(new NomenclatureParametersProvider());
				}
				return nomenclatureRepository;
			}
		}
		
		private IEntityAutocompleteSelectorFactory counterpartySelectorFactory;
		public virtual IEntityAutocompleteSelectorFactory CounterpartySelectorFactory {
			get {
				if(counterpartySelectorFactory == null) {
					counterpartySelectorFactory =
						new DefaultEntityAutocompleteSelectorFactory<Counterparty, CounterpartyJournalViewModel,
							CounterpartyJournalFilterViewModel>(ServicesConfig.CommonServices);
				};
				return counterpartySelectorFactory;
			}
		}
		
		private IEntityAutocompleteSelectorFactory nomenclatureSelectorFactory;
		public virtual IEntityAutocompleteSelectorFactory NomenclatureSelectorFactory {
			get {
				if(nomenclatureSelectorFactory == null) {
					nomenclatureSelectorFactory =
						new NomenclatureAutoCompleteSelectorFactory<Nomenclature, NomenclaturesJournalViewModel>(
							ServicesConfig.CommonServices, new NomenclatureFilterViewModel(), CounterpartySelectorFactory,
							NomenclatureRepository, userRepository);
				}
				return nomenclatureSelectorFactory;
			}
		}
		
		public GenericObservableList<RouteListControlNotLoadedNode> ObservableNotLoadedList { get; set; }
			= new GenericObservableList<RouteListControlNotLoadedNode>();
			
		private CallTaskWorker callTaskWorker;
		public virtual CallTaskWorker CallTaskWorker {
			get {
				if(callTaskWorker == null) {
					callTaskWorker = new CallTaskWorker(
						CallTaskSingletonFactory.GetInstance(),
						new CallTaskRepository(),
						OrderSingletonRepository.GetInstance(),
						EmployeeSingletonRepository.GetInstance(),
						new BaseParametersProvider(),
						ServicesConfig.CommonServices.UserService,
						SingletonErrorReporter.Instance);
				}
				return callTaskWorker;
			}
			set { callTaskWorker = value; }
		}

		public RouteListControlDlg(RouteList sub) : this(sub.Id) { }

		public RouteListControlDlg(int id)
		{
			this.Build();
			UoWGeneric = UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			ConfigureDlg();
		}

		public override bool Save()
		{
			var valid = new QSValidator<RouteList>(UoWGeneric.Root, new Dictionary<object, object>(){ { nameof(IRouteListItemRepository), new RouteListItemRepository() } });
			if(valid.RunDlgIfNotValid((Gtk.Window)this.Toplevel))
				return false;

			logger.Info("Сохраняем маршрутный лист...");
			UoWGeneric.Save();
			logger.Info("Ok");
			return true;
		}

		private void ConfigureDlg()
		{
			btnSendEnRoute.Visible = Entity.Status == RouteListStatus.InLoading;
			ytreeviewNotLoaded.ColumnsConfig = ColumnsConfigFactory.Create<RouteListControlNotLoadedNode>()
				.AddColumn("Номенклатура")
					.AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Погружено")
					.AddTextRenderer(x => x.CountLoadedString, useMarkup: true)
				.AddColumn("Всего")
					.AddNumericRenderer(x => x.CountTotal)
				.AddColumn("Осталось погрузить")
					.AddNumericRenderer(x => x.CountNotLoaded)
				.RowCells()
				.Finish();

			UpdateLists();
		}

		private void UpdateLists()
		{
			var goods = routeListRepository.GetGoodsAndEquipsInRL(UoW, Entity);
			var notLoadedNomenclatures = Entity.NotLoadedNomenclatures(true);
			
			ObservableNotLoadedList = new GenericObservableList<RouteListControlNotLoadedNode>(notLoadedNomenclatures);

			ytreeviewNotLoaded.ItemsDataSource = ObservableNotLoadedList;
		}

		protected void OnBtnSendEnRouteClicked(object sender, EventArgs e)
		{
			#region костыль
			//FIXME пока не можем найти причину бага с несменой статуса на в пути при полной отгрузке, позволяем логистам отправлять МЛ в путь из этого диалога
			bool fullyLoaded = false;
			if(Entity.ShipIfCan(UoW, CallTaskWorker)) {
				fullyLoaded = true;
				MessageDialogHelper.RunInfoDialog("Маршрутный лист отгружен полностью.");
			}
			#endregion

			if(
				!fullyLoaded &&
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_send_not_loaded_route_lists_en_route") &&
				MessageDialogHelper.RunQuestionWithTitleDialog(
					"Оптправить в путь?",
					string.Format(
						"{0} погружен <span foreground=\"Red\">НЕ ПОЛНОСТЬЮ</span> и будет переведён в статус \"{1}\". После сохранения изменений откат этого действия будет невозможен.\nВы уверены что хотите отправить МЛ в путь?",
						Entity.Title,
						RouteListStatus.EnRoute.GetEnumTitle()
					)
				)
			) {
				Entity.ChangeStatusAndCreateTask(RouteListStatus.EnRoute, CallTaskWorker);
				Entity.NotFullyLoaded = true;
			} else if(!fullyLoaded && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_send_not_loaded_route_lists_en_route")) {
				MessageDialogHelper.RunWarningDialog(
					"Недостаточно прав",
					string.Format("У вас нет прав для перевода не полностью погруженных МЛ в статус \"{0}\"", RouteListStatus.EnRoute.GetEnumTitle()),
					Gtk.ButtonsType.Ok
				);
			}
		}
	}
}