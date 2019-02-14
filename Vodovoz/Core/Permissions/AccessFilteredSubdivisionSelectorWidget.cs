using System;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Domain.Permissions;

namespace Vodovoz.Core.Permissions
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class AccessFilteredSubdivisionSelectorWidget : Gtk.Bin
	{
		IUnitOfWork uow;

		public bool NeedChooseSubdivision { get; set; }

		public bool AllSelected => yspeccomboboxSubdivision.IsSelectedAll;

		private IEnumerable<Subdivision> subdivisions;

		public Subdivision SelectedSubdivision
		{
			get {
				var subdivisionSelected = yspeccomboboxSubdivision.SelectedItem as Subdivision;
				if(subdivisionSelected != null) {
					return subdivisionSelected;
				}
				if(yspeccomboboxSubdivision.IsSelectedAll) {
					return null;
				}
				var firstSubdivision = subdivisions.First();
				yspeccomboboxSubdivision.SelectedItem = firstSubdivision;
				return firstSubdivision;
			}
			  
		}

		public event EventHandler OnSelected;

		public string ValidationErrorMessage => "Ни одно подразделение, к которым у пользователя установлены права для кассы, нет прав работать с этим документом.";

		public AccessFilteredSubdivisionSelectorWidget()
		{
			this.Build();
			yspeccomboboxSubdivision.ItemSelected += YspeccomboboxSubdivision_ItemSelected;
		}

		void YspeccomboboxSubdivision_ItemSelected(object sender, Gamma.Widgets.ItemSelectedEventArgs e)
		{
			OnSelected?.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Конифгурирование с валидацией по типам документов
		/// </summary>
		public bool Configure(IUnitOfWork uow, bool ShowSpecialStateAll, params Type[] entityTypes)
		{
			this.uow = uow;
			yspeccomboboxSubdivision.ShowSpecialStateAll = ShowSpecialStateAll;
			return ValidateAndFillList(entityTypes);
		}

		/// <summary>
		/// Конфигурирование по готовому списку подразделений
		/// </summary>
		public void Configure(IUnitOfWork uow, IEnumerable<Subdivision> subdivisions)
		{
			this.uow = uow;
			FillList(subdivisions);
		}

		private bool ValidateAndFillList(Type[] entityTypes)
		{
			var validationResult = EntitySubdivisionForUserPermissionValidator.Validate(uow, entityTypes);

			var subdivisionsList = new List<Subdivision>();
			foreach(var item in entityTypes) {
				subdivisionsList.AddRange(validationResult
					.Where(x => x.GetPermission(item).Read)
					.Select(x => x.Subdivision)
				);
			}

			NeedChooseSubdivision = validationResult.Any(x => !x.IsMainSubdivision) && subdivisionsList.Any();
			ShowFilter();
			if(!subdivisionsList.Any()) {
				return false;
			}
			FillList(subdivisionsList);
			return true;
		}

		private void FillList(IEnumerable<Subdivision> subdivisions)
		{
			this.subdivisions = subdivisions.Distinct();
			yspeccomboboxSubdivision.ItemsList = this.subdivisions;
			if(!yspeccomboboxSubdivision.ShowSpecialStateAll) {
				yspeccomboboxSubdivision.SelectedItem = this.subdivisions.First();
			}
		}

		protected override void OnShown()
		{
			base.OnShown();
			ShowFilter();
		}

		private void ShowFilter()
		{
			Visible = NeedChooseSubdivision;
		}

	}
}
