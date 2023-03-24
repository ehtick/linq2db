// ---------------------------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by LinqToDB scaffolding tool (https://github.com/linq2db/linq2db).
// Changes to this file may cause incorrect behavior and will be lost if the code is regenerated.
// </auto-generated>
// ---------------------------------------------------------------------------------------------------

using LinqToDB;
using LinqToDB.Mapping;
using LinqToDB.Tools.Comparers;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;

#pragma warning disable 1573, 1591
#nullable enable

namespace Cli.All.SqlServerNorthwind
{
	[Table("EmployeeTerritories")]
	public class EmployeeTerritory : IEquatable<EmployeeTerritory>
	{
		[Column("EmployeeID" , DataType = DataType.Int32   , DbType = "int"         , IsPrimaryKey = true, PrimaryKeyOrder = 0                        )] public SqlInt32  EmployeeId  { get; set; } // int
		[Column("TerritoryID", DataType = DataType.NVarChar, DbType = "nvarchar(20)", Length       = 20  , IsPrimaryKey    = true, PrimaryKeyOrder = 1)] public SqlString TerritoryId { get; set; } // nvarchar(20)

		#region IEquatable<T> support
		private static readonly IEqualityComparer<EmployeeTerritory> _equalityComparer = ComparerBuilder.GetEqualityComparer<EmployeeTerritory>(c => c.EmployeeId, c => c.TerritoryId);

		public bool Equals(EmployeeTerritory? other)
		{
			return _equalityComparer.Equals(this, other!);
		}

		public override int GetHashCode()
		{
			return _equalityComparer.GetHashCode(this);
		}

		public override bool Equals(object? obj)
		{
			return Equals(obj as EmployeeTerritory);
		}
		#endregion

		#region Associations
		/// <summary>
		/// FK_EmployeeTerritories_Employees
		/// </summary>
		[Association(CanBeNull = false, ThisKey = nameof(EmployeeId), OtherKey = nameof(Employee.EmployeeId))]
		public Employee Employees { get; set; } = null!;

		/// <summary>
		/// FK_EmployeeTerritories_Territories
		/// </summary>
		[Association(CanBeNull = false, ThisKey = nameof(TerritoryId), OtherKey = nameof(Territory.TerritoryId))]
		public Territory Territories { get; set; } = null!;
		#endregion
	}
}