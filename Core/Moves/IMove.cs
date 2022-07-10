using Core.Game;

namespace Core.Moves
{
    public interface IMove
    {
        Unit Unit { get; }

        bool IsForced();

        void Apply();

        IMove Clone(Board targetBoard);
    }
}