﻿using CommunityToolkit.Maui.Views;
using FlowHub.Main.PopUpPages;
using FlowHub.Main.Utilities;
using FlowHub.Main.ViewModels.Incomes;
using System.Diagnostics;

namespace FlowHub.Main.Views.Mobile.Incomes;

public partial class ManageIncomesM : ContentPage
{
    private readonly ManageIncomesVM viewModel;
    List<string> FilterResult { get; set; }
    public ManageIncomesM(ManageIncomesVM vm)
    {
        InitializeComponent();
        viewModel = vm;
        this.BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.PageLoadedCommand.Execute(null);
    }
    private async void ExportToPDFImageButton_Clicked(object sender, EventArgs e)
    {
        if (viewModel.IncomesList?.Count < 1)
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Cannot Save an Empty List to PDF"));
        }
        else
        {
            PrintProgressBarIndic.IsVisible = true;
            PrintProgressBarIndic.Progress = 0;
            await PrintProgressBarIndic.ProgressTo(1, 1000, easing: Easing.Linear);

            await viewModel.PrintIncomesBtnCommand.ExecuteAsync(null);
            PrintProgressBarIndic.IsVisible = false;
        }
    }
    private async void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        PopUpCloseResult result = (PopUpCloseResult) await Shell.Current.ShowPopupAsync(new InputPopUpPage(InputType.Numeric, new List<string>() { "Amount" }, "Enter New Pocket Money"));
        if (result.Result is PopupResult.OK)
        {
            double NewAmount = (double)result.Data;
            viewModel.ResetUserPocketMoneyCommand.Execute(NewAmount);
        }
    }
}
