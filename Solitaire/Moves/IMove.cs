using Solitaire.Game;

namespace Solitaire.Moves
{
    public interface IMove
    {
        bool IsForced();

        void Apply();

        IMove Clone(Board targetBoard);
    }
}