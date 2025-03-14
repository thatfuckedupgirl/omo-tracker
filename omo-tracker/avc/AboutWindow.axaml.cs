using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace omo_tracker.avc;

public partial class AboutWindow : Window {
    public AboutWindow() {InitializeComponent();}
    private void Button_OnClick(object? sender, RoutedEventArgs e) {
        this.Close();
    }
}