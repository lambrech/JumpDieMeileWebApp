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

        private List<Runner> lastFetchedRunners = new();

        private List<Run> lastFetchedRuns = new();

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

            var sql = $"SELECT * FROM {RunnerTableName}";
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
                    @$"INSERT INTO `{RunTableName}` (`Id`, `ModelVersion`, `CreationTimestampUtc`, `RunnerId`, `DistanceKm`, `StartTimestampUtc`, `Duration`) VALUES
('{MySqlHelper.EscapeString(run.Id.ToString())}',
{run.ModelVersion},
'{MySqlHelper.EscapeString(JsonSerializer.Serialize(run.CreationTimestampUtc).Trim('\"'))}',
'{MySqlHelper.EscapeString(run.Runner.Id.ToString())}',
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

    public class RunnerDeserializeJsonConverter : JsonConverter<Runner>
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
            Runner value,
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