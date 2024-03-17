using QS.Commands;
using System;
using System.ComponentModel;

namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public interface IEntitySelectionViewModel : INotifyPropertyChanged, IDisposable
	{
		bool DisposeViewModel { get; set; }

		#region Выбранная сущьность
		string EntityTitle { get; }
		object Entity { get; set; }
		#endregion

		#region События для внешних подписчиков
		event EventHandler Changed;
		event EventHandler ChangedByUser;
		#endregion

		#region Настройки виджета
		bool IsEditable { get; set; }
		#endregion

		#region Доступность функций View
		bool CanSelectEntity { get; }
		bool CanClearEntity { get; }
		#endregion

		#region Команды от View
		DelegateCommand OpenSelectDialogCommand { get; }
		DelegateCommand ClearEntityCommand { get; }
		#endregion
	}
}
