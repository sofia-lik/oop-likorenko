using System.Collections.Generic;
using System.Linq;

public interface ICommand
{
    void Execute();
    void Undo();
}

public class AddShapeCommand : ICommand
{
    private readonly Drawing _drawing;
    private readonly Shape _shape;

    public AddShapeCommand(Drawing drawing, Shape shape)
    {
        _drawing = drawing;
        _shape = shape;
    }

    public void Execute() => _drawing.Add(_shape);
    public void Undo() => _drawing.Remove(_shape);
}

public class RemoveShapeCommand : ICommand
{
    private readonly Drawing _drawing;
    private readonly Shape _shape;
    private int _index = -1;

    public RemoveShapeCommand(Drawing drawing, Shape shape)
    {
        _drawing = drawing;
        _shape = shape;
    }

    public void Execute()
    {
        _index = _drawing.IndexOf(_shape);
        _drawing.Remove(_shape);
    }

    public void Undo() => _drawing.Insert(_index, _shape);
}

public class MoveShapeCommand : ICommand
{
    private readonly Shape _shape;
    private readonly int _dx, _dy;

    public MoveShapeCommand(Shape shape, int dx, int dy)
    {
        _shape = shape;
        _dx = dx;
        _dy = dy;
    }

    public void Execute() => _shape.Move(_dx, _dy);
    public void Undo() => _shape.Move(-_dx, -_dy);
}

public class ClearCommand : ICommand
{
    private readonly Drawing _drawing;
    private readonly Shape[] _saved;

    public ClearCommand(Drawing drawing)
    {
        _drawing = drawing;
        _saved = drawing.GetAll().ToArray();
    }

    public void Execute() => _drawing.Clear();

    public void Undo()
    {
        foreach (var s in _saved) _drawing.Add(s);
    }
}

public class History
{
    private readonly Stack<ICommand> _undo = new();
    private readonly Stack<ICommand> _redo = new();

    public void Do(ICommand cmd)
    {
        cmd.Execute();
        Record(cmd);
    }

    public void Record(ICommand cmd)
    {
        _undo.Push(cmd);
        _redo.Clear();
    }

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public void Undo()
    {
        if (!CanUndo) return;
        var cmd = _undo.Pop();
        cmd.Undo();
        _redo.Push(cmd);
    }

    public void Redo()
    {
        if (!CanRedo) return;
        var cmd = _redo.Pop();
        cmd.Execute();
        _undo.Push(cmd);
    }
}
