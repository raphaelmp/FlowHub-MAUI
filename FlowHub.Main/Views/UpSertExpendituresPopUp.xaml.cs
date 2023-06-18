using CommunityToolkit.Maui.Views;
using FlowHub.Main.Utilities;
using FlowHub.Main.ViewModels.Expenditures;
using UraniumUI.Material.Controls;

namespace FlowHub.Main.Views;

public partial class UpSertExpendituresPopUp : Popup
{
	readonly UpSertExpenditureVM viewModel;
	public UpSertExpendituresPopUp(UpSertExpenditureVM vm)
	{
		InitializeComponent();
		viewModel = vm;
        this.BindingContext = vm;
        viewModel.PageLoadedCommand.Execute(null);

        viewModel.PropertyChanged += ViewModel_PropertyChanged;
	}

    private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(viewModel.ClosePopUp) && viewModel.ClosePopUp)
        {
            viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            if (viewModel.ThisPopUpResult == PopupResult.Cancel)
            {
                Close(new PopUpCloseResult() { Data = null, Result = PopupResult.Cancel });
                return;
            }
            Close(new PopUpCloseResult() { Data = viewModel.SingleExpenditureDetails, Result = viewModel.ThisPopUpResult});
        }
    }

    

    private void UnitPrice_TextChanged(object sender, TextChangedEventArgs e)
    {
        var s = sender as TextField;
        if (s.Text?.Length == 0)
        {
            viewModel.SingleExpenditureDetails.UnitPrice = 0;
        }

        viewModel.UnitPriceOrQuantityChanged();
    }
    private void UnitPrice_Focused(object sender, FocusEventArgs e)
    {
        if (UnitPrice.Text == "0")
        {
            UnitPrice.Text = "";
        }
    }
}

