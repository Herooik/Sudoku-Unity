using System;
using NUnit.Framework;
using Sudoku.Core;

namespace Sudoku.Tests.EditMode
{
	public class CellSymbolTests
	{
		[TestCase(0, "")]
		[TestCase(1, "1")]
		[TestCase(9, "9")]
		[TestCase(10, "A")]
		[TestCase(16, "G")]
		public void ToSymbol_SupportedValue_ReturnsSymbol(int value, string expected)
		{
			Assert.That(CellSymbol.ToSymbol(value), Is.EqualTo(expected));
		}

		[TestCase(-1)]
		[TestCase(17)]
		public void ToSymbol_ValueOutsideRange_ThrowsArgumentOutOfRangeException(int value)
		{
			Assert.That(() => CellSymbol.ToSymbol(value), Throws.TypeOf<ArgumentOutOfRangeException>());
		}

		[Test]
		public void TryParse_Dot_ReturnsTrueWithZero()
		{
			Assert.That(CellSymbol.TryParse('.', out int value), Is.True);
			Assert.That(value, Is.EqualTo(0));
		}

		[Test]
		public void TryParse_EverySymbolFromToSymbol_ReturnsOriginalValue()
		{
			for (int value = 1; value <= 16; value++)
			{
				char symbol = CellSymbol.ToSymbol(value)[0];

				Assert.That(CellSymbol.TryParse(symbol, out int parsed), Is.True, $"symbol '{symbol}'");
				Assert.That(parsed, Is.EqualTo(value), $"symbol '{symbol}'");
			}
		}

		[TestCase('x')]
		[TestCase('0')]
		[TestCase('H')]
		[TestCase('a')]
		[TestCase(' ')]
		public void TryParse_UnknownSymbol_ReturnsFalse(char symbol)
		{
			Assert.That(CellSymbol.TryParse(symbol, out _), Is.False);
		}
	}
}
