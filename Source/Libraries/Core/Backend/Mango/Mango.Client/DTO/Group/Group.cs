using System;
namespace Mango.Client.DTO.Group
{
	/// <summary>
	/// Алгоритм распределения звонков в группе
	/// </summary>
	public enum GroupDialogAlg
	{
		/// <summary>
		/// Последовательный обзвон
		/// </summary>
		ALG_SERIAL_PRIOR,
		/// <summary>
		/// Параллельный по приоритету (по квалификации)
		/// </summary>
		ALG_PARALLEL_PRIOR,
		/// <summary>
		/// Одновременно всем свободным
		/// </summary>
		ALG_PARALLEL,
		/// <summary>
		/// В случайном порядке
		/// </summary>
		ALG_RANDOM,
		/// <summary>
		/// Равномерный (наиболее свободному)
		/// </summary>
		ALG_MOST_IDLE
	}

	/// <summary>
	/// Алгоритм дозвона до сотрудников в группе
	/// </summary>
	public enum UsersDialogAlg
	{
		/// <summary>
		/// На все контакты сотрудника одновременно
		/// </summary>
		ALG_M_ALL,
		/// <summary>
		/// На основные номера сотрудников
		/// </summary>
		ALG_M_MAIN,
		/// <summary>
		/// Только на SIP-учетные записи сотрудника
		/// </summary>
		ALG_M_SIP,
		/// <summary>
		/// На все контакты сотрудника по-очереди
		/// </summary>
		ALG_M_LINE,
		/// <summary>
		/// Как настроено в карточке сотрудника
		/// </summary>
		ALG_M_CARD
	}

	public class Group
	{
		public int id { get; set; }
		public string name { get; set; }
		public string descpription { get; set; }
		public string extension { get; set; }
		public int dial_alg_group { get; set; }
		public int dial_alg_users { get; set; }
		public int auto_redirect { get; set; }
		public int? auto_dial { get; set; }
		public int? line_id { get; set; }
		public int? use_dynamic_ivr { get; set; }
		public int? use_dynamic_seq_num { get; set; }
		public int? melody_id { get; set; }
		//public Operator operators { get; set; }
	}
	[Obsolete]
	public class Operator
	{
		public int id { get; set; }
		public string name { get; set; }
		public string extension { get; set; }
		public int priority { get; set; }
		public int order { get; set; }
	}
}
