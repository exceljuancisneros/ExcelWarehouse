using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace PrintLabels;

public partial class ItemSelectionModal : ContentPage
{
    private readonly TaskCompletionSource<ItemData?> _tcs;
    public ObservableCollection<ItemData> Items { get; }

    public Task<ItemData?> SelectedItemTask => _tcs.Task;

    public ItemSelectionModal(ObservableCollection<ItemData> items)
    {
        Items = items;
        _tcs = new TaskCompletionSource<ItemData?>();
        InitializeComponent();
        ItemList.ItemsSource = Items;
        CountLabel.Text = $"{items.Count} items found";
    }

    private void OnItemTapped(object sender, ItemTappedEventArgs e)
    {
        var selectedItem = (ItemData)e.Item;
        _tcs.TrySetResult(selectedItem);
        ItemList.SelectedItem = null; // Deselect after tap
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        _tcs.TrySetResult(null);
        Navigation.PopModalAsync(animated: true);
    }
}
