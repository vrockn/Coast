namespace Coast.PostgreSql
{
    using System;
    using System.Data;
    using System.Threading;
    using System.Threading.Tasks;
    using Coast.Core;
    using Coast.Core.MigrationManager;
    using Dapper;
    using Microsoft.Extensions.Options;
    using Npgsql;

    /// <summary>
    /// Init DataBase. Create table.
    /// </summary>
    public class CoastDBInitializer : ICoastDBInitializer
    {
        private readonly IOptions<DBOptions> _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoastDBInitializer"/> class.
        /// </summary>
        /// <param name="_options">Db configuration.</param>
        public CoastDBInitializer(IOptions<DBOptions> options)
        {
            _options = options;
        }

        /// <inheritdoc/>
        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var sql = CreateTableSql();
            using var connection = PostgreSqlDataConnection.OpenConnection(_options.Value.ConnectionString);
            await connection.ExecuteAsync(sql).ConfigureAwait(false);
        }

        private string CreateTableSql()
        {
            var sql = $@"
CREATE TABLE IF NOT EXISTS ""Coast_Barrier""(
    ""TransactionType"" int NOT NULL,
	""CorrelationId"" bigint NOT NULL,
	""StepId"" bigint NOT NULL,
	""StepType"" int NULL,
    ""CreateTime"" TIMESTAMP NULL,
    UNIQUE (""CorrelationId"", ""StepId"", ""StepType"")
);

CREATE TABLE IF NOT EXISTS ""Coast_Saga""(
	""Id"" bigint PRIMARY KEY NOT NULL,
    ""Status"" int NOT NULL,
    ""CurrentStep"" bigint NULL,
    ""CreateTime"" TIMESTAMP NULL
) ;

CREATE TABLE IF NOT EXISTS ""Coast_SagaStep""(
	""Id"" bigint NOT NULL,
    ""CorrelationId"" bigint NOT NULL,
    ""EventName"" VARCHAR(250) NOT NULL,
    ""StepType"" int NOT NULL,
    ""Status""   int NOT NULL,
    ""RequestBody"" text NULL,
    ""FailedReason"" text NULL,
    ""CreateTime"" TIMESTAMP NULL,
    ""PublishedTime"" TIMESTAMP NULL
) ;
CREATE INDEX IF NOT EXISTS SagaStep_idx ON ""Coast_SagaStep"" (""CorrelationId"");"
;

            return sql;
        }
    }
}
