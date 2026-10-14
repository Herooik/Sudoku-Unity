using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sudoku.Services;
using UnityEngine;
using VContainer.Unity;

namespace Sudoku.UI
{
	public sealed class MenuPresenter : IStartable, IDisposable
	{
		private readonly BoardPresenter _boardPresenter;
		private readonly CancellationTokenSource _cts;
		private readonly IMenuView _menuView;
		private readonly IPuzzleProvider _puzzleProvider;

		private bool _isLoading;

		public MenuPresenter(IMenuView menuView, IPuzzleProvider puzzleProvider, BoardPresenter boardPresenter)
		{
			_menuView = menuView;
			_puzzleProvider = puzzleProvider;
			_boardPresenter = boardPresenter;
			_cts = new CancellationTokenSource();
		}

		public void Dispose()
		{
			_menuView.NewGameClicked -= OnNewGameClicked;
			_cts.Cancel();
			_cts.Dispose();
		}

		public void Start()
		{
			_menuView.NewGameClicked += OnNewGameClicked;
		}

		private void OnNewGameClicked()
		{
			StartNewGameAsync().Forget();
		}

		private async UniTaskVoid StartNewGameAsync()
		{
			if (_isLoading)
			{
				return;
			}

			_isLoading = true;

			try
			{
				var puzzle = await _puzzleProvider.LoadAsync(_cts.Token);

				if (_cts.IsCancellationRequested)
				{
					return;
				}

				_menuView.SetVisible(false);
				_boardPresenter.Show(puzzle);
			}
			catch (OperationCanceledException)
			{
				// The scope is being torn down mid-load; the views may already be destroyed, so touch nothing.
			}
			catch (Exception exception)
			{
				Debug.LogException(exception);
			}
			finally
			{
				_isLoading = false;
			}
		}
	}
}
