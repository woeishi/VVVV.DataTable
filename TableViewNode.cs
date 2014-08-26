#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V2;

#endregion usings

namespace VVVV.Nodes.Table
{
    #region PluginInfo
    [PluginInfo(Name = "TableView", Category = TableDefaults.CATEGORY, Help = "View and edit the data of a Table", Tags = TableDefaults.TAGS, Author="elliotwoods, "+TableDefaults.AUTHOR, AutoEvaluate = true)]
    #endregion PluginInfo
    public class TableViewNode : UserControl, IPluginEvaluate
    {
        #region fields & pins
        [Input("Table")]
        ISpread<Table> FPinInTable;
        
        [Input("Table Index", IsSingle = true)]
        ISpread<int> FTableIndex;
        
        [Input("Allow Add Row", IsSingle = true, DefaultBoolean = true)]
        IDiffSpread<bool> FAllowAddRow;
        
        [Input("Up", IsSingle = true, IsBang = true)]
        ISpread<bool> FUp;

        [Input("Down", IsSingle = true, IsBang = true)]
        ISpread<bool> FDown;
        
        [Input("Edit On Enter", IsSingle = true)]
        IDiffSpread<bool> FEditOnEnter;

        [Output("Selected Row")]
        ISpread<int> FCurrentIndex;
        
        [Import()]
        ILogger FLogger;
        IPluginHost2 FPluginHost;

        private DataGridView FDataGridView;
        private FolderBrowserDialog FFolderBrowserDialog;
        private Table FData;
        private bool FNeedsUpdate = false;
        
        #endregion fields & pins

        #region constructor and init

        [ImportingConstructor]
        public TableViewNode(IPluginHost2 plugHost2)
        {
            FPluginHost = plugHost2;
            
            //setup the gui
            InitializeComponent();
            FDataGridView.CellValueChanged += FDataGridView_CellValueChanged;
            FDataGridView.RowsRemoved += FDataGridView_RowsRemoved;
            
        }

        void FDataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            OnDataChanged();
        }

        void OnDataChanged()
        {
            if (this.FData != null)
            {
                FData.OnDataChange(this);
            }
        }

        void InitializeComponent()
        {
            
            System.Windows.Forms.DataGridViewCellStyle headerCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle defaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle rowHeadersDefaultCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
            
            this.FDataGridView = new System.Windows.Forms.DataGridView();
            this.FFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            
            ((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).BeginInit();
            this.SuspendLayout();
            
            // 
            // FDataGridView
            // 
            
            
            this.FDataGridView.AllowDrop = true;
            this.FDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.FDataGridView.BackgroundColor = System.Drawing.Color.DimGray;
            this.FDataGridView.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.FDataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;

            this.FDataGridView.Name = "FDataGridView";
            this.FDataGridView.Location = new System.Drawing.Point(0, 0);
            this.FDataGridView.Size = this.Size;
            this.FDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            

            headerCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            headerCellStyle.BackColor = System.Drawing.Color.DimGray;
            headerCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            headerCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            headerCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            headerCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            headerCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.FDataGridView.ColumnHeadersDefaultCellStyle = headerCellStyle;
            this.FDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;

            defaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            defaultCellStyle.BackColor = System.Drawing.Color.DimGray;
            defaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            defaultCellStyle.ForeColor = System.Drawing.Color.White;
            defaultCellStyle.SelectionBackColor = System.Drawing.Color.Black;
            defaultCellStyle.SelectionForeColor = System.Drawing.Color.White;
            defaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.FDataGridView.DefaultCellStyle = defaultCellStyle;
            this.FDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FDataGridView.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnKeystrokeOrF2;
            this.FDataGridView.Location = new System.Drawing.Point(0, 0);
            this.FDataGridView.Name = "FDataGridView";
            this.FDataGridView.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            rowHeadersDefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            rowHeadersDefaultCellStyle.BackColor = System.Drawing.Color.DimGray;
            rowHeadersDefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            rowHeadersDefaultCellStyle.ForeColor = System.Drawing.SystemColors.WindowText;
            rowHeadersDefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            rowHeadersDefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            rowHeadersDefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.FDataGridView.RowHeadersDefaultCellStyle = rowHeadersDefaultCellStyle;
            this.FDataGridView.TabIndex = 0;
            
            this.FDataGridView.CellMouseDown += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.FDataGridView_CellMouseDown);
            this.FDataGridView.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.FDataGridView_CellValueChanged);
            this.FDataGridView.ColumnAdded += new System.Windows.Forms.DataGridViewColumnEventHandler(this.FDataGridView_ColumnAdded);
            this.FDataGridView.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.FDataGridView_DataError);
            this.FDataGridView.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.FDataGridView_UserDeletedRow);
            this.FDataGridView.DragDrop += new System.Windows.Forms.DragEventHandler(this.FDataGridView_DragDrop);
            this.FDataGridView.DragOver += new System.Windows.Forms.DragEventHandler(this.FDataGridView_DragOver);
            this.FDataGridView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.FDataGridView_KeyDown);
            this.FDataGridView.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.FDataGridView_CellValidating);
            this.FDataGridView.MouseMove += new System.Windows.Forms.MouseEventHandler(FDataGridView_MouseMove);
            this.FDataGridView.CellMouseDoubleClick += HandleCellMouseDoubleClick;
            this.FDataGridView.CurrentCellDirtyStateChanged += HandleCurrentCellDirtyStateChanged;
            
            // 
            // XMLGridViewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Size = new System.Drawing.Size(563, 231);
            
            DataGridViewCellStyle defaultStyle = new DataGridViewCellStyle();
            defaultStyle.SelectionBackColor = System.Drawing.Color.Gray;
            defaultStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            defaultStyle.Format = string.Empty;
            defaultStyle.NullValue = "+";
            
            DataGridViewCellStyle defaultAltStyle = new DataGridViewCellStyle(defaultStyle);
            defaultAltStyle.BackColor = System.Drawing.Color.FromArgb(224,224,224);
            defaultAltStyle.SelectionBackColor = System.Drawing.Color.Gray;
            

            // 
            // TableViewNode
            // 
            this.Controls.Add(this.FDataGridView);
            this.Name = "TableViewNode";
            ((System.ComponentModel.ISupportInitialize)(this.FDataGridView)).EndInit();
            this.ResumeLayout(false);

        }
        
        void FDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            this.Validate();
            OnDataChanged();
        }
        
        void FDataGridView_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            this.Validate();
            OnDataChanged();
        }
		
        //apply stle for one column
        void FDataGridView_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
            e.Column.MinimumWidth = 60;
            
            if(e.Column is DataGridViewCheckBoxColumn)
            {
                var col = e.Column as DataGridViewCheckBoxColumn;
                col.FlatStyle = FlatStyle.Popup;
            }
            else if(e.Column is DataGridViewButtonColumn)
            {
                var col = e.Column as DataGridViewButtonColumn;
                col.FlatStyle = FlatStyle.Flat;
            }
            
            if(e.Column.Index % 2 == 1)
            {
                var gray = 95;
                var c =  Color.FromArgb(gray, gray, gray);
                e.Column.DefaultCellStyle.BackColor = c;
                e.Column.HeaderCell.Style.BackColor = c;
            }
        }
        
        //parsing error
        void FDataGridView_DataError(object sender, System.Windows.Forms.DataGridViewDataErrorEventArgs e)
        {
            if(e.Context.HasFlag(DataGridViewDataErrorContexts.Parsing))
            {
                MessageBox.Show("Parsing Error, please enter a cell value with correct formatting");
                e.Cancel = true;
            }
            else
            {
                //throw e.Exception;
                //MessageBox.Show("Data Grid Error: " + e.Exception.Message);
            }
        }
        
        #region reorder
        private Rectangle FDragBoxFromMouseDown;
        private int FRowIndexFromMouseDown;
        private int FRowIndexOfItemUnderMouseToDrop;

        private void FDataGridView_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void FDataGridView_DragDrop(object sender, DragEventArgs e)
        {
            // The mouse locations are relative to the screen, so they must be
            // converted to client coordinates.
            Point clientPoint = FDataGridView.PointToClient(new Point(e.X, e.Y));

            // Get the row index of the item the mouse is below.
            FRowIndexOfItemUnderMouseToDrop =
                FDataGridView.HitTest(clientPoint.X, clientPoint.Y).RowIndex;

            // If the drag operation was a move then remove and insert the row.
            if (e.Effect== DragDropEffects.Move)
            {
                int idx = FDataGridView.SelectedRows[0].Index;
                var value = FData.Rows[idx];
                //clone
                var newRow = FData.NewRow();
                newRow.ItemArray = value.ItemArray;
                
                if(FRowIndexOfItemUnderMouseToDrop < 0)
                    FRowIndexOfItemUnderMouseToDrop = FData.Rows.Count - 1;
                 
                FData.Rows.Remove(value);                
                FData.Rows.InsertAt(newRow, FRowIndexOfItemUnderMouseToDrop);
                
                OnDataChanged();
            }
        }
        
        void FDataGridView_CellMouseDown(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(e.ColumnIndex != -1)
            {
                if(e.Button == MouseButtons.Right)
                {
                    var index = e.ColumnIndex;
                    
                    if(index >= 0 && FData.Columns[index].DataType == typeof(string))
                    {
                        FFolderBrowserDialog.SelectedPath = (string)FDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
                        FFolderBrowserDialog.ShowDialog(this);
                        FDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = FFolderBrowserDialog.SelectedPath;
                        return; // done
                    }
                    if(index >= 0 && FData.Columns[index].DataType == typeof(bool))
                    {
                        return;
                    }
                    else //number box, start value drag
                    {
                        FMouseDragging = true;
                        FMouseLast = e.Location;
                    }
                }
            }
            
            // Get the index of the item the mouse is below.
            FRowIndexFromMouseDown = e.RowIndex;
            if (FRowIndexFromMouseDown != -1)
            {
                // Remember the point where the mouse down occurred.
                // The DragSize indicates the size that the mouse can move
                // before a drag event should be started.
                Size dragSize = SystemInformation.DragSize;

                // Create a rectangle using the DragSize, with the mouse position being
                // at the center of the rectangle.
                FDragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width / 2),
                                                                e.Y - (dragSize.Height / 2)),
                                                      dragSize);
            }
            else
                // Reset the rectangle if the mouse is not over an item in the ListBox.
                FDragBoxFromMouseDown = Rectangle.Empty;
        }
        #endregion reorder
        
        #region copy/paste
        private void FDataGridView_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.C:
                            CopyToClipboard();
                            break;

                        case Keys.V:
                            PasteClipboardValue();
                            break;
                            
                        case Keys.X:
                            CopyToClipboard();

                            //Clear selected cells
                            foreach (DataGridViewCell dgvCell in FDataGridView.SelectedCells)
                                dgvCell.Value = string.Empty;
                            break;
                            
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Copy/paste operation failed. "+ex.Message, "Copy/Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CopyToClipboard()
        {
            //Copy to clipboard
            DataObject dataObj = FDataGridView.GetClipboardContent();
            if (dataObj != null)
                Clipboard.SetDataObject(dataObj);
        }

        private void PasteClipboardValue()
        {
            FDataGridView.BeginEdit(false);
            //Show Error if no cell is selected
            if (FDataGridView.SelectedCells.Count == 0)
            {
                MessageBox.Show("Please select a cell", "Paste", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            //Get the satring Cell
            DataGridViewCell startCell = GetStartCell(FDataGridView);
            //Get the clipboard value in a dictionary
            Dictionary<int, Dictionary<int, string>> cbValue = ClipBoardValues(Clipboard.GetText());

            int iRowIndex = startCell.RowIndex;
            foreach (int rowKey in cbValue.Keys)
            {
                int iColIndex = startCell.ColumnIndex;
                foreach (int cellKey in cbValue[rowKey].Keys)
                {
                    //Check if the index is with in the limit
                    if (iColIndex <= FDataGridView.Columns.Count - 1 && iRowIndex <= FDataGridView.Rows.Count - 1)
                    {
                        DataGridViewCell cell = FDataGridView[iColIndex, iRowIndex];

                        cell.Value = cbValue[rowKey][cellKey];
                    }
                    iColIndex++;
                }
                iRowIndex++;
            }
            FDataGridView.EndEdit();
        }

        private DataGridViewCell GetStartCell(DataGridView dgView)
        {
            //get the smallest row,column index
            if (dgView.SelectedCells.Count == 0)
                return null;

            int rowIndex = dgView.Rows.Count - 1;
            int colIndex = dgView.Columns.Count - 1;

            foreach (DataGridViewCell dgvCell in dgView.SelectedCells)
            {
                if (dgvCell.RowIndex < rowIndex)
                    rowIndex = dgvCell.RowIndex;
                if (dgvCell.ColumnIndex < colIndex)
                    colIndex = dgvCell.ColumnIndex;
            }

            return dgView[colIndex, rowIndex];
        }

        private Dictionary<int, Dictionary<int, string>> ClipBoardValues(string clipboardValue)
        {
            Dictionary<int, Dictionary<int, string>> copyValues = new Dictionary<int, Dictionary<int, string>>();

            String[] lines = clipboardValue.Split('\n');

            for (int line = 0; line <= lines.Length - 1; line++)
            {
                copyValues[line] = new Dictionary<int, string>();
                String[] lineContent = lines[line].Split('\t');

                //if an empty cell value copied, then set the dictionay with an empty string
                //else Set value to dictionary
                if (lineContent.Length == 0)
                    copyValues[line][0] = string.Empty;
                else
                {
                    for (int j = 0; j <= lineContent.Length - 1; j++)
                        copyValues[line][j] = lineContent[j];
                }
            }
            return copyValues;
        }
        
        #endregion copy/paste
        
        
        private void FDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                System.Convert.ChangeType(e.FormattedValue,this.FDataGridView.Columns[e.ColumnIndex].ValueType);
            }
            catch
            {
                e.Cancel = true;
            }
        }

        //begin edit on double click
        void HandleCellMouseDoubleClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e)
        {
            FDataGridView.BeginEdit(true);
        }

        //commit change for checkboxes
        void HandleCurrentCellDirtyStateChanged(object sender, System.EventArgs e)
        {
            if (FDataGridView.IsCurrentCellDirty && FDataGridView.CurrentCell is DataGridViewCheckBoxCell)
            {
                FDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        Point FMouseLast;
        bool FMouseDragging = false;
        void FDataGridView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //row dragging
            if ((e.Button & MouseButtons.Left) == MouseButtons.Left)
            {
                // If the mouse moves outside the rectangle, start the drag.
                if (FDragBoxFromMouseDown != Rectangle.Empty &&
                    !FDragBoxFromMouseDown.Contains(e.X, e.Y))
                {

                    // Proceed with the drag and drop, passing in the list item.
                    DragDropEffects dropEffect = FDataGridView.DoDragDrop(
                        FDataGridView.Rows[FRowIndexFromMouseDown],
                        DragDropEffects.Move);
                }
            }
            
            //value change
            if (e.Button.HasFlag(System.Windows.Forms.MouseButtons.Right))
            {
                if (FMouseDragging)
                {
                    int stepOrder = 0;
                    Debug.Print(Form.ModifierKeys.ToString());
                    stepOrder += Form.ModifierKeys.HasFlag(Keys.Shift) ? 1 : 0;
                    stepOrder += Form.ModifierKeys.HasFlag(Keys.Control)? 1 : 0;
                    stepOrder *= Form.ModifierKeys.HasFlag(Keys.Alt) ? 1 : -1;
                    double step = 0.01 * Math.Pow(10, stepOrder);

                    double delta = - step * (double)(e.Y - FMouseLast.Y);
                    foreach (DataGridViewCell cell in FDataGridView.SelectedCells)
                    {
                        if (cell.ValueType == typeof(System.Double) && cell.RowIndex < FData.Rows.Count) //avoids selection of the 'new row' at bottom or invalid cells
                            cell.Value = (double)cell.Value + delta;
                        if (cell.ValueType == typeof(float) && cell.RowIndex < FData.Rows.Count) //avoids selection of the 'new row' at bottom or invalid cells
                            cell.Value = (float)cell.Value + (float)delta;
                        else if (cell.ValueType == typeof(int) && cell.RowIndex < FData.Rows.Count)
                            cell.Value = (int)cell.Value + (int)(delta*100);
                        else if (cell.ValueType == typeof(Int64) && cell.RowIndex < FData.Rows.Count)
                            cell.Value = (Int64)cell.Value + (Int64)(delta*100);
                    }
                    OnDataChanged();
                }
                
                FMouseLast = e.Location;
            }
            else
                FMouseDragging = false;
        }
        #endregion constructor and init

        public void Evaluate(int SpreadMax)
        {
            if (FPinInTable.SliceCount == 0)
            {
                FData = null;
                FDataGridView.DataSource = null;
                return;
            }
            
            if (FPinInTable[FTableIndex[0]] != FData)
            {
                FData = FPinInTable[FTableIndex[0]];
                FDataGridView.DataSource = FData;

                if (FData != null)
                {
                    FDataGridView.TopLeftHeaderCell.Value = FData.TableName;
                    FDataGridView.AllowUserToAddRows = FAllowAddRow[0];
                    foreach(DataGridViewColumn column in FDataGridView.Columns)
                    {
                        column.SortMode = DataGridViewColumnSortMode.NotSortable;
                    }
                }
                
                FPluginHost.Window.Caption = FData.TableName;
            }

            if (FData == null)
                return;
            
            if (FAllowAddRow.IsChanged)
                FDataGridView.AllowUserToAddRows = FAllowAddRow[0];
            
            if(FEditOnEnter.IsChanged)
                FDataGridView.EditMode = FEditOnEnter[0] ? DataGridViewEditMode.EditOnEnter : DataGridViewEditMode.EditOnKeystrokeOrF2;

            if (FData.Rows.Count > 0)
            {
                bool moveRow = false;
                int selectedRow = 0;

                if (FDataGridView.SelectedCells.Count > 0)
                    selectedRow = FDataGridView.SelectedCells[0].RowIndex;
                else
                    selectedRow = 0;

                if (FUp[0])
                {
                    selectedRow++;
                    selectedRow %= FData.Rows.Count;
                    moveRow = true;
                }

                if (FDown[0])
                {
                    selectedRow--;
                    if (selectedRow < 0)
                        selectedRow += FData.Rows.Count;
                    moveRow = true;
                }

                if (moveRow)
                {
                    FDataGridView.ClearSelection();
                    FDataGridView.Rows[selectedRow].Selected = true;
                }
            }

            if (FData.Rows.Count == 0)
            {
                FCurrentIndex.SliceCount = 1;
                FCurrentIndex[0] = 0;
            }
            else
            {
                var rows = FDataGridView.SelectedRows;
                if (rows.Count > 0)
                {
                    FCurrentIndex.SliceCount = 0;
                    foreach (DataGridViewRow row in rows)
                    {
                        FCurrentIndex.Add(row.Index);
                    }
                }
                else
                {
                    int row = FDataGridView.CurrentCellAddress.Y;
                    FCurrentIndex.SliceCount = 1;
                    FCurrentIndex[0] = row;
                }
            }
        }
    }
}
