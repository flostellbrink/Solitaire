using System;
using System.Collections.Generic;
using System.Linq;
using Solitaire.Stacks;

namespace Solitaire.Game
{
    public class InteractiveBoard : Board
    {
        private static readonly Color[] Colors = Enum.GetValues(typeof(Color)).Cast<Color>().ToArray();

        private static readonly Value[] Values = Enum.GetValues(typeof(Value)).Cast<Value>().ToArray();

        public InteractiveBoard(bool applyForcedMoves = true) : base(applyForcedMoves)
        {
            AskForCard();
        }

        private static void PopulateLockable(LockableStack stack)
        {

        }

        private static Card AskForCard()
        {
            var color = "Chose a color from {0}".AskForDecision(Colors);
            var value = "Chose a value from {0}".AskForDecision(Values);
            return new Card(color, value);
        }
    }
}
