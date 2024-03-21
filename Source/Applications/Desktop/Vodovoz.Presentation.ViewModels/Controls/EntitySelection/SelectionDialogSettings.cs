namespace Vodovoz.Presentation.ViewModels.Controls.EntitySelection
{
	public class SelectionDialogSettings
	{
		public string Title { get; set; } = "Выберите значение";
		public string TopLabelText { get; set; } = "Выберите значение";
		public string NoEntitiesMessage { get; set; } = "Элементы отсутствуют";
		public string SelectFromJournalButtonLabelText { get; set; } = "Выбрать из журнала";
		public bool IsCanOpenJournal { get; set; } = true;
	}
}
