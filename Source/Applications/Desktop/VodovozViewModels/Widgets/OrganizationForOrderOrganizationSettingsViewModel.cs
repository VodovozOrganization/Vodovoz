using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using QS.DomainModel.Entity;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Journal;
using QS.ViewModels.Dialog;
using Vodovoz.Domain.Organizations;
using Vodovoz.ViewModels.Organizations;
using VodovozBusiness.Domain.Settings;
using VodovozBusiness.Nodes;

namespace Vodovoz.ViewModels.Widgets
{
	public class OrganizationForOrderOrganizationSettingsViewModel : AddOrRemoveIDomainObjectViewModelBase
	{
		private readonly INavigationManager _navigation;
		private readonly IOrganizationForOrderFromSet _organizationForOrderFromSet;
		private readonly IDictionary<int, (TimeSpan From, TimeSpan To)> _organizationsChoiceTime =
			new Dictionary<int, (TimeSpan From, TimeSpan To)>();
		private OrganizationChoiceParametersForOrderOrganizationSettings _organizationChoiceParameters;

		public OrganizationForOrderOrganizationSettingsViewModel(
			INavigationManager navigation,
			IOrganizationForOrderFromSet organizationForOrderFromSet)
		{
			_navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
			_organizationForOrderFromSet =
				organizationForOrderFromSet ?? throw new ArgumentNullException(nameof(organizationForOrderFromSet));
			
			Title = "Организации";
		}
		
		public void Configure(
			bool canEdit,
			IUnitOfWork uow,
			DialogViewModelBase parentViewModel,
			OrganizationChoiceParametersForOrderOrganizationSettings organizationChoiceParameters,
			IEnumerable<INamedDomainObject> entities)
		{
			SetCanEdit(canEdit);
			ParentViewModel = parentViewModel;
			_organizationChoiceParameters = organizationChoiceParameters;
			Entities = entities;
			UpdateChooseTime();
			UoW = uow;
			
			InitializeCommands();
		}
		
		public string GetOrganizationChoiceTime(int organizationId)
		{
			var choiceTime = _organizationsChoiceTime[organizationId];
			return $"{choiceTime.From} - {choiceTime.To}";
		}

		protected override void AddEntity()
		{
			var viewModel = _navigation.OpenViewModel<OrganizationJournalViewModel>(
				ParentViewModel,
				OpenPageOptions.AsSlave,
				vm =>
				{
					var filter = vm.JournalFilter as OrganizationJournalFilterViewModel;
					
					if(_organizationChoiceParameters.NeedAvangardShopId)
					{
						filter.HasAvangardShopId = true;
					}
					
					if(_organizationChoiceParameters.NeedCashBoxId)
					{
						filter.HasCashBoxId = true;
					}
					
					if(_organizationChoiceParameters.NeedTaxcomEdoAccountId)
					{
						filter.HasTaxcomEdoAccountId = true;
					}
				}
			).ViewModel;

			if(!(viewModel is JournalViewModelBase journal))
			{
				return;
			}

			journal.SelectionMode = JournalSelectionMode.Multiple;
			journal.OnSelectResult += OnEntitySelectResult;
		}

		private void OnEntitySelectResult(object sender, JournalSelectedEventArgs e)
		{
			(sender as JournalViewModelBase).OnSelectResult -= OnEntitySelectResult;
			
			var addingEntities = e.SelectedObjects;

			foreach(var addingEntity in addingEntities)
			{
				var entity = UoW.GetById<Organization>(DomainHelper.GetId(addingEntity));
				
				if(Entities.Contains(entity))
				{
					continue;
				}
				
				(Entities as IList).Add(entity);
				UpdateChooseTime();
			}
		}

		protected override void RemoveEntity()
		{
			base.RemoveEntity();
			UpdateChooseTime();
		}
		
		private void UpdateChooseTime()
		{
			_organizationsChoiceTime.Clear();
			var choiceTime = _organizationForOrderFromSet.GetChoiceTimeOrganizationFromSet(Entities);

			foreach(var keyPairValue in choiceTime)
			{
				_organizationsChoiceTime.Add(keyPairValue);
			}
		}
	}
}
