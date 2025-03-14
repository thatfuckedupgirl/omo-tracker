using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using omo_tracker.avc;

namespace omo_tracker;

public partial class MainWindow : Window {
    private int ToDrink;
    public MainWindow() {
        InitializeComponent();
        ToDrinkBox.AddHandler(TextInputEvent, ToDrinkBox_OnTextInput, RoutingStrategies.Tunnel);
        DataIO.GetChangedProfiles = getChangedProfiles;
        AsH.RequestProfileUiUpdateAsh += SufficeProfileUiUpdateAsh;
        AsH.RequestUiUpdateAsH += SufficeUiUpdate;
        AsH.RequestHistoryUiUpdateAsh += SufficeHistoryUiUpdate;
        MainHoldng.RequestUiUpdateMainHolding += SufficeUiUpdate;
        MainHoldng.RequestHistoryUiMainHolding += SufficeHistoryUiUpdate;
        Profile.RequestUiUpdateProfile += SufficeUiUpdate;
        HoldData.RequestUiUpdateHoldData += SufficeUiUpdate;
    }
    private (List<Profile>, int) getChangedProfiles() {
        var ret = Dispatcher.UIThread.InvokeAsync(async () => {
                                                  ProfilesNewProfile pnp = new ProfilesNewProfile();
                                                  (List<Profile>, int) ret = await pnp.ShowDialog<(List<Profile>, int)>(this);
                                                  return ret;});
        return ret.GetAwaiter().GetResult();
    }
    private void TopLevel_OnOpened(object? sender, EventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        Task.Run(()=>AsH.Setup());
    }
    private void SufficeProfileUiUpdateAsh(object? sender, EventArgs e) {
        UiProfileData data = AsH.GetProfileUiData();
        Dispatcher.UIThread.InvokeAsync(() => {
                                            chpf.Content = data.nickname;
                                            pfp.Source = data.pfp;
                                        });
    }
    private void SufficeUiUpdate(object? sender, EventArgs e) {
        var uidata = AsH.GetUiData();
        Dispatcher.UIThread.InvokeAsync(() => {
                                            StartButton.Content = uidata.isholding? "Stop holding" : "Start holding";
                                            ToDrinkBox.IsEnabled = uidata.isholding;
                                            DrinkButton.IsEnabled = uidata.isholding;
                                            watervol.Text = $"{uidata.water}ml";
                                            watrimg.Source = uidata.waterimg;
                                            nonwatrimg.Source = uidata.nonwatrimg;
                                            holdtime.Text =
                                                $"{(int)uidata.holdingtime.TotalHours:00}:{uidata.holdingtime.Minutes:00}:{uidata.holdingtime.Seconds:00}";
                                            nonwatervol.Text = $"{uidata.nonwater}ml";});
    }
    private void SufficeHistoryUiUpdate(object? sender, EventArgs e) {
        var historydata = AsH.GetHistoryData();
        Dispatcher.UIThread.InvokeAsync(() => {
                                            HistoryBox.Items.Clear();
                                            if (historydata == null) {
                                                HistoryBox.Items.Add(new ListBoxItem() {
                                                                         Content = new TextBlock() {
                                                                             VerticalAlignment = VerticalAlignment.Center,
                                                                             HorizontalAlignment = HorizontalAlignment.Center,
                                                                             FontSize = 20,
                                                                             Text = "Empty"
                                                                         }
                                                                     });
                                                return;
                                            }
                                            foreach (var history in historydata) {
                                                var TS =  history.timeend - history.timestart;
                                                int hours = TS == null? 0 : (int)TS.Value.TotalHours;
                                                int mins = TS?.Minutes ?? 0;
                                                HistoryBox.Items.Add(new ListBoxItem() {
                                                                         Content =new HistoryBoxItem() {
                                                                             WaterImg = history.waterimg,
                                                                             Water = history.water.ToString(),
                                                                             Time = $"{hours:00}:{mins:00}",
                                                                             NonWater = history.nonwater.ToString(),
                                                                             NonWaterImg = history.nonwaterimg
                                                                         }});}});
    }


    private void ToDrinkBox_OnTextChanged(object? sender, TextChangedEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        int.TryParse(ToDrinkBox.Text ?? "0", out ToDrink);
    }
    private void ToDrinkBox_OnTextInput(object? sender, TextInputEventArgs e) {
        if (!int.TryParse(e.Text, out int todrink)) {
            Console.WriteLine(todrink);
            e.Handled = true;    
        }
    }
    private void StartButton_OnClick(object? sender, RoutedEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        if (AsH._mainHoldng == null) { return; }
        if (AsH.IsActive()) { AsH._mainHoldng.StopHolding(); } else { AsH._mainHoldng.StartHold(); }
    }
    private void DrinkButton_OnClick(object? sender, RoutedEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        if (AsH._mainHoldng == null || AsH._mainHoldng.profile_.current == null) {
            return;
        }
        AsH._mainHoldng.profile_.current.Drink(ToDrink, false);
    }
    private void TopLevel_OnClosed(object? sender, EventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        Profile.RequestUiUpdateProfile -= SufficeUiUpdate;
        HoldData.RequestUiUpdateHoldData -= SufficeUiUpdate;
    }
    private void Chpf_OnClick(object? sender, RoutedEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        Task.Run(DataIO.ChangeOrCreateProfile);
    }
    private void MenuItem_OnClick(object? sender, RoutedEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        AboutWindow aboutWindow = new AboutWindow();
        aboutWindow.ShowDialog(this);
    }
}