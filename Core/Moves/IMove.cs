using Core.Game;

namespace Core.Moves;

public interface IMove
{
    bool IsForced(Board board);

    void Apply(Board board);

    void Undo(Board board);

    string Stringify(Board board);
}
