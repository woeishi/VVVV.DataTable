#region usings
using System;
using System.Data;
using System.Collections.Generic;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Core.Logging;

#endregion usings

namespace VVVV.Nodes.Table
{
	internal class TableCommands
	{
		public List<Action<ISpread<Table>,Table.DataChangedHandler>> cmds;
		public TableCommands()
		{
			this.cmds = new List<Action<ISpread<Table>,Table.DataChangedHandler>>();
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "CreateTable", Category = TableDefaults.CATEGORY, Help = "Create an instance of a Table to be used elsewhere", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR, AutoEvaluate = true)]
	#endregion PluginInfo
	public class CreateTableNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Commands In", IsSingle = true)]
		ISpread<TableCommands> FIn;
		
		[Input("Table Name", DefaultString="MyTable")]
		ISpread<string> FTableName;
		
		[Input("Column Names", DefaultString="x,y,z")]
		ISpread<string> FColumnNames;
		
		[Input("Column Types", DefaultString="d,d,d")]
		ISpread<string> FColumnTypes;
		
//		[Input("Index")] 
//		ISpread<int> FIndex;
		
		[Input("Insert", IsBang = true)]
		IDiffSpread<bool> FInsert;
		
		[Output("Commands Out")]
		ISpread<TableCommands> FOut;
		
		[Import()]
		ILogger FLogger;
		#endregion
		
		public void Evaluate(int spreadMax)
		{
			FOut[0] = FIn[0];
			if (FInsert.IsChanged)
			{
				for (int i=0; i<spreadMax; i++)
				{
					if (FInsert[i])
					{
						Action<ISpread<Table>,Table.DataChangedHandler> create = delegate(ISpread<Table> tables,Table.DataChangedHandler eventHandler)
						{
							Table t = new Table();
							t.InitTable(FTableName[i], tables.SliceCount);
							tables.Add(t);
							
							t.SetupColumns(FColumnNames[i],FColumnTypes[i]);
							t.DataChanged += eventHandler;
						};
						if (FOut[0] == null)
							FOut[0] = new TableCommands();
						FOut[0].cmds.Add(create);
					}
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Table", Category = TableDefaults.CATEGORY, Help = "Create an instance of a Table to be used elsewhere", Tags = TableDefaults.TAGS, Author = "elliotwoods, "+TableDefaults.AUTHOR, AutoEvaluate = true)]
	#endregion PluginInfo
	public class TableManagerNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Commands In", IsSingle = true)]
		ISpread<TableCommands> FCmds;
		
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
		ISpread<Table> FTables;
		
		[Output("Has Changed")]
		ISpread<bool> FHasChanged;

		[Output("Status")]
		ISpread<string> FOutStatus;
		
		[Import()]
		ILogger FLogger;

		Spread<bool> FFreshData = new Spread<bool>(0);
		
		bool AnyChanged = false;
		bool FFirstRun = true;
		string FFilename = "spreadtable.xml";
		#endregion fields & pins
		
		public void OnImportsSatisfied()
        {
			FTables.SliceCount = 0;
		}
		
		void FTable_DataChanged(Object sender, TableEventArgs e)
		{
			if (sender != this)
			{
				FFreshData[e.Table.SliceIndex] = true;
			}
		}
		
		
		//called when data for any output pin is requested
		public void Evaluate(int spreadMax)
		{
			if (FCmds.SliceCount == 1)
			{
				if (FCmds[0] != null)
				{
					foreach (var a in FCmds[0].cmds)
					{
						a(FTables,FTable_DataChanged);
					}
				}
			}
			
			if (FPinInFilename.IsChanged)
				FFilename = FPinInFilename[0];
			
			if (FPinInLoad[0] || (FFirstRun && FPinInAutoSave[0]))
				Load();
			
			
			FFreshData.ResizeAndDismiss(FTables.SliceCount, () => true);
			
			FOutStatus.SliceCount = FTables.SliceCount;
			FHasChanged.SliceCount = FTables.SliceCount;
			for (int i=0; i<FTables.SliceCount; i++)
			{
				if (FHasChanged[i])
					FHasChanged[i]=false;
				
				if (FPinInClear[i])
				{
					FTables[i].Rows.Clear();
					FFreshData[i] = true;
				}
				
				if (FPinInLoad[0])
					FFreshData[i] = true;
	
				if (FFreshData[i] || FFirstRun)
				{
					FTables[i].OnDataChange(this);
					FFreshData[i] = false;
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
					FTables.ResizeAndDispose(0, () => new Table());
					
					DataSet ds = new DataSet();
					ds.DataSetName = System.IO.Path.GetFileNameWithoutExtension(FFilename);
					ds.ReadXml(FFilename);
					ds.AcceptChanges();
					FOutStatus.SliceCount = 0;
					for (int i=0; i<ds.Tables.Count; i++)
					{
						System.IO.MemoryStream s = new System.IO.MemoryStream();
						ds.Tables[i].WriteXml(s,XmlWriteMode.WriteSchema, false);
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
					
					DataSet ds = new DataSet();
					ds.DataSetName =System.IO.Path.GetFileNameWithoutExtension(FFilename);
					for (int i=0; i<FTables.SliceCount; i++)
					{
						System.IO.MemoryStream s = new System.IO.MemoryStream();
						FTables[i].WriteXml(s,XmlWriteMode.WriteSchema, false);
						s.Position= 0;
						ds.Tables.Add(FTables[i].TableName);
						ds.Tables[FTables[i].TableName].ReadXml(s);
						s.Dispose();
					}
					ds.WriteXml(FFilename, XmlWriteMode.WriteSchema);
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
