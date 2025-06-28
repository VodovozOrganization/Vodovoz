using System.ComponentModel;
using Gtk;
using QS.Views;
using Vodovoz.ViewModels.ViewModels.Counterparty;

namespace Vodovoz.Views.Client
{
	[ToolboxItem(true)]
	public partial class CounterpartyEdoAccountsByOrganizationView : ViewBase<CounterpartyEdoAccountsByOrganizationViewModel>
	{
		private RadioButton _firstIsDefaultAccountRadioBtn;
		
		public CounterpartyEdoAccountsByOrganizationView(
			CounterpartyEdoAccountsByOrganizationViewModel viewModel) : base(viewModel)
		{
			Build();
			Configure();
		}

		private void Configure()
		{
			InitializeCurrentEdoAccountsViews();
			edoLightsMatrixView.ViewModel = ViewModel.EdoLightsMatrixViewModel;
			
			ViewModel.EdoAccountViewModelAdded += OnEdoAccountViewModelAdded;
			ViewModel.EdoAccountViewModelRemoved += OnEdoAccountViewModelRemoved;
		}

		private void InitializeCurrentEdoAccountsViews()
		{
			var i = 0; 
			foreach(var edoAccountViewModel in ViewModel.EdoAccountsViewModels)
			{
				AddEdoAccountView(edoAccountViewModel);
			}

			var lightMatrixBox = (Box.BoxChild)vboxEdoAccountsByOrganization[hboxEdoLightsMatrixByOrganization];
			lightMatrixBox.PackType = PackType.End;
			lightMatrixBox.Position = 0;
		}

		private void OnEdoAccountViewModelAdded(EdoAccountViewModel edoAccountViewModel)
		{
			AddEdoAccountView(edoAccountViewModel);
		}

		private void AddEdoAccountView(EdoAccountViewModel edoAccountViewModel)
		{
			var edoAccountView = new EdoAccountView(edoAccountViewModel);
			
			if(_firstIsDefaultAccountRadioBtn is null)
			{
				_firstIsDefaultAccountRadioBtn = edoAccountView.IsDefaultAccountRbtn;
			}
			else
			{
				edoAccountView.IsDefaultAccountRbtn.Group = _firstIsDefaultAccountRadioBtn.Group;
			}
			
			vboxEdoAccountsByOrganization.Add(edoAccountView);
			edoAccountView.Show();
		}
		
		private void OnEdoAccountViewModelRemoved(EdoAccountViewModel edoAccountViewModel, int index)
		{
			var edoAccountView = vboxEdoAccountsByOrganization.Children[index];
			edoAccountView.Destroy();
		}

		protected override void OnDestroyed()
		{
			ViewModel.EdoAccountViewModelAdded -= OnEdoAccountViewModelAdded;
			base.OnDestroyed();
		}
	}
}
