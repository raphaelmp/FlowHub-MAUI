using FlowHub.Main.Views.Desktop;
using FlowHub.Main.Views.Desktop.Expenditures;
using FlowHub.Main.Views.Desktop.Settings;

namespace FlowHub.Main;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(HomePageD), typeof(HomePageD));
		Routing.RegisterRoute(nameof(LoginD), typeof(LoginD));

		Routing.RegisterRoute(nameof(ManageExpendituresD), typeof(ManageExpendituresD));
		Routing.RegisterRoute(nameof(UpSertExpenditurePageD), typeof(UpSertExpenditurePageD));

		Routing.RegisterRoute(nameof(UserSettingsPageD), typeof(UserSettingsPageD));
	}
}