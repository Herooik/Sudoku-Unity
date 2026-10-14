#nullable enable

using System;

namespace Sudoku.Core
{
	// One place that knows "A = 10": both parsing puzzles and showing values on the board use it.
	public static class CellSymbol
	{
		public static string ToSymbol(int value)
		{
			switch (value)
			{
				case 0:
					return "";
				case >= 1 and <= 9:
				{
					var symbol = (char)(value + '0');
					return symbol.ToString();
				}
				case >= 10 and <= 16:
				{
					var symbol = (char)(value + 'A' - 10);
					return symbol.ToString();
				}
				default:
					throw new ArgumentOutOfRangeException(nameof(value), value, "Cell value must be in 0..16.");
			}
		}


		// Returns false instead of throwing: only the caller knows the index and parameter for a useful message.
		public static bool TryParse(char symbol, out int value)
		{
			switch (symbol)
			{
				case '.':
					value = 0;
					return true;
				case >= '1' and <= '9':
					value = symbol - '0';
					return true;
				case >= 'A' and <= 'G':
					value = symbol - 'A' + 10;
					return true;
				default:
					value = 0;
					return false;
			}
		}
	}
}
