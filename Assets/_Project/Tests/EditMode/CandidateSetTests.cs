using System;
using System.Collections.Generic;
using NUnit.Framework;
using Sudoku.Core;

namespace Sudoku.Tests.EditMode
{
	public class CandidateSetTests
	{
		[Test]
		public void Empty_Always_HasNoDigits()
		{
			var set = CandidateSet.Empty;

			Assert.That(set.IsEmpty, Is.True);
			Assert.That(set.Count, Is.EqualTo(0));
			Assert.That(set.Bits, Is.EqualTo(0u));
		}

		[Test]
		public void All_Size9_ContainsExactlyOneToNine()
		{
			var set = CandidateSet.All(9);

			Assert.That(Digits(set), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }));
			Assert.That(set.Contains(10), Is.False);
		}

		[Test]
		public void All_Size16_UsesBitsOneToSixteen()
		{
			Assert.That(CandidateSet.All(16).Bits, Is.EqualTo(0x1FFFEu));
		}

		[TestCase(0)]
		[TestCase(17)]
		public void All_SizeOutOfRange_Throws(int size)
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => CandidateSet.All(size));
		}

		[Test]
		public void Bits_SixBySixExampleCandidates_Is96()
		{
			// Cell (3,4) of the 6×6 example in game-design.md has candidates {5, 6}: bits 5 and 6, 32 + 64.
			var set = CandidateSet.Empty.With(5).With(6);

			Assert.That(set.Bits, Is.EqualTo(96u));
		}

		[Test]
		public void With_NewDigit_AddsIt()
		{
			var set = CandidateSet.Empty.With(4);

			Assert.That(set.Contains(4), Is.True);
			Assert.That(set.Count, Is.EqualTo(1));
		}

		[Test]
		public void With_Called_DoesNotChangeOriginal()
		{
			var original = CandidateSet.Empty.With(2);

			original.With(7);

			Assert.That(Digits(original), Is.EqualTo(new[] { 2 }));
		}

		[Test]
		public void Without_DigitInSet_RemovesIt()
		{
			var notes = CandidateSet.Empty.With(2).With(5).With(7);

			var result = notes.Without(5);

			Assert.That(Digits(result), Is.EqualTo(new[] { 2, 7 }));
		}

		[Test]
		public void Without_DigitNotInSet_ReturnsSameSet()
		{
			var notes = CandidateSet.Empty.With(2).With(7);

			Assert.That(notes.Without(5), Is.EqualTo(notes));
		}

		[Test]
		public void Toggle_DigitNotInSet_AddsIt()
		{
			Assert.That(CandidateSet.Empty.Toggle(3).Contains(3), Is.True);
		}

		[Test]
		public void Toggle_TwiceSameDigit_RestoresSet()
		{
			var notes = CandidateSet.Empty.With(2);

			Assert.That(notes.Toggle(9).Toggle(9), Is.EqualTo(notes));
		}

		[TestCase(0)]
		[TestCase(17)]
		[TestCase(-1)]
		public void DigitOperations_DigitOutOfRange_Throw(int digit)
		{
			var set = CandidateSet.All(9);

			Assert.Throws<ArgumentOutOfRangeException>(() => set.Contains(digit));
			Assert.Throws<ArgumentOutOfRangeException>(() => set.With(digit));
			Assert.Throws<ArgumentOutOfRangeException>(() => set.Without(digit));
			Assert.Throws<ArgumentOutOfRangeException>(() => set.Toggle(digit));
		}

		[Test]
		public void Enumerate_SeveralDigits_ReturnsThemAscending()
		{
			var set = CandidateSet.Empty.With(16).With(2).With(7);

			Assert.That(Digits(set), Is.EqualTo(new[] { 2, 7, 16 }));
		}

		[Test]
		public void FromBits_BitsOfExistingSet_EqualsIt()
		{
			var set = CandidateSet.Empty.With(1).With(9).With(16);

			Assert.That(CandidateSet.FromBits(set.Bits), Is.EqualTo(set));
		}

		[TestCase(1u)]
		[TestCase(1u << 17)]
		public void FromBits_BitZeroOrAboveSixteen_Throws(uint bits)
		{
			Assert.Throws<ArgumentException>(() => CandidateSet.FromBits(bits));
		}

		[Test]
		public void Equality_SameDigitsAddedInDifferentOrder_AreEqual()
		{
			var first = CandidateSet.Empty.With(3).With(8);
			var second = CandidateSet.Empty.With(8).With(3);

			Assert.That(first == second, Is.True);
			Assert.That(first.GetHashCode(), Is.EqualTo(second.GetHashCode()));
		}

		[Test]
		public void ToString_SeveralDigits_ListsThem()
		{
			Assert.That(CandidateSet.Empty.With(5).With(2).ToString(), Is.EqualTo("{2, 5}"));
		}

		private static List<int> Digits(CandidateSet set)
		{
			var digits = new List<int>();
			foreach (var digit in set)
			{
				digits.Add(digit);
			}

			return digits;
		}
	}
}
