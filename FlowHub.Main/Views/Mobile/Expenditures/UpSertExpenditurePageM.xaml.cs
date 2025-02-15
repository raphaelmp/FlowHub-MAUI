using FlowHub.Main.ViewModels.Expenditures;
using FlowHub.Models;
using System.Diagnostics;

namespace FlowHub.Main.Views.Mobile.Expenditures;

public partial class UpSertExpenditurePageM : ContentPage
{
    private readonly UpSertExpenditureVM viewModel;
    double CurrentBalance;
    double InitialExpAmountSpent;
    public UpSertExpenditurePageM(UpSertExpenditureVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);
        if (viewModel.IsAddTaxesChecked)
        {
            AddTaxCheckBox.IsChecked = true;
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        AddTaxCheckBox.IsChecked = false;
    }

    private void UnitPriceOrQty_TextChanged(object sender, TextChangedEventArgs e)
    {
        viewModel.UnitPriceOrQuantityChanged();
    }

    private void UnitPrice_Focused(object sender, FocusEventArgs e)
    {
        if (UnitPrice.Text == "0")
        {
            UnitPrice.Text = "";
        }
    }

    private void TaxCheckbox_CheckChanged(object sender, EventArgs e)
    {
        var s = sender as InputKit.Shared.Controls.CheckBox;
        var tax = (TaxModel)s.BindingContext;
        if (s.IsChecked)
        {
            tax.IsChecked = true;
            viewModel.AddTax(tax);
        }
        else
        {
            tax.IsChecked = false;

            viewModel.RemoveTax(tax);
        }
    }

    private void AddTax_CheckChanged(object sender, EventArgs e)
    {
        if (AddTaxCheckBox.IsChecked)
        {
            foreach (TaxModel tax in TaxesList.ItemsSource)
            {
                viewModel.AddTax(tax);
            }
        }
        else
        {
            foreach (TaxModel tax in TaxesList.ItemsSource)
            {
                viewModel.RemoveTax(tax);
            }
        }
    }
}