﻿namespace JumpDieMeileWebApp.Persistence
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
    using JumpDieMeileWebApp.Models;
    using MySql.Data.MySqlClient;

    public class DbRelayPersistenceProvider : IPersistenceProvider
    {
        private static string RelayUri = "https://impulse-online.de/jdm_wa_db_relay/handle_post.php";
        private static string RunnerTableName = "runner_table";
        private static string RunTableName = "run_table";

        public async Task<PersistResult> PersistRunner(Runner runner)
        {
            try
            {
                var sql = @$"INSERT INTO `{RunnerTableName}` (`Id`, `ModelVersion`, `CreationTimestampUtc`, `FirstName`, `LastName`, `Username`, `Email`) VALUES
('{MySqlHelper.EscapeString(runner.Id.ToString())}',
{runner.ModelVersion},
'{MySqlHelper.EscapeString(JsonSerializer.Serialize(runner.CreationTimestampUtc).Trim('\"'))}',
'{MySqlHelper.EscapeString(runner.FirstName)}',
'{MySqlHelper.EscapeString(runner.LastName)}',
'{MySqlHelper.EscapeString(runner.Username)}',
'{MySqlHelper.EscapeString(runner.Email)}');";

                string response = await QuerySqlAsync(sql);

                if (response.Contains("Query completed successfully"))
                {
                    return new PersistResultSuccess();
                }
                else if (response.Contains("Query FAILED"))
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

        private static async Task<string> QuerySqlAsync(string sql)
        {
            using var client = new HttpClient();
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(RelayUri),
                Method = HttpMethod.Post,
            };
            request.Content = new StringContent(sql);

            var protocolVersionBytes = BitConverter.GetBytes((int)1);
            var payloadBytes = Encoding.UTF8.GetBytes(sql);
            var payloadLength = BitConverter.GetBytes(payloadBytes.Length);

            var finalPayload = protocolVersionBytes.Concat(payloadLength).Concat(payloadBytes).ToArray();

            request.Content = new ByteArrayContent(finalPayload);
            var result = await client.SendAsync(request);
            return await result.Content.ReadAsStringAsync();
        }

        public async Task<IList<Runner>> GetAllPersistedRunners()
        {
            var sql = $"SELECT * FROM {RunnerTableName}";
            var queryResult = await QuerySqlAsync(sql);

            return JsonSerializer.Deserialize<List<Runner>>(queryResult) ?? throw new Exception("Datenbank Rückgabe konnte nicht verarbeitet werden");
        }

        public Task<PersistResult> PersistRun(Run run)
        {
            return Task.FromResult((PersistResult)new PersistResultError() { ErrorMessage = "just not implemented yet" });
        }

        public async Task<IList<Run>> GetAllPersistedRuns()
        {
            var sql = $"SELECT * FROM {RunTableName}";
            var queryResult = await QuerySqlAsync(sql);

            return JsonSerializer.Deserialize<List<Run>>(queryResult) ?? throw new Exception("Datenbank Rückgabe konnte nicht verarbeitet werden");
        }
    }
}