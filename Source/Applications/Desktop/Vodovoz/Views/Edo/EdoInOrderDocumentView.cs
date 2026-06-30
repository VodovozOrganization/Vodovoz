using Gtk;
using QS.Views.GtkUI;
using System;
using System.ComponentModel;
using Vodovoz.ViewModels.Edo;
namespace Vodovoz.Views.Edo
{
	[ToolboxItem(true)]
	public partial class EdoInOrderDocumentView : WidgetViewBase<EdoInOrderDocumentViewModel>
	{
		private Widget _stageView;

		public EdoInOrderDocumentView()
		{
			this.Build();
		}

		public override EdoInOrderDocumentViewModel ViewModel
		{
			get => base.ViewModel;
			set
			{
				if(base.ViewModel != null)
				{
					base.ViewModel.PropertyChanged -= ViewModelPropertyChanged;
				}
				base.ViewModel = value;
				if(base.ViewModel != null)
				{
					base.ViewModel.PropertyChanged += ViewModelPropertyChanged;
				}
			}
		}

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == nameof(EdoInOrderDocumentViewModel.StageViewModel))
			{
				ShowStage();
			}
		}

		private void ShowStage()
		{
			CloseStageView();
			_stageView = ResolveStageView();
			if(_stageView != null)
			{
				yhboxDocumentStage.PackStart(_stageView);
				_stageView.Show();
			}
		}

		private Widget ResolveStageView()
		{
			if(ViewModel.StageViewModel == null)
			{
				CloseStageView();
				return null;
			}

			switch(ViewModel.StageViewModel)
			{
				case EdoInOrderTransferStageViewModel transfer:
					var transferView = new EdoInOrderTransferStageView();
					transferView.ViewModel = transfer;
					return transferView;
				default:
					throw new NotSupportedException($"Не поддерживаемый тип стадии: {ViewModel.StageViewModel.GetType()}");
			}
		}

		private void CloseStageView()
		{
			yhboxDocumentStage.Remove(_stageView);
			_stageView?.Destroy();
		}
	}
}
