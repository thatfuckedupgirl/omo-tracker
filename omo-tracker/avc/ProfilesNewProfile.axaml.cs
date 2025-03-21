using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace omo_tracker.avc;

public partial class ProfilesNewProfile : Window {
    private bool manual ;
    private List<Profile>? profiles;
    private int ind;
    public ProfilesNewProfile() {
        InitializeComponent();
        SzTextBox.AddHandler(TextInputEvent, ToDrinkBox_OnTextInput, RoutingStrategies.Tunnel);
        if (!Design.IsDesignMode) { InitList(); }
    }
    private void InitList() {
        profilesbox.Items.Clear();
        profiles = DataIO.GetAllProfiles();
        profilesbox.Items.Add(new ListBoxItem() {Content = "New profile"});
        if (profiles == null) {
            profiles = new List<Profile> {new Profile()};
            profilesbox.SelectedIndex = 0;
            state.Text = $"Creating new profile";
            ind = -1; 
        } else { 
            profiles.Sort((x, y) => x.profid.CompareTo(y.profid)); 
            foreach (var prof in profiles) { 
                profilesbox.Items.Add(new ListBoxItem() {Content = $"q{prof.profid:00}: {prof.nickname}"});
            }
            profiles.Add(new Profile());
            profilesbox.SelectedIndex = 1;
            state.Text = $"Editing profile q{profiles.First().profid}";
            SzTextBox.Text = profiles.First().size.ToString();
            nicknamebox.Text = profiles.First().nickname;
            imgsrctextbox.Text = profiles.First().pfpsrs;
            SzTextBox.Text = profiles.First().size.ToString();
            pfp.Source = DataIO.GetBitmap(profiles.First().pfpsrs);
        }
        profilesbox.SelectionChanged += ProfilesboxOnSelectionChanged;
    }
    private void ProfilesboxOnSelectionChanged(object? sender, SelectionChangedEventArgs e) {
        if (profiles == null) { e.Handled = true; return; }
        SaveChangedProfile();
        ind = (sender as ListBox)?.SelectedIndex??0;
        ind--;
        if (ind < 0) {
            state.Text = $"Creating new profile";
            nicknamebox.Text = "";
            pfp.Source = DataIO.GetBitmap("res\\ph.png");
            imgsrctextbox.Text = "";
            SzTextBox.Text = "";
            return;
        } else if (ind > profiles.Count) {
            profilesbox.SelectedIndex = 0;
            return;
        }
        state.Text = $"Editing profile q{(profiles[ind].profid):00}";
        nicknamebox.Text = profiles[ind].nickname;
        pfp.Source = profiles[ind].GetImage();
        imgsrctextbox.Text = profiles[ind].pfpsrs;
        SzTextBox.Text = profiles[ind].size.ToString();
    }
    private void Button_OnClick(object? sender, RoutedEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        SaveChangedProfile();
        if ( string.IsNullOrEmpty(nicknamebox.Text)) {
            nicknamebox.Foreground = Brushes.Red;
            return;
        }
        if (profiles?.Last().nickname == "") {
            profiles.RemoveAt(profiles.Count-1);
        }
        manual = true;
        if (profiles != null) {
            Close((profiles, Math.Clamp((ind == -1? profiles.Count : ind + 1), 1, profiles?.Count ?? 1)));
        }
    }
    
    private void ToDrinkBox_OnTextChanged(object? sender, TextChangedEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        if (profiles == null) {return; }
        int.TryParse(SzTextBox.Text ?? "0", out int i );
        Console.WriteLine(i);
        SaveChangedProfile();
    }
    private void ToDrinkBox_OnTextInput(object? sender, TextInputEventArgs e) {
        if (!int.TryParse(e.Text, out int todrink)) {
            Console.WriteLine(todrink);
            e.Handled = true;    
        }
    }
    
    
    private async void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel?.StorageProvider is { } storageProvider) {
            Task<IReadOnlyList<IStorageFile>> files = storageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
                {Title = "Select a file", AllowMultiple = false, FileTypeFilter = new[] 
                        {new FilePickerFileType("All Images") {Patterns = new[] { "*.png", "*.jpg", "*.jpeg", "*.bmp", },
                         MimeTypes = new[] {"image/png", "image/jpeg", "image/bmp",}}}});
            IReadOnlyList<IStorageFile> file = await files;
            imgsrctextbox.Text = file.Count != 0 ? file[0].Path.LocalPath.Replace("file://", "") : "";
            pfp.Source = DataIO.GetBitmap(file[0].Path.LocalPath.Replace("file://", ""));
        }
        SaveChangedProfile();
    }
    private void Window_OnClosing(object? sender, WindowClosingEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        if (profiles == null) {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => {
                                                                if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
                                                                    desktop.Shutdown();
                                                                }
                                                        });
        } 
        if (!manual) {
            manual = true;
            Button_OnClick(null, new RoutedEventArgs());
            e.Cancel = true;
        }
    }
    private void SaveChangedProfile() {
        if (nicknamebox.Text == "") {
            return;   
        }
        if (profiles == null) {
            return;
        }
        if (ind < 0 ) {
            profiles.Last().profid = DataIO.GetFreeProfid();
            profiles.Last().nickname = nicknamebox.Text ?? "";
            profiles.Last().pfpsrs = imgsrctextbox.Text ?? "";
            profiles.Last().size = SzTextBox.Text != ""? int.Parse(SzTextBox.Text ?? "1000") : 1000;
             
        } else if (ind < profiles.Count) {
            profiles[ind].nickname = nicknamebox.Text??"";
            profiles[ind].pfpsrs = imgsrctextbox.Text??"";
            profiles[ind].size = int.Parse(SzTextBox.Text??"1000");
        } else {
            profilesbox.SelectedIndex = 1;
        }
    }
    private async void Delete_OnClick(object? sender, RoutedEventArgs e) {
        Console.WriteLine(sender);
        Console.WriteLine(e);
        var result = await MessageBoxManager.GetMessageBoxStandard("Are you sure?", "Confirmation", ButtonEnum.YesNo).ShowAsync();
        if (result == ButtonResult.Yes) {
            if (profiles == null||ind < 0 || ind+1 > profiles.Count) { return; }
            int id = profiles[ind].profid;
            File.Delete($"Profiles/profile-q{id:00}.json");
            profilesbox.SelectionChanged -= ProfilesboxOnSelectionChanged;
            InitList();
        }
    }
}