using System;
using System.Collections.Generic;
using System.Linq;
using QS.Dialog.GtkUI;
using QS.DomainModel.UoW;
using QS.Project.Services;
using QS.Report;
using QS.Services;
using QSReport;
using Vodovoz.Domain.Cash;
using Vodovoz.Domain.Organizations;
using Vodovoz.EntityRepositories.Subdivisions;
using Vodovoz.Reports;

namespace Vodovoz.ReportsParameters
{
	[System.ComponentModel.ToolboxItem(true)]
	public partial class CashBookReport : SingleUoWWidgetBase, IParametersWidget
	{
		private string reportPath;
		private List<Subdivision> UserSubdivisions { get; }
		private IEnumerable<Organization> Organizations { get; }

		private readonly IReportInfoFactory _reportInfoFactory;
		private readonly ISubdivisionRepository subdivisionRepository;
		private readonly ICommonServices commonServices;

		public CashBookReport(IReportInfoFactory reportInfoFactory, ISubdivisionRepository subdivisionRepository, ICommonServices commonServices)
		{
			this.Build();
			_reportInfoFactory = reportInfoFactory ?? throw new ArgumentNullException(nameof(reportInfoFactory));
			this.subdivisionRepository = subdivisionRepository ?? throw new ArgumentNullException(nameof(subdivisionRepository));
			this.commonServices = commonServices ?? throw new ArgumentNullException(nameof(commonServices));
			UoW = ServicesConfig.UnitOfWorkFactory.CreateWithoutRoot ();
			UserSubdivisions = GetSubdivisionsForUser();
			
			dateperiodpicker.StartDate = DateTime.Now.Date;
			dateperiodpicker.EndDate = DateTime.Now.Date;

			#region Выбор кассы
			var subdivisions = subdivisionRepository.GetSubdivisionsForDocumentTypes(UoW, new Type[] { typeof(Income), typeof(Income) });
			var itemsList = subdivisions.ToList();
			{
				IEnumerable<int> fromTypes = itemsList.Select(x => x.Id);
				IEnumerable<int> fromUser = UserSubdivisions.Select(x => x.Id);
				if (! new HashSet<int>(fromTypes).IsSupersetOf(fromUser))
				{
					subdivisions = itemsList.Concat(UserSubdivisions);
				}
			}
			itemsList.Add(new Subdivision{Name = "Все"});

			yspeccomboboxCashSubdivision.SetRenderTextFunc<Subdivision>(s => s.Name);
			yspeccomboboxCashSubdivision.ItemsList = itemsList;
			yspeccomboboxCashSubdivision.SelectedItem = UserSubdivisions.Count != 0 ? UserSubdivisions?.First() : itemsList.First();
			#endregion

			hboxOrganisations.Visible = false;
			Organizations = UoW.GetAll<Organization>();
			specialListCmbOrganisations.SetRenderTextFunc<Organization>(s => s.Name);
			specialListCmbOrganisations.ItemsList = Organizations;
			
			int currentUserId = commonServices.UserService.CurrentUserId;
			bool canCreateCashReportsForOrganisations = 
				commonServices.PermissionService.ValidateUserPresetPermission("can_create_cash_reports_for_organisations", currentUserId);
			ycheckOrganisations.Visible = canCreateCashReportsForOrganisations;
			ycheckOrganisations.Toggled += CheckOrganisationsToggled;

			buttonCreateRepot.Clicked += OnButtonCreateRepotClicked;
		}

		private void CheckOrganisationsToggled(object sender, EventArgs e)
		{
			if(ycheckOrganisations.Active) {
				hboxCash.Visible = false;
				hboxOrganisations.Visible = true;
			} else {
				hboxCash.Visible = true;
				hboxOrganisations.Visible = false;
			}
		}

		private List<Subdivision> GetSubdivisionsForUser()
		{
			var availableSubdivisionsForUser = subdivisionRepository.GetCashSubdivisionsAvailableForUser
				(UoW, commonServices.UserService.GetCurrentUser());
			return new List<Subdivision>(availableSubdivisionsForUser);
		}
		
		private ReportInfo GetReportInfo()
		{
			if (yspeccomboboxCashSubdivision.SelectedItem == null)
			{
				throw new ArgumentNullException("Для формирования отчета необходимо выбрать кассу!");
			}

			bool allCashes = ((Subdivision) yspeccomboboxCashSubdivision.SelectedItem)?.Name == "Все";

			var parameters = new Dictionary<string, object> {
				{ "StartDate", dateperiodpicker.StartDateOrNull.Value},
				{ "EndDate", dateperiodpicker.EndDateOrNull.Value.AddHours(23).AddMinutes(59).AddSeconds(59) },
			};

			if (ycheckOrganisations.Active)
			{
				reportPath = "Cash.CashBookOrganisations";
				parameters.Add("organisation", 
					(specialListCmbOrganisations.SelectedItem as Organization)?.Id ?? -1);
				parameters.Add("organisation_name", 
					(specialListCmbOrganisations.SelectedItem as Organization)?.Name ?? string.Empty);
			}
			else
			{
				reportPath = "Cash.CashBook";
				parameters.Add("Cash", allCashes ? -1 : ((Subdivision) yspeccomboboxCashSubdivision.SelectedItem)?.Id);
			}

			var reportInfo = _reportInfoFactory.Create(reportPath, Title, parameters);
			reportInfo.UseUserVariables = true;

			return reportInfo;
		}

		void OnUpdate(bool hide = false) => 
			LoadReport?.Invoke(this, new LoadReportEventArgs(GetReportInfo(), hide));
		
		protected void OnButtonCreateRepotClicked(object sender, EventArgs e) => OnUpdate(true);

		public event EventHandler<LoadReportEventArgs> LoadReport;
		public string Title => "Кассовая книга";
	}
}
