using Core.Game;

namespace Core.Moves
{
    public interface IMove
    {
        bool IsForced();

        void Apply();

        IMove Clone(Board targetBoard);
    }
}