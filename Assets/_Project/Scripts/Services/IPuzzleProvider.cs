using System.Threading;
using Cysharp.Threading.Tasks;
using Sudoku.Core;

namespace Sudoku.Services
{
	// Async from day one: the final provider loads packs through Addressables, and callers should not change then.
	public interface IPuzzleProvider
	{
		UniTask<Puzzle> LoadAsync(CancellationToken cancellationToken);
	}
}
