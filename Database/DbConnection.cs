using Npgsql;
using Serilog;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Tack.Database;
internal abstract class DbConnection : IDisposable
{
    private const string ConnectionString = $"Host={Config.Host};Username={Config.DbUsername};Password={Config.Password};Database={Config.DatabaseName}";

    public QueryFactory QueryFactory { get; protected set; }
    public HttpClient Requests { get; private set; } = new();

    private NpgsqlConnection Connection { get; set; } = new(ConnectionString);
    private PostgresCompiler Compiler { get; set; } = new();

    protected DbConnection()
    {
        QueryFactory = new QueryFactory(Connection, Compiler);
        QueryFactory.Logger = compiled => Log.Debug(compiled.ToString());
    }

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
                QueryFactory.Dispose();
            }

            QueryFactory = default!;
            Compiler = default!;
            Requests = default!;
            Connection = default!;
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    ~DbConnection() { Dispose(disposing: false); }
}
