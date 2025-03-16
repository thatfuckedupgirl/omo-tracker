using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;

namespace omo_tracker;
public class HoldData {
    [JsonInclude]
    public int holdID{get; set;}
    [JsonInclude]
    public double water {get; set;}
    [JsonInclude]
    public double currentrate{get; set;}
    [JsonInclude]
    public double mod{get; set;} = 1;
    [JsonInclude]
    public double nonwater{get; set;} 
    [JsonInclude]
    public DateTime? starttime {get; set;}
    [JsonInclude]
    public DateTime? endtime {get; set;}
    public static event EventHandler RequestUiUpdateHoldData  = delegate {};
    private DispatcherTimer? transfer;
    public bool ison;
    public void Start() {
        transfer ??= new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.MaxValue, TransferOnTick);
        ison = true; starttime = DateTime.Now; transfer.Start();}
    public void Stop() {
        transfer ??= new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.MaxValue, TransferOnTick);
        ison = false; endtime = DateTime.Now; transfer.Stop();
    }
    private void TransferOnTick(object? sender, EventArgs e) {
        water -= currentrate;
        nonwater += currentrate;
        if (mod > 0.2) { mod -= 0.0003; }
        CalculateVars();
    }
    public void Drink(double ml, bool caffeine) {
        water += ml;
        mod = caffeine ? 1.2 : 1;
        CalculateVars();
    }
    private void CalculateVars() {
        currentrate = (10 * mod) / 60;
        RequestUiUpdateHoldData.Invoke(null,EventArgs.Empty);
    }
    
}
public class Profile {
    [JsonInclude]
    public int profid {get; set;}
    [JsonInclude]
    public int size {get; set;}
    [JsonInclude]
    public string pfpsrs {get; set;} = "";
    [JsonInclude]
    public string nickname{get; set;} = "";
    [JsonInclude]
    public HoldData? current{get; set;} 
    [JsonInclude]
    public List<HoldData> history {get; set;} = [];
    public static event EventHandler RequestUiUpdateProfile = delegate {};
    public Bitmap GetImage() {return DataIO.GetBitmap(pfpsrs);}
    public void SetHoldData() {
        if (current != null) { return; }
        current = new HoldData() {holdID = history.Count() != 0 ? (history.Last().holdID + 1 ): 0};
        current.Start();
    }
    public void EndHoldData() {
        if (current != null) {
            current.Stop();
            if (((current.endtime - current.starttime)!).Value.TotalMinutes > 1) { history.Add(current); }
        }
        current = null;
        DataIO.SaveData(this);
    }
}


public class MainHoldng(int profid) {
    public readonly Profile profile_ = DataIO.GetData(profid);
    public static EventHandler RequestUiUpdateMainHolding = delegate {};
    public static EventHandler RequestHistoryUiMainHolding = delegate {};

    public void StartHold() {
        profile_.SetHoldData();
        DataIO.PrintDebug($"{(profile_.current != null ? profile_.current.holdID.ToString() : "its null. not")} OK");
        RequestUiUpdateMainHolding.Invoke(null, EventArgs.Empty);
        RequestHistoryUiMainHolding.Invoke(null, EventArgs.Empty);
    }
    public void StopHolding() {
        profile_.EndHoldData();
        DataIO.PrintDebug($"{(profile_.current == null ? "null" : "null not")} OK");
        RequestUiUpdateMainHolding.Invoke(null, EventArgs.Empty);
        RequestHistoryUiMainHolding.Invoke(null, EventArgs.Empty);

    }
}

public class History {
    public int water;
    public int nonwater;
    public DateTime? timestart;
    public DateTime? timeend;
    public Bitmap waterimg = DataIO.GetBitmap("res\\water\\wmt.bmp");
    public Bitmap nonwaterimg = DataIO.GetBitmap("res\\nonwater\\umt.bmp");
}
public class UiData {
    public int water;
    public int nonwater;
    public Bitmap waterimg = DataIO.GetBitmap("res\\water\\wmt.bmp");
    public Bitmap nonwatrimg = DataIO.GetBitmap("res\\nonwater\\umt.bmp");
    public bool isholding;
    public TimeSpan holdingtime = TimeSpan.FromSeconds(0);
}

public class UiProfileData {
    public Bitmap pfp = DataIO.GetBitmap("res\\water\\wmt.bmp");
    public string nickname = "Choose profile";
}
public static class AsH {
    public static List<Profile>? _allprofiles;
    public static MainHoldng? _mainHoldng ;
    public static EventHandler RequestUiUpdateAsH = delegate {};
    public static EventHandler RequestProfileUiUpdateAsh = delegate {};
    public static EventHandler RequestHistoryUiUpdateAsh = delegate {};

    public static bool IsActive() {
        bool ret = _mainHoldng?.profile_.current?.ison ?? false;
        return ret;
    }
    public static UiProfileData GetProfileUiData() {
        if (_mainHoldng == null) {
            return new UiProfileData();
        }
        UiProfileData uiProfileData = new UiProfileData() {
                                                              nickname = _mainHoldng.profile_.nickname,
                                                              pfp = DataIO.GetBitmap(_mainHoldng.profile_.pfpsrs)
                                                          };
        return uiProfileData;
    }
    public static List<History>? GetHistoryData() {
        if (_mainHoldng == null) { return null; }
        List<History> historylist = [];
        foreach (HoldData holdData in _mainHoldng.profile_.history) {
            History history = new History() {
                                          nonwater = (int)holdData.nonwater,
                                          water = (int)holdData.water,
                                          timeend = holdData.endtime,
                                          timestart = holdData.starttime,
                                          
                                      };
            history.waterimg = DataIO.GetBitmap($"res\\water\\w{(history.water <= 0 ? "mt" : Math.Clamp((int)(((double)history.water/1000)*10), 1, 10)):00}.bmp");
            history.nonwaterimg = DataIO.GetBitmap($"res\\nonwater\\u{(history.nonwater <= 0 ? "mt" : Math.Clamp((int)(((double)history.nonwater/_mainHoldng?.profile_?.size ?? 1000)*10), 1, 10)):00}.bmp");
            historylist.Add(history);
        }
        return historylist;
    }
    public static UiData GetUiData() {
        DateTime now = DateTime.Now;
        UiData uiData = new UiData {
                                       water = (int)(_mainHoldng?.profile_.current?.water ?? 0), 
                                       nonwater = (int)(_mainHoldng?.profile_.current?.nonwater ?? 0)
                                   };
        uiData.waterimg = DataIO.GetBitmap(
                                           $"res\\water\\w{(uiData.water <= 0 ?
                                                                "mt" :
                                                                Math.Clamp((int)(((double)uiData.water/1000)*10), 1, 10)):00}.bmp"
                                           );
        uiData.nonwatrimg = DataIO.GetBitmap(
                                             $"res\\nonwater\\u{(uiData.nonwater <= 0 ? 
                                                                     "mt" : 
                                                                     Math.Clamp((int)(((double)uiData.nonwater/_mainHoldng?.profile_.size ?? 1000)*10), 1, 10)):00}.bmp"
                                             );
        uiData.isholding = IsActive();
        uiData.holdingtime = (now - (_mainHoldng?.profile_.current?.starttime ?? now));
        return uiData;
    }
    public static void Setup(bool fromselector = false, int selid = 1) {
        try {
            if (fromselector && DataIO.CheckProfileFile(selid)) {
                    _mainHoldng = new MainHoldng(selid);
                    RequestUiUpdateAsH.Invoke(null, EventArgs.Empty);
                    RequestProfileUiUpdateAsh.Invoke(null, EventArgs.Empty);
                    RequestHistoryUiUpdateAsh.Invoke(null, EventArgs.Empty);
            } else { DataIO.ChangeOrCreateProfile(); }
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }
}


public static class DataIO {
    public static void PrintDebug(string txt) {Console.WriteLine(txt);}
    public static Func<(List<Profile>, int)>? GetChangedProfiles;
    public static void ChangeOrCreateProfile() {
        if (GetChangedProfiles == null) { return; }
        (List<Profile> newprofiles, int newprofid) = GetChangedProfiles.Invoke();
        if (newprofid == -99) { return; }
        List<Profile>? profiles = DataIO.GetAllProfiles();
        if (profiles != null && profiles != newprofiles ) {
            int prol = profiles.Count, newl = newprofiles.Count;
            if (prol != newl) {
                profiles.AddRange(newprofiles.GetRange(prol, newl - prol));
            }
            for (int i = 0; i < newl; i++) {
                profiles[i].nickname = newprofiles[i].nickname;
                profiles[i].size = newprofiles[i].size;
                profiles[i].pfpsrs = newprofiles[i].pfpsrs;
                Console.WriteLine($"Saving Profile {i}: {profiles[i].nickname}, {profiles[i].size}, {profiles[i].pfpsrs}");
                int index = i;
                DataIO.SaveData(profiles[index]);
            }
        } else if (profiles == null) {
            profiles = newprofiles;
            foreach (var profile in profiles) {
                DataIO.SaveData(profile);
            }
        }
        AsH.Setup(true, newprofid);
    }
    public static Bitmap GetBitmap(string resourcePath) {
        try {
            if (File.Exists(resourcePath)) {
                return new Bitmap(resourcePath);
            }
            var uri = new Uri($"avares://omo-tracker/{resourcePath}");
            Bitmap ret;
            try {
                using (Stream stream = AssetLoader.Open(uri)) {
                    ret = new Bitmap(stream);    
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                ret = GetBitmap("res\\ph.png");
            }
            return ret;
        } catch (Exception e) {
            PrintDebug(e.Message);
            throw;
        }
    }
    public static void SaveData(Profile profile) {
        try {
            if (!Directory.Exists("Profiles")) { Directory.CreateDirectory("Profiles"); }
            string json = JsonSerializer.Serialize(profile);
            File.WriteAllText($"Profiles/profile-q{profile.profid:00}.json", json);
        } catch (Exception e) {
            PrintDebug(e.Message);
            throw;
        }
    }
    public static bool CheckProfileFile(int profid) {
        return File.Exists($"Profiles/profile-q{profid:00}.json");
    }
    public static Profile GetData(int profid) {
        try {
            string json = File.ReadAllText($"Profiles/profile-q{profid:00}.json");
            Profile retprof = JsonSerializer.Deserialize<Profile>(json)?? throw new JsonException();
            return retprof;
        } catch (Exception e) {
            PrintDebug(e.Message);
            throw;
        }
    }
    public static int GetFreeProfid() {
        try {
            if (!Directory.Exists("Profiles")) { Directory.CreateDirectory("Profiles"); }
            List<int> profids = [];
            IEnumerable<string?> profilefiles = Directory.EnumerateFiles($"Profiles", "profile-q*").Select(Path.GetFileNameWithoutExtension);
            foreach (string? file in profilefiles) {
                if (file == null) { continue; }
                if (int.TryParse(file.Remove(0,9), out int profid)) { profids.Add(profid); } 
            }
            profids.Sort();
            int freeid = 1;
            foreach (int id in profids) {
                if (id != freeid) { break; }
                freeid++;
            }
            return freeid;
        } catch (Exception e) {
            PrintDebug(e.Message);
            throw;
        }
    }
    public static List<Profile>? GetAllProfiles() {
        try {
            List<int> profids = GetAllProfids();
            List<Profile> profiles = [];
            foreach (var profidout in profids) { profiles.Add(GetData(profidout)); }
            return profiles.Count == 0 ? null : profiles;
        } catch (Exception e) {
            PrintDebug(e.Message);
            throw;
        }
    }
    private static List<int> GetAllProfids() {
        try {
            List<int> profids = [];
            if (!Directory.Exists($"Profiles")) { Directory.CreateDirectory($"Profiles"); }
            IEnumerable<string?> profilefiles = Directory.EnumerateFiles($"Profiles", "profile-q*").Select(Path.GetFileNameWithoutExtension);
            foreach (string? file in profilefiles) {
                if (file == null) {continue;}
                if (int.TryParse(file.Remove(0,9), out int profid)) { profids.Add(profid); } 
            }
            return profids;
        } catch (Exception e) {
            Console.WriteLine(e);
            throw;
        }
    }
}