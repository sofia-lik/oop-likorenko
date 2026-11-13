using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;

namespace oop_lab2.WinFormsApp;

public partial class MainForm : Form
{
    private const string SelectTool = "Select";

    private Drawing _drawing = new();
    private History _history = new();
    private Shape? _current;
    private Shape? _selected;

    private string _tool = "RectShape";
    private string _color = "#000000";
    private int _width = 2;

    private readonly DrawPanel _canvas = new();
    private readonly List<Button> _colorButtons = new();

    private Point _lastPos;
    private Point _moveStart;
    private bool _isMoving;

    public MainForm()
    {
        Text = "Paint";
        Size = new Size(1000, 700);

        BuildCanvas();
        BuildToolbar();

        KeyPreview = true;
        KeyDown += OnKeyDown;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        bool typing = ActiveControl is TextBoxBase;

        if (e.KeyCode == Keys.Delete && _selected != null && !typing)
        {
            _history.Do(new RemoveShapeCommand(_drawing, _selected));
            _selected = null;
            _canvas.Invalidate();
            e.Handled = true;
        }
        else if (e.Control && e.KeyCode == Keys.Z)
        {
            UndoAction();
            e.SuppressKeyPress = true;
        }
        else if (e.Control && e.KeyCode == Keys.Y)
        {
            RedoAction();
            e.SuppressKeyPress = true;
        }
    }

    private void UndoAction()
    {
        _history.Undo();
        _selected = null;
        _canvas.Invalidate();
    }

    private void RedoAction()
    {
        _history.Redo();
        _selected = null;
        _canvas.Invalidate();
    }

    private void ClearAction()
    {
        _selected = null;
        _history.Do(new ClearCommand(_drawing));
        _canvas.Invalidate();
    }

    private void BuildToolbar()
    {
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            WrapContents = true,
            Padding = new Padding(4)
        };

        var toolNames = new Dictionary<string, string>
        {
            [SelectTool] = "Выбрать",
            ["LineShape"] = "Линия",
            ["RectShape"] = "Прямоугольник",
            ["EllipseShape"] = "Эллипс",
            ["PencilShape"] = "Карандаш"
        };

        var kinds = new List<string> { SelectTool };
        kinds.AddRange(ShapeFactory.Kinds.Keys);

        foreach (var kind in kinds)
        {
            var displayName = toolNames.TryGetValue(kind, out var name) ? name : kind;
            var rb = new RadioButton
            {
                Text = displayName,
                Tag = kind,
                AutoSize = true,
                Checked = kind == _tool
            };
            rb.CheckedChanged += (s, e) =>
            {
                var r = (RadioButton)s!;
                if (!r.Checked) return;
                _tool = (string)r.Tag!;
                _selected = null;
                _canvas.Invalidate();
            };
            bar.Controls.Add(rb);
        }

        foreach (var c in new[] { "#000000", "#FF0000", "#0000FF", "#008000", "#FFA500" })
        {
            var btn = new Button
            {
                BackColor = ColorTranslator.FromHtml(c),
                Width = 30,
                Height = 30,
                FlatStyle = FlatStyle.Flat,
                Tag = c
            };
            btn.Click += (s, e) =>
            {
                _color = c;
                UpdateColorButtons();
            };
            _colorButtons.Add(btn);
            bar.Controls.Add(btn);
        }
        UpdateColorButtons();

        bar.Controls.Add(new Label { Text = "Толщина:", AutoSize = true, Margin = new Padding(8, 8, 0, 0) });

        var tb = new TextBox { Width = 40, Text = _width.ToString() };
        tb.TextChanged += (s, e) =>
        {
            if (int.TryParse(tb.Text, out int w) && w > 0)
                _width = w;
        };
        bar.Controls.Add(tb);

        bar.Controls.Add(MakeBtn("Отменить", UndoAction));
        bar.Controls.Add(MakeBtn("Повторить", RedoAction));
        bar.Controls.Add(MakeBtn("Очистить", ClearAction));
        bar.Controls.Add(MakeBtn("Сохранить", Save));
        bar.Controls.Add(MakeBtn("Открыть", OpenFile));
        bar.Controls.Add(MakeBtn("PNG", ExportPng));

        Controls.Add(bar);
    }

    private void UpdateColorButtons()
    {
        foreach (var b in _colorButtons)
        {
            bool active = (string)b.Tag! == _color;
            b.FlatAppearance.BorderSize = active ? 3 : 1;
            b.FlatAppearance.BorderColor = active ? Color.DodgerBlue : Color.Gray;
        }
    }

    private Button MakeBtn(string text, Action action)
    {
        var b = new Button { Text = text, AutoSize = true };
        b.Click += (s, e) => action();
        return b;
    }

    private void BuildCanvas()
    {
        _canvas.Dock = DockStyle.Fill;
        _canvas.BackColor = Color.White;
        _canvas.BorderStyle = BorderStyle.FixedSingle;

        _canvas.MouseDown += OnCanvasMouseDown;
        _canvas.MouseMove += OnCanvasMouseMove;
        _canvas.MouseUp += OnCanvasMouseUp;
        _canvas.Paint += OnCanvasPaint;

        Controls.Add(_canvas);
    }

    private void OnCanvasMouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        _canvas.Focus(); 
        var p = e.Location;

        if (_tool == SelectTool)
        {
            _selected = _drawing.GetShapeAt(p);
            if (_selected != null)
            {
                _isMoving = true;
                _lastPos = p;
                _moveStart = p;
            }
        }
        else
        {
            _selected = null;
            _current = ShapeFactory.Create(_tool, p.X, p.Y, _color, _width);
        }
        _canvas.Invalidate();
    }

    private void OnCanvasMouseMove(object? sender, MouseEventArgs e)
    {
        if (_isMoving && _selected != null)
        {
            _selected.Move(e.X - _lastPos.X, e.Y - _lastPos.Y);
            _lastPos = e.Location;
            _canvas.Invalidate();
        }
        else if (_current != null)
        {
            _current.MoveEnd(e.X, e.Y);
            _canvas.Invalidate();
        }
    }

    private void OnCanvasMouseUp(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left) return;

        if (_isMoving)
        {
            _isMoving = false;
            int dx = _lastPos.X - _moveStart.X;
            int dy = _lastPos.Y - _moveStart.Y;

            if (_selected != null && (dx != 0 || dy != 0))
                _history.Record(new MoveShapeCommand(_selected, dx, dy));
        }
        else if (_current != null)
        {
            var (_, _, w, h) = _current.Box();
            if (w > 0 || h > 0)
                _history.Do(new AddShapeCommand(_drawing, _current));
            _current = null;
        }
        _canvas.Invalidate();
    }

    private void OnCanvasPaint(object? sender, PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var canvas = new GraphicsCanvas(e.Graphics);
        _drawing.Draw(canvas);
        _current?.Draw(canvas);

        if (_selected != null)
        {
            using var pen = new Pen(Color.Gray, 1) { DashStyle = DashStyle.Dash };
            var (x, y, w, h) = _selected.Box();
            e.Graphics.DrawRectangle(pen, x - 3, y - 3, w + 6, h + 6);
        }
    }

    private void Save()
    {
        using var dlg = new SaveFileDialog { Filter = "JSON|*.json" };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            File.WriteAllText(dlg.FileName, _drawing.ToJson());
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось сохранить файл: " + ex.Message);
        }
    }

    private void OpenFile()
    {
        using var dlg = new OpenFileDialog { Filter = "JSON|*.json" };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            _drawing = Drawing.FromJson(File.ReadAllText(dlg.FileName));
            _history = new History();
            _selected = null;
            _current = null;
            _canvas.Invalidate();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Не удалось открыть файл: " + ex.Message);
        }
    }

    private void ExportPng()
    {
        using var dlg = new SaveFileDialog
        {
            Filter = "PNG images|*.png",
            DefaultExt = "png"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        using var bitmap = new Bitmap(_canvas.ClientSize.Width, _canvas.ClientSize.Height);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            _drawing.Draw(new GraphicsCanvas(g));
        }
        bitmap.Save(dlg.FileName, System.Drawing.Imaging.ImageFormat.Png);
    }
}

internal class DrawPanel : Panel
{
    public DrawPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
    }
}

