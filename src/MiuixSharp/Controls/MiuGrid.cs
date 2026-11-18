using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using MiuixSharp.Drawing;
using MiuixSharp.Theme;

namespace MiuixSharp.Controls;

/// <summary>
/// A Miuix-styled DataGridView that DERIVES FROM DataGridView — fully API-compatible
/// with existing code, with Miuix surface colors applied to the header, cells and
/// selection. Standard DataGridView painting is preserved so header captions and
/// all built-in behaviors work unchanged.
/// </summary>
public class MiuGrid : DataGridView
{
    public MiuGrid()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        BorderStyle = BorderStyle.None;
        BackgroundColor = MiuTheme.Surface;
        GridColor = MiuTheme.Outline;
        Font = MiuTheme.Font;
        RowHeadersVisible = false;
        AllowUserToAddRows = false;
        AllowUserToDeleteRows = false;
        SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        EnableHeadersVisualStyles = false;
        ColumnHeadersHeight = 36;
        RowTemplate.Height = 36;

        ColumnHeadersVisible = true;
        ColumnHeadersDefaultCellStyle.BackColor = MiuTheme.SurfaceAlt;
        ColumnHeadersDefaultCellStyle.ForeColor = MiuTheme.Colors.OnSurfaceVariantSummary;
        ColumnHeadersDefaultCellStyle.Font = MiuTheme.FontSmall;
        ColumnHeadersDefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
        ColumnHeadersDefaultCellStyle.Padding = new Padding(4, 0, 2, 0);
        ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;

        DefaultCellStyle.ForeColor = MiuTheme.Text;
        DefaultCellStyle.BackColor = MiuTheme.Surface;
        DefaultCellStyle.SelectionBackColor = RoundedPath.Blend(MiuTheme.PrimaryContainer, MiuTheme.Surface, 0.35f);
        DefaultCellStyle.SelectionForeColor = MiuTheme.Text;
        DefaultCellStyle.Padding = new Padding(6, 0, 4, 0);

        MiuTheme.Changed += (_, __) => ApplyTheme();
    }

    private void ApplyTheme()
    {
        BackgroundColor = MiuTheme.Surface;
        GridColor = MiuTheme.Outline;
        ColumnHeadersDefaultCellStyle.BackColor = MiuTheme.SurfaceAlt;
        ColumnHeadersDefaultCellStyle.ForeColor = MiuTheme.Colors.OnSurfaceVariantSummary;
        DefaultCellStyle.ForeColor = MiuTheme.Text;
        DefaultCellStyle.BackColor = MiuTheme.Surface;
        DefaultCellStyle.SelectionBackColor = RoundedPath.Blend(MiuTheme.PrimaryContainer, MiuTheme.Surface, 0.35f);
        DefaultCellStyle.SelectionForeColor = MiuTheme.Text;
        Invalidate();
    }

    // NOTE: no Region clipping here — rebuilding the region on every OnPaint
    // interferes with DataGridView's header layout. Rounded look comes from the
    // containing MiuCard-style panel instead.

    // Row striping / hover are intentionally NOT drawn via OnCellPainting:
    // overriding it interfered with header caption rendering in this app, so
    // the grid uses standard painting with Miuix colors only.
}
