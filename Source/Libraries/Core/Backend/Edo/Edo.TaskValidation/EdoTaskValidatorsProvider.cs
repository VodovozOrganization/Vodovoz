using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Vodovoz.Core.Domain.Edo;

namespace Edo.TaskValidation
{
	public class EdoTaskValidatorsProvider
	{
		private readonly IUnitOfWorkFactory _uowFactory;
		private readonly EdoTaskValidatorsPersister _edoTaskValidatorsPersister;

		public EdoTaskValidatorsProvider(IUnitOfWorkFactory uowFactory, EdoTaskValidatorsPersister edoTaskValidatorsPersister)
		{
			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_edoTaskValidatorsPersister = edoTaskValidatorsPersister ?? throw new ArgumentNullException(nameof(edoTaskValidatorsPersister));
		}

		public IEnumerable<IEdoTaskValidator> GetAllValidators()
		{
			return _edoTaskValidatorsPersister.GetEdoTaskValidators(_uowFactory);
		}

		public IEnumerable<IEdoTaskValidator> GetValidatorsFor(EdoTask task)
		{
			var allValidators = _edoTaskValidatorsPersister.GetEdoTaskValidators(_uowFactory);
			return allValidators.Where(x => x.IsApplicable(task));
		}
	}
}
