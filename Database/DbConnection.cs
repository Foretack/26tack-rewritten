using System.Text;
using Npgsql;
using Serilog;

namespace Tack.Database;
internal abstract class DbConnection : IDisposable
{
    #region Properties
    public HttpClient Requests { get; private set; } = new HttpClient();

    private const string ConnectionString = $"Host={Config.Host};Username={Config.DbUsername};Password={Config.Password};Database={Config.DatabaseName}";
    private NpgsqlConnection Connection { get; set; } = new NpgsqlConnection(ConnectionString);
    #endregion

    #region Query properties
    private QueryTypes? QueryType { get; set; } = null;
    private string? TableName { get; set; } = null;
    private string? SortMethod { get; set; } = null;
    private string[]? SelectedValues { get; set; } = null;
    private string[]? ValuesSchema { get; set; } = null;
    private string[]? Conditions { get; set; } = null;
    private int? SelectionLimit { get; set; } = null;
    private int? SelectionOffset { get; set; } = null;
    #endregion

    protected DbConnection() { Connection.Open(); }

    #region Methods
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
    #endregion

    #region Execution
    public async Task<ExecutionResult> TryExecute()
    {
        string? query = BuildQueryString();
        Log.Verbose(query ?? "null query");
        if (query is null) return new ExecutionResult(false, Array.Empty<object[]>());
        NpgsqlCommand cmd = new NpgsqlCommand(query, Connection);

        if (QueryType == QueryTypes.Insert || QueryType == QueryTypes.Update || QueryType == QueryTypes.Delete)
        {
            try
            {
                await cmd.ExecuteNonQueryAsync();
                await cmd.DisposeAsync();
                return new ExecutionResult(true, Array.Empty<object[]>());
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
                List<short> ordinals = new List<short>();
                if (!ValuesSchema!.Contains("*") && await r.ReadAsync())
                {
                    foreach (var column in ValuesSchema!) 
                    { 
                        ordinals.Add((short)r.GetOrdinal(column)); 
                    }
                }
                else if (await r.ReadAsync())
                {
                    int columnCount = r.GetColumnSchema().Count;
                    for (short i = 0; i < columnCount; i++) ordinals.Add(i);
                }
                await r.CloseAsync();

                NpgsqlDataReader r2 = await cmd.ExecuteReaderAsync();
                List<object[]> values = new List<object[]>();
                List<object> valuesInner = new List<object>();
                while (await r2.ReadAsync())
                {
                    foreach (short o in ordinals) { valuesInner.Add(r.GetValue(o)); }
                    values.Add(valuesInner.ToArray());
                    valuesInner.Clear();
                }
                await r2.CloseAsync();

                await r.DisposeAsync();
                await r2.DisposeAsync();
                await cmd.DisposeAsync();
                return new ExecutionResult(true, values.ToArray());
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Exception was thrown during {QueryType} query" +
                    $"\n Full Query:\n {query}");
            }
        }
        return new ExecutionResult(false, Array.Empty<object[]>());
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
                    .Append($"({string.Join(", ", ValuesSchema!)}) ")
                    .Append("VALUES " + (sv.Contains('(') ? $"{sv}" : $"({sv})"));
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

        bool valid = QueryType switch
        {
            QueryTypes.Delete when Conditions is null     => false,
            QueryTypes.Select when ValuesSchema is null   => false,
            QueryTypes.Update when ValuesSchema is null   => false,
            QueryTypes.Insert when SelectedValues is null => false,
            QueryTypes.Insert when ValuesSchema is null   => false,
            _                                             => true
        };
        if (!valid) Log.Error($"An invalid query was attempted! {{ Conditions: {Conditions}, ValuesSchema: {ValuesSchema}, SelectedValues: {SelectedValues} }}");

        return valid;
    }
    #endregion

    private enum QueryTypes { Insert, Update, Delete, Select }
    public record ExecutionResult(bool Success, object[][] Results);

    #region Disposal
    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                Connection.Close();
                Connection.Dispose();
                Requests.Dispose();
            }

            Requests = default!;
            Connection = default!;
            QueryType = null;
            TableName = null;
            SortMethod = null;
            SelectedValues = null;
            ValuesSchema = null;
            Conditions = null;
            SelectionLimit = null;
            SelectionLimit = null;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~DbConnection() { Dispose(disposing: false); }
    #endregion
}
