﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Core.Game;
using Core.Stacks;

namespace Core.Moves
{
    internal class LockMove : IMove
    {
        public readonly Board Board;

        public readonly ICollection<IStack> Sources;

        public readonly LockableStack Destination;

        public Unit Unit { get; }

        public LockMove(
            Board board,
            ICollection<IStack> sources,
            LockableStack destination,
            Unit unit
        )
        {
            Board = board;
            Sources = sources;
            Destination = destination;
            Unit = unit;
        }

        public IMove Clone(Board targetBoard)
        {
            var sources = Sources.Select(
                ms => targetBoard.AllStacks.Single(ts => ts.ToString() == ms.ToString())
            );
            var target = targetBoard.AllStacks.Single(d => d.ToString() == Destination.ToString());

            return new LockMove(targetBoard, sources.ToList(), (LockableStack)target, Unit);
        }

        public bool IsForced() => false;

        public void Apply()
        {
            Debug.Assert(Destination.Cards.Count <= 1);
            Debug.Assert(Destination.Cards.All(card => card.Color == Unit.Cards.First().Color));
            Debug.Assert(Destination.Cards.All(card => card.Value == Value.Dragon));

            foreach (var source in Sources)
            {
                source.Remove(Unit);
                Destination.Add(Unit);
            }

            Destination.Locked = true;

            Debug.Assert(Destination.Cards.Count == 4);
            Debug.Assert(Destination.Cards.All(card => card.Color == Unit.Cards.First().Color));
            Debug.Assert(Destination.Cards.All(card => card.Value == Value.Dragon));

            Board.MoveHistory.Push(this);
            Board.ApplyForcedMoves();
        }

        public override string ToString()
        {
            return $"Lock {Unit.Cards.FirstOrDefault()} from {string.Join(", ", Sources)} in at {Destination}";
        }
    }
}
