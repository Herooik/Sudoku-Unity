using NUnit.Framework;
using Sudoku.Core;
using Sudoku.UI;

namespace Sudoku.Tests.EditMode
{
	public class MenuPresenterTests
	{
		private FakeMenuView _menuView;
		private FakeBoardView _boardView;
		private FakePuzzleProvider _provider;
		private MenuPresenter _presenter;

		[SetUp]
		public void SetUp()
		{
			_menuView = new FakeMenuView();
			_boardView = new FakeBoardView();
			_provider = new FakePuzzleProvider();
			_presenter = new MenuPresenter(_menuView, _provider, new BoardPresenter(_boardView));
		}

		[Test]
		public void Start_Called_SubscribesToNewGameClick()
		{
			_presenter.Start();

			Assert.That(_menuView.HasSubscribers, Is.True);
		}

		[Test]
		public void NewGameClicked_Clicked_StartsLoadingPuzzle()
		{
			_presenter.Start();

			_menuView.ClickNewGame();

			Assert.That(_provider.LoadCount, Is.EqualTo(1));
		}

		[Test]
		public void NewGameClicked_WhileLoading_DoesNotShowBoardYet()
		{
			_presenter.Start();

			_menuView.ClickNewGame();

			Assert.That(_menuView.IsVisible, Is.True);
			Assert.That(_boardView.IsVisible, Is.False);
		}

		[Test]
		public void NewGameClicked_LoadFinished_HidesMenuAndShowsBoard()
		{
			_presenter.Start();
			_menuView.ClickNewGame();

			_provider.Complete(CreatePuzzle());

			Assert.That(_menuView.IsVisible, Is.False);
			Assert.That(_boardView.IsVisible, Is.True);
			Assert.That(_boardView.CellTexts[0], Is.EqualTo("1"));
		}

		[Test]
		public void NewGameClicked_TwiceWhileLoading_LoadsOnlyOnce()
		{
			_presenter.Start();

			_menuView.ClickNewGame();
			_menuView.ClickNewGame();

			Assert.That(_provider.LoadCount, Is.EqualTo(1));
		}

		[Test]
		public void Dispose_Called_UnsubscribesFromNewGameClick()
		{
			_presenter.Start();

			_presenter.Dispose();

			Assert.That(_menuView.HasSubscribers, Is.False);
		}

		[Test]
		public void Dispose_DuringLoad_CancelsTheLoadToken()
		{
			_presenter.Start();
			_menuView.ClickNewGame();

			_presenter.Dispose();

			Assert.That(_provider.LastToken.IsCancellationRequested, Is.True);
		}

		[Test]
		public void Dispose_DuringLoad_DoesNotShowBoardWhenLoadFinishesLater()
		{
			_presenter.Start();
			_menuView.ClickNewGame();
			_presenter.Dispose();

			// A provider that ignores the token can still finish after the screen is gone.
			_provider.Complete(CreatePuzzle());

			Assert.That(_boardView.IsVisible, Is.False);
		}

		private static Puzzle CreatePuzzle() =>
			new Puzzle("4x4-test", new GridShape(2, 2), "1..4.4..2..3...1", "1234341221434321");
	}
}
