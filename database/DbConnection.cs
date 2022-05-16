﻿using Npgsql;
using Serilog;
using System.Text;
using Dasync.Collections;

namespace _26tack_rewritten.database;
internal abstract class DbConnection
{
    public HttpClient Requests { get; } = new HttpClient();

    private const string ConnectionString = $"Host={Config.Host};Username={Config.DbUsername};Password={Config.Password};Database={Config.DatabaseName}";
    private NpgsqlConnection Connection { get; } = new NpgsqlConnection(ConnectionString);

    private QueryTypes? QueryType { get; set; } = null;
    private string? TableName { get; set; } = null;
    private string? SortMethod { get; set; } = null;
    private string[]? SelectedValues { get; set; } = null;
    private string[]? ValuesSchema { get; set; } = null;
    private string[]? Conditions { get; set; } = null;
    private int? SelectionLimit { get; set; } = null;
    private int? SelectionOffset { get; set; } = null;

    protected DbConnection() { Connection.Open(); }

    public DbConnection Insert() { QueryType = QueryTypes.Insert; return this; }
    public DbConnection Update() { QueryType = QueryTypes.Update; return this; }
    public DbConnection Delete() { QueryType = QueryTypes.Delete; return this; }
    public DbConnection Select() { QueryType = QueryTypes.Select; return this; }
    public DbConnection Table(string table) { TableName = table; return this; }
    public DbConnection Sort(string sortingMethod) { SortMethod = sortingMethod; return this; }
    public DbConnection Values(params string[] values) { SelectedValues = values; return this; }
    public DbConnection Schema(params string[] schema) { ValuesSchema = schema; return this; }
    public DbConnection Where(params string[] where) { Conditions = where; return this; }
    public DbConnection Limit(int limit) { SelectionLimit = limit; return this; }
    public DbConnection Offset(int offset) { SelectionOffset = offset; return this; }

    public async Task<ExecutionResult> TryExecute()
    {
        string? query = BuildQueryString();
        Log.Verbose(query ?? "null query");
        if (query is null) return new ExecutionResult(false, null);
        NpgsqlCommand cmd = new NpgsqlCommand(query, Connection);

        if (QueryType == QueryTypes.Insert || QueryType == QueryTypes.Update || QueryType == QueryTypes.Delete)
        {
            try
            {
                await cmd.ExecuteNonQueryAsync();
                return new ExecutionResult(true, null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception was thrown during {QueryType} query");
            }
        }
        if (QueryType == QueryTypes.Select)
        {
            try
            {
                NpgsqlDataReader r = await cmd.ExecuteReaderAsync();
                List<int> ordinals = new List<int>();
                while (await r.ReadAsync())
                {
                    foreach (var column in ValuesSchema!) { ordinals.Add(r.GetOrdinal(column)); }
                    break;
                }
                await r.CloseAsync();
                
                NpgsqlDataReader r2 = await cmd.ExecuteReaderAsync();
                List<object[]> values = new List<object[]>();
                List<object> valuesInner = new List<object>();
                while (await r2.ReadAsync())
                {
                    foreach (int o in ordinals) { valuesInner.Add(r.GetValue(o)); }
                    values.Add(valuesInner.ToArray());
                    valuesInner.Clear();
                }
                await r2.CloseAsync();

                return new ExecutionResult(true, values.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception was thrown during {QueryType} query");
            }
        }
        return new ExecutionResult(false, null);
    }

    private string? BuildQueryString()
    {
        if (!ValidQuery()) return null;
        StringBuilder qs = new StringBuilder();

        switch (QueryType)
        {
            case QueryTypes.Insert:
                string sv = string.Join(", ", SelectedValues!);
                qs.Append($"INSERT INTO {TableName} ")
                    .Append($"({string.Join(", ", SelectedValues!)}) ")
                    .Append(sv.Contains('(') ? $"{sv}" : $"({sv})");
                break;

            case QueryTypes.Update:
                qs.Append($"UPDATE {TableName} ")
                    .Append($"SET {string.Join(", ", ValuesSchema!)} ")
                    .Append($"= {string.Join(", ", SelectedValues!)} ")
                    .Append($"{(Conditions is not null && Conditions.Length > 0 ? " WHERE " + string.Join(" AND ", Conditions) : "")}");
                break;

            case QueryTypes.Delete:
                qs.Append($"DELETE FROM {TableName} ")
                    .Append($"WHERE {string.Join(" AND ", Conditions!)}");
                break;

            case QueryTypes.Select:
                qs.Append($"SELECT {string.Join(", ", ValuesSchema!)}")
                      .Append($" FROM {TableName} ")
                      .Append(Conditions is not null ? $"WHERE {string.Join(" AND ", Conditions)} " : "")
                      .Append(SortMethod is not null ? $"ORDER BY {SortMethod} " : "")
                      .Append(SelectionLimit is not null ? $"LIMIT {SelectionLimit} " : "")
                      .Append(SelectionOffset is not null ? $" OFFSET {SelectionOffset} " : "");
                break;
        }
        qs.Append(';');

        return qs.ToString();
    }

    private bool ValidQuery()
    {
        if (TableName is null) { Log.Error($"{QueryType} query failed: No table specified"); return false; }
        if (QueryType is null) { Log.Error($"Query to \"{TableName}\" failed: Query type is not specified"); return false; }
        if (QueryType == QueryTypes.Delete && Conditions is null) { Log.Error($"Deletion failed: No conditions specified with \"Where()\""); return false; }
        if (QueryType == QueryTypes.Select && ValuesSchema is null) { Log.Error($"Selection failed: No column selected with \"Schema()\""); return false; }
        if (QueryType == QueryTypes.Insert && (SelectedValues is null || ValuesSchema is null)) { Log.Error($"Insertion failed: columns \"Schema()\" or values \"Values()\" missing"); return false; }
        if (QueryType == QueryTypes.Update && (SelectedValues is null || ValuesSchema is null)) { Log.Error($"Update query failed: column name \"Schema()\" or new value \"Values()\" missing"); return false; }
        return true;
    }

    private enum QueryTypes { Insert, Update, Delete, Select }
    public record ExecutionResult(bool Success, object[][]? Results);

    ~DbConnection()
    {
        Connection.Close();
        Connection.Dispose();
        Requests.Dispose();
    }
}
