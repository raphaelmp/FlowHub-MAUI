﻿using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Core.Extensions;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FlowHub.DataAccess.IRepositories;
using FlowHub.Main.PDF_Classes;
using FlowHub.Main.Platforms.NavigationMethods;
using FlowHub.Main.PopUpPages;
using FlowHub.Main.Utilities;
using FlowHub.Main.Views;
using FlowHub.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

//This is the view model for the page that shows ALL expenditures
namespace FlowHub.Main.ViewModels.Expenditures;
public partial class ManageExpendituresVM : ObservableObject
{
    public readonly IExpendituresRepository expendituresService;
    private readonly IUsersRepository userService;

    
    public ManageExpendituresVM(IExpendituresRepository expendituresRepository, IUsersRepository usersRepository)
    {
        expendituresService = expendituresRepository;
        userService = usersRepository;
        ExpendituresCat = ExpenditureCategoryDescriptions.Descriptions;
    }

    [ObservableProperty]
    ObservableCollection<ExpendituresModel> expendituresList;

    [ObservableProperty]
    ObservableCollection<DateGroup> groupedExpenditures;

    [ObservableProperty]
    private double totalAmount;

    [ObservableProperty]
    private int totalExpenditures;

    [ObservableProperty]
    private string userCurrency;

    [ObservableProperty]
    private double userPocketMoney;

    [ObservableProperty]
    private bool isBusy = true;

    [ObservableProperty]
    private string expTitle;

    private UsersModel ActiveUser = new();

    [ObservableProperty]
    private bool activ;

    [ObservableProperty]
    private bool showDayFilter;

    [ObservableProperty]
    private bool showStatisticBtn;

    [ObservableProperty]
    private int dayFilterMonth ;

    [ObservableProperty]
    private int dayFilterYear;

    [ObservableProperty]
    private int dayFilterDay;

    [ObservableProperty]
    private string filterTitle;

    [ObservableProperty]
    private bool showClearDayButton;

    [ObservableProperty]
    private bool isExpanderExpanded;

    [ObservableProperty]
    private bool isSyncing;

    [ObservableProperty]
    List<string> expendituresCat;

    private string filterOption;
    private int GlobalSortNamePosition = 1;

    private string monthName;
    [RelayCommand]
    public async void PageloadedAsync()
    {
        DayFilterYear = DateTime.UtcNow.Year;
        DayFilterMonth = DateTime.UtcNow.Month;

        UsersModel user = userService.OfflineUser;
        ActiveUser = user;

        UserPocketMoney = ActiveUser.PocketMoney;
        UserCurrency = ActiveUser.UserCurrency;
        await expendituresService.GetAllExpendituresAsync();
      //  filterOption = "Filter_Curr_Month";

        FilterGetAllExp();
    }

    [RelayCommand]
    public void Sorting(int SortNamePosition)
    {
        IsBusy= true;
        GlobalSortNamePosition = SortNamePosition;

        var expList = new List<ExpendituresModel>();
        switch (SortNamePosition)
        {
            case 0:
                expList = ExpendituresList.OrderBy(x => x.DateSpent).ToList();
                FilterTitle = "Date Spent Ascending";
                break;
            case 1:
                expList = ExpendituresList.OrderByDescending(x => x.DateSpent).ToList();
                FilterTitle = "Date Spent Descending";
                break;
            case 2:
                expList = ExpendituresList.OrderBy(x => x.AmountSpent).ToList();
                FilterTitle = "Amount Spent Ascending";
                break;
            case 3:
                expList = ExpendituresList.OrderByDescending(x => x.AmountSpent).ToList();
                FilterTitle = "Amount Spent Descending";
                break;
            default:
                break;
        }

        ExpendituresList.Clear();

        foreach (ExpendituresModel exp in expList)
        {
            ExpendituresList.Add(exp);
        }
        IsBusy = false;
    }

    [RelayCommand]
    //THIS Function Shows all Expenditures for the current month
    public void FilterExpListOfCurrentMonth()
    {
        // expendituresService.OfflineExpendituresList is ALREADY loaded since it was filled in the HomePageVM
        try
        {
            ShowDayFilter = true;
            ShowClearDayButton = false;
            DayFilterMonth = DateTime.UtcNow.Month;
            DayFilterYear = DateTime.UtcNow.Year;
            filterOption = "Filter_Curr_Month";
            List<ExpendituresModel> expOfCurrentMonth = new();

            expOfCurrentMonth = expendituresService.OfflineExpendituresList
                            .FindAll(x => x.DateSpent.Month == DateTime.Today.Month)
                            .OrderByDescending(x => x.DateSpent)
                            .ToList();
            FilterTitle = "Date Spent Descending";

            //IsBusy = true;

            ExpendituresList = new ObservableCollection<ExpendituresModel>(expOfCurrentMonth);
            //ExpendituresList.Clear();

            ExpTitle = $"Flow Outs For {DateTime.Now:MMM - yyyy}";

            TotalAmount = ExpendituresList.Count > 0 ? ExpendituresList.Count : 0;
            IsBusy = false;

            ShowStatisticBtn = expOfCurrentMonth.Count >= 3;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public void FilterExpListOfSpecificMonth()
    {
        try
        {
            ShowDayFilter = true;
            ShowClearDayButton = false;
            DayFilterDay = 0;

            filterOption = "Filter_Spec_Month";
            List<ExpendituresModel> expOfSpecMonth = new();

            expOfSpecMonth = expendituresService.OfflineExpendituresList
                            .FindAll(x => x.DateSpent.Month == DayFilterMonth && x.DateSpent.Year == DayFilterYear)
                            .OrderByDescending(x => x.DateSpent)
                            .ToList();
            IsBusy = true;
            double tot = 0;

            if (expOfSpecMonth.Count > 0)
            {
                IsBusy = false;
                ExpendituresList.Clear();
                for (int i = 0; i < expOfSpecMonth.Count; i++)
                {
                    ExpendituresList.Add(expOfSpecMonth[i]);
                    tot += expOfSpecMonth[i].AmountSpent;
                }

                TotalAmount = tot;
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {monthName} - {DayFilterYear}";
            }
            else
            {
                IsBusy=false;
                ExpendituresList.Clear();
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {monthName} - {DayFilterYear}";
                TotalAmount = 0;
            }

            ShowStatisticBtn = expOfSpecMonth.Count >= 3;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    //Function to show very single expenditure from DB
    public void FilterGetAllExp()
    {
        try
        {
            ShowDayFilter = false;
            filterOption = "Filter_All";
            ExpTitle = "All Flow Outs";
            FilterTitle = "Date Spent Descending";
            List<ExpendituresModel> expList = new ();

            expList = expendituresService.OfflineExpendituresList
                .OrderByDescending(x => x.DateSpent).ToList();


            var groupedData = expList.GroupBy(e => e.DateSpent.Date)
                .Select(g => new DateGroup(g.Key, g.ToList()))
                .ToList();

#if WINDOWS
            ExpendituresList = new ObservableCollection<ExpendituresModel>(expList);
#elif ANDROID
            GroupedExpenditures = new ObservableCollection<DateGroup>(groupedData);
            OnPropertyChanged(nameof(GroupedExpenditures));
#endif

            TotalAmount = expList.AsParallel().Sum(x => x.AmountSpent);
            TotalExpenditures = expList.Count;

            IsBusy = false;

            ShowStatisticBtn = expList.Count >= 3;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    //the Function below can be used to find exps for CURRENT DAY
    public void FilterGetExpOfToday()
    {
        try
        {
            Debug.WriteLine("entered today");
            ShowDayFilter = false;
            filterOption = "Filter_Today";
            List<ExpendituresModel> expOfToday = new();

            expOfToday = expendituresService.OfflineExpendituresList
                        .FindAll(x => x.DateSpent.Date == DateTime.Today.Date)
                        .OrderByDescending(x => x.DateSpent)
                        .ToList();
            FilterTitle = "Date Spent Descending";

            IsBusy = true;
            double tot = 0;

            if (expOfToday.Count > 0)
            {
                IsBusy = false;
                ExpendituresList.Clear();
                foreach (var exp in expOfToday)
                {
                    ExpendituresList.Add(exp);
                    tot += exp.AmountSpent;
                }

                TotalAmount = tot;
                TotalExpenditures = expOfToday.Count;
                ExpTitle = "Today's Flow Out";
            }
            else
            {
                IsBusy = false;
                ExpendituresList.Clear();
                TotalAmount = 0;
                TotalExpenditures = 0;
                ExpTitle = "Today's Flow Out";
            }
            ShowStatisticBtn = expOfToday.Count >= 3;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception MESSAGE: {ex.Message}");
        }
    }

    [RelayCommand]
    public void FilterGetExpOfSpecificDayMonth()
    {
        try
        {
            ShowClearDayButton = true;
            ShowDayFilter = true;
            IsExpanderExpanded = false;

            filterOption = "Filter_Spec_Day_Month";
            DateTime specificDate = new(DayFilterYear, DayFilterMonth, DayFilterDay);
            List<ExpendituresModel> expOfSpecDayInMonth = new();

            expOfSpecDayInMonth = expendituresService.OfflineExpendituresList
                            .FindAll(x => x.DateSpent.Date == specificDate)
                            .OrderByDescending(x => x.DateSpent)
                            .ToList();
            FilterTitle = "Date Spent Descending";

            IsBusy = true;
            double tot = 0;
            if (expOfSpecDayInMonth.Count > 0)
            {
                IsBusy = false;
                ExpendituresList.Clear();
                for (int i = 0; i < expOfSpecDayInMonth.Count; i++)
                {
                    ExpendituresList.Add(expOfSpecDayInMonth[i]);
                    tot += expOfSpecDayInMonth[i].AmountSpent;
                }

                TotalAmount = tot;
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {specificDate:dd, MMM yyyy}";
            }
            else
            {
                IsBusy = false;
                ExpendituresList.Clear();
                TotalExpenditures = ExpendituresList.Count;
                ExpTitle = $"Flow Outs For {specificDate:dd, MMM yyyy}";
                TotalAmount = 0;
            }
            ShowStatisticBtn = expOfSpecDayInMonth.Count >= 3;
            GlobalSortNamePosition = 1;
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.Message);
        }
    }

    [RelayCommand]
    public async void ShowAddExpenditurePopUp()
    {
        if (ActiveUser is null)
        {
            Debug.WriteLine("Can't Open PopUp");
           await Shell.Current.DisplayAlert("Wait", "Cannot go", "Ok");
        }
        else
        {
            var newExpenditure = new ExpendituresModel() { DateSpent = DateTime.Now };
            string pageTitle = "Add New Flow Out";
            bool isAdd = true;

            await AddEditExpediture(newExpenditure, pageTitle, isAdd);
        }
    }

    [RelayCommand]
    public async void ShowEditExpenditurePopUp(ExpendituresModel expenditure)
    {
        await AddEditExpediture(expenditure, "Edit Flow Out", false);
    }

    int count = 0;
    [RelayCommand]
    public void SamepleAdd()
    {
        ExpendituresModel ss = new() { DateSpent = DateTime.Now, Reason = $"Test {count}", AmountSpent = 2 };
        ExpendituresList.Add(ss);
        count++;
        TotalExpenditures = ExpendituresList.Count;
    }
    private async Task AddEditExpediture(ExpendituresModel expenditure, string pageTitle, bool isAdd)
    {
        ExpendituresModel nExp = expenditure;
        var NewUpSertVM = new UpSertExpenditureVM(expendituresService, userService, nExp, pageTitle, isAdd, ActiveUser);
        var UpSertResult = (PopUpCloseResult)await Shell.Current.ShowPopupAsync(new UpSertExpendituresPopUp(NewUpSertVM));

        if (UpSertResult.Result == PopupResult.OK)
        {
           IsBusy = true;
          
           FilterGetAllExp();
        }
    }

    [RelayCommand]
    public async Task GoToSpecificStatsPage()
    {
        int monthNumb = DayFilterMonth;
        int YearNumb = DayFilterYear;

        var navParam = new Dictionary<string, object>
        {
            { "MonthNumber",  monthNumb },
            { "YearNumber",  YearNumb},
            { "ExpendituresList", ExpendituresList }
        };
        switch (filterOption)
        {
            case "Filter_All":
                navParam.Add("PageTitle", new string("Statistics of All Flow Outs"));
                break;
            case "Filter_Today":
                navParam.Add("PageTitle", new string("Statistics of Today's Flow Outs"));
                break;
            case "Filter_Curr_Month":
                navParam.Add("PageTitle", new string("Statistics of this Month's Flow Outs"));
                break;
            case "Filter_Spec_Month":
                navParam.Add("PageTitle", new string($"Statistics of Flow Outs for {monthNumb} - {YearNumb}"));
                break;
            case "Filter_Spec_Day_Month":
                navParam.Add("PageTitle", new string($"Statistics of date {DayFilterDay}-{DayFilterMonth}-{DayFilterYear}"));
                break;
            default:
                //Debug.WriteLine("Cancelled");
                break;
        }

        await ManageExpendituresNavs.FromManageExpToSingleMonthStats(navParam);
    }

    [RelayCommand]
    public async void DeleteExpenditureBtn(ExpendituresModel expenditure)
    {
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        string text;
        bool response = (bool)(await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do You want to Delete?")))!;
        if (response)
        {
            IsBusy = true;
            var deleteResponse = await expendituresService.DeleteExpenditureAsync(expenditure.Id); //delete the expenditure from db

            if (deleteResponse)
            {
                text = "Flow Out Deleted";
                expendituresService.OfflineExpendituresList.Remove(expenditure);

                FilterGetAllExp();
                ActiveUser.TotalExpendituresAmount -= expenditure.AmountSpent;
                ActiveUser.PocketMoney += expenditure.AmountSpent;
                UserPocketMoney += expenditure.AmountSpent;
                await userService.UpdateUserAsync(ActiveUser);
            }
            else
            {
                 text = "Flow Out Not Deleted";
            }
            var toast = Toast.Make(text, duration, fontSize);
            await toast.Show(cancellationTokenSource.Token); //toast a notification about exp deletion
          //  Sorting(GlobalSortNamePosition);
            IsBusy = false;
        }
    }

    [RelayCommand]
    public async Task PrintExpendituresBtn()
    {
        Activ = true;
#if ANDROID
        ExpendituresList = GroupedExpenditures.SelectMany(x => x).ToObservableCollection();
#endif

        if (ExpendituresList?.Count < 1 || ExpendituresList is null)
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("Cannot save an Empty list to PDF"));
            return;
        }
        string dialogueResponse = (string)await Shell.Current.ShowPopupAsync(new InputCurrencyForPrintPopUpPage("Please Select Currency", UserCurrency));
        if (dialogueResponse is "Cancel")
        {
            return;
        }

        if (dialogueResponse != UserCurrency && !Connectivity.NetworkAccess.Equals(NetworkAccess.Internet))
        {
            await Shell.Current.ShowPopupAsync(new ErrorNotificationPopUpAlert("No Internet !\nPlease Connect to the Internet in order to save in other currencies"));
            return;
        }
        await PrintExpenditures.SaveExpenditureToPDF(ExpendituresList, ActiveUser.UserCurrency, dialogueResponse, ActiveUser.Username);
    }
    [RelayCommand]
    public async void ShowFilterPopUpPage()
    {
        List<string> FilterResult = new();

        FilterResult = (List<string>)await Shell.Current.ShowPopupAsync(new FilterOptionsPopUp(filterOption, DayFilterMonth,DayFilterYear));
        GlobalSortNamePosition = 1;
        if (FilterResult.Count == 1)
        {
            filterOption = FilterResult[0];
            switch (filterOption)
            {
                case "Filter_All":
                    FilterGetAllExp();
                    break;
                case "Filter_Today":
                    FilterGetExpOfToday();
                    break;
                case "Filter_Curr_Month":
                    FilterExpListOfCurrentMonth();
                    break;
                default:
                    //Debug.WriteLine("Cancelled");
                    break;
            }
        }
        else
        {
            filterOption = FilterResult[0];
            DayFilterMonth= int.Parse(FilterResult[1]);
            DayFilterYear = int.Parse(FilterResult[2]);
            monthName= FilterResult[3];
            FilterExpListOfSpecificMonth();
        }
    }

    [RelayCommand]
    public async void SyncExpTest()
    {
        bool response = (bool)await Shell.Current.ShowPopupAsync(new AcceptCancelPopUpAlert("Do you want to Sync data?"));
        if (response)
        {
            IsBusy = true;
            if (await expendituresService.SynchronizeExpendituresAsync(ActiveUser.Email, ActiveUser.Password))
            {
                PageloadedAsync();
                IsBusy = false;
                CancellationTokenSource cancellationTokenSource = new();
                const ToastDuration duration = ToastDuration.Short;
                const double fontSize = 16;
                string text = "All Synchronized!";
                var toast = Toast.Make(text, duration, fontSize);
                await toast.Show(cancellationTokenSource.Token); //toast a notification about Sync Success !
            }
        }
    }

    [RelayCommand]
    public static async void CopyToClipboard(ExpendituresModel singlExp)
    {
        await Clipboard.SetTextAsync($"{singlExp.Reason} : {singlExp.AmountSpent}");
        CancellationTokenSource cancellationTokenSource = new();
        const ToastDuration duration = ToastDuration.Short;
        const double fontSize = 14;
        string text = "Flow Out Details Copied to Clipboard";
        var toast = Toast.Make(text, duration, fontSize);
        await toast.Show(cancellationTokenSource.Token); //toast a notification about exp being copied to clipboard
    }

    [RelayCommand]
    public async void DropCollection()
    {
        await expendituresService.DropExpendituresCollection();
    }
}

public class DateGroup : List<ExpendituresModel>
{
    public DateTime Date { get; set; }
    public double TotalAmount { get; set; }
    public int TotalCount { get; set; }
    public string Currency { get; }
    public DateGroup(DateTime date, List<ExpendituresModel> expenditures) : base(expenditures)
    {
       Date = date;
       TotalAmount = expenditures.Sum(x => x.AmountSpent);
       TotalCount = expenditures.Count;
       Currency = expenditures[0].Currency;
    }
}