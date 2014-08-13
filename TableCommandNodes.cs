using System;
using System.Data;
using System.Collections.Generic;

using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Nodes.Table
{
	internal class TableCommands
	{
		public List<Func<ISpread<Table>,Table.DataChangedHandler, int>> cmds;
		public TableCommands()
		{
			this.cmds = new List<Func<ISpread<Table>,Table.DataChangedHandler, int>>();
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "CreateTable", Category = TableDefaults.CATEGORY, Help = "Create an instance of a Table to be used elsewhere", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR)]
	#endregion PluginInfo
	public class CreateTableNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Commands", IsSingle = true)]
		ISpread<TableCommands> FInCmds;
		
		[Input("Table Name", DefaultString="MyTable")]
		ISpread<string> FTableName;
		
		[Input("Column Names", DefaultString="x,y,z")]
		ISpread<string> FColumnNames;
		
		[Input("Column Types", DefaultString="d,d,d")]
		ISpread<string> FColumnTypes;
		
		[Input("Index")] 
		ISpread<int> FIndex;
		
		[Input("Insert", IsBang = true)]
		IDiffSpread<bool> FInsert;
		
		[Output("Commands")]
		ISpread<TableCommands> FOutCmds;
		#pragma warning restore
		#endregion
		
		public void Evaluate(int spreadMax)
		{
			FOutCmds[0] = FInCmds[0];
			if (FInsert.IsChanged)
			{
				for (int i=0; i<spreadMax; i++)
				{
					if (FInsert[i])
					{
						Func<ISpread<Table>,Table.DataChangedHandler,int> create = delegate(ISpread<Table> tables,Table.DataChangedHandler eventHandler)
						{
							int index = VVVV.Utils.VMath.VMath.Zmod(FIndex[i],tables.SliceCount+1);
							int duplicate = -1;
							for (int d=0; d<tables.SliceCount; d++)
								if (tables[d].TableName == FTableName[i])
									duplicate = d;
							if (duplicate > -1)
							{
								throw new DuplicateNameException(FTableName[i]+" already exists|"+duplicate.ToString());
							}
							else
							{
								Table t = new Table();
								t.TableName = FTableName[i];
								
								tables.Insert(index,t);
								
								t.SetupColumns(FColumnNames[i],FColumnTypes[i]);
								t.DataChanged += eventHandler;
							}
							return index;
						};
						if (FOutCmds[0] == null)
							FOutCmds[0] = new TableCommands();
						FOutCmds[0].cmds.Add(create);
					}
				}
			}
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "DeleteTable", Category = TableDefaults.CATEGORY, Help = "Delete an instance of a Table at a specific index", Tags = TableDefaults.TAGS, Author = TableDefaults.AUTHOR)]
	#endregion PluginInfo
	public class DeleteTableNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Commands", IsSingle = true)]
		ISpread<TableCommands> FInCmds;
		
		[Input("Index")] 
		ISpread<int> FIndex;
		
		[Input("Delete", IsBang = true)]
		IDiffSpread<bool> FDelete;
		
		[Output("Commands")]
		ISpread<TableCommands> FOutCmds;
		#pragma warning restore
		#endregion
		
		public void Evaluate(int spreadMax)
		{
			FOutCmds[0] = FInCmds[0];
			if (FDelete.IsChanged)
			{
				for (int i=0; i<spreadMax; i++)
				{
					if (FDelete[i])
					{
						Func<ISpread<Table>,Table.DataChangedHandler,int> create = delegate(ISpread<Table> tables,Table.DataChangedHandler eventHandler)
						{
							int index = VVVV.Utils.VMath.VMath.Zmod(FIndex[i],tables.SliceCount);
							tables[index].Dispose();
							tables.RemoveAt(index);
							return index;
						};
						if (FOutCmds[0] == null)
							FOutCmds[0] = new TableCommands();
						FOutCmds[0].cmds.Add(create);
					}
				}
			}
		}
	}
}
