using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Contracts;
using Ukinee.Infrastructure.Ddd.Local.EfCore.Domain;

namespace Ukinee.Infrastructure.Ddd.Local.EfCore.Postgres.Services;

public class PostgresVersionStrategy<TTag> : IVersionStrategy<TTag>
{
    public async ValueTask<long> GetNextVersionAsync(DbContext context, CancellationToken cancellationToken)
    {
        var provider = context.Database.ProviderName;

        if (provider != "Npgsql.EntityFrameworkCore.PostgreSQL")
            throw new InvalidOperationException($"Cannot use {nameof(PostgresVersionStrategy<>)} with {provider}");

        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.Transaction = context.Database.CurrentTransaction?.GetDbTransaction();

        command.CommandText = $"""
                                          UPDATE "{ChangeSequenceUtils<TTag>.TableName}" 
                                          SET "Value" = "Value" + 1 
                                          WHERE "Tag" = @tag 
                                          RETURNING "Value";
                              """;

        var param = command.CreateParameter();
        param.ParameterName = "@tag";
        param.Value = ChangeSequenceUtils<TTag>.TagName;

        command.Parameters.Add(param);

        var result = await command.ExecuteScalarAsync(cancellationToken);

        if (result == null || result == DBNull.Value)
        {
            throw new InvalidOperationException($"Sequence for tag '{typeof(TTag).Name}' was not seeded in {ChangeSequenceUtils<TTag>.TableName} table.");
        }

        return Convert.ToInt64(result);
    }
}
