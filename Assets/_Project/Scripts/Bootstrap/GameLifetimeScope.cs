using Sudoku.Infrastructure;
using Sudoku.Services;
using Sudoku.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Sudoku.Bootstrap
{
	// Composition root: the only place that knows which concrete classes implement the interfaces.
	public sealed class GameLifetimeScope : LifetimeScope
	{
		[SerializeField] private MenuView _menuView;
		[SerializeField] private BoardView _boardView;
		[SerializeField] private NumberPadView _numberPadView;
		[SerializeField] private KeyboardInputView _keyboardInputView;
		[SerializeField] private TextAsset _puzzleJson;

		protected override void Configure(IContainerBuilder builder)
		{
			builder.RegisterComponent(_menuView).As<IMenuView>();
			builder.RegisterComponent(_boardView).As<IBoardView>();
			builder.RegisterComponent(_numberPadView).As<INumberPadView>();
			builder.RegisterComponent(_keyboardInputView).As<IBoardKeyboardView>();

			// Swapping to Addressables later changes only this registration (ADR 002).
			builder.Register<IPuzzleProvider, TextAssetPuzzleProvider>(Lifetime.Singleton).WithParameter(_puzzleJson);

			builder.Register<BoardPresenter>(Lifetime.Singleton);

			// Entry point: VContainer calls Start() after building the graph and Dispose() when the scope is destroyed.
			builder.RegisterEntryPoint<MenuPresenter>();
		}
	}
}
