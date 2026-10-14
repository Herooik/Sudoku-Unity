using System;
using System.IO;
using System.Threading;
using NUnit.Framework;
using Sudoku.Core;
using Sudoku.Infrastructure;
using UnityEngine;

namespace Sudoku.Tests.EditMode
{
	public class TextAssetPuzzleProviderTests
	{
		private const string ValidJson =
			"{\"version\":1,\"id\":\"4x4-test\",\"boxRows\":2,\"boxCols\":2," +
			"\"givens\":\"1..4.4..2..3...1\",\"solution\":\"1234341221434321\"}";

		private TextAsset _asset;

		[TearDown]
		public void TearDown()
		{
			if (_asset != null)
			{
				UnityEngine.Object.DestroyImmediate(_asset);
			}
		}

		[Test]
		public void LoadAsync_ValidJson_ReturnsPuzzleWithShapeFromFile()
		{
			var provider = new TextAssetPuzzleProvider(CreateAsset(ValidJson));

			// The provider completes synchronously, so the result is available without awaiting a frame.
			Puzzle puzzle = provider.LoadAsync(CancellationToken.None).GetAwaiter().GetResult();

			Assert.That(puzzle.Id, Is.EqualTo("4x4-test"));
			Assert.That(puzzle.Shape.BoxRows, Is.EqualTo(2));
			Assert.That(puzzle.Shape.BoxColumns, Is.EqualTo(2));
			Assert.That(puzzle.Givens[0], Is.EqualTo(1));
		}

		[TestCase("not json at all")]
		[TestCase("")]
		[TestCase("{\"version\":2,\"id\":\"4x4-test\",\"boxRows\":2,\"boxCols\":2," +
			"\"givens\":\"1..4.4..2..3...1\",\"solution\":\"1234341221434321\"}")]
		[TestCase("{\"version\":1,\"id\":\"4x4-test\",\"boxRows\":3,\"boxCols\":2," +
			"\"givens\":\"1..4.4..2..3...1\",\"solution\":\"1234341221434321\"}")]
		[TestCase("{\"version\":1,\"boxRows\":2,\"boxCols\":2," +
			"\"givens\":\"1..4.4..2..3...1\",\"solution\":\"1234341221434321\"}")]
		public void LoadAsync_InvalidFile_ThrowsInvalidDataExceptionNamingTheAsset(string json)
		{
			var provider = new TextAssetPuzzleProvider(CreateAsset(json));

			Assert.That(() => provider.LoadAsync(CancellationToken.None).GetAwaiter().GetResult(),
				Throws.TypeOf<InvalidDataException>().With.Message.Contains("test-puzzle"));
		}

		[Test]
		public void LoadAsync_CancelledToken_ThrowsOperationCanceledException()
		{
			var provider = new TextAssetPuzzleProvider(CreateAsset(ValidJson));
			var cancelled = new CancellationToken(true);

			Assert.That(() => provider.LoadAsync(cancelled).GetAwaiter().GetResult(),
				Throws.InstanceOf<OperationCanceledException>());
		}

		[Test]
		public void Constructor_NullAsset_ThrowsArgumentNullException()
		{
			Assert.That(() => new TextAssetPuzzleProvider(null), Throws.TypeOf<ArgumentNullException>());
		}

		private TextAsset CreateAsset(string json)
		{
			_asset = new TextAsset(json) { name = "test-puzzle" };
			return _asset;
		}
	}
}
