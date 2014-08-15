using System;
using System.Data;

namespace VVVV.Nodes.Table
{
	static class TableDefaults
	{
		public const string STRING = "abc";
		public const double DOUBLE = 0.0;
		
		public const string CATEGORY = "DataTable";
		public const string AUTHOR = "woei";
		public const string TAGS = "";
	}
	
	public class TableEventArgs : EventArgs
	{
		private Table _table;
		public Table Table { get {return _table;} }
		public TableEventArgs(Table t) { _table = t; }
	}
	
	public class Table : DataTable
	{
		#region events
		public delegate void DataChangedHandler(Object sender, TableEventArgs e);
		public event DataChangedHandler DataChanged;
		
		void OnDataChange(object sender, TableEventArgs e)
		{
			if (DataChanged != null)
				DataChanged(sender, e);
		}

		public void OnDataChange(Object sender)
		{
			OnDataChange(sender, new TableEventArgs(this));
		}
		
		
		public delegate void StructureChangedHandler(Object sender, TableEventArgs e);
		public event StructureChangedHandler StructureChanged;
		
		void OnStructureChange(object sender, TableEventArgs e)
		{
			if (DataChanged != null)
				StructureChanged(sender, e);
		}

		public void OnStructureChange(Object sender)
		{
			OnStructureChange(sender, new TableEventArgs(this));
		}
		#endregion

		#region structure methods
		public void SetupColumns(string columnNames, string columnTypes = "d")
		{
			string[] colNameArray;
			if (string.IsNullOrEmpty(columnNames))
				colNameArray = new string[]{};
			else
				colNameArray = columnNames.Split(new char[]{',',' '},StringSplitOptions.RemoveEmptyEntries);
			
			string[] colTypeArray;
			if (string.IsNullOrEmpty(columnTypes))
				colTypeArray = new string[]{};
			else
				colTypeArray = columnTypes.Split(new char[]{',',' '},StringSplitOptions.RemoveEmptyEntries);
			
			int count = colNameArray.Length;
			for (int r=this.Columns.Count-1; r>=count; r--) //first remove avoiding too many duplicates
				this.Columns.RemoveAt(r);
			
			for (int a=0; a<count; a++)
				this.AddOrSetColumn(colNameArray[a], colTypeArray[a%colTypeArray.Length], a);
			
			
			OnStructureChange(this);
		}
		
		public void AddOrSetColumn(string name, string typeString, int ord)
		{
			Type type = TryGetType(typeString, typeof(double));
				
			if ((ord<0) || (ord >= this.Columns.Count))
			{
				this.AddColumn(name, type, ord);
				OnDataChange(null);
			}
			else if (this.Columns[ord].DataType != type)
			{
				var removeCol = this.Columns[ord];
				this.Columns.Remove(removeCol);
				removeCol.Dispose();
				
				this.AddColumn(name, type, ord);
			}
			
			if (this.Columns[ord].ColumnName != name)
			{
				if (this.Columns.Contains(name))
					this.Columns[name].ColumnName += "woei";
				this.Columns[ord].ColumnName = name;
			}
		}
		
		private Type TryGetType(string typeString, Type defaultType)
		{
			Type type = defaultType;
			switch (typeString)
			{
				case "s":
				case "text":
					type = typeof(string);
					break;
				case "d":
				case "f":
				case "n":
				case "value":
					type = typeof(double);
					break;
				case "i":
				case "int":
				case "integer":
					type = typeof(int);
					break;
				case "b":
				case "bool":
				case "boolean":
					type = typeof(bool);
					break;
				default:
					try
					{
						type = Type.GetType(typeString, true, true);
					}
					catch
					{
						type = typeof(double);
					}
					break;
			}
			return type;
		}
			
		
		private void AddColumn(string name, Type type, int ord = -1)
		{
			DataColumn newCol = this.Columns.Add(name, type);
			
			if (type == typeof(string))
				newCol.DefaultValue = TableDefaults.STRING;
			else
				newCol.DefaultValue = TableDefaults.DOUBLE;
			
			if (ord >= 0 && ord <= this.Columns.Count)
				newCol.SetOrdinal(ord);
			
//			foreach (DataRow testRow in Rows)
//			{
//				if (testRow[newCol].GetType() == typeof(DBNull))
//					testRow[newCol] = newCol.DefaultValue;
//			}
			newCol.AllowDBNull = false;
		}
		#endregion
	}
}
