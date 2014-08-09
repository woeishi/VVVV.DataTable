using System;
using System.Collections.Generic;
using System.Data;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using VVVV.Core.Logging;

namespace VVVV.Nodes.Table
{
	public abstract class TableRowDynamicPluginEvaluate : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Config("Name")]
		IDiffSpread<string> FName;
		
		[Config("Type")]
		IDiffSpread<string> FType;
		
		[Input("Table", Order = int.MinValue, IsSingle = true)]
		private IDiffSpread<Table> FTables;
		
		[Output("Status", Order = int.MaxValue, Visibility = PinVisibility.Hidden)]
		private ISpread<string> FStatus;
		
		[Import()]
		private IIOFactory FIOFactory;
		
		[Import()]
		private ILogger FLogger;

		private Table FTable;
		private bool firstFrame = true;
		private bool IsChanged = false; //flag is raised when DataChanged event is called
		private bool ColChanged = false;
		
		private Dictionary<string,IIOContainer> FPins = new Dictionary<string, IIOContainer>();
		#endregion
		
		public void OnImportsSatisfied()
        {
			FName.Changed += InitialCreatePins;
			FType.Changed += InitialCreatePins;
        }

		public void Evaluate(int SpreadMax)
		{
			if (firstFrame)
			{
				FName.Changed -= InitialCreatePins;
				FType.Changed -= InitialCreatePins;
				firstFrame = false;
			}
			
			if (FTables.SliceCount == 0)
				return;
			
			if (FTables[0] == null)
			{
				if (FTable != null)
				{
					FTable.DataChanged -= Tables_Changed;
					FTable.StructureChanged -= Column_Changed;
					IsChanged = true;
					ColChanged = true;
					FTable = null;
				}
			}
			else if (FTable != FTables[0])
			{
				if (FTable != null)
				{
					FTable.DataChanged -= Tables_Changed;
					FTable.StructureChanged -= Column_Changed;
				}
				FTable = FTables[0];
				FTable.DataChanged += Tables_Changed;
				FTable.StructureChanged += Column_Changed;
				IsChanged = true;
				ColChanged = true;
				
			}
			
			if (ColChanged) //table layout might have changed
			{
				FName.SliceCount = 0;
				FType.SliceCount = 0;
				if (FTable != null)
				{
					foreach (DataColumn col in FTable.Columns)
					{
						FName.Add(col.ColumnName);
						FType.Add(col.DataType.ToString());
					}
				}
				this.CreatePins(); //seems like events cause pin creation in the next frame
			}
			
			try
			{
				this.EvaluateTable(FTable, FPins, IsChanged);
				FStatus[0] = string.Empty;
			}
			catch (Exception e) 
			{ 
				FStatus[0] = e.Message; 
			}
			
			this.IsChanged = false;
			this.ColChanged = false;
		}

		protected abstract void EvaluateTable(Table tables, Dictionary<string,IIOContainer> FPins, bool isChanged);
		
		protected abstract IOAttribute CreatePinAttribute(string name);

		private void Tables_Changed(object sender, TableEventArgs e) 
		{
			if (sender != this)
			{
				IsChanged = true;
			}
		}
		
		private void Column_Changed(object sender, TableEventArgs e)
		{
			this.ColChanged = true;
		}
		
		private void CreatePins()
		{
			var pins = new Dictionary<string,IIOContainer>(FPins);
			for (int i=0; i<FName.SliceCount; i++)
			{
				if ((!string.IsNullOrEmpty(FName[i])) && (!string.IsNullOrEmpty(FType[i])))
				{
					if(!pins.ContainsKey(FName[i]+FType[i]))
					{
						Type pinType = typeof(ISpread<>).MakeGenericType(Type.GetType(FType[i],true,true));
						IIOContainer container = FIOFactory.CreateIOContainer(pinType,CreatePinAttribute(FName[i]));
						FPins.Add(FName[i]+FType[i], container);
					}
					else
					{
						pins.Remove(FName[i]+FType[i]);
					}
				}
			}
			
			foreach(var k in pins.Keys)
			{
				FPins[k].Dispose();
				FPins.Remove(k);
			}
		}
		
		private void InitialCreatePins(IDiffSpread<string> spread)
		{
			 this.CreatePins();
		}
	}
	
	[PluginInfo(Name = "GetRow", Category = TableDefaults.CATEGORY, Version = "Dynamic", Help = "Gets values of a table row", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class GetRowDynamicNode : TableRowDynamicPluginEvaluate
	{
		#region fields & pins
		[Input("Index")]
		IDiffSpread<int> FIndex;
		
		[Output("Status", Order = int.MaxValue)]
		ISpread<string> FStatus;
		
		[Import()]
		ILogger FLogger;
		#endregion
		
		protected override IOAttribute CreatePinAttribute(string name)
		{
			return new OutputAttribute(name);
		}
		
		protected override void EvaluateTable(Table table, Dictionary<string,IIOContainer> pins, bool isChanged)
		{
			if (isChanged || FIndex.IsChanged)
			{
				foreach (var p in pins.Values)
            		(p.RawIOObject as ISpread).SliceCount = 0;
				
				if (table != null && table.Columns.Count > 0)
				{
					foreach (var p in pins.Values)
            			(p.RawIOObject as ISpread).SliceCount = table.Rows.Count;
					for (int i=0; i<FIndex.SliceCount; i++)
					{
						if (table.Rows.Count > 0)
						{
							int id = VVVV.Utils.VMath.VMath.Zmod(FIndex[i], table.Rows.Count);
							
							for (int c=0; c<table.Columns.Count; c++)
							{
								string _n = table.Columns[c].ColumnName;
								string _t = table.Columns[c].DataType.ToString();
								
								string key = _n+_t;
								try
								{
									(pins[key].RawIOObject as ISpread)[i] = table.Rows[id][c];
								}
								catch (Exception e)
								{
									FStatus[0] = key+":"+e.Message;
								}
							}
						}
					}
				}
			}
		}
	}
		
	[PluginInfo(Name = "SetRow", Category = TableDefaults.CATEGORY, Version = "Dynamic", Help = "Sets values of a table row", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class SetRowDynamicNode : TableRowDynamicPluginEvaluate
	{
		#region fields & pins
		[Input("Index", Order = int.MaxValue-1)]
		ISpread<int> FIndex;
		
		[Input("Set", IsBang = true, Order = int.MaxValue)]
		IDiffSpread<bool> FSet;
		
		[Output("Status", Order = int.MaxValue)]
		ISpread<string> FStatus;
		
		[Import()]
		ILogger FLogger;
		#endregion
		
		protected override IOAttribute CreatePinAttribute(string name)
		{
			return new InputAttribute(name);
		}
		
		protected override void EvaluateTable(Table table, Dictionary<string,IIOContainer> pins, bool isChanged)
		{
			if (table != null && table.Columns.Count > 0 && table.Rows.Count > 0 && FSet.IsChanged)
			{
				int spreadMax = FIndex.SliceCount.CombineWith(FSet);
				foreach (var p in pins.Values)
					spreadMax = spreadMax.CombineSpreads((p.RawIOObject as ISpread).SliceCount);
				
				for (int i=0; i<spreadMax; i++)
				{
					if (FSet[i])
					{
						int id = VVVV.Utils.VMath.VMath.Zmod(FIndex[i], table.Rows.Count);
						
						for (int c=0; c<table.Columns.Count; c++)
						{
							string _n = table.Columns[c].ColumnName;
							string _t = table.Columns[c].DataType.ToString();
							string key = _n+_t;
							table.Set((pins[key].RawIOObject as ISpread)[i],id,c);
						}
					}
				}
			}
		}
	}
	
	[PluginInfo(Name = "InsertRow", Category = TableDefaults.CATEGORY, Version = "Dynamic", Help = "Inserts values of a table row", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	public class InsertRowDynamicNode : TableRowDynamicPluginEvaluate
	{
		#region fields & pins
		[Input("Index", Order = int.MaxValue-1)]
		ISpread<int> FIndex;
		
		[Input("Insert", IsBang = true, Order = int.MaxValue)]
		IDiffSpread<bool> FInsert;
		
		[Output("Status", Order = int.MaxValue)]
		ISpread<string> FStatus;
		
		[Import()]
		ILogger FLogger;
		#endregion
		
		protected override IOAttribute CreatePinAttribute(string name)
		{
			return new InputAttribute(name);
		}
		
		protected override void EvaluateTable(Table table, Dictionary<string,IIOContainer> pins, bool isChanged)
		{
			if (table != null && table.Columns.Count > 0 && FInsert.IsChanged)
			{
				int spreadMax = FIndex.SliceCount.CombineWith(FInsert);
				foreach (var p in pins.Values)
					spreadMax = spreadMax.CombineSpreads((p.RawIOObject as ISpread).SliceCount);
				
				for (int i=0; i<spreadMax; i++)
				{
					if (FInsert[i])
					{
						DataRow row = table.NewRow();
						for (int c=0; c<table.Columns.Count; c++)
						{
							string _n = table.Columns[c].ColumnName;
							string _t = table.Columns[c].DataType.ToString();
							string key = _n+_t;
							row[c] = (pins[key].RawIOObject as ISpread)[i];
						}

						int index = FIndex[i];
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
						table.OnDataChange(this);
					}
				}
			}
		}
	}

}
