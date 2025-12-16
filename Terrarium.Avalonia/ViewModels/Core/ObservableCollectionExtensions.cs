using System.Collections.ObjectModel;

namespace Terrarium.Avalonia.ViewModels.Core
{
    public static class ObservableCollectionExtensions
    {
        public static void Move<T>(this ObservableCollection<T> collection, int oldIndex, int newIndex)
        {
            if (oldIndex < 0 || newIndex < 0 || oldIndex >= collection.Count || newIndex >= collection.Count) return;
            if (oldIndex == newIndex) return;

            var item = collection[oldIndex];
            collection.RemoveAt(oldIndex);
            collection.Insert(newIndex, item);
        }
    }
}