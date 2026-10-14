using System;

namespace Sudoku.Infrastructure
{
	// JsonUtility maps fields by name, so these mirror the JSON keys (camelCase) instead of C# naming.
	// Public fields rather than [SerializeField] private ones: private fields set only by deserialisation
	// raise CS0649 "never assigned" warnings.
	[Serializable]
	public sealed class PuzzleJson
	{
		public int version;
		public string id;
		public int boxRows;
		public int boxCols;
		public string givens;
		public string solution;
	}
}
