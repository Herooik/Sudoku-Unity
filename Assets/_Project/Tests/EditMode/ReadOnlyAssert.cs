using System.Collections.Generic;
using NUnit.Framework;

namespace Sudoku.Tests.EditMode
{
	internal static class ReadOnlyAssert
	{
		public static void CannotBeModified<T>(IReadOnlyList<T> list)
		{
			// An array passes the IsReadOnly check (fixed size), yet a caller can cast it back and overwrite elements.
			Assert.That(list, Is.Not.InstanceOf<T[]>());
			if (list is ICollection<T> collection)
			{
				Assert.That(collection.IsReadOnly, Is.True);
			}
		}
	}
}
