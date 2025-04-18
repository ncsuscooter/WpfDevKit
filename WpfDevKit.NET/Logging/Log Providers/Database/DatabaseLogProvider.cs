using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Logging
{
    [DebuggerStepThrough]
    internal class DatabaseLogProvider : ILogProvider
    {
        private readonly DatabaseLogProviderOptions options;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseLogProvider"/> class.
        /// This log provider only supports Microsoft SQL Server.
        /// </summary>
        /// <param name="options">The options for configuring the database log provider.</param>
        public DatabaseLogProvider(IOptions<DatabaseLogProviderOptions> options) => this.options = options.Value;

        /// <inheritdoc/>
        /// <remarks>
        /// The default values are <see cref="TLogCategory.StartStop"/>, <see cref="TLogCategory.Info"/>, <see cref="TLogCategory.Warning"/>,
        /// <see cref="TLogCategory.Error"/>, and <see cref="TLogCategory.Fatal"/>.
        /// </remarks>
        public TLogCategory EnabledCategories { get; set; } = TLogCategory.Info | TLogCategory.StartStop | TLogCategory.Warning | TLogCategory.Error | TLogCategory.Fatal;

        /// <inheritdoc/>
        /// <remarks>
        /// The default value is <see cref="TLogCategory.Trace"/>, meaning only Trace categories 
        /// are disabled by default.
        /// </remarks>
        public TLogCategory DisabledCategories => TLogCategory.Trace;

        /// <inheritdoc/>
        /// <exception cref="InvalidOperationException" />
        public async Task LogAsync(ILogMessage message)
        {
            void ThrowOrIgnore(string s)
            {
                if (options.ThrowOnFailure)
                    throw new InvalidOperationException($"[DatabaseLogProvider] Validation failed: {s}");
            }
            using (var connection = new SqlConnection(options.ConnectionString))
            using (var command = new SqlCommand(options.CommandText, connection))
            {
                foreach (var definition in options.Definitions)
                {
                    if (string.IsNullOrWhiteSpace(definition.ColumnName))
                        continue;
                    var valueAsObject = definition.DataFactory(message);
                    var valueAsString = Convert.ToString(valueAsObject);
                    if (!definition.IsNullable && string.IsNullOrWhiteSpace(valueAsString))
                        ThrowOrIgnore($"Column '{definition.ColumnName}' is not nullable but received null or empty value.");
                    if (definition.MaxLength.HasValue && valueAsString.Length > definition.MaxLength.Value)
                        ThrowOrIgnore($"Value for column '{definition.ColumnName}' exceeds max length: {valueAsString.Length} > {definition.MaxLength}");
                    command.Parameters.AddWithValue($"@{definition.ColumnName}", valueAsObject ?? DBNull.Value);
                }
                await connection.OpenAsync();
                var count = await command.ExecuteNonQueryAsync();
                if (count < 1 || count > 1)
                    ThrowOrIgnore($"Expected 1 row inserted, but {count} rows affected.");
            }
        }
    }
}
