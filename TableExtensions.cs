using System;
using System.Data;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Table
{
	/// <summary>
	/// Description of TableExtensions.
	/// </summary>
	public static class TableExtensions
	{
		/// <summary>
		/// Creates a shallow copy of elements of a row of the source table
		/// </summary>
		/// <param name="table">The table to get the row from</param>
		/// <param name="index">The index of the row</param>
		/// <returns>A shallow copy of elements of a row of the source table</returns>
		public static List<T> GetRow<T>(this Table table, int index)
		{
			if (table.Rows.Count > 0)
			{
				index = VVVV.Utils.VMath.VMath.Zmod(index, table.Rows.Count);
				int colCount = table.Rows[index].ItemArray.Length;
				List<T> result = new List<T>(colCount);
				for (int i=0; i<colCount; i++)
					result.Add((T)table.Rows[index][i]);
				return result;
			}
			else
			{
				return new List<T>(0);
			}
		}
		
		/// <summary>
		/// Creates a shallow copy of elements of a column of the source table
		/// </summary>
		/// <param name="table">The table to get the column from</param>
		/// <param name="index">The index of the column</param>
		/// <returns>A shallow copy of elements of a column of the source table</returns>
		public static List<T> GetColumn<T>(this Table table, int index)
		{
			if (table.Rows.Count > 0)
			{
				index = VVVV.Utils.VMath.VMath.Zmod(index, table.Columns.Count);
				List<T> result = new List<T>(table.Rows.Count);
				for (int i=0; i<table.Rows.Count; i++)
					result.Add((T)table.Rows[i][index]);
				return result;
			}
			else
			{
				return new List<T>(0);
			}
		}
		
		/// <summary>
		/// Gets the value of a cell of the source table
		/// </summary>
		/// <param name="table">The table to get the cell from</param>
		/// <param name="rowIndex">The zero-based index of the row</param>
		/// <param name="columnIndex">The zero-based index of the column</param>
		/// <returns></returns>
		public static T Get<T>(this Table table, int rowIndex, int columnIndex)
		{
			if (table.Rows.Count > 0)
			{
				rowIndex = VVVV.Utils.VMath.VMath.Zmod(rowIndex, table.Rows.Count);
				columnIndex = VVVV.Utils.VMath.VMath.Zmod(columnIndex, table.Columns.Count);
				return (T)table.Rows[rowIndex][columnIndex];
			}
			else
			{
				return default(T);
			}
		}
		
		/// <summary>
		/// Sets the value of a cell of the source table
		/// </summary>
		/// <param name="table">The table to modify</param>
		/// <param name="input">The value to be set</param>
		/// <param name="rowIndex">The zero-based index of the row to be modified</param>
		/// <param name="columnIndex">The zero-based index of the cell to be modified</param>
		public static void Set<T>(this Table table, T input, int rowIndex, int columnIndex)
		{
			rowIndex = VVVV.Utils.VMath.VMath.Zmod(rowIndex, table.Rows.Count);
			columnIndex = VVVV.Utils.VMath.VMath.Zmod(columnIndex, table.Columns.Count);
			table.Rows[rowIndex][columnIndex] = input;
			table.OnDataChange(null);
		}
		
		/// <summary>
		/// Sets the values of a column of the source table
		/// </summary>
		/// <param name="table">The table to modify</param>
		/// <param name="spread">The spread of values to be set</param>
		/// <param name="index">The zero-based index of the column to be modified</param>
		public static void SetColumn<T>(this Table table, ISpread<T> spread, int index)
		{
			if (table.Columns.Count == 0)
			{
//				InsertRow(valueSpread, 0);
			}
			else
			{
				index = VVVV.Utils.VMath.VMath.Zmod(index, table.Columns.Count);
				for (int r=0; r<table.Rows.Count; r++)
					table.Rows[r][index] = spread[r];
				table.OnDataChange(null);
			}
		}
		
		/// <summary>
		/// Sets the values of a row of the source table
		/// </summary>
		/// <param name="table">The table to modify</param>
		/// <param name="spread">The spread of values to be set</param>
		/// <param name="index">The zero-based index of the column to be modified</param>
		public static void SetRow<T>(this Table table, ISpread<T> spread, int index)
		{
			if (table.Rows.Count == 0)
			{
				table.InsertRow(spread, 0);
			}
			else
			{
				index = VVVV.Utils.VMath.VMath.Zmod(index, table.Rows.Count);
				table.SetRow(table.Rows[index], spread);
			}
		}
		
		/// <summary>
		/// Sets the values of a row of the source table
		/// </summary>
		/// <param name="table">The table to which the data row belongs</param>
		/// <param name="row">The data row to be modified</param>
		/// <param name="spread">The spread of values to be set</param>
		public static void SetRow<T>(this Table table, DataRow row, ISpread<T> spread)
		{
			for (int i = 0; i < row.ItemArray.Length; i++)
				row[i] = spread[i];
			table.OnDataChange(null);
		}
		
		/// <summary>
		/// Inserts a row of values into the source table
		/// </summary>
		/// <param name="table">The table to insert the row to</param>
		/// <param name="spread">The spread of values to be inserted</param>
		/// <param name="index">The zero-based index to insert the row to</param>
		public static void InsertRow<T>(this Table table, ISpread<T> spread, int index)
		{
			//prepare row for insertion
			DataRow row = table.NewRow();
			table.SetRow(row, spread);

			if (table.Rows.Count == 0 || table.Rows.Count == index)
			{
				//if we're blank, or insert index is next slot
				table.Rows.Add(row);
			}
			else
			{
				index = VVVV.Utils.VMath.VMath.Zmod(index, table.Rows.Count + 1);
				table.Rows.InsertAt(row, index); // insert it somewhere inside collection
			}
		}
		
		/// <summary>
		/// Inserts a rows of values into the source table
		/// </summary>
		/// <param name="table">The table to insert the row to</param>
		/// <param name="spread">The spread of spread of values to be inserted</param>
		/// <param name="index">The zero-based index to insert the row to</param>
		public static void InsertRow<T>(this Table table, ISpread<ISpread<T>> spread, int index)
		{
			foreach (var row in spread)
			{
				table.InsertRow(row, index);
			}
		}
	}
}
