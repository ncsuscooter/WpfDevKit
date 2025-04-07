using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace WpfDevKit.Logging
{

    /// <summary>
    /// Represents the configuration for a database column used by the <see cref="DatabaseLogProvider"/>.
    /// This struct defines how a specific <see cref="TLogElement"/> maps to a target SQL column, including value extraction,
    /// nullability rules, and length constraints.
    /// </summary>
    [DebuggerStepThrough]
    internal struct DatabaseLogColumn
    {
        /// <summary>
        /// A delegate that extracts a value from an <see cref="ILogMessage"/> for this column.
        /// </summary>
        public Func<ILogMessage, object> DataFactory;

        /// <summary>
        /// The name of the target database column.
        /// </summary>
        public string ColumnName;

        /// <summary>
        /// Indicates whether the target column accepts null values.
        /// </summary>
        public bool IsNullable;

        /// <summary>
        /// An optional maximum character length for the column value.
        /// Used for validation before executing the SQL insert.
        /// </summary>
        public int? MaxLength;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseLogColumn"/> struct with the specified mapping settings.
        /// </summary>
        /// <param name="dataFactory">A function that extracts the value from a log message.</param>
        /// <param name="columnName">The name of the column in the database.</param>
        /// <param name="isNullable">Whether the column allows null values.</param>
        /// <param name="maxLength">An optional maximum character length.</param>
        public DatabaseLogColumn(Func<ILogMessage, object> dataFactory, string columnName, bool isNullable, int? maxLength) =>
            (DataFactory, ColumnName, IsNullable, MaxLength) = (dataFactory, columnName, isNullable, maxLength);

        /// <summary>
        /// Determines whether the specified object is equal to the current instance.
        /// </summary>
        public override bool Equals(object obj) =>
            obj is DatabaseLogColumn other
            && EqualityComparer<Func<ILogMessage, object>>.Default.Equals(DataFactory, other.DataFactory)
            && ColumnName == other.ColumnName
            && IsNullable == other.IsNullable
            && MaxLength == other.MaxLength;

        /// <summary>
        /// Returns a hash code for the current instance.
        /// </summary>
        public override int GetHashCode()
        {
            int hashCode = -1726572946;
            hashCode = hashCode * -1521134295 + EqualityComparer<Func<ILogMessage, object>>.Default.GetHashCode(DataFactory);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ColumnName);
            hashCode = hashCode * -1521134295 + IsNullable.GetHashCode();
            hashCode = hashCode * -1521134295 + MaxLength.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Deconstructs the column definition into its component parts.
        /// </summary>
        /// <param name="dataFactory">The value extraction delegate.</param>
        /// <param name="name">The column name.</param>
        /// <param name="isNullable">Whether the column is nullable.</param>
        /// <param name="maxLength">The optional maximum string length.</param>
        public void Deconstruct(out Func<ILogMessage, object> dataFactory, out string name, out bool isNullable, out int? maxLength) =>
            (dataFactory, name, isNullable, maxLength) = (DataFactory, ColumnName, IsNullable, MaxLength);

        /// <summary>
        /// Converts a <see cref="DatabaseLogColumn"/> to its tuple representation.
        /// </summary>
        public static implicit operator (Func<ILogMessage, object> dataFactory, string name, bool isNullable, int? maxLength)(DatabaseLogColumn value) =>
            (value.DataFactory, value.ColumnName, value.IsNullable, value.MaxLength);

        /// <summary>
        /// Converts a tuple into a <see cref="DatabaseLogColumn"/> instance.
        /// </summary>
        public static implicit operator DatabaseLogColumn((Func<ILogMessage, object> dataFactory, string name, bool isNullable, int? maxLength) value) =>
            new DatabaseLogColumn(value.dataFactory, value.name, value.isNullable, value.maxLength);
    }
}
