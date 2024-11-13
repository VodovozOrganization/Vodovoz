using NHibernate;
using QS.Commands;
using QS.Dialog;
using QS.DomainModel.UoW;
using QS.Navigation;
using QS.Project.Domain;
using QS.Services;
using QS.Tdi;
using QS.ViewModels;
using QS.ViewModels.Extension;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vodovoz.Domain.Logistic;
using Vodovoz.EntityRepositories.Employees;
using VodovozBusiness.Domain.Service;

namespace Vodovoz.ViewModels.Logistic
{
	public sealed class ServiceDistrictsSetActivationViewModel : EntityTabViewModelBase<ServiceDistrictsSet>, ITDICloseControlTab, IAskSaveOnCloseViewModel
	{
		private readonly IEmployeeRepository _employeeRepository;
		private bool _activationInProgress;
		private string _activationStatus;
		private ServiceDistrictsSet _activeServiceDistrictsSet;
		private List<DriverDistrictPriority> _notCopiedPriorities = new List<DriverDistrictPriority>();
		private bool _wasActivated;
		private readonly TimeSpan _queriesTimeout = TimeSpan.FromSeconds(240);

		public ServiceDistrictsSetActivationViewModel(
			IEntityUoWBuilder uowBuilder,
			IUnitOfWorkFactory unitOfWorkFactory,
			ICommonServices commonServices,
			IEmployeeRepository employeeRepository,
			INavigationManager navigation = null)
			: base(uowBuilder, unitOfWorkFactory, commonServices, navigation)
		{
			TabName = $"Активация версии районов \"{Entity.Name}\"";

			ActiveServiceDistrictsSet = UoW.Session.QueryOver<ServiceDistrictsSet>()
				.SetTimeout((int)_queriesTimeout.TotalSeconds)
				.Where(x => x.Status == ServiceDistrictsSetStatus.Active)
				.Take(1)
				.SingleOrDefault();

			_employeeRepository = employeeRepository;

			ActivateCommand = new DelegateCommand(Activate);
		}

		private void Activate()
		{
			if(!CommonServices.InteractiveService.Question($"Переключить базу на версию районов \"{Entity.Name}\""))
			{
				return;
			}
			try
			{
				_ = ActivateAsync();
			}
			catch
			{
				throw;
			}
		}

		public override bool HasChanges => false;
		public bool AskSaveOnClose => false;

		public ServiceDistrictsSet ActiveServiceDistrictsSet
		{
			get => _activeServiceDistrictsSet;
			set => SetField(ref _activeServiceDistrictsSet, value);
		}

		public bool ActivationInProgress
		{
			get => _activationInProgress;
			set => SetField(ref _activationInProgress, value);
		}

		public string ActivationStatus
		{
			get => _activationStatus;
			set => SetField(ref _activationStatus, value);
		}

		public List<DriverDistrictPriority> NotCopiedPriorities
		{
			get => _notCopiedPriorities;
			set => SetField(ref _notCopiedPriorities, value);
		}

		public bool WasActivated
		{
			get => _wasActivated;
			private set => SetField(ref _wasActivated, value);
		}
		public DelegateCommand ActivateCommand { get; set; }
		public string ActiveServiceDistrictsSetName => ActiveServiceDistrictsSet?.Name ?? "-";

		public object SelectedServiceDistrictName => Entity?.Name ?? "";

		public bool CanClose()
		{
			if(ActivationInProgress)
			{
				CommonServices.InteractiveService.ShowMessage(ImportanceLevel.Info, "Дождитесь окончания активации версии районов",
					"Активация версии районов");
			}
			return !ActivationInProgress;
		}

		public async Task ActivateAsync()
		{
			try
			{
				ActivationInProgress = true;

				var task = Task.Run(() =>
				{
					var unitOfWorkTitle = TabName;

					using(var localUoW = UnitOfWorkFactory.CreateWithoutRoot(unitOfWorkTitle))
					{
						ServiceDistrictsSet currentActiveDistrictSet = null;

						if(ActiveServiceDistrictsSet != null)
						{
							currentActiveDistrictSet = localUoW.GetById<ServiceDistrictsSet>(ActiveServiceDistrictsSet.Id);
						}

						var districtSetToActivate = localUoW.GetById<ServiceDistrictsSet>(Entity.Id);

						localUoW.Session.BeginTransaction();
						if(currentActiveDistrictSet != null)
						{
							currentActiveDistrictSet.Status = ServiceDistrictsSetStatus.Draft;
							localUoW.Save(currentActiveDistrictSet);
						}
						districtSetToActivate.Status = ServiceDistrictsSetStatus.Active;

						districtSetToActivate.DateActivated = DateTime.Now;
						localUoW.Save(districtSetToActivate);
						localUoW.Commit();
					}
				});

				await task;

				WasActivated = true;
				ActivationStatus = "Активация завершена";
				OnPropertyChanged(nameof(NotCopiedPriorities));
				UoW.Session.Refresh(Entity);
				UoW.Session.Refresh(ActiveServiceDistrictsSet);

				ActiveServiceDistrictsSet = Entity;
			}
			catch
			{
				ActivationStatus = "Ошибка при активации версии районов. Попробуйте активировать версию ещё раз";
				throw;
			}
			finally
			{
				ActivationInProgress = false;
			}
		}
	}
}
