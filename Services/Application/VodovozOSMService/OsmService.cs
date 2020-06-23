using System;
using System.Collections.Generic;
using System.Linq;
using Nini.Config;
using NLog;
using Npgsql;
using QS.Osm;
using QS.Osm.DTO;

namespace VodovozOSMService
{
	public class OsmService : IOsmService
	{
		private static string pgserver;
		private static string pguser;
		private static string pgpass;
		private static string pgdb;

		Logger logger = LogManager.GetCurrentClassLogger();

		static NpgsqlConnection GetPostgisConnection()
		{
			return new NpgsqlConnection(PostgresConnectionString);
		}

		public static string PostgresConnectionString {
			get {
				return String.Format("Host={0};Username={1};Password={2};Database={3}",
					pgserver, pguser, pgpass, pgdb);
			}
		}

		public static void ConfigureService(IniConfigSource configFile)
		{
			IConfig config = configFile.Configs["OsmService"];
			pgserver = config.GetString("pgserver");
			pguser = config.GetString("pguser");
			pgpass = config.GetString("pgpassword");
			pgdb = config.GetString("pgdatabase");
		}

		public bool ServiceStatus()
		{
			try {
				//TODO Возможно стоит добавить проверку дополнительных методов службы, по необходимости

				var cities = GetCities();
				return cities.Any();
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при проверке работоспособности службы OSM");
				return false;
			}
		}

		public List<OsmCity> GetCities()
		{
			var cities = new List<OsmCity>();

			try {
				using(var connection = GetPostgisConnection()) {
					connection.Open();
					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;

						// Insert some data
						cmd.CommandText = "SELECT osm_cities.id, osm_cities.name, osm_cities.place, osm_suburb_districts.name " +
						"FROM osm_cities " +
						"LEFT JOIN osm_suburb_districts ON osm_suburb_districts.id = osm_cities.suburb_district_id " +
						"AND osm_cities.name IS NOT NULL AND osm_cities.name <> '';";
						using(var reader = cmd.ExecuteReader()) {
							while(reader.Read()) {
								long id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
								string name = reader.IsDBNull(1) ? String.Empty : reader.GetString(1);
								string locality = reader.IsDBNull(2) ? String.Empty : reader.GetString(2);
								string district = reader.IsDBNull(3) ? String.Empty : reader.GetString(3);
								cities.Add(new OsmCity(id, name, district, AddressHelper.GetLocalityTypeByName(locality)));
							}
						}
					}
				}
				cities.Sort(new OsmCityComparer());
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при получении списка городов.");
			}
			return cities;
		}

		public List<OsmCity> GetCitiesByCriteria(string searchString, int limit)
		{
			logger.Info("загрузка городов по критерию");
			var cities = new List<OsmCity>();

			try {
				using(var connection = GetPostgisConnection()) {
					connection.Open();
					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;

						// Insert some data
						cmd.CommandText = "SELECT osm_cities.id, osm_cities.name, osm_cities.place, osm_suburb_districts.name " +
							"FROM osm_cities " +
							"LEFT JOIN osm_suburb_districts ON osm_suburb_districts.id = osm_cities.suburb_district_id " +
							"WHERE osm_cities.name IS NOT NULL " +
							"AND osm_cities.name <> ''" +
							$"AND osm_cities.name ILIKE '%{searchString.ToLower()}%' " +
							"LIMIT @limit;";
						cmd.Parameters.AddWithValue("@limit", limit);

						using(var reader = cmd.ExecuteReader()) {
							while(reader.Read()) {
								long id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
								string name = reader.IsDBNull(1) ? String.Empty : reader.GetString(1);
								string locality = reader.IsDBNull(2) ? String.Empty : reader.GetString(2);
								string district = reader.IsDBNull(3) ? String.Empty : reader.GetString(3);
								cities.Add(new OsmCity(id, name, district, AddressHelper.GetLocalityTypeByName(locality)));
							}
						}
					}
				}
				logger.Info($"городов загруженно {cities.Count}");
				cities.Sort(new OsmCityComparer());
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при получении списка городов.");
			}
			return cities;
		}

		public List<OsmStreet> GetStreets(long cityId)
		{
			var streets = new List<OsmStreet>();

			try {
				using(var connection = GetPostgisConnection()) {
					connection.Open();
					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;

						// Insert some data
						cmd.CommandText = "SELECT osm_streets.id, osm_streets.name, string_agg(osm_city_districts.name, ',') " +
						"FROM osm_streets " +
						"LEFT JOIN osm_cities ON osm_cities.id = osm_streets.city_id " +
						"LEFT JOIN osm_streets_to_districts ON osm_streets_to_districts.street_id = osm_streets.id " +
						"LEFT JOIN osm_city_districts ON osm_streets_to_districts.district_id = osm_city_districts.id " +
						"WHERE osm_cities.id = @city_id GROUP BY osm_streets.id, osm_streets.name;";
						cmd.Parameters.AddWithValue("@city_id", cityId);
						using(var reader = cmd.ExecuteReader()) {
							while(reader.Read()) {
								long id = reader.GetInt64(0);
								string name = reader.IsDBNull(1) ? String.Empty : reader.GetString(1);
								string districts = reader.IsDBNull(2) ? String.Empty : reader.GetString(2);
								streets.Add(new OsmStreet(id, cityId, name, districts));
							}
						}
					}
				}
				streets.Sort(new OsmStreetComparer());

			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при получении улиц города с osm_id={0}.", cityId);
			}
			return streets;
		}

		public List<OsmStreet> GetStreetsByCriteria(long cityId, string searchString, int limit)
		{
			var streets = new List<OsmStreet>();

			try {
				using(var connection = GetPostgisConnection()) {
					connection.Open();
					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;
						// Insert some data
						cmd.CommandText = "SELECT osm_streets.id, osm_streets.name, string_agg(osm_city_districts.name, ',') " +
						"FROM osm_streets " +
						"LEFT JOIN osm_cities ON osm_cities.id = osm_streets.city_id " +
						"LEFT JOIN osm_streets_to_districts ON osm_streets_to_districts.street_id = osm_streets.id " +
						"LEFT JOIN osm_city_districts ON osm_streets_to_districts.district_id = osm_city_districts.id " +
						"WHERE osm_cities.id = @city_id " +
						$"AND osm_streets.name ILIKE '%{searchString.ToLower()}%' " +
						"GROUP BY osm_streets.id, osm_streets.name;";
						//"LIMIT @limit;";
						cmd.Parameters.AddWithValue("@city_id", cityId);
						cmd.Parameters.AddWithValue("@limit", limit);
						using(var reader = cmd.ExecuteReader()) {
							while(reader.Read()) {
								long id = reader.GetInt64(0);
								string name = reader.IsDBNull(1) ? String.Empty : reader.GetString(1);
								string districts = reader.IsDBNull(2) ? String.Empty : reader.GetString(2);
								streets.Add(new OsmStreet(id, cityId, name, districts));
							}
						}
					}
				}
				streets.Sort(new OsmStreetComparer());

			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при получении улиц города с osm_id={0}.", cityId);
			}
			return streets;
		}

		public long GetCityId(string City, string District, string Locality)
		{
			long id = new long();
			try {
				using(var connection = GetPostgisConnection()) {
					connection.Open();
					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;
						cmd.CommandText = "SELECT osm_cities.id " +
						"FROM osm_cities " +
						"LEFT JOIN osm_suburb_districts ON osm_suburb_districts.id = osm_cities.suburb_district_id " +
						"WHERE osm_cities.name = @name " +
						//Locality query
						(String.IsNullOrWhiteSpace(Locality) ?
							"AND osm_cities.place IS NULL " :
							"AND osm_cities.place = @place ") +
						//District query
						(String.IsNullOrWhiteSpace(District) ?
							"AND osm_suburb_districts.name IS NULL;" :
							"AND osm_suburb_districts.name = @district;");
						cmd.Parameters.AddWithValue("@name", City);
						if(!String.IsNullOrWhiteSpace(Locality))
							cmd.Parameters.AddWithValue("@place", Locality);
						if(!String.IsNullOrWhiteSpace(District))
							cmd.Parameters.AddWithValue("@district", District);
						using(var reader = cmd.ExecuteReader()) {
							if(reader.Read()) {
								id = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
							}
						}
					}
				}
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при получении osm_id города с параметрами name={0}, district={1}, place={2}.", City, District, Locality.ToString());
			}
			return id;
		}

		public string GetPointRegion(long OsmId)
		{
			string region = String.Empty;
			bool SPb = false;
			bool LO = false;
			try {
				using(var connection = GetPostgisConnection()) {
					connection.Open();
					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;
						cmd.CommandText = "SELECT " +
						"ST_Contains((SELECT way FROM planet_osm_polygon WHERE admin_level = '4' AND boundary = 'administrative' AND name = 'Санкт-Петербург'), way) AS SPb, " +
						"ST_Contains((SELECT way FROM planet_osm_polygon WHERE admin_level = '4' AND boundary = 'administrative' AND name = 'Ленинградская область'), way) AS LO " +
						"FROM planet_osm_polygon " +
						"WHERE osm_id = @osm_id;";
						cmd.Parameters.AddWithValue("@osm_id", OsmId);
						using(var reader = cmd.ExecuteReader()) {
							if(reader.Read()) {
								SPb = reader.GetBoolean(0);
								LO = reader.GetBoolean(1);
							}
						}
					}
				}
				if(SPb)
					region = "Санкт-Петербург";
				else if(LO)
					region = "Ленинградская область";
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при получении региона точки с osm_id={0}.", OsmId);
			}
			return region;
		}

		public List<OsmHouse> GetHouseNumbersWithoutDistrict(long cityId, string street)
		{
			return GetHouseNumbers(cityId, street, String.Empty);
		}

		public List<OsmHouse> GetHouseNumbers(long cityId, string street, string districts)
		{
			var houses = new List<OsmHouse>();
			string city = String.Empty;
			street = street.Replace('+', ' ');
			districts = districts.Replace('+', ' ');

			string[] districtsArray = districts.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

			try {
				using(var connection = GetPostgisConnection()) {
					connection.Open();
					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;
						cmd.CommandText = "SELECT name FROM osm_cities WHERE id = @id;";
						cmd.Parameters.AddWithValue("@id", cityId);
						using(var reader = cmd.ExecuteReader()) {
							if(reader.Read()) {
								city = reader.IsDBNull(0) ? String.Empty : reader.GetString(0);
							}
						}
					}

					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;
						cmd.CommandText = "SELECT DISTINCT ON (\"addr:housenumber\", tags->'addr:letter') " +
						"osm_id, \"addr:housenumber\", " +
						"tags->'addr:letter' as letter, name, " +
						"ST_X(ST_Centroid(ST_Transform(way, 4674))) AS X, " +
						"ST_Y(ST_Centroid(ST_Transform(way, 4674))) AS Y " +
						"FROM planet_osm_polygon " +
						"WHERE \"addr:housenumber\" IS NOT NULL " +
						"AND \"addr:housenumber\" <> '' " +
						"AND tags->'addr:city' = @city " +
						"AND tags->'addr:street' = @street " +
						"AND EXISTS(SELECT 1 FROM osm_city_ways WHERE city_id = @city_id AND ST_Contains (osm_city_ways.way, planet_osm_polygon.way)) " +
						(districtsArray.Length < 1 ?
								";" :
							"AND tags->'addr:district' IN (");
						cmd.Parameters.AddWithValue("@city", city);
						cmd.Parameters.AddWithValue("@street", street);
						cmd.Parameters.AddWithValue("@city_id", cityId);

						for(int i = 0; i < districtsArray.Length; i++) {
							if(i == 0) {
								cmd.CommandText += String.Format("@district{0}", i);
							} else {
								cmd.CommandText += String.Format(",@district{0}", i);
							}
							cmd.Parameters.AddWithValue(String.Format("@district{0}", i), districtsArray[i]);
						}
						if(districtsArray.Length > 0)
							cmd.CommandText += ");";

						using(var reader = cmd.ExecuteReader()) {
							while(reader.Read()) {
								houses.Add(new OsmHouse(
									reader.GetInt64(0),
									reader.IsDBNull(1) ? String.Empty : reader.GetString(1),
									reader.IsDBNull(2) ? String.Empty : reader.GetString(2),
									reader.IsDBNull(3) ? String.Empty : reader.GetString(3),
									(Decimal)reader.GetDouble(4),
									(Decimal)reader.GetDouble(5)
								));
							}
						}
					}
				}
			}
			catch(Exception ex) {
				logger.Error(ex, "Ошибка при получении номеров домов с city_osm_id={0}, street={1}, district={2}.",
					cityId, street, districts);
			}
			//houses.Sort (new StringWorks.NaturalStringComparerNonStatic ());
			return houses;
		}

		public long GetBuildingCountInCity(string city)
		{
			long result = 0;
			try {
				using(var connection = GetPostgisConnection()) {
					connection.Open();

					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;
						cmd.CommandText = "SELECT COUNT(*) " +
							"FROM planet_osm_polygon " +
							"WHERE \"addr:housenumber\" <> '' " +
							"AND tags->'addr:city' = @city ";
						cmd.Parameters.AddWithValue("@city", city);

						using(var reader = cmd.ExecuteReader()) {
								if(reader.Read())
									result = reader.GetInt64(0);
						}
					}
				}
			}
			catch(Exception ex) {
				logger.Error(ex, $"Ошибка при получении кол-ва домов в {city}");
			}
			return result;
		}

		public long GetBuildingCountInRegion(string region)
		{
			long result = 0;
			try {
				using(var connection = GetPostgisConnection()) {
					connection.Open();

					using(var cmd = new NpgsqlCommand()) {
						cmd.Connection = connection;
						cmd.CommandTimeout = 200;
						cmd.CommandText = "SELECT COUNT(*) " +
							"FROM planet_osm_polygon " +
							"WHERE \"addr:housenumber\" <> '' " +
							"AND ST_Contains((" +
								"SELECT region.way " +
								"FROM planet_osm_polygon AS region " +
								"WHERE region.admin_level = '4' AND region.boundary = 'administrative' " +
								"AND region.name = @region LIMIT 1), " +
							"planet_osm_polygon.way) ;";
						cmd.Parameters.AddWithValue("@region", region);

						using(var reader = cmd.ExecuteReader()) {
							if(reader.Read())
								result = reader.GetInt64(0);
						}
					}
				}
			}
			catch(Exception ex) {
				logger.Error(ex, $"Ошибка при получении кол-ва домов в {region}");
			}
			return result;
		}
		
	}
}

