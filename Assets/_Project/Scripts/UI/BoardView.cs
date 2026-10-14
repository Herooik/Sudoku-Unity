using System;
using System.Collections.Generic;
using Sudoku.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Sudoku.UI
{
	public sealed class BoardView : MonoBehaviour, IBoardView
	{
		// The space the board may use. Margins live in the scene, not in code.
		[SerializeField] private RectTransform _boardArea;
		[SerializeField] private GridLayoutGroup _boxesLayout;
		[SerializeField] private GridLayoutGroup _boxPrefab;
		[SerializeField] private CellView _cellPrefab;

		private readonly List<GridLayoutGroup> _boxes = new();
		private CellView[] _cells = Array.Empty<CellView>();
		private GridShape _shape;

		// Called by Unity when the rect changes: panel shown, WebGL window resized, phone rotated.
		private void OnRectTransformDimensionsChange()
		{
			ApplySizes();
		}

		public void Build(GridShape shape)
		{
			ClearBoxes();
			_shape = shape;
			_cells = new CellView[shape.CellCount];

			// Boxes per band is Size / BoxColumns, which always equals BoxRows.
			_boxesLayout.constraintCount = shape.BoxRows;

			for (var box = 0; box < shape.Size; box++)
			{
				var boxLayout = Instantiate(_boxPrefab, _boxesLayout.transform);
				boxLayout.constraintCount = shape.BoxColumns;
				_boxes.Add(boxLayout);

				foreach (var cellIndex in shape.Boxes[box])
				{
					_cells[cellIndex] = Instantiate(_cellPrefab, boxLayout.transform);
				}
			}

			ApplySizes();
		}

		public void SetCellText(int cellIndex, string text)
		{
			if (cellIndex < 0 || cellIndex >= _cells.Length)
			{
				throw new ArgumentOutOfRangeException(nameof(cellIndex), cellIndex,
					$"Board has {_cells.Length} cells. Was Build called first?");
			}

			_cells[cellIndex].SetText(text);
		}

		public void SetVisible(bool visible)
		{
			gameObject.SetActive(visible);
		}

		// The board fills the area in whichever dimension runs out first: width in portrait, height in landscape.
		private void ApplySizes()
		{
			if (_shape == null)
			{
				return;
			}

			var area = _boardArea.rect;
			if (area.width <= 0f || area.height <= 0f)
			{
				return;
			}

			// A band holds BoxRows boxes side by side; the board has BoxColumns bands from top to bottom.
			var boxesAcross = _shape.BoxRows;
			var boxesDown = _shape.BoxColumns;

			var cellFromWidth = CellSizeFor(area.width, boxesAcross, _boxesLayout.padding.horizontal,
				_boxesLayout.spacing.x, _shape.BoxColumns, _boxPrefab.padding.horizontal, _boxPrefab.spacing.x);
			var cellFromHeight = CellSizeFor(area.height, boxesDown, _boxesLayout.padding.vertical,
				_boxesLayout.spacing.y, _shape.BoxRows, _boxPrefab.padding.vertical, _boxPrefab.spacing.y);
			var cellSize = Mathf.Min(cellFromWidth, cellFromHeight);
			if (cellSize <= 0f)
			{
				return;
			}

			var boxWidth = _boxPrefab.padding.horizontal
				+ cellSize * _shape.BoxColumns + _boxPrefab.spacing.x * (_shape.BoxColumns - 1);
			var boxHeight = _boxPrefab.padding.vertical
				+ cellSize * _shape.BoxRows + _boxPrefab.spacing.y * (_shape.BoxRows - 1);
			var boardWidth = _boxesLayout.padding.horizontal
				+ boxWidth * boxesAcross + _boxesLayout.spacing.x * (boxesAcross - 1);

			// Height is left to the ContentSizeFitter on the board.
			((RectTransform)_boxesLayout.transform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boardWidth);
			_boxesLayout.cellSize = new Vector2(boxWidth, boxHeight);
			foreach (var box in _boxes)
			{
				box.cellSize = new Vector2(cellSize, cellSize);
			}
		}

		// Same split along one axis: the length shared by the boxes, then by the cells inside one box.
		private static float CellSizeFor(float length, int boxCount, float boardPadding, float boxGap,
			int cellsPerBox, float boxPadding, float cellGap)
		{
			var boxLength = (length - boardPadding - boxGap * (boxCount - 1)) / boxCount;
			return (boxLength - boxPadding - cellGap * (cellsPerBox - 1)) / cellsPerBox;
		}

		private void ClearBoxes()
		{
			foreach (var box in _boxes)
			{
				// Destroy runs at the end of the frame; deactivating first keeps the layout from counting old boxes.
				box.gameObject.SetActive(false);
				Destroy(box.gameObject);
			}

			_boxes.Clear();
		}
	}
}
