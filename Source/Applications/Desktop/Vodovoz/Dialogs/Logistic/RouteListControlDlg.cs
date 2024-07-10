using Autofac;
using Gamma.GtkWidgets;
using Gamma.Utilities;
using Microsoft.Extensions.Logging;
using QS.Dialog.GtkUI;
using QS.Project.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Logistic;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Services.Logistics;
using Vodovoz.Settings.Nomenclature;

namespace Vodovoz.Dialogs.Logistic
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class RouteListControlDlg : QS.Dialog.Gtk.EntityDialogBase<RouteList>
	{
		private readonly ILifetimeScope _lifetimeScope;
		private readonly ILogger<RouteListControlDlg> _logger;
		private readonly INomenclatureSettings _nomenclatureSettings;
		private readonly IRouteListService _routeListService;
		private readonly IRouteListRepository _routeListRepository;
		private readonly IRouteListItemRepository _routeListItemRepository;

		public GenericObservableList<RouteListControlNotLoadedNode> ObservableNotLoadedList { get; set; }
			= new GenericObservableList<RouteListControlNotLoadedNode>();


		private RouteListControlDlg()
		{
			_lifetimeScope = Startup.AppDIContainer.BeginLifetimeScope();
			_logger = _lifetimeScope.Resolve<ILogger<RouteListControlDlg>>();
			_nomenclatureSettings = _lifetimeScope.Resolve<INomenclatureSettings>();
			_routeListService = _lifetimeScope.Resolve<IRouteListService>();
			_routeListRepository = _lifetimeScope.Resolve<IRouteListRepository>();
			_routeListItemRepository = _lifetimeScope.Resolve<IRouteListItemRepository>();
		}

		public RouteListControlDlg(RouteList sub) : this(sub.Id) { }

		public RouteListControlDlg(int id) : this()
		{
			Build();
			UoWGeneric = ServicesConfig.UnitOfWorkFactory.CreateForRoot<RouteList>(id);
			ConfigureDlg();
		}

		public override bool Save()
		{
			var contextItems = new Dictionary<object, object>
			{
				{nameof(IRouteListItemRepository), _routeListItemRepository}
			};

			var context = new ValidationContext(Entity, null, contextItems);
			var validator = ServicesConfig.ValidationService;

			if(!validator.Validate(Entity, context))
			{
				return false;
			}

			_logger.LogInformation("Сохраняем маршрутный лист...");
			UoWGeneric.Save();
			_logger.LogInformation("Ok");

			return true;
		}

		private void ConfigureDlg()
		{
			btnSendEnRoute.Visible = Entity.Status == RouteListStatus.InLoading;
			ytreeviewNotLoaded.ColumnsConfig = ColumnsConfigFactory.Create<RouteListControlNotLoadedNode>()
				.AddColumn("Номенклатура")
					.AddTextRenderer(x => x.Nomenclature.Name)
				.AddColumn("Погружено")
					.AddTextRenderer(x =>
						$"<span foreground=\"{(x.CountLoaded > 0 ? GdkColors.Orange.ToHtmlColor() : GdkColors.DangerText.ToHtmlColor())}\">{x.CountLoaded}</span>", useMarkup: true)
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
			var notLoadedNomenclatures = Entity.NotLoadedNomenclatures(true, _nomenclatureSettings.NomenclatureIdForTerminal);
			ObservableNotLoadedList = new GenericObservableList<RouteListControlNotLoadedNode>(notLoadedNomenclatures);

			ytreeviewNotLoaded.ItemsDataSource = ObservableNotLoadedList;
		}

		protected void OnBtnSendEnRouteClicked(object sender, EventArgs e)
		{
			#region костыль
			//FIXME пока не можем найти причину бага с несменой статуса на в пути при полной отгрузке, позволяем логистам отправлять МЛ в путь из этого диалога
			bool fullyLoaded = false;

			if(_routeListService.TrySendEnRoute(UoW, Entity, out _))
			{
				fullyLoaded = true;
				MessageDialogHelper.RunInfoDialog("Маршрутный лист отгружен полностью.");
			}
			#endregion

			if(!fullyLoaded &&
				ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_send_not_loaded_route_lists_en_route") &&
				MessageDialogHelper.RunQuestionWithTitleDialog(
					"Оптправить в путь?",
					$"{Entity.Title} погружен <span foreground=\"{GdkColors.DangerText.ToHtmlColor()}\">НЕ ПОЛНОСТЬЮ</span> и будет переведён в статус \"{RouteListStatus.EnRoute.GetEnumTitle()}\". После сохранения изменений откат этого действия будет невозможен.\nВы уверены что хотите отправить МЛ в путь?"))
			{
				_routeListService.SendEnRoute(UoW, Entity.Id);
				Entity.NotFullyLoaded = true;
			}
			else if(!fullyLoaded && !ServicesConfig.CommonServices.CurrentPermissionService.ValidatePresetPermission("can_send_not_loaded_route_lists_en_route"))
			{
				MessageDialogHelper.RunWarningDialog(
					"Недостаточно прав",
					$"У вас нет прав для перевода не полностью погруженных МЛ в статус \"{RouteListStatus.EnRoute.GetEnumTitle()}\"",
					Gtk.ButtonsType.Ok
				);
			}
		}
	}
}
