using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;
using omo_tracker.avc;

namespace omo_tracker;

public partial class MainWindow : Window {
    private int ToDrink;
    private int ToLeak;
    public MainWindow() {
        InitializeComponent();
        ToDrinkBox.AddHandler(TextInputEvent, ToDrinkBox_OnTextInput, RoutingStrategies.Tunnel);
        ToLeakBox.AddHandler(TextInputEvent, ToDrinkBox_OnTextInput, RoutingStrategies.Tunnel);
        DataIO.GetChangedProfiles = getChangedProfiles;
        AsH.RequestProfileUiUpdateAsh += SufficeProfileUiUpdateAsh;
        AsH.RequestUiUpdateAsH += SufficeUiUpdate;
        AsH.RequestHistoryUiUpdateAsh += SufficeHistoryUiUpdate;
        MainHolding.RequestUiUpdateMainHolding += SufficeUiUpdate;
        MainHolding.RequestHistoryUiMainHolding += SufficeHistoryUiUpdate;
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
        debugswitch.IsChecked = File.Exists("debug");
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
                                            startfrommenu.Header = StartButton.Content = uidata.isholding? "Stop holding" : "Start holding";
                                            DrinkButton.IsEnabled = ToDrinkBox.IsEnabled = cancelButton.IsEnabled = LeakButton.IsEnabled = ToLeakBox.IsEnabled = SetData.IsEnabled = wetButton.IsEnabled = uidata.isholding;
                                            watervol.Text = $"{uidata.water}ml";
                                            watrimg.Source = uidata.waterimg;
                                            nonwatrimg.Source = uidata.nonwatrimg;
                                            nonwatervol.Text = $"{uidata.nonwater}ml";
                                            holdtime.Text = uidata.isholding?
                                                $"{(int)uidata.holdingtime.TotalHours:00}:{uidata.holdingtime.Minutes:00}:{uidata.holdingtime.Seconds:00}":"00:00:00";
                                            starttime.Text = uidata.isholding? $"{(uidata.starttime.Hour >= 13? uidata.starttime.Hour - 12 : uidata.starttime.Hour):00}:" +
                                                                              $"{uidata.starttime.Minute:00}:{uidata.starttime.Second:00}":"00:00:00";
                                            endttime.Text =uidata.isholding?
                                                               $"{(uidata.nowtime.Hour >= 13? uidata.nowtime.Hour - 12 : uidata.nowtime.Hour):00}:" +
                                                               $"{uidata.nowtime.Minute:00}:{uidata.nowtime.Second:00}":"00:00:00";
                                            cancelButton.Content = $"Cancel ({uidata.prevact})";

                                        });
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


    private void ToDrinkBox_OnTextChanged(object? sender, TextChangedEventArgs e) {Console.WriteLine(sender); Console.WriteLine(e);
        int.TryParse((sender as TextBox)?.Text ?? "0",  out int neval);
        if ((sender as TextBox)?.Name?.Contains("Drink") == true) {
            ToDrink = neval;
        } else {
            ToLeak = neval;
        }
            
    }
    private void ToDrinkBox_OnTextInput(object? sender, TextInputEventArgs e) {
        if (!int.TryParse(e.Text, out int todrink)) {
            e.Handled = true;    
        }
    }
    private void StartButton_OnClick(object? sender, RoutedEventArgs e) {Console.WriteLine(sender); Console.WriteLine(e);
        if (AsH._mainHoldng == null) { return; }
        if (AsH.IsActive()) { AsH._mainHoldng.StopHolding(); } else { AsH._mainHoldng.StartHold(); }
    }
    private void DrinkButton_OnClick(object? sender, RoutedEventArgs e) {Console.WriteLine(sender); Console.WriteLine(e);
        if (!AsH.IsActive()) { return; }
        Task.Run(()=>AsH._mainHoldng?.profile_.DrinkWater(ToDrink, false));
    }
    private void TopLevel_OnClosed(object? sender, EventArgs e) {Console.WriteLine(sender); Console.WriteLine(e);
        Profile.RequestUiUpdateProfile -= SufficeUiUpdate;
        HoldData.RequestUiUpdateHoldData -= SufficeUiUpdate;
    }
    private void Chpf_OnClick(object? sender, RoutedEventArgs e) {Console.WriteLine(sender); Console.WriteLine(e);
        Task.Run(DataIO.ChangeOrCreateProfile);
    }
    private void MenuItem_OnClick(object? sender, RoutedEventArgs e) {Console.WriteLine(sender); Console.WriteLine(e);
        AboutWindow aboutWindow = new AboutWindow();
        aboutWindow.ShowDialog(this);
    }
    private void Control_OnSizeChanged(object? sender, SizeChangedEventArgs e) {

    }
    private void SetData_OnClick(object? sender, RoutedEventArgs e) {
        if (!AsH.IsActive()) { return; }
        return;
        Task.Run(() => AsH._mainHoldng?.profile_.SetChData(new HoldData()));
    }
    private void CancelButton_OnClick(object? sender, RoutedEventArgs e) {
        if (!AsH.IsActive()) { return; }
        Task.Run(() => AsH._mainHoldng?.profile_.UndoAct());
    }
    private void WetButton_OnClick(object? sender, RoutedEventArgs e) {
        if (!AsH.IsActive()) { return; }
        Task.Run(() => AsH._mainHoldng?.StopHolding(true));
    }
    private void LeakButton_OnClick(object? sender, RoutedEventArgs e) {
        Task.Run(() => AsH._mainHoldng?.profile_.Leak(ToLeak));
    }
    private void ToggleButton_OnIsCheckedChanged(object? sender, RoutedEventArgs e) {
        if ((sender as ToggleButton).IsChecked == true) {
            File.WriteAllText("debug", "debug");
            IsDebug = true;
        } else {
            File.Delete("debug");
            IsDebug = false;
        }
    }
}