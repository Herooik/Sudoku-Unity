using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sudoku.Core;
using Sudoku.Services;
using UnityEngine;

namespace Sudoku.Infrastructure
{
	// Temporary source of a single puzzle until Addressables packs replace it (ADR 002).
	public sealed class TextAssetPuzzleProvider : IPuzzleProvider
	{
		private const int SupportedVersion = 1;

		private readonly TextAsset _puzzleJson;

		public TextAssetPuzzleProvider(TextAsset puzzleJson)
		{
			if (puzzleJson == null)
			{
				throw new ArgumentNullException(nameof(puzzleJson));
			}

			_puzzleJson = puzzleJson;
		}

		public UniTask<Puzzle> LoadAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return UniTask.FromResult(Parse());
		}

		private Puzzle Parse()
		{
			PuzzleJson data;
			try
			{
				data = JsonUtility.FromJson<PuzzleJson>(_puzzleJson.text);
			}
			catch (ArgumentException exception)
			{
				throw Invalid(exception.Message, exception);
			}

			if (data == null)
			{
				throw Invalid("the file contains no JSON object.");
			}

			if (data.version != SupportedVersion)
			{
				throw Invalid($"format version {data.version} is not supported (expected {SupportedVersion}).");
			}

			try
			{
				var shape = new GridShape(data.boxRows, data.boxCols);
				return new Puzzle(data.id, shape, data.givens, data.solution);
			}
			catch (ArgumentException exception)
			{
				throw Invalid(exception.Message, exception);
			}
		}

		// Core only knows the bad value; the asset name tells which pack file to fix.
		private InvalidDataException Invalid(string reason, Exception inner = null)
		{
			return new InvalidDataException($"Puzzle asset '{_puzzleJson.name}' is invalid: {reason}", inner);
		}
	}
}
