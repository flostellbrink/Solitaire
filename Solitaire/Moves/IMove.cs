using Solitaire.Game;

namespace Solitaire.Moves
{
    public interface IMove
    {
        bool IsForced(Board board);

        void Apply(Board board);
    }
}