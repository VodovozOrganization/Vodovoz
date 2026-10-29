using System;
using System.Collections.Generic;
using System.Linq;

namespace Edo.DeviationMonitoring.Validation
{
	/// <summary>
	/// Набор валидаторов отклонений, включенных в справочнике,
	/// упорядоченных по типу отклонения
	/// </summary>
	public class EdoDeviationValidators
	{
		public EdoDeviationValidators(
			IReadOnlyList<EdoDeviationValidation<IEdoTaskDeviationValidator>> taskValidators,
			IReadOnlyList<EdoDeviationValidation<IEdoRequestDeviationValidator>> requestValidators,
			IReadOnlyList<EdoDeviationValidation<IEdoTransferDeviationValidator>> transferValidators)
		{
			TaskValidators = taskValidators ?? throw new ArgumentNullException(nameof(taskValidators));
			RequestValidators = requestValidators ?? throw new ArgumentNullException(nameof(requestValidators));
			TransferValidators = transferValidators ?? throw new ArgumentNullException(nameof(transferValidators));

			FinishedTaskValidators = TaskValidators
				.Where(x => x.Validator.IsAppliesToFinishedTask)
				.ToList();

			FinishedTransferValidators = TransferValidators
				.Where(x => x.Validator.IsAppliesToFinishedTask)
				.ToList();
		}

		/// <summary>
		/// Валидаторы отклонений по задачам ЭДО
		/// </summary>
		public IReadOnlyList<EdoDeviationValidation<IEdoTaskDeviationValidator>> TaskValidators { get; }

		/// <summary>
		/// Валидаторы отклонений по заявкам ЭДО без задач
		/// </summary>
		public IReadOnlyList<EdoDeviationValidation<IEdoRequestDeviationValidator>> RequestValidators { get; }

		/// <summary>
		/// Валидаторы отклонений по задачам трансфера
		/// </summary>
		public IReadOnlyList<EdoDeviationValidation<IEdoTransferDeviationValidator>> TransferValidators { get; }

		/// <summary>
		/// Валидаторы отклонений по задачам трансфера, которые проверяются и по завершенным задачам
		/// </summary>
		public IReadOnlyList<EdoDeviationValidation<IEdoTransferDeviationValidator>> FinishedTransferValidators { get; }

		/// <summary>
		/// Валидаторы отклонений по задачам, которые проверяются и по завершенным задачам
		/// </summary>
		public IReadOnlyList<EdoDeviationValidation<IEdoTaskDeviationValidator>> FinishedTaskValidators { get; }
	}
}
