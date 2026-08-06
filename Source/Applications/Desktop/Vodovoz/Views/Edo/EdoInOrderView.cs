using Gamma.ColumnConfig;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.Core.Domain.Edo;
using Vodovoz.Dialogs.OrderWidgets;
using Vodovoz.Infrastructure;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class EdoInOrderView : WidgetViewBase<EdoInOrderViewModel>, IActivatableOrderTab
	{
		public EdoInOrderView()
		{
			this.Build();
		}

		protected override void ConfigureWidget()
		{
			base.ConfigureWidget();

			CreateHelpTab();

			ytreeviewDocTypes.HeightRequest = 140;
			ytreeviewDocTypes.ColumnsConfig = FluentColumnsConfig<EdoInOrderDocumentTypeViewModel>.Create()
				.AddColumn("Тип документа")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Title)
					.XAlign(0.5f)
				.AddColumn("Кол-во")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.Quantity).Editing(false)
					.XAlign(0.5f)
				.Finish();
			ytreeviewDocTypes.Selection.Mode = Gtk.SelectionMode.Single;
			ytreeviewDocTypes.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.DocumentGroupTypes, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedDocumentGroupType, w => w.SelectedRow)
				.InitializeFromSource();

			ytreeviewDocuments.ColumnsConfig = FluentColumnsConfig<EdoInOrderDocumentHistoryRowViewModel>.Create()
				.AddColumn("Время начала")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.TimeString)
					.XAlign(0.5f)
				.AddColumn("Кто запустил")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.SourceString)
					.XAlign(0.5f)
				.AddColumn("Статус задачи")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.StatusString)
					.XAlign(0.5f)
					.AddSetter((c, n) => {
						if(n.Status == EdoTaskStatus.Problem)
						{
							c.CellBackgroundGdk = GdkColors.DangerBase;
						}
						else
						{
							c.CellBackgroundGdk = GdkColors.PrimaryBase;
						}
					})
				.AddColumn("Статус ДО")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.EdoDocumentStatusString)
					.XAlign(0.5f)
					.AddSetter((c, n) => {
						if(n.EdoDocumentStatus == EdoDocumentStatus.Error)
						{
							c.CellBackgroundGdk = GdkColors.DangerBase;
						}
						else
						{
							c.CellBackgroundGdk = GdkColors.PrimaryBase;
						}
					})
				.AddColumn("Документ")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.DocumentTypeString)
					.XAlign(0.5f)
				.AddColumn("Кодов")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CodesQuantityString)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();
			ytreeviewDocuments.Selection.Mode = Gtk.SelectionMode.Single;
			ytreeviewDocuments.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.Documents, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedDocument, w => w.SelectedRow)
				.InitializeFromSource();

			pipelineDocumentStages.PipelineVerticalPadding = 5;
			pipelineDocumentStages.PipelineSidePadding = 10;
			pipelineDocumentStages.HorizontalAlignment = 0f;
			pipelineDocumentStages.VerticalAlignment = 0f;
			pipelineDocumentStages.HeightRequest = 0;
			pipelineDocumentStages.StageCircleRadius = 16;
			pipelineDocumentStages.StageAdditionalInfoHeight = 14;
			pipelineDocumentStages.TitleHeight = 12;
			pipelineDocumentStages.TitleBottomSpacing = 4;
			pipelineDocumentStages.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.PipelineViewModel, w => w.ViewModel)
				.InitializeFromSource();

			ytreeviewProblems.ColumnsConfig = FluentColumnsConfig<EdoInOrderProblemViewModel>.Create()
				.AddColumn("Время")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.CreationTime).Editable(false)
					.XAlign(0.5f)
				.AddColumn("Состояние")
					.HeaderAlignment(0.5f)
					.AddNumericRenderer(x => x.State).Editing(false)
					.XAlign(0.5f)
					.AddSetter((c, n) =>
					{
						if(n.ProblemNode.State == TaskProblemState.Active)
						{
							c.BackgroundGdk = GdkColors.DangerBase;
						}
						else
						{
							c.BackgroundGdk = GdkColors.SuccessBase;
						}
					})
				.AddColumn("Название")
					.HeaderAlignment(0.5f)
					.AddTextRenderer(x => x.Message).Editable(false)
					.XAlign(0.5f)
				.AddColumn("")
				.Finish();

			ytreeviewProblems.Selection.Mode = Gtk.SelectionMode.Single;
			ytreeviewProblems.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.Problems, w => w.ItemsDataSource)
				.AddBinding(vm => vm.SelectedProblem, w => w.SelectedRow)
				.InitializeFromSource();

			textViewProblemDescription.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ProblemDescription, w => w.Buffer.Text)
				.InitializeFromSource();
			textViewProblemRecommendation.Binding.AddSource(ViewModel)
				.AddBinding(vm => vm.ProblemRecommendation, w => w.Buffer.Text)
				.InitializeFromSource();

			ytreeviewProblemItems.ColumnsConfig = FluentColumnsConfig<string>.Create()
				.AddColumn("")
				.HeaderAlignment(0.5f)
				.AddTextRenderer(x => x).Editable(false)
				.XAlign(0.5f)
				.Finish();
			ytreeviewProblemItems.Binding
				.AddSource(ViewModel)
				.AddBinding(vm => vm.ProblemItems, w => w.ItemsDataSource)
				.InitializeFromSource();
			tabLabelProblems.UseMarkup = true;

			edoinorderactionsview.ViewModel = ViewModel.EdoInOrderDocumentActionsViewModel;

			ordercodesview1.ViewModel = ViewModel.OrderCodesViewModel;

			buttonRefresh.BindCommand(ViewModel.RefreshCommnand);

			radiobuttonHelp.Toggled += RadiobuttonHelpToggled;
			radiobuttonDocuments.Toggled += RadiobuttonDocumentsToggled;
			radiobuttonCodes.Toggled += RadiobuttonCodesToggled;
			radiobuttonDocuments.Click();

			ViewModel.PropertyChanged += ViewModelPropertyChanged;
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(EdoInOrderViewModel.HasActiveProblems))
			{
				UpdateProblemsTab();
			}

			if(e.PropertyName == nameof(EdoInOrderViewModel.Problems))
			{
				UpdateProblemsTab();
			}

			if(e.PropertyName == nameof(EdoInOrderViewModel.DocumentViewModel))
			{
				edoinorderdocumentview1.ViewModel = ViewModel.DocumentViewModel;
			}
		}

		private void UpdateProblemsTab()
		{
			var problemsCount = ViewModel.Problems.Count > 0
				? $" ({ViewModel.Problems.Count})"
				: "";

			if(ViewModel.HasActiveProblems)
			{
				tabLabelProblems.Markup = $"<span foreground=\"red\"><b>Проблемы{problemsCount}</b></span>";
			}
			else
			{
				tabLabelProblems.LabelProp = $"Проблемы{problemsCount}";
			}
		}

		private void RadiobuttonHelpToggled(object sender, EventArgs e)
		{
			if(radiobuttonHelp.Active)
			{
				ynotebookEdoInOrder.CurrentPage = 0;
			}
		}

		private void RadiobuttonDocumentsToggled(object sender, EventArgs e)
		{
			if(radiobuttonDocuments.Active)
			{
				ynotebookEdoInOrder.CurrentPage = 1;
			}
		}
		private void RadiobuttonCodesToggled(object sender, EventArgs e)
		{
			if(radiobuttonCodes.Active)
			{
				ynotebookEdoInOrder.CurrentPage = 2;
				ViewModel.LoadCodes();
			}
		}

		void IActivatableOrderTab.Activate()
		{
			ViewModel.Load();
		}

		private void CreateHelpTab()
		{
			textViewHelp.Buffer.Text =
@"В данном диалоге отображается процесс электронного документооборота по заказу.
Здесь можно посмотреть:
	- какие документы были созданы по заказу;
	- на каком этапе находится их отправка;
	- кто запустил создание документа;
	- сколько кодов маркировки связано с документом;
	- возникли ли проблемы при обработке или отправке;
	- выполнялся ли трансфер кодов маркировки между организациями.

1. История отправки документов

В даной таблице отображаются все задачи, которые были созданы для подготовки и отправки документов по заказу.

По заказу могут обрабатываться следующие типы документов:
	- Электронная первичика: это УПД, чеки, тендеры
Почему первичка: потому что эти документы автоматически создаются при завершении заказа и получении кодов от
 	- водителя
 	- склада
 	- самовывоза 
	- Вывод из оборота: это документы, которые снимают с учета в Честном знаке наших компаний маркированные товары

2. Таблица отправленных документов

В таблице отображаются основные сведения о каждой задаче на отправку документов.

Колонка "" Кто запустил"":
  	- Водитель (документ создан автоматически при завершении заказа водителем);
  	- Склад (документ создан автоматически при переводе складом сетевого заказа в путь);
  	- Самовывоз (документ создан автоматически при завершении заказа через самовывоз);
  	- Вручную (документ переотправлен пользователем вручную);

Что такое задача на отправку документов (задача ЭДО):
Задача ЭДО — это процесс подготовки документа и его последующей отправки.
В рамках задачи система может выполнять следующие действия:
	- получение кодов маркировки
	- проверку кодов маркировки в Честном знаке
	- подготовку трансфера кодов маркировки между организациями ВВ
	- ведение трансфера до его завершения
	- распределения кодов маркировки на товары в заказе
	- заполнение отправляемого документа
	- отправка документа в необходимые сервисы (Модуль касса для чека, Такском для УПД)

Колонка ""Статус задачи"":
	- Новая: задача создана, но еще была взята в работу
	- В процессе: идет подготовка документа к отправке
	- Завершена: документ отправлен
	- Ожидание: задача ждет выполнения каких-либо автоматических действий с документом, подробности этого можно увидеть в разделе ""Проблемы"".
	- Проблема: выполнению отправки документа мешает какая-то проблема, подробности этого можно увидеть в разделе ""Проблемы"".
	- Отменяется: по задаче ведется процесс аннулирования документооборота.
	- Отменена: по задаче были отменены все документообороты.

Колонка ""Документ"".
Отображает на какой документ распределилась отправка ""электронной первички"":
	- УПД
	- Чек
	- Тендер

Колонка ""Кодов"":
В колонке «Кодов» отображается количество кодов маркировки, привязанных к документу.

У разных документов есть разные стадии отправки:

Стадии отправки УПД: 
	- Распределение: это стадия на которой выбирается, какой документ необходимо отправить:
		- УПД
		- чек
		- тендер
		- Не отправлять документ, а сохранить коды маркировки в пул.
	- Трансфер: это процесс внутренней продажи между организациями ВВ, с целью передачи кода в организацию которая указана в заказе.
	- Отправляется: значит что УПД находиться в процессе отправки в Такском
	- Отправлен: значит что УПД успешно отправлен в Такском
	- Завершен: значит что УПД получен клиентом в Такском

Стадии отправки Чека: 
	- Распределение: это стадия на которой выбирается, какой документ необходимо отправить:
		- УПД
		- чек
		- тендер
		- Не отправлять документ, а сохранить коды маркировки в пул.
	- Трансфер: это процесс внутренней продажи между организациями ВВ, с целью передачи кода в организацию которая указана в заказе.
	- Отправляется: значит документ находиться в процессе передачи чеков в кассу
	- Отправлен: значит что чеки дошли до кассы, и находятся в процессе обработки кассой
	- Завершен: значит что чеки успешно обработаны кассой
	При выборе стадий (Отправляется, Отправлен, Завершен) отображается информация о созданных чеках по заказу.
	Для больших заказов может быть создано несколько чеков, поэтому для отображения списка чеков представлена таблица Чеки.

Стадии отправки Тендера: 
	- Распределение: это стадия на которой выбирается, какой документ необходимо отправить:
		- УПД
		- чек
		- тендер
		- Не отправлять документ, а сохранить коды маркировки в пул.
	- Трансфер: это процесс внутренней продажи между организациями ВВ, с целью передачи кода в организацию которая указана в заказе.
	- Отправляется: значит что документ передан для ручной отправки кодов.
	- Коды выгружены вручную: значит что коды переданы вручную сотрудником.

Отображение информации о трансфере:
Если выбрать стадию трансфера в документе, то откроется диалог трансфера:
В таблице указано с какой организации и куда производиться трансфер (внутренняя продажа) кодов маркировки
В таблице перемещаемые коды выведен список кодов выбранного трансфера.
По выбранному трансферу отображаются стадии отправки его УПД:
	- Отправляется
	- В работе
	- Завершен:
	Отображается информация о документообороте с Такском:
		- Документооборот в ДВ: это исходящий документ в ДВ связанный с возможным множеством отправок в такском
		- Отправки в Такском: перечень попыток отправить наш исходящий документ в Такском:
		- Идентификатор в Такском: уникальный идентификатор по которому можно отрыть документ в личном кабинете Такском.
		- Статус: статус документа в системе Такском
		- Статус ГИС МТ: отображает результат обработки кодов маркировки в системе Честный знак
		- Ошибка: описание ошибки произошедшей в Такском
	
Диалог ""Проблемы"":
При подготовке или отправке документа могут возникнуть ошибки. Информация о них отображается во вкладке «Проблемы».
Если по документу есть нерешённая проблема:
	- вкладка «Проблемы» выделяется красным цветом;
	- статус задачи в списке документов отображается на красном фоне;
	- проблемная стадия отправки также отмечается соответствующим индикатором.
В таблице ""Относятся к проблеме"" отображаются коды маркировки или Gtin которые могли быть связаны с возникшей проблемой.
";
		}

		protected override void OnDestroyed()
		{
			if(ViewModel != null)
			{
				ViewModel.PropertyChanged -= ViewModelPropertyChanged;
			}
			base.OnDestroyed();
		}
	}
}
