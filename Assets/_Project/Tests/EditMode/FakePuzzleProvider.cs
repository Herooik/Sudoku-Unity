using System.Threading;
using Cysharp.Threading.Tasks;
using Sudoku.Core;
using Sudoku.Services;

namespace Sudoku.Tests.EditMode
{
	// Never finishes on its own: the test decides when the load ends via Complete().
	internal sealed class FakePuzzleProvider : IPuzzleProvider
	{
		private readonly UniTaskCompletionSource<Puzzle> _completion = new UniTaskCompletionSource<Puzzle>();

		public int LoadCount { get; private set; }

		public CancellationToken LastToken { get; private set; }

		public UniTask<Puzzle> LoadAsync(CancellationToken cancellationToken)
		{
			LoadCount++;
			LastToken = cancellationToken;
			return _completion.Task;
		}

		public void Complete(Puzzle puzzle)
		{
			_completion.TrySetResult(puzzle);
		}
	}
}
