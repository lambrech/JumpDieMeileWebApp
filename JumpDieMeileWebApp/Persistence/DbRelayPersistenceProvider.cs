namespace JumpDieMeileWebApp.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Models;
    using MySql.Data.MySqlClient;

    public class DbRelayPersistenceProvider : IPersistenceProvider
    {
        private static readonly string RelayUri = "https://impulse-online.de/jdm_wa_db_relay/handle_post.php";

        private static readonly string RunnerTableName = "runner_table";

        private static readonly string RunTableName = "run_table";

        private static readonly string SponsoringTableName = "sponsoring_table";

        private List<Runner> lastFetchedRunners = new();

        private List<Run> lastFetchedRuns = new();

        private List<SponsoringEntry> lastFetchedSponsoringEntries = new();

        public async Task<PersistResult> PersistRunner(Runner runner)
        {
            try
            {
                var sql = @$"INSERT INTO `{RunnerTableName}` (`Id`, `ModelVersion`, `CreationTimestampUtc`, `FirstName`, `LastName`, `Username`, `Email`, `Location`, `Postcode`, `StreetHouseNr`, `Gender`, `Comment`) VALUES
('{MySqlHelper.EscapeString(runner.Id.ToString())}',
{runner.ModelVersion},
'{MySqlHelper.EscapeString(JsonSerializer.Serialize(runner.CreationTimestampUtc).Trim('\"'))}',
'{MySqlHelper.EscapeString(runner.FirstName)}',
'{MySqlHelper.EscapeString(runner.LastName)}',
'{MySqlHelper.EscapeString(runner.Username)}',
'{MySqlHelper.EscapeString(runner.Email)}',
'{MySqlHelper.EscapeString(runner.Location)}',
'{MySqlHelper.EscapeString(runner.Postcode)}',
'{MySqlHelper.EscapeString(runner.StreetHouseNr)}',
'{(runner.Gender.HasValue ? (int)runner.Gender.Value : "NULL")}',
'{MySqlHelper.EscapeString(runner.Comment)}');";

                var response = await QuerySqlAsync(sql);

                if (response.Contains("Query completed successfully"))
                {
                    return new PersistResultSuccess();
                }

                if (response.Contains("Query FAILED"))
                {
                    return new PersistResultError { ErrorMessage = "Fehler beim speichern." };
                }
            }
            catch (Exception e)
            {
                return new PersistResultError { ErrorMessage = e.ToString() };
            }

            return new PersistResultError { ErrorMessage = "Unexpected error" };
        }

        public async Task<IList<Runner>> GetAllPersistedRunners()
        {
            var sqlCount = $"SELECT Count(*) as `count` FROM {RunnerTableName}";
            var queryResultCount = await QuerySqlAsync(sqlCount);

            var currentCount = JsonSerializerExtensions.DeserializeAnonymousType(queryResultCount.Trim().TrimStart('[').TrimEnd(']'), new { count = 0 })?.count;

            if (currentCount == this.lastFetchedRunners.Count)
            {
                return this.lastFetchedRunners;
            }

            var sql = $"SELECT `Id`, `ModelVersion`, `CreationTimestampUtc`, `Username` FROM {RunnerTableName}";
            var queryResult = await QuerySqlAsync(sql);

            var list = JsonSerializer.Deserialize<List<Runner>>(
                queryResult,
                new JsonSerializerOptions { Converters = { new StringDeserializeJsonConverter() } });
            if (list != null)
            {
                this.lastFetchedRunners = list;
                return list;
            }

#pragma warning disable CA2201 // Do not raise reserved exception types
            throw new Exception("Datenbank Rückgabe konnte nicht verarbeitet werden");
#pragma warning restore CA2201 // Do not raise reserved exception types
        }

        public async Task<PersistResult> PersistRun(Run run)
        {
            try
            {
                var sql =
                    @$"INSERT INTO `{RunTableName}` (`Id`, `ModelVersion`, `CreationTimestampUtc`, `Runner`, `DistanceKm`, `StartTimestampUtc`, `Duration`) VALUES
('{MySqlHelper.EscapeString(run.Id.ToString())}',
{run.ModelVersion},
'{MySqlHelper.EscapeString(JsonSerializer.Serialize(run.CreationTimestampUtc).Trim('\"'))}',
'{MySqlHelper.EscapeString(run.Runner?.Id.ToString())}',
{run.DistanceKm.ToString(CultureInfo.InvariantCulture)},
'{MySqlHelper.EscapeString(JsonSerializer.Serialize(run.StartTimestampUtc).Trim('\"'))}',
{run.Duration?.Ticks.ToString(CultureInfo.InvariantCulture) ?? "NULL"});";

				var response = await QuerySqlAsync(sql);

                if (response.Contains("Query completed successfully"))
                {
                    return new PersistResultSuccess();
                }

                if (response.Contains("Query FAILED"))
                {
                    return new PersistResultError { ErrorMessage = "Fehler beim speichern." };
                }
            }
            catch (Exception e)
            {
                return new PersistResultError { ErrorMessage = e.ToString() };
            }

            return new PersistResultError { ErrorMessage = "Unexpected error" };
        }

        public async Task<IList<Run>> GetAllPersistedRuns()
        {
            var sqlCount = $"SELECT Count(*) as `count` FROM {RunTableName}";
            var queryResultCount = await QuerySqlAsync(sqlCount);

            var currentCount = JsonSerializerExtensions.DeserializeAnonymousType(queryResultCount.Trim().TrimStart('[').TrimEnd(']'), new { count = 0 })?.count;

            if (currentCount == this.lastFetchedRunners.Count)
            {
                return this.lastFetchedRuns;
            }

            var sql = $"SELECT * FROM {RunTableName}";
            var queryResult = await QuerySqlAsync(sql);

            var currentRunners = await this.GetAllPersistedRunners();


            var converter = new RunnerDeserializeJsonConverter(currentRunners);

            var list = JsonSerializer.Deserialize<List<Run>>(
                queryResult,
                new JsonSerializerOptions { Converters = { converter, new TimeSpanDeserializeJsonConverter(), new StringDeserializeJsonConverter() } });

            if (list != null)
            {
                this.lastFetchedRuns = list;
                return list;
            }

#pragma warning disable CA2201 // Do not raise reserved exception types
            throw new Exception("Datenbank Rückgabe konnte nicht verarbeitet werden");
#pragma warning restore CA2201 // Do not raise reserved exception types
        }

        public async Task<PersistResult> PersistSponsoringEntry(SponsoringEntry sponsoringEntry)
        {
            try
            {
                var sql = @$"INSERT INTO `{SponsoringTableName}` (`Id`, `ModelVersion`, `CreationTimestampUtc`, `FirstName`, `LastName`, `Email`, `Location`, `Postcode`, `StreetHouseNr`, `Gender`, `SponsoringMode`, `SponsoredRunner`, `ImmediateInEuro`, `PerKmInEuro`) VALUES
('{MySqlHelper.EscapeString(sponsoringEntry.Id.ToString())}',
{sponsoringEntry.ModelVersion},
'{MySqlHelper.EscapeString(JsonSerializer.Serialize(sponsoringEntry.CreationTimestampUtc).Trim('\"'))}',
'{MySqlHelper.EscapeString(sponsoringEntry.FirstName)}',
'{MySqlHelper.EscapeString(sponsoringEntry.LastName)}',
'{MySqlHelper.EscapeString(sponsoringEntry.Email)}',
'{MySqlHelper.EscapeString(sponsoringEntry.Location)}',
'{MySqlHelper.EscapeString(sponsoringEntry.Postcode)}',
'{MySqlHelper.EscapeString(sponsoringEntry.StreetHouseNr)}',
'{(sponsoringEntry.Gender.HasValue ? (int)sponsoringEntry.Gender.Value : "NULL")}',
'{(sponsoringEntry.SponsoringMode.HasValue ? (int)sponsoringEntry.SponsoringMode.Value : "NULL")}',
{(sponsoringEntry.SponsoredRunner != null ? $"'{MySqlHelper.EscapeString(sponsoringEntry.SponsoredRunner.Id.ToString())}'" : "NULL")},
{(sponsoringEntry.ImmediateInEuro.HasValue ? sponsoringEntry.ImmediateInEuro.Value.ToString(CultureInfo.InvariantCulture) : "NULL")},
{(sponsoringEntry.PerKmInEuro.HasValue ? sponsoringEntry.PerKmInEuro.Value.ToString(CultureInfo.InvariantCulture) : "NULL")});";

                var response = await QuerySqlAsync(sql);

                if (response.Contains("Query completed successfully"))
                {
                    return new PersistResultSuccess();
                }

                if (response.Contains("Query FAILED"))
                {
                    return new PersistResultError { ErrorMessage = "Fehler beim speichern." };
                }
            }
            catch (Exception e)
            {
                return new PersistResultError { ErrorMessage = e.ToString() };
            }

            return new PersistResultError { ErrorMessage = "Unexpected error" };
        }

        public async Task<IList<SponsoringEntry>> GetAllPersistedSponsoringEntries()
        {
            var sqlCount = $"SELECT Count(*) as `count` FROM {SponsoringTableName}";
            var queryResultCount = await QuerySqlAsync(sqlCount);

            var currentCount = JsonSerializerExtensions.DeserializeAnonymousType(queryResultCount.Trim().TrimStart('[').TrimEnd(']'), new { count = 0 })?.count;

            if (currentCount == this.lastFetchedSponsoringEntries.Count)
            {
                return this.lastFetchedSponsoringEntries;
            }

            var sql = $"SELECT `Id`, `ModelVersion`, `CreationTimestampUtc`, `SponsoringMode`, `SponsoredRunner`, `ImmediateInEuro`, `PerKmInEuro` FROM {SponsoringTableName}";
            var queryResult = await QuerySqlAsync(sql);

            var currentRunners = await this.GetAllPersistedRunners();


            var converter = new RunnerDeserializeJsonConverter(currentRunners);

            var list = JsonSerializer.Deserialize<List<SponsoringEntry>>(
                queryResult,
                new JsonSerializerOptions { Converters = { converter, new TimeSpanDeserializeJsonConverter(), new StringDeserializeJsonConverter() } });

            if (list != null)
            {
                this.lastFetchedSponsoringEntries = list;
                return list;
            }

#pragma warning disable CA2201 // Do not raise reserved exception types
            throw new Exception("Datenbank Rückgabe konnte nicht verarbeitet werden");
#pragma warning restore CA2201 // Do not raise reserved exception types
        }

        public async Task<decimal> GetDistanceSumOfAllRuns()
        {
            try
            {
                var sqlCount = $"SELECT Count(*) as `count` FROM {RunTableName}";
                var queryResultCount = await QuerySqlAsync(sqlCount);

                var currentCount = JsonSerializerExtensions.DeserializeAnonymousType(queryResultCount.Trim().TrimStart('[').TrimEnd(']'), new { count = 0 })
                                                          ?.count;

                if (currentCount == 0)
                {
                    return 0;
                }

                var runsSumSql = $"SELECT SUM(DistanceKm) as `sum` FROM {RunTableName}";
                var queryResultSum = await QuerySqlAsync(runsSumSql);

                var runsSum = JsonSerializerExtensions.DeserializeAnonymousType(queryResultSum.Trim().TrimStart('[').TrimEnd(']'), new { sum = (decimal)0 })
                                                     ?.sum;

                return runsSum ?? throw new Exception("Laden der Summe aller Läufe fehlgeschlagen");
            }
            catch (Exception e)
            {
                throw new Exception("Laden der Summe aller Läufe fehlgeschlagen", e);
            }
        }

        private static async Task<string> QuerySqlAsync(string sql)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(RelayUri),
                Method = HttpMethod.Post
            };
            request.Content = new StringContent(sql);

            var protocolVersionBytes = BitConverter.GetBytes(1);
            var payloadBytes = Encoding.UTF8.GetBytes(sql);
            var payloadLength = BitConverter.GetBytes(payloadBytes.Length);

            var finalPayload = protocolVersionBytes.Concat(payloadLength).Concat(payloadBytes).ToArray();

            request.Content = new ByteArrayContent(finalPayload);
            var result = await client.SendAsync(request);
            return await result.Content.ReadAsStringAsync();
        }
    }

    public static class JsonSerializerExtensions
    {
        public static T? DeserializeAnonymousType<T>(string json, T _, JsonSerializerOptions? options = default)
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }

    public class RunnerDeserializeJsonConverter : JsonConverter<Runner?>
    {
        private readonly IList<Runner> availableRunners;

        public RunnerDeserializeJsonConverter(IList<Runner> availableRunners)
        {
            this.availableRunners = availableRunners;
        }

        public override Runner? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var guidString = reader.GetString();
            return guidString != null ? this.availableRunners.FirstOrDefault(x => x.Id == Guid.Parse(guidString)) : null;
        }

        public override void Write(
            Utf8JsonWriter writer,
            Runner? value,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        //writer.WriteStringValue(dateTimeValue.ToString(
        //    "MM/dd/yyyy", CultureInfo.InvariantCulture));
    }

    public class TimeSpanDeserializeJsonConverter : JsonConverter<TimeSpan?>
    {
        public override TimeSpan? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var timespanTicks = reader.GetInt64();
            return TimeSpan.FromTicks(timespanTicks);
        }

        public override void Write(
            Utf8JsonWriter writer,
            TimeSpan? value,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
        //writer.WriteStringValue(dateTimeValue.ToString(
        //    "MM/dd/yyyy", CultureInfo.InvariantCulture));
    }

    public class StringDeserializeJsonConverter : JsonConverter<string?>
    {
        public override string? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                var value = reader.GetInt32();
                return value.ToString();
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString();
            }

            if (reader.TokenType == JsonTokenType.True)
            {
                return "true";
            }

            if (reader.TokenType == JsonTokenType.False)
            {
                return "false";
            }

            throw new JsonException();
        }

        public override void Write(
            Utf8JsonWriter writer,
            string? dateTimeValue,
            JsonSerializerOptions options) => throw new NotImplementedException();
        //writer.WriteStringValue(dateTimeValue.ToString(
        //    "MM/dd/yyyy", CultureInfo.InvariantCulture));
    }
}