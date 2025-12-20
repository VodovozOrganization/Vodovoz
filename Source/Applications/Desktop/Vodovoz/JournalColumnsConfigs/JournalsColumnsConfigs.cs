using Gamma.Binding;
using Gamma.ColumnConfig;
using Gdk;
using Gtk;
using QS.Journal.GtkUI;
using System;
using System.Linq;
using Gamma.Utilities;
using Vodovoz.Domain.Cash.FinancialCategoriesGroups;
using Vodovoz.Extensions;
using Vodovoz.Infrastructure;
using Vodovoz.Journals.JournalNodes;
using Vodovoz.Journals.JournalViewModels.Organizations;
using Vodovoz.ViewModels.Cash.FinancialCategoriesGroups;
using Vodovoz.ViewModels.Goods.ProductGroups;
using Vodovoz.ViewModels.Journals.JournalNodes.Roboats;
using Vodovoz.ViewModels.Journals.JournalNodes.WageCalculation;
using Vodovoz.ViewModels.Journals.JournalViewModels.Roboats;
using Vodovoz.ViewModels.Journals.JournalViewModels.WageCalculation;
using Vodovoz.Core.Domain.Goods;

namespace Vodovoz.JournalColumnsConfigs
{
	public static class JournalsColumnsConfigs
	{
		private static Pixbuf _folderImg = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.folder16.png");
		private static Pixbuf _emptyImg = new Pixbuf(typeof(Startup).Assembly, "Vodovoz.icons.common.empty16.png");

		public static void RegisterColumns()
		{
			var registratorGeneric = typeof(IColumnsConfigRegistrar<,>);
			var types = typeof(JournalsColumnsConfigs).Assembly.GetTypes()
				.Where(p => p.IsClass
					&& !p.IsAbstract
					&& p.GetInterfaces().Any(x =>
						x.IsGenericType &&
						x.GetGenericTypeDefinition() == registratorGeneric));

			foreach(var type in types)
			{
				Activator.CreateInstance(type);
			}

			TreeViewColumnsConfigFactory.Register<FinancialCategoriesGroupsJournalViewModel>(
				(vm) => FluentColumnsConfig<FinancialCategoriesJournalNode>.Create()
					.SetTreeModel(() => new RecursiveTreeModel<FinancialCategoriesJournalNode>(vm.Items.Cast<FinancialCategoriesJournalNode>(), vm.RecuresiveConfig))
					.AddColumn("Код")
						.AddNumericRenderer(node => node.Id)
						.AddPixbufRenderer(node => node.JournalNodeType == typeof(FinancialCategoriesGroup) ? _folderImg : _emptyImg)
					.AddColumn("Нумерация").AddTextRenderer(node => node.Numbering)
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.Finish()
				);

			TreeViewColumnsConfigFactory.Register<ProductGroupsJournalViewModel>(
				(vm) => FluentColumnsConfig<ProductGroupsJournalNode>.Create()
					.SetTreeModel(() => new RecursiveTreeModel<ProductGroupsJournalNode>(vm.Items.Cast<ProductGroupsJournalNode>(), vm.RecuresiveConfig))
					.AddColumn("Код")
						.AddNumericRenderer(node => node.Id)
						.AddPixbufRenderer(node => node.JournalNodeType == typeof(ProductGroup) ? _folderImg : _emptyImg)
					.AddColumn("Название").AddTextRenderer(node => node.Name)
					.Finish()
				);

			TreeViewColumnsConfigFactory.Register<SubdivisionsJournalViewModel>(
				(vm) => FluentColumnsConfig<SubdivisionJournalNode>.Create()
					.SetTreeModel(() => new RecursiveTreeModel<SubdivisionJournalNode>(vm.Items.Cast<SubdivisionJournalNode>(), vm.RecuresiveConfig))
					.AddColumn("Название").AddTextRenderer(node => node.Name).AddSetter((cell, node) =>
					{
						var color = GdkColors.PrimaryText;
						if(node.IsArchive)
						{
							color = GdkColors.InsensitiveText;
						}

						cell.ForegroundGdk = color;
					})
					.AddColumn("Руководитель").AddTextRenderer(node => node.ChiefName).AddSetter((cell, node) =>
					{
						var color = GdkColors.PrimaryText;
						if(node.IsArchive)
						{
							color = GdkColors.InsensitiveText;
						}

						cell.ForegroundGdk = color;
					})
					.AddColumn("Код").AddNumericRenderer(node => node.Id).AddSetter((cell, node) =>
					{
						var color = GdkColors.PrimaryText;
						if(node.IsArchive)
						{
							color = GdkColors.InsensitiveText;
						}

						cell.ForegroundGdk = color;
					})
					.Finish());

			TreeViewColumnsConfigFactory.Register<RoboatsCallsRegistryJournalViewModel>(
				(vm) => FluentColumnsConfig<RoboatsCallJournalNode>.Create()
					.SetTreeModel(() => new RecursiveTreeModel<RoboatsCallJournalNode>(vm.Items.Cast<RoboatsCallJournalNode>(), vm.RecuresiveConfig))
					.AddColumn("Код").AddNumericRenderer(node => node.EntityId)
					.AddColumn("Время").AddTextRenderer(node => node.Time.ToString("dd.MM.yyyy HH:mm:ss"))
					.AddColumn("Телефон").AddTextRenderer(node => node.Phone)
					.AddColumn("Статус").AddTextRenderer(node => node.CallStatus)
					.AddColumn("Результат").AddTextRenderer(node => node.CallResult)
					.AddColumn("Детали звонка").AddTextRenderer(node => node.Description)
					.RowCells()
					.AddSetter<CellRenderer>(
						(cell, node) => {
							var color = GdkColors.PrimaryBase;
							if(node.NodeType == RoboatsCallNodeType.RoboatsCallDetail)
							{
								color = GdkColors.InsensitiveBase;
							}
							cell.CellBackgroundGdk = color;
						}
					)
					.Finish()
			);

			TreeViewColumnsConfigFactory.Register<CashReceiptsJournalViewModel>(
				(vm) => FluentColumnsConfig<CashReceiptJournalNode>.Create()
					.SetTreeModel(() => new RecursiveTreeModel<CashReceiptJournalNode>(vm.Items.Cast<CashReceiptJournalNode>(), vm.RecuresiveConfig))
					.AddColumn(" ┯ Код чека\n └ Код маркир. ").AddNumericRenderer(node => node.ReceiptOrProductCodeId).Editing(false)
					.AddColumn(" ━ Id чека \n").AddTextRenderer(node => node.ReceiptDocId)
					.AddColumn(" ━ Создан \n").AddTextRenderer(node => node.CreatedTime)
					.AddColumn(" ━ Изменен \n").AddTextRenderer(node => node.ChangedTime)
					.AddColumn(" ━ Статус \n").AddTextRenderer(node => node.Status)
					.AddColumn(" ━ Сумма \n").AddNumericRenderer(node => node.ReceiptSum).Digits(2).Editing(false)
					.AddColumn(" ━ Код МЛ \n").AddTextRenderer(node => node.RouteList)
					.AddColumn(" ━ Водитель \n").AddTextRenderer(node => node.DriverFIO)
					.AddColumn(" ┯ Код заказа\n └ Код стр. заказа ").AddNumericRenderer(node => node.OrderAndItemId).Digits(0).Editing(false)
					.AddColumn(" ┯ Статус фиск. док.\n └ Исх. GTIN ").AddTextRenderer(node => node.FiscalDocStatusOrSourceGtin)
					.AddColumn(" ┯ Номер фиск. док.\n └ Исх. Сер. номер ").AddTextRenderer(node => node.FiscalDocNumberOrSourceCodeInfo)
					.AddColumn(" ┯ Дата фиск. док.\n └ Итог. GTIN ").AddTextRenderer(node => node.FiscalDocDateOrResultGtin)
					.AddColumn(" ┯ Дата статуса фиск. док.\n └ Итог. Сер. номер ").AddTextRenderer(node => node.FiscalDocStatusDateOrResultSerialnumber)
					.AddColumn(" ┯ Ручная отправка\n └ Брак ").AddToggleRenderer(node => node.IsManualSentOrIsDefectiveCode).Editing(false)
					.AddColumn(" ━ Отправлен на \n").AddTextRenderer(node => node.Contact)
					.AddColumn(" ━ Причина не отскан. бутылей \n").AddTextRenderer(node => node.UnscannedReason).WrapMode(Pango.WrapMode.Word).WrapWidth(300)
					.AddColumn(" ━ Описание ошибки \n ").AddTextRenderer(node => node.ErrorDescription).WrapMode(Pango.WrapMode.Word).WrapWidth(300)
					.AddColumn("")
					.RowCells()
					.AddSetter<CellRenderer>(
						(cell, node) => {
							var color = GdkColors.PrimaryBase;
							if(node.NodeType == CashReceiptNodeType.Code)
							{
								color = GdkColors.InsensitiveBase;
							}
							cell.CellBackgroundGdk = color;
						}
					)
					.Finish()
			);
			
			 TreeViewColumnsConfigFactory.Register<CallCenterMotivationCoefficientJournalViewModel>(ViewModel =>
				FluentColumnsConfig<CallCenterMotivationCoefficientJournalNode>.Create()
					.SetTreeModel(ViewModel.CreateAndSaveTreeModel)
					.AddColumn("Код")
						.AddNumericRenderer(node => node.Id)
						.AddPixbufRenderer(node => node.JournalNodeType == typeof(ProductGroup) ? _folderImg : _emptyImg)
					.AddColumn("Название")
						.AddTextRenderer(node => node.Name)
					.AddColumn("Тип коэффициента")
						.AddComboRenderer(x => x.MotivationUnitType)
						.SetDisplayFunc(x => x.GetEnumTitle())
						.FillItems(ViewModel.MotivationUnitTypeList, "✗ Очистить")
						.XAlign(0.5f)
						.Editing()
						.EditedEvent((o, args) =>
						{
							var node = ViewModel.TreeModel.NodeAtPath(new TreePath(args.Path)) as CallCenterMotivationCoefficientJournalNode;
							if(args.NewText != node.MotivationUnitType?.GetEnumTitle())
							{
								Gtk.Application.Invoke((s, e) => ViewModel.OnMotivationUnitTypeEdited(node));
							}
						})
					.AddColumn("Значение коэффициента")
						.AddTextRenderer(node => node.MotivationCoefficientText)
						.Editable()
						.AddSetter((cell, node) =>
						{
							cell.Editable = node.MotivationUnitType.HasValue;
							cell.Markup = ViewModel.IsValidNode(node)? node.MotivationCoefficientText : $"<span background=\"red\"><s>{node.MotivationCoefficientText}</s></span>";
						})
						.EditedEvent((o, args) =>
						{
							var node = ViewModel.TreeModel.NodeAtPath(new TreePath(args.Path)) as  CallCenterMotivationCoefficientJournalNode;
							ViewModel.OnMotivationCoefficientEdited(node);
						})
						.EditingStartedEvent((editable, args) =>
						{
							if(args.Editable is Entry entry)
							{
								entry.SetNumericValidation(999999.99m);
							}
						})
					.AddColumn("")
					.Finish()
			);
		}
	}
}
