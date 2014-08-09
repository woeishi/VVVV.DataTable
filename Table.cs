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

		#region ctor & fields
		private bool isChanging; //seemingly some parallelism going on
		public Table()
		{
			isChanging = false;
			ClearAll();
		}
		public int SliceIndex { get; set; }
		public string NiceName { get; set; }
		public string ColumnNames
		{
			get 
			{
				string ret = string.Empty;
				foreach (DataColumn c in this.Columns)
					ret += c.ColumnName + ",";
				
				if (ret.Length > 1)
					ret = ret.Substring(0,ret.Length-1);
				
				return ret;
			}
		}
		#endregion
		
		#region structure methods
		public void InitTable(string name, int slice)
		{
			this.SliceIndex = slice;
			this.NiceName = name;
			this.TableName = NiceName+"_"+slice.ToString();
		}
		
		public void InitTable(int slice)
		{
			this.SliceIndex = slice;
			this.NiceName = this.TableName.Replace("_"+slice.ToString(),"");
		}

		public bool CompareTableName(string name, int i)
		{
			return this.TableName == name+"_"+i.ToString();
		}

		public void SetupColumns(string columnNames, string columnTypes = "d")
		{
			if (!isChanging)
			{
				isChanging = true;
				if (columnNames == "")
					return;
				
				string[] colNameArray;
				if (string.IsNullOrEmpty(columnNames))
					colNameArray = new string[]{};
				else
					colNameArray = columnNames.Split(',');			
				string[] colTypeArray;
				if (string.IsNullOrEmpty(columnTypes))
					colTypeArray = new string[]{};
				else
					colTypeArray = columnTypes.Split(',');
				
				int count = colNameArray.Length;
				
				for (int a=0; a<count; a++)
					this.AddOrSetColumn(colNameArray[a], colTypeArray[a%colTypeArray.Length], a);
				
				for (int r=this.Columns.Count-1; r>=count; r--) //first remove avoiding too many duplicates
					this.Columns.RemoveAt(r);
				isChanging = false;
				OnStructureChange(null);
			}
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

		public void ClearAll()
		{
			this.Rows.Clear();
			this.Columns.Clear();
			OnDataChange(this);
		}
	}
}
