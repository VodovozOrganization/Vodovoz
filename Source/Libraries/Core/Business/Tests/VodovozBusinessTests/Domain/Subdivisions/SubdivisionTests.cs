using System.ComponentModel.DataAnnotations;
using System.Linq;
using NUnit.Framework;
using Vodovoz;
using Vodovoz.Core.Domain.Organizations;
using Vodovoz.Domain.Sale;

namespace VodovozBusinessTests.Domain.Subdivisions
{
	[TestFixture(TestOf = typeof(Subdivision))]
	public class SubdivisionTests
	{
		private const string _cfoParentError = "У ЦФО не может быть вышестоящего подразделения";
		private const string _departmentParentError = "Укажите вышестоящее подразделение уровня \"ЦФО\" для отдела";
		private const string _sectorParentError = "Укажите вышестоящее подразделение уровня \"Отдел\" для сектора";
		private const string _emptyNameError = "Название подразделения должно быть заполнено.";
		private const string _shortNameNotSetError = "Укажите сокращённое название отдела.";
		private const string _nameLengthError = "Название подразделения не может превышать 45 символов";
		private const string _shortNameLengthError = "Сокращенное наименование не может превышать 45 символов";

		#region Test cases

		[Test(Description = "Если вышестоящее соответствует уровню, то ошибок нет")]
		[TestCase(SubdivisionLevel.Cfo, null)]
		[TestCase(SubdivisionLevel.Department, SubdivisionLevel.Cfo)]
		[TestCase(SubdivisionLevel.Sector, SubdivisionLevel.Department)]
		public void Validate_ParentMatchesLevel_NoErrors(SubdivisionLevel level, SubdivisionLevel? parentLevel)
		{
			//arrange
			var subdivision = CreateSubdivision(level, CreateParent(parentLevel));
			//act
			var errors = GetErrors(subdivision);
			//assert
			Assert.That(errors, Is.Empty);
		}

		[Test(Description = "Если вышестоящее не соответствует уровню, то одна ошибка уровня вышестоящего")]
		[TestCase(SubdivisionLevel.Cfo, SubdivisionLevel.Cfo, _cfoParentError)]
		[TestCase(SubdivisionLevel.Cfo, SubdivisionLevel.Department, _cfoParentError)]
		[TestCase(SubdivisionLevel.Cfo, SubdivisionLevel.Sector, _cfoParentError)]
		[TestCase(SubdivisionLevel.Department, null, _departmentParentError)]
		[TestCase(SubdivisionLevel.Department, SubdivisionLevel.Department, _departmentParentError)]
		[TestCase(SubdivisionLevel.Department, SubdivisionLevel.Sector, _departmentParentError)]
		[TestCase(SubdivisionLevel.Sector, null, _sectorParentError)]
		[TestCase(SubdivisionLevel.Sector, SubdivisionLevel.Cfo, _sectorParentError)]
		[TestCase(SubdivisionLevel.Sector, SubdivisionLevel.Sector, _sectorParentError)]
		public void Validate_ParentDoesNotMatchLevel_ReturnsParentLevelError(
			SubdivisionLevel level, SubdivisionLevel? parentLevel, string expectedError)
		{
			//arrange
			var subdivision = CreateSubdivision(level, CreateParent(parentLevel));
			//act
			var errors = GetErrors(subdivision);
			//assert
			Assert.That(errors, Is.EqualTo(new[] { expectedError }));
		}

		[Test(Description = "Архивный сектор под сектором не проходит проверку вышестоящего")]
		public void Validate_ArchivedSectorUnderSector_ReturnsSectorParentError()
		{
			//arrange
			var subdivision = CreateSubdivision(SubdivisionLevel.Sector, CreateSubdivision(SubdivisionLevel.Sector));
			subdivision.Name = "old_Сектор";
			subdivision.IsArchive = true;
			//act
			var errors = GetErrors(subdivision);
			//assert
			Assert.That(errors, Is.EqualTo(new[] { _sectorParentError }));
		}

		[Test(Description = "Если название пустое, то ошибка названия")]
		public void Validate_EmptyName_ReturnsNameError()
		{
			//arrange
			var subdivision = CreateValidDepartment();
			subdivision.Name = "";
			//act
			var errors = GetErrors(subdivision);
			//assert
			Assert.That(errors, Is.EqualTo(new[] { _emptyNameError }));
		}

		[Test(Description = "Если краткое наименование не задано, то ошибка краткого наименования")]
		public void Validate_ShortNameNotSet_ReturnsShortNameError()
		{
			//arrange
			var subdivision = CreateValidDepartment();
			subdivision.ShortName = null;
			//act
			var errors = GetErrors(subdivision);
			//assert
			Assert.That(errors, Is.EqualTo(new[] { _shortNameNotSetError }));
		}

		[Test(Description = "Если краткое наименование не длиннее 45 символов, то ошибок нет")]
		[TestCase(0)]
		[TestCase(44)]
		[TestCase(45)]
		public void Validate_ShortNameUpToMaxLength_NoErrors(int length)
		{
			//arrange
			var subdivision = CreateValidDepartment();
			subdivision.ShortName = new string('а', length);
			//act
			var errors = GetErrors(subdivision);
			//assert
			Assert.That(errors, Is.Empty);
		}

		[Test(Description = "Если краткое наименование длиннее 45 символов, то ошибка длины")]
		public void Validate_ShortNameLongerThanMaxLength_ReturnsLengthError()
		{
			//arrange
			var subdivision = CreateValidDepartment();
			subdivision.ShortName = new string('а', 46);
			//act
			var errors = GetErrors(subdivision);
			//assert
			Assert.That(errors, Is.EqualTo(new[] { _shortNameLengthError }));
		}

		[Test(Description = "Если название не длиннее 45 символов, то ошибок нет")]
		[TestCase(44)]
		[TestCase(45)]
		public void Validate_NameUpToMaxLength_NoErrors(int length)
		{
			//arrange
			var subdivision = CreateValidDepartment();
			subdivision.Name = new string('а', length);
			//act
			var errors = GetErrors(subdivision);
			//assert
			Assert.That(errors, Is.Empty);
		}

		[Test(Description = "Если название длиннее 45 символов, то ошибка длины")]
		public void Validate_NameLongerThanMaxLength_ReturnsLengthError()
		{
			//arrange
			var subdivision = CreateValidDepartment();
			subdivision.Name = new string('а', 46);
			//act
			var errors = GetErrors(subdivision);
			//assert
			Assert.That(errors, Is.EqualTo(new[] { _nameLengthError }));
		}

		[Test(Description = "Если сектор под отделом с частью города, то у сектора часть города отдела")]
		public void CopyGeographicGroupFromParentDepartment_SectorUnderDepartmentWithGeoGroup_CopiesDepartmentGeoGroup()
		{
			//arrange
			var north = new GeoGroup { Id = 1, Name = "Север" };
			var south = new GeoGroup { Id = 2, Name = "Юг" };
			var department = CreateSubdivision(SubdivisionLevel.Department);
			department.GeographicGroup = north;
			var sector = CreateSubdivision(SubdivisionLevel.Sector, department);
			sector.GeographicGroup = south;
			//act
			sector.CopyGeographicGroupFromParentDepartment();
			//assert
			Assert.That(sector.GeographicGroup, Is.SameAs(north));
		}

		[Test(Description = "Если сектор под отделом без части города, то у сектора часть города очищается")]
		public void CopyGeographicGroupFromParentDepartment_SectorUnderDepartmentWithoutGeoGroup_ClearsSectorGeoGroup()
		{
			//arrange
			var south = new GeoGroup { Id = 2, Name = "Юг" };
			var department = CreateSubdivision(SubdivisionLevel.Department);
			var sector = CreateSubdivision(SubdivisionLevel.Sector, department);
			sector.GeographicGroup = south;
			//act
			sector.CopyGeographicGroupFromParentDepartment();
			//assert
			Assert.That(sector.GeographicGroup, Is.Null);
		}

		[Test(Description = "Если вышестоящее не отдел или подразделение не сектор, то часть города не меняется")]
		[TestCase(SubdivisionLevel.Sector, SubdivisionLevel.Cfo)]
		[TestCase(SubdivisionLevel.Sector, null)]
		[TestCase(SubdivisionLevel.Department, SubdivisionLevel.Cfo)]
		public void CopyGeographicGroupFromParentDepartment_ParentIsNotDepartment_KeepsGeoGroup(
			SubdivisionLevel level, SubdivisionLevel? parentLevel)
		{
			//arrange
			var north = new GeoGroup { Id = 1, Name = "Север" };
			var south = new GeoGroup { Id = 2, Name = "Юг" };
			var parent = CreateParent(parentLevel);

			if(parent != null)
			{
				parent.GeographicGroup = north;
			}

			var subdivision = CreateSubdivision(level, parent);
			subdivision.GeographicGroup = south;
			//act
			subdivision.CopyGeographicGroupFromParentDepartment();
			//assert
			Assert.That(subdivision.GeographicGroup, Is.SameAs(south));
		}

		#endregion Test cases

		private static Subdivision CreateSubdivision(SubdivisionLevel level, Subdivision parent = null)
		{
			return new Subdivision
			{
				Name = "Подразделение",
				ShortName = "Подр",
				Level = level,
				ParentSubdivision = parent
			};
		}

		private static Subdivision CreateParent(SubdivisionLevel? parentLevel)
		{
			return parentLevel.HasValue ? CreateSubdivision(parentLevel.Value) : null;
		}

		private static Subdivision CreateValidDepartment()
		{
			return CreateSubdivision(SubdivisionLevel.Department, CreateSubdivision(SubdivisionLevel.Cfo));
		}

		private static string[] GetErrors(Subdivision subdivision)
		{
			return subdivision.Validate(new ValidationContext(subdivision)).Select(x => x.ErrorMessage).ToArray();
		}
	}
}
