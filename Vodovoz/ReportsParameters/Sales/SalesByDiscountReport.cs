using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Bindings.Collections.Generic;
using System.Linq;
using Gamma.ColumnConfig;
using Gamma.GtkWidgets;
using NHibernate.Transform;
using QS.Dialog.GtkUI;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Report;
using QSReport;
using Vodovoz.Domain.Orders;

namespace Vodovoz.ReportsParameters.Sales
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class SalesByDiscountReport : SingleUoWWidgetBase, IParametersWidget
	{
		class SalesByDiscountReportNode : PropertyChangedBase
		{
			public int Id { get; set; }

			public string Name { get; set; }

			private bool selected;

			public bool Selected {
				get => selected;
				set => SetField(ref selected, value, () => Selected);
			}
		}

		/// <summary>
		/// Конструкция фильтра где фильтры могут быть связаны друг с другом,
		/// позволяет снимать выделение документов и фильтровать наборы данных других фильтров
		/// 
		/// Принцип работы: Выбираемые в одном фильтре Id передается связанным фильтрам по FilteringRelation
		/// в связанных фильтрах наборы данных фильтруются по переданным им Id, условия фильтрации определяются
		/// в функции переданной при создании фильтра.
		/// </summary>
		class Criterion
		{
			//Набор данных
			private List<SalesByDiscountReportNode> list = new List<SalesByDiscountReportNode>();
			//Observable служит для подсчета количества выделенных элементов при их выделении
			public GenericObservableList<SalesByDiscountReportNode> ObservableList;

			/// <summary>
			/// Функция по получению набора данных, принимающая массив id для фильтрации
			/// </summary>
			private Func<int[], List<SalesByDiscountReportNode>> sourceFunction;

			public List<Criterion> UnselectRelation = new List<Criterion>();
			public List<Criterion> FilteringRelation = new List<Criterion>();
			/// <summary>
			/// Хранит массив id  
			/// </summary>
			/// <value>The filtered identifier.</value>
			public int[] FilteredId { get; set; }

			public event Action<string> Changed;

			public void SubcribeWithClearOld(Action<string> action)
			{
				Changed = delegate { };
				Changed += action;
			}

			public bool HaveSelected {
				get { return list.Any(x => x.Selected); }
			}

			public void Unselect()
			{
				if(HaveSelected) {
					ObservableList.ElementChanged -= ObservableList_ElementChanged_Unselect;
					list.Where(x => x.Selected).ToList().ForEach((obj) => { obj.Selected = false; });
					ObservableList.ElementChanged += ObservableList_ElementChanged_Unselect;
				}
			}

			public void RefreshItems()
			{
				list = sourceFunction.Invoke(FilteredId);
				ObservableList = new GenericObservableList<SalesByDiscountReportNode>(list);

				ObservableList.ElementChanged -= ObservableList_ElementChanged_Unselect;
				ObservableList.ElementChanged += ObservableList_ElementChanged_Unselect;

				ObservableList.ElementChanged -= ObservableList_ElementChanged_Filtering;
				ObservableList.ElementChanged += ObservableList_ElementChanged_Filtering;
			}

			void ObservableList_ElementChanged_Unselect(object aList, int[] aIdx)
			{
				if(UnselectRelation.Any()) {
					UnselectRelation.ForEach(x => x.Unselect());
				}
				Changed?.Invoke(SumSelected());
			}

			void ObservableList_ElementChanged_Filtering(object aList, int[] aIdx)
			{
				FilteringRelation.ForEach(x => x.Unselect());
				FilteringRelation.ForEach(x => x.FilteredId = ObservableList.Where(o => o.Selected).Select(o => o.Id).ToArray());
				FilteringRelation.ForEach(x => x.RefreshItems());
				Changed?.Invoke(SumSelected());
			}

			public Criterion(Func<int[], List<SalesByDiscountReportNode>> sourceFunc)
			{
				sourceFunction = sourceFunc;
				RefreshItems();
			}

			string SumSelected() => ObservableList.Count(x => x.Selected).ToString();
		}

		enum FilterTypes
		{
			DiscountReasonInclude,
			DiscountReasonExclude
		}

		Dictionary<FilterTypes, Criterion> criterions = new Dictionary<FilterTypes, Criterion>();

		public SalesByDiscountReport()
		{
			this.Build();
			UoW = UnitOfWorkFactory.CreateWithoutRoot();
			dateperiodpicker.StartDate = dateperiodpicker.EndDate = DateTime.Today;
			ConfigureFilters();
			ytreeviewSelectedList.ColumnsConfig = columnsConfig;
			yenumcomboboxDateType.ItemsEnum = typeof(OrderDateType);
			yenumcomboboxDateType.SelectedItem = OrderDateType.DeliveryDate;
		}

		private void ConfigureFilters()
		{
			// Основания скидок
			Criterion discountReasonIncludeCrit = CreateDiscountReasonCriterion();
			Criterion discountReasonExcludeCrit = CreateDiscountReasonCriterion();

			//Основания для скидок
			discountReasonIncludeCrit.UnselectRelation.Add(discountReasonExcludeCrit);
			discountReasonExcludeCrit.UnselectRelation.Add(discountReasonIncludeCrit);


			//Сохранение фильтров для использования
			criterions.Add(FilterTypes.DiscountReasonInclude, discountReasonIncludeCrit);
			criterions.Add(FilterTypes.DiscountReasonExclude, discountReasonExcludeCrit);
		}

		#region Создание фильтров

		private Criterion CreateDiscountReasonCriterion()
		{
			return new Criterion((arg) => {
				SalesByDiscountReportNode alias = null;
				var query = UoW.Session.QueryOver<DiscountReason>();
				var queryResult = query.SelectList(list => list
						 .Select(x => x.Id).WithAlias(() => alias.Id)
						 .Select(x => x.Name).WithAlias(() => alias.Name)
						)
				.TransformUsing(Transformers.AliasToBean<SalesByDiscountReportNode>())
				.List<SalesByDiscountReportNode>()
				.OrderBy(x => x.Name);
				return queryResult.ToList();
			});
		}

		#endregion

		private IColumnsConfig columnsConfig = ColumnsConfigFactory
			.Create<SalesByDiscountReportNode>()
			.AddColumn("Выбрать").AddToggleRenderer(node => node.Selected).Editing()
			.AddColumn("Название").AddTextRenderer(node => node.Name)
			.Finish();

		#region IParametersWidget implementation

		public event EventHandler<LoadReportEventArgs> LoadReport;

		public string Title => "Отчет по скидкам";

		#endregion

		private int[] GetResultIds(IEnumerable<int> ids)
		{
			return ids.Any() ? ids.ToArray() : new int[] { 0 };
		}

		private ReportInfo GetReportInfo()
		{
			return new ReportInfo {
				Identifier = "Sales.SalesByDiscountReport",
				Parameters = new Dictionary<string, object>
				{
					{ "StartDate", dateperiodpicker.StartDateOrNull },
					{ "EndDate", dateperiodpicker.EndDateOrNull },
					//основания для скидок
					{ "discountreason_include", GetResultIds(criterions[FilterTypes.DiscountReasonInclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) },
					{ "discountreason_exclude", GetResultIds(criterions[FilterTypes.DiscountReasonExclude].ObservableList.Where(x => x.Selected).Select(d => d.Id)) },
					{ "DateType", yenumcomboboxDateType.SelectedItem.ToString()}
				}
			};
		}

		protected void OnButtonCreateReportClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}

		GenericObservableList<SalesByDiscountReportNode> treeNodes;

		protected void OnButtonDiscountReasonSelectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.DiscountReasonInclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.DiscountReasonInclude].SubcribeWithClearOld((string obj) => {
				ylabelDiscountReason.Text = String.Format("Вкл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Включаемые основания скидок";
		}

		protected void OnButtonDiscountReasonUnselectClicked(object sender, EventArgs e)
		{
			treeNodes = criterions[FilterTypes.DiscountReasonExclude].ObservableList;
			ytreeviewSelectedList.ItemsDataSource = treeNodes;
			criterions[FilterTypes.DiscountReasonExclude].SubcribeWithClearOld((string obj) => {
				ylabelDiscountReason.Text = String.Format("Искл.: {0} елем.", obj);
			});
			labelTableTitle.Text = "Исключаемые основания скидок";
		}


		protected void OnButtonSelectAllClicked(object sender, EventArgs e)
		{
			object source = ytreeviewSelectedList.ItemsDataSource;
			if(source is GenericObservableList<SalesByDiscountReportNode>) {
				foreach(SalesByDiscountReportNode item in (source as GenericObservableList<SalesByDiscountReportNode>)) {
					item.Selected = true;
				}
			}
		}

		protected void OnButtonUnselectAllClicked(object sender, EventArgs e)
		{
			object source = ytreeviewSelectedList.ItemsDataSource;
			if(source is GenericObservableList<SalesByDiscountReportNode>) {
				foreach(SalesByDiscountReportNode item in (source as GenericObservableList<SalesByDiscountReportNode>)) {
					item.Selected = false;
				}
			}
		}

		protected void OnSearchEntityInSelectedListTextChanged(object sender, EventArgs e)
		{
			if(treeNodes != null) {
				if(searchEntityInSelectedList.Text.Length > 0)
					ytreeviewSelectedList.ItemsDataSource = new GenericObservableList<SalesByDiscountReportNode>(
						treeNodes
						.Where(
							n => n.Name
							.ToLower()
							.Contains(
								searchEntityInSelectedList
								.Text
								.ToLower()
							)
						)
						.ToList()
					);
				else
					ytreeviewSelectedList.ItemsDataSource = treeNodes;
			} else {
				searchEntityInSelectedList.Text = String.Empty;
			}
		}
	}

	public enum OrderDateType
	{
		[Display(Name = "По дате доставки")]
		DeliveryDate,
		[Display(Name = "По дате создания")]
		CreationDate
	}
}
