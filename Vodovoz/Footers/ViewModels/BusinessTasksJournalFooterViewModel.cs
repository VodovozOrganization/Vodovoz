using QS.ViewModels;

namespace Vodovoz.Footers.ViewModels
{
	public class BusinessTasksJournalFooterViewModel : WidgetViewModelBase
	{
		// Кол-во звонков
		private int ringCount;
		public int RingCount {
			get => ringCount;
			set => SetField(ref ringCount, value);
		}

		// Кол-во сложных клиентов
		private int hardClientsCount;
		public int HardClientsCount {
			get => hardClientsCount;
			set => SetField(ref hardClientsCount, value);
		}

		// Кол-во заданий
		private int tasksCount;
		public int TasksCount {
			get => tasksCount;
			set => SetField(ref tasksCount, value);
		}

		// Кол-во задач
		private int tasks;
		public int Tasks {
			get => tasks;
			set => SetField(ref tasks, value);
		}

		// Кол-во тары на забор
		private int tareReturn;
		public int TareReturn {
			get => tareReturn;
			set => SetField(ref tareReturn, value);
		}
	}
}
