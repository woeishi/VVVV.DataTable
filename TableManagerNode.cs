#region usings
using System;
using System.Data;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Nodes.Table
{

	#region PluginInfo
	[PluginInfo(Name = "Table", Category = TableDefaults.CATEGORY, Help = "Create an instance of a Table to be used elsewhere", Tags = TableDefaults.TAGS, Author = "elliotwoods, "+TableDefaults.AUTHOR, AutoEvaluate = true)]
	#endregion PluginInfo
	public class TableManagerNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Table Name", DefaultString="MyTable")]
		IDiffSpread<string> FTableName;
		
		[Input("Column Names", DefaultString="x,y,z")]
		IDiffSpread<string> FPinInColumnNames;

		[Input("Clear", IsBang = true)]
		ISpread<bool> FPinInClear;
		
		[Input("Load", IsBang = true, IsSingle = true)]
		ISpread<bool> FPinInLoad;

		[Input("Save", IsBang = true, IsSingle = true)]
		ISpread<bool> FPinInSave;

		[Input("Auto Save", IsSingle = true)]
		IDiffSpread<bool> FPinInAutoSave;

		[Input("Filename", IsSingle = true, DefaultString="spreadtable.xml", FileMask="XML File (*.xml)|*.xml", StringType=StringType.Filename)]
		IDiffSpread<string> FPinInFilename;

		[Output("Table")]
		ISpread<Table> FPinOutTable;
		
		[Output("Has Changed")]
		ISpread<bool> FHasChanged;

		[Output("Status")]
		ISpread<string> FOutStatus;
		
		[Import()]
		ILogger FLogger;

		DataSet FDataSet = new DataSet();
		Spread<Table> FTables = new Spread<Table>(0);
		Spread<bool> FFreshData = new Spread<bool>(0);
		
		bool AnyChanged = false;
		bool FFirstRun = true;
		string FFilename = "spreadtable.xml";
		#endregion fields & pins
		
		void FTable_DataChanged(Object sender, TableEventArgs e)
		{
			if (sender != this)
			{
				FFreshData[e.Table.SliceIndex] = true;
				e.Table.SetupColumns(FPinInColumnNames[e.Table.SliceIndex]);
			}
		}
		
		private Table CreateTable(int slice)
		{
			Table t = new Table();
			t.InitTable(FTableName[slice], slice);
			
			FDataSet.Tables.Add(t);
			t.DataChanged += new Table.DataChangedHandler(FTable_DataChanged);
			return t;
		}
		
		private void RemoveTable(Table t)
		{
			t.DataChanged -= new Table.DataChangedHandler(FTable_DataChanged);
			FDataSet.Tables.RemoveAt(t.SliceIndex);
			t.Dispose();
		}

		
		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (FPinInFilename.IsChanged)
				FFilename = FPinInFilename[0];
			
			if (FPinInLoad[0] || (FFirstRun && FPinInAutoSave[0]))
				Load();
			
			if ((!FTableName.IsChanged) && (!FPinInColumnNames.IsChanged))
				spreadMax = FDataSet.Tables.Count;
			
			FFreshData.ResizeAndDismiss(spreadMax, () => true);
			FTables.Resize(spreadMax, CreateTable, (t) => RemoveTable(t));
			
			FOutStatus.SliceCount = spreadMax;
			FHasChanged.SliceCount = spreadMax;
			FPinOutTable.SliceCount = spreadMax;
			
			for (int i=0; i<spreadMax; i++)
			{
				if (FHasChanged[i])
					FHasChanged[i]=false;
				
				//set table name
				if (FTableName.IsChanged)
				{
					if (!FTables[i].CompareTableName(FTableName[i],i))
					{
						FTables[i].InitTable(FTableName[i], i);
						FFreshData[i] = true;
					}
				}
				
				//set column names;
				if (FPinInColumnNames.IsChanged)
				{
					if (FTables[i].ColumnNames != FPinInColumnNames[i])
					{
						try
						{
							FTables[i].SetupColumns(FPinInColumnNames[i]);
							FFreshData[i] = true;
							FOutStatus[i] = "OK";
						}
						catch(Exception e)
						{
							FOutStatus[i] = e.Message;
						}
					}
				}
				
				if (FPinInClear[i])
				{
					FTables[i].ClearAll();
					FTables[i].SetupColumns(FPinInColumnNames[i]);
					FFreshData[i] = true;
				}
				
				if (FPinInLoad[0])
					FFreshData[i] = true;
	
				if (FFreshData[i] || FFirstRun)
				{
					FTables[i].OnDataChange(this);
					FFreshData[i] = false;
					FPinOutTable[i] = FTables[i];
					FHasChanged[i] = true;
					AnyChanged = true;
				}
			}
			
			if (FPinInSave[0])
				Save();
			
			if (FPinInAutoSave.IsChanged || AnyChanged)
				if (FPinInAutoSave[0])
					Save();
			
			if (FFirstRun)
				FFirstRun = false;
			
			AnyChanged = false;
		}

		private bool Load()
		{
			if (FFilename != "")
			{
				try
				{
					for (int r=FTables.SliceCount-1; r>=0; r--)
					{
						FTables[r].Dispose();
						FTables.RemoveAt(r);
					}
					FDataSet.Dispose();
					FDataSet = new DataSet();
					FDataSet.DataSetName = System.IO.Path.GetFileNameWithoutExtension(FFilename);
					FDataSet.ReadXml(FFilename);
					FDataSet.AcceptChanges();
					FOutStatus.SliceCount = 0;
					for (int i=0; i<FDataSet.Tables.Count; i++)
					{
						System.IO.MemoryStream s = new System.IO.MemoryStream();
						FDataSet.Tables[i].WriteXml(s,XmlWriteMode.WriteSchema, false);
						Table t = new Table();
						s.Position= 0;
						t.ReadXml(s);
						t.InitTable(i);
						FTables.Add(t);
						t.DataChanged += new Table.DataChangedHandler(FTable_DataChanged);
						s.Dispose();
						FOutStatus.Add(t.NiceName+" loaded OK");
					}
					return true;
				}
				catch(Exception e)
				{
					for (int i=0; i<FOutStatus.SliceCount; i++)
						FOutStatus[i] = e.Message;
					return false;
				}
			}
			else
			{
				for (int i=0; i<FOutStatus.SliceCount; i++)
						FOutStatus[i] = "no file specified to load";
				return false;
			}
		}

		private bool Save()
		{
			if (FFilename != "")
			{
				try
				{
					System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(FFilename));
					for (int i=0; i<FTables.SliceCount; i++)
					{
						System.IO.MemoryStream s = new System.IO.MemoryStream();
						FTables[i].WriteXml(s,XmlWriteMode.WriteSchema, false);
						s.Position= 0;
						FDataSet.Tables[i].Clear();
						FDataSet.Tables[i].ReadXml(s);
						s.Dispose();
					}
					FDataSet.WriteXml(FFilename, XmlWriteMode.WriteSchema);
					for (int i=0; i<FOutStatus.SliceCount; i++)
						FOutStatus[i] = FFilename+" saved OK";
					return true;
				}
				catch (Exception e)
				{
					for (int i=0; i<FOutStatus.SliceCount; i++)
						FOutStatus[i] = e.Message;
					return false;
				}
			}
			else
			{
				for (int i=0; i<FOutStatus.SliceCount; i++)
						FOutStatus[i] = "no file specified to save to";
				return false;
			}
		}
	}
}
