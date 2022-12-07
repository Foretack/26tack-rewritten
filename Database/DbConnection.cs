﻿using Npgsql;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Tack.Database;
internal abstract class DbConnection : IDisposable
{
    private static readonly string ConnectionString =
        $"Host={AppConfigLoader.Config.DbHost};" +
        $"Username={AppConfigLoader.Config.DbUser};" +
        $"Password={AppConfigLoader.Config.DbPass};" +
        $"Database={AppConfigLoader.Config.DbName}";

    public QueryFactory QueryFactory => _qf;

    private static bool _initialized;
    private static NpgsqlConnection _connection;
    private static PostgresCompiler _compiler;
    private static QueryFactory _qf;

    protected DbConnection()
    {
        if (!_initialized)
        {
            _connection = new(ConnectionString);
            _compiler = new();
            _qf = new QueryFactory(_connection, _compiler);
        }
        _initialized = true;
    }

    public SqlKata.Query this[string table] => QueryFactory.Query(table);

    private bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _connection.Close();
                _connection.Dispose();
                _qf.Dispose();
            }

            _compiler = default!;
            _connection = default!;
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
