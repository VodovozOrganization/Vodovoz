using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.Domain.Cash;
using Vodovoz.EntityRepositories.Subdivisions;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashBookReport : SingleUoWWidgetBase, IParametersWidget
	{
		private string reportPath = "Wages.CashBook";
		private List<Subdivision> UserSubdivisions { get; }
		
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly ICommonServices commonServices;
		public CashBookReport(ISubdivisionRepository subdivisionRepository, ICommonServices commonServices)
		{
			this.Build();
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			UoW = UnitOfWorkFactory.CreateWithoutRoot ();
			UserSubdivisions = GetSubdivisionsForUser();
			
			dateperiodpicker.StartDate = DateTime.Now.Date;
			dateperiodpicker.EndDate = DateTime.Now.Date;

			#region Выбор кассы
			var subdivisions = subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income), typeof(Income) });
			{
				IEnumerable<int> fromTypes = subdivisions.Select(x => x.Id);
				IEnumerable<int> fromUser = UserSubdivisions.Select(x => x.Id);
				if (! new HashSet<int>(fromTypes).IsSupersetOf(fromUser))
				{
					subdivisions.Concat(UserSubdivisions);
				}
			}
			
			yspeccomboboxCashSubdivision.SetRenderTextFunc<Subdivision>(s => s.Name);
			yspeccomboboxCashSubdivision.ItemsList = subdivisions;
			yspeccomboboxCashSubdivision.SelectedItem = UserSubdivisions.First();
			#endregion
			
			
			buttonCreateRepot.Clicked += OnButtonCreateRepotClicked;
		}
		
		private List<Subdivision> GetSubdivisionsForUser()
		{
			var availableSubdivisionsForUser = subdivisionRepository.GetCashSubdivisionsAvailableForUser(UoW, commonServices.UserService.GetCurrentUser(UoW));

			return new List<Subdivision>(availableSubdivisionsForUser);
		}
		
		private ReportInfo GetReportInfo()
		{
			string startDate = $"{dateperiodpicker.StartDate:yyyy-MM-dd}";
			string endDate = $"{dateperiodpicker.EndDate:yyyy-MM-dd}";
			var parameters = new Dictionary<string, object> {
				{ "StartDate", startDate },
				{ "EndDate", endDate },
				{ "CashBox", ((Subdivision) yspeccomboboxCashSubdivision.SelectedItem).Id }
			};

			return new ReportInfo {
				Identifier = reportPath,
				UseUserVariables = true,
				Parameters = parameters
			};
		}

		void OnUpdate(bool hide = false)
		{
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		}
		protected void OnButtonCreateRepotClicked(object sender, EventArgs e)
		{
			OnUpdate(true);
		}

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public string Title => "Кассовая книга";
	}
}
