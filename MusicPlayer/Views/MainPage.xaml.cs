using MusicPlayer.ViewModels;
namespace MusicPlayer.Views;


public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
        BindingContext = new MainViewModel();
    }
}