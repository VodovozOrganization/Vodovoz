using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Core.Data.NHibernate.Repositories.Organizations;
using Vodovoz.Testing.Database;
using Xunit;

namespace Vodovoz.Core.Data.NHibernate.IntegrationTests
{
	/// <summary>
	/// Проверяет загрузку организации на настоящем маппинге: при формировании УПД трансфера читаются её ленивые поля.
	/// </summary>
	public class OrganizationRepositoryTests
	{
		/// <summary>
		/// Организация загружена в переданном UoW: версия и настройки ЭДО доступны, пока он открыт.
		/// </summary>
		[DatabaseFact]
		public async Task GetOrganizationByIdAsync_WhenLoadedInUow_LazyFieldsAvailable()
		{
			using(var database = new ProductionMappingDatabase())
			{
				var organizationId = database.CreateOrganization("Юридический адрес", "Аккаунт ЭДО");
				var repository = new OrganizationRepository(database.UnitOfWorkFactory);

				using(var uow = database.UnitOfWorkFactory.CreateWithoutRoot("Организация в сессии задачи"))
				{
					var organization = await repository.GetOrganizationByIdAsync(uow, organizationId, CancellationToken.None);

					Assert.True(uow.Session.Contains(organization));
					Assert.Equal("Юридический адрес", organization.ActiveOrganizationVersion.JurAddress);
					Assert.Equal("Аккаунт ЭДО", organization.TaxcomEdoSettings.EdoAccount);
				}
			}
		}
	}
}
