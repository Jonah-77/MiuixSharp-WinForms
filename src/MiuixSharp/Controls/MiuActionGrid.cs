using System.Windows.Forms;

namespace MiuixSharp.Controls;

/// <summary>
/// A <see cref="DataGridView"/> that shows the hand cursor over designated "action" columns.
///
/// The built-in grid resets <see cref="Control.Cursor"/> inside its own mouse handling, so
/// reacting to <c>CellMouseEnter</c> is not enough — the cursor has to be set *after* the
/// base implementation ran. This subclass does exactly that, which is why it exists
/// instead of a plain event handler.
///
/// Usage: add column names to <see cref="ActionColumns"/>.
/// </summary>
[System.Drawing.ToolboxBitmap(typeof(MiuActionGrid), "MiuixSharp.Icons.MiuActionGrid.bmp")]
[System.ComponentModel.Description("带行内操作列的表格")]
public class MiuActionGrid : DataGridView
{
    private readonly HashSet<string> _actionColumns = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Names of the columns that behave like buttons (hand cursor on hover).</summary>
    public IEnumerable<string> ActionColumns => _actionColumns;

    public MiuActionGrid()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        BorderStyle = BorderStyle.None;
        RowHeadersVisible = false;
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        EnableHeadersVisualStyles = false;
    }

    /// <summary>Mark a column as an action column.</summary>
    public void AddActionColumn(string name)
    {
        if (!string.IsNullOrEmpty(name)) _actionColumns.Add(name);
    }

    /// <summary>Mark several columns as action columns.</summary>
    public void AddActionColumns(params string[] names)
    {
        if (names == null) return;
        foreach (var n in names) AddActionColumn(n);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        bool hand = false;
        if (_actionColumns.Count > 0)
        {
            var hit = HitTest(e.X, e.Y);
            if (hit.ColumnIndex >= 0 && hit.ColumnIndex < Columns.Count)
                hand = _actionColumns.Contains(Columns[hit.ColumnIndex].Name);
        }

        // Assigning the same cursor repeatedly is harmless, so this can run on every move.
        Cursor = hand ? Cursors.Hand : Cursors.Default;
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        Cursor = Cursors.Default;
    }
}
