using QS.DomainModel.UoW;
using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Core.Domain.Edo;

namespace Edo.Problems.Exception
{
	public class EdoTaskExceptionSourcesPersister
	{
		private readonly IUnitOfWorkFactory _uowFactory;

		private IEnumerable<EdoTaskProblemExceptionSource> _persistedSources;
		private IEnumerable<EdoTaskProblemExceptionSource> _registeredSources;

		public EdoTaskExceptionSourcesPersister(
			IUnitOfWorkFactory uowFactory,
			IEnumerable<EdoTaskProblemExceptionSource> exceptionSources
			)
		{
			var sameNameSources = exceptionSources.GroupBy(x => x.Name).Where(x => x.Count() > 1);
			if(sameNameSources.Count() > 0)
			{
				throw new InvalidOperationException($"Объекты описания проблем не могут иметь одинаковые имена: " +
					$"{string.Join(", ", sameNameSources.Select(x => x.Key))}");
			}

			_uowFactory = uowFactory ?? throw new ArgumentNullException(nameof(uowFactory));
			_registeredSources = exceptionSources;

			Persist();
		}

		private void Persist()
		{
			using(var uow = _uowFactory.CreateWithoutRoot())
			{
				var changed = false;
				var savedSources = uow.Session.QueryOver<EdoTaskProblemExceptionSourceEntity>().List();
				foreach(var registeredSource in _registeredSources)
				{
					var savedSource = savedSources.FirstOrDefault(x => x.Name == registeredSource.Name);
					if(savedSource == null)
					{
						savedSource = new EdoTaskProblemExceptionSourceEntity
						{
							Name = registeredSource.Name,
							Importance = registeredSource.Importance,
							Description = registeredSource.Description,
							Recommendation = registeredSource.Recommendation
						};
						uow.Save(savedSource);
						changed = true;
						continue;
					}

					if(savedSource.Equals(registeredSource))
					{
						continue;
					}

					savedSource.Importance = registeredSource.Importance;
					savedSource.Description = registeredSource.Description;
					savedSource.Recommendation = registeredSource.Recommendation;

					uow.Save(savedSource);
					changed = true;
				}
				if(changed)
				{
					uow.Commit();
				}
			}

			_persistedSources = _registeredSources;
		}

		public T GetExceptionSource<T>()
			where T : EdoTaskProblemExceptionSource
		{
			return _persistedSources.OfType<T>().FirstOrDefault();
		}

		public IEnumerable<EdoTaskProblemExceptionSource> GetEdoProblemExceptionSources()
		{
			return _persistedSources;
		}
	}
}
