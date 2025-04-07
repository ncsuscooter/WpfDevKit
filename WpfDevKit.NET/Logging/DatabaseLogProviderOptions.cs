using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace WpfDevKit.Logging
{
    /// <summary>
    /// Provides configuration options for the <see cref="DatabaseLogProvider"/>, including connection details,
    /// schema/table settings, column definitions, and error-handling behavior.
    /// </summary>
    [DebuggerStepThrough]
    public sealed class DatabaseLogProviderOptions
    {
        private readonly ConcurrentDictionary<TLogElement, DatabaseLogColumn> elements =
            new ConcurrentDictionary<TLogElement, DatabaseLogColumn>();

        private string tableName;
        private string schemaName;

        /// <summary>
        /// Gets or sets a value indicating whether the log provider should throw an exception on validation or insert failure.
        /// When set to <c>false</c>, validation errors and SQL issues are logged internally and do not propagate.
        /// </summary>
        public bool ThrowOnFailure { get; set; } = false;

        /// <summary>
        /// Gets or sets the name of the target database table where log entries will be inserted.
        /// Setting this property automatically rebuilds the underlying SQL <c>INSERT</c> command.
        /// </summary>
        public string TableName
        {
            get => tableName;
            set
            {
                if (string.Equals(TableName, value, StringComparison.OrdinalIgnoreCase))
                    return;
                tableName = string.IsNullOrWhiteSpace(value) ? value : value.Replace("[", "").Replace("]", "").Replace("'", "''");
                BuildCommandText();
            }
        }

        /// <summary>
        /// Gets or sets the name of the database schema where the log table resides.
        /// Setting this property automatically rebuilds the underlying SQL <c>INSERT</c> command.
        /// </summary>
        public string SchemaName
        {
            get => schemaName;
            set
            {
                if (string.Equals(SchemaName, value, StringComparison.OrdinalIgnoreCase))
                    return;
                schemaName = string.IsNullOrWhiteSpace(value) ? value : value.Replace("[", "").Replace("]", "").Replace("'", "''");
                BuildCommandText();
            }
        }

        /// <summary>
        /// Gets or sets the database connection string used by the <see cref="DatabaseLogProvider"/>.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets the fully generated SQL <c>INSERT</c> command based on the current schema, table name, and element mappings.
        /// </summary>
        internal string CommandText { get; private set; }

        /// <summary>
        /// Gets the collection of column definitions currently registered with the log provider.
        /// This is a read-only snapshot of the internal element mapping used during SQL execution.
        /// </summary>
        internal IReadOnlyCollection<DatabaseLogColumn> Definitions { get; private set; } = new List<DatabaseLogColumn>();

        /// <summary>
        /// Registers a new log message element to be persisted in the database log.
        /// Using this method automatically rebuilds the underlying SQL <c>INSERT</c> command.
        /// </summary>
        /// <param name="element">The log element (such as timestamp, message, level, etc.) to track.</param>
        /// <param name="dataFactory">A delegate that extracts the corresponding value from a <see cref="ILogMessage"/> instance.</param>
        /// <param name="columnName">The target database column name.</param>
        /// <param name="isNullable">Indicates whether the column allows null values.</param>
        /// <param name="maxLength">An optional maximum character length for validation.</param>
        public void AddElement(TLogElement element,
                               Func<ILogMessage, object> dataFactory,
                               string columnName,
                               bool isNullable,
                               int? maxLength = null)
        {
            if (elements.TryAdd(element, (dataFactory, columnName, isNullable, maxLength)))
                BuildCommandText();
        }

        /// <summary>
        /// Rebuilds the SQL <c>INSERT</c> statement and refreshes the <see cref="Definitions"/> collection.
        /// This is automatically called when the schema, table, or elements change.
        /// </summary>
        private void BuildCommandText()
        {
            CommandText = string.Empty;
            if (elements.Count > 0)
            {
                var columns = string.Join(",", elements.Values.Select(x => x.ColumnName));
                var parameters = string.Concat("@", string.Join(",@", elements.Keys));
                CommandText = $"INSERT INTO [{SchemaName}].[{TableName}] ({columns}) VALUES ({parameters})";
                Definitions = elements.Values.ToList().AsReadOnly();
            }
        }
    }
}
