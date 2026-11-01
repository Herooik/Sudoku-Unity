#nullable enable

using System;
using System.Text;

namespace Sudoku.Core
{
	// A set of digits 1..16 in one uint: bit k means digit k, bit 0 is never used.
	// Used for the player's notes and for candidates computed from the rules.
	// Immutable: in an array, notes[i] = notes[i].With(d) is explicit, while a mutable struct would silently change a copy.
	// It does not know the grid size; whoever knows it (Board, solver) checks digits against it.
	public readonly struct CandidateSet : IEquatable<CandidateSet>
	{
		public const int MaxDigit = 16;

		private const uint ValidBits = ((1u << (MaxDigit + 1)) - 1) & ~1u;

		private CandidateSet(uint bits)
		{
			Bits = bits;
		}

		public static CandidateSet Empty => default;

		public uint Bits { get; }

		public bool IsEmpty => Bits == 0;

		public int Count => PopCount(Bits);

		public static CandidateSet All(int size)
		{
			if (size < 1 || size > MaxDigit)
			{
				throw new ArgumentOutOfRangeException(nameof(size), size, $"Size must be in 1..{MaxDigit}.");
			}

			return new CandidateSet(((1u << (size + 1)) - 1) & ~1u);
		}

		// For reading saves: anything outside bits 1..16 means the data is broken, so it is rejected instead of masked.
		public static CandidateSet FromBits(uint bits)
		{
			if ((bits & ~ValidBits) != 0)
			{
				throw new ArgumentException($"Bits 0x{bits:X} use bit 0 or bits above {MaxDigit}.", nameof(bits));
			}

			return new CandidateSet(bits);
		}

		public static bool operator ==(CandidateSet left, CandidateSet right) => left.Equals(right);

		public static bool operator !=(CandidateSet left, CandidateSet right) => !left.Equals(right);

		public bool Contains(int digit)
		{
			return (Bits & BitOf(digit)) != 0;
		}

		public CandidateSet With(int digit)
		{
			return new CandidateSet(Bits | BitOf(digit));
		}

		// Idempotent, unlike Toggle: clearing a placed digit from peer notes must never add it where it was missing.
		public CandidateSet Without(int digit)
		{
			return new CandidateSet(Bits & ~BitOf(digit));
		}

		public CandidateSet Toggle(int digit)
		{
			return new CandidateSet(Bits ^ BitOf(digit));
		}

		// A struct enumerator: foreach over the notes of every cell on every redraw allocates nothing.
		public Enumerator GetEnumerator() => new Enumerator(Bits);

		public bool Equals(CandidateSet other) => Bits == other.Bits;

		public override bool Equals(object? obj) => obj is CandidateSet other && Equals(other);

		public override int GetHashCode() => (int)Bits;

		public override string ToString()
		{
			var text = new StringBuilder("{");
			foreach (var digit in this)
			{
				if (text.Length > 1)
				{
					text.Append(", ");
				}

				text.Append(digit);
			}

			return text.Append('}').ToString();
		}

		private static uint BitOf(int digit)
		{
			if (digit < 1 || digit > MaxDigit)
			{
				throw new ArgumentOutOfRangeException(nameof(digit), digit, $"Digit must be in 1..{MaxDigit}.");
			}

			return 1u << digit;
		}

		// System.Numerics.BitOperations is not part of .NET Standard 2.1, which Unity compiles against.
		private static int PopCount(uint value)
		{
			var count = 0;
			while (value != 0)
			{
				// Clears the lowest set bit, so the loop runs once per digit, not once per bit.
				value &= value - 1;
				count++;
			}

			return count;
		}

		public struct Enumerator
		{
			private uint _remaining;

			internal Enumerator(uint bits)
			{
				_remaining = bits;
				Current = 0;
			}

			public int Current { get; private set; }

			public bool MoveNext()
			{
				if (_remaining == 0)
				{
					return false;
				}

				var digit = Current + 1;
				while ((_remaining & (1u << digit)) == 0)
				{
					digit++;
				}

				_remaining &= ~(1u << digit);
				Current = digit;
				return true;
			}
		}
	}
}
