using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace omo_tracker;

public static class Stopwatch {
    private static readonly Dictionary<string, System.Diagnostics.Stopwatch> _stopwatches = new Dictionary<string, System.Diagnostics.Stopwatch>();
    public static void SWS(string id) {
        if (!IsDebug) { return; }
        if (!_stopwatches.TryGetValue(id, out var sw)) {
            _stopwatches.Add(id, new System.Diagnostics.Stopwatch());
        } else if (sw.IsRunning) {
            PrintDebug("Stopwatch.SWS", FAILURE, "already running");
        }
        sw?.Start();
        PrintDebug("Stopwatch.SWS", Status.SUCESS, id);
    }
    public static TimeSpan? SWT(string id) {
        if (!IsDebug) { return null; }
        if (!_stopwatches.TryGetValue(id, out var sw)) {
            PrintDebug("Stopwatch.SWT", Status.FAILURE, $"{id} does not exist");
            return null;
        }
        var ret = sw.Elapsed;
        return ret;
    }
    public static TimeSpan? SWE(string id) {
        if (!IsDebug) { return null; }
        if (!_stopwatches.TryGetValue(id, out var sw)) {
            PrintDebug("Stopwatch.SWE", Status.FAILURE, $"{id} does not exist");
            return null;
        }
        var ret = sw.Elapsed;
        sw.Reset();
        return ret;
    }
}

public static class DataIO {
    private static bool debug;
    public static bool IsDebug {
        get {
            return debug;
        }
        set {
            debug = value;
        }
    }
    public static void PrintDebug(string function, Status status, string? desc = null, TimeSpan? time = null) {
        if (!IsDebug) { return; }
        Console.WriteLine($"{function}: " +
                          $"{status.ToString()} in" +
                          $"{(time!=null ?
                                  $"{((int)time.Value.TotalMinutes):00}:" +
                                  $"{time.Value.Seconds:00}." +
                                  $"{time.Value.Milliseconds:000}." +
                                  $"{time.Value.Nanoseconds:000000}/" +
                                  $"{time.Value.TotalMilliseconds}" :
                                  "")
                          }" +
                          $"{(desc != null? $"\r\n{desc}": "")}");
    }
    public static Func<(List<Profile>, int)>? GetChangedProfiles;
    public static void ChangeOrCreateProfile() {
        string funcid = "DataIO.ChangeOrCreateProfile";
        try {
            SWS(funcid);
            if (GetChangedProfiles == null) {
                PrintDebug(funcid, FAILURE, "Cancelled (GetChangedProfiles == null)", SWE(funcid));
                return;
            }
            (List<Profile> newprofiles, int newprofid) = GetChangedProfiles.Invoke();
            if (newprofid == -99) {
                PrintDebug(funcid, FAILURE, "Cancelled (newprofid == -99)", SWE(funcid));
                return;
            }
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
            PrintDebug(funcid, SUCESS, "", SWE(funcid));
            AsH.Setup(true, newprofid);
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public static Bitmap GetBitmap(string resourcePath) {
        string funcid = "DataIO.GetBitmap";
        try {
            SWS(funcid);
            if (File.Exists(resourcePath)) {
                PrintDebug(funcid, SUCESS, "Bitmap exists", SWE(funcid));
                return new Bitmap(resourcePath);
            }
            var uri = new Uri($"avares://omo-tracker/{resourcePath}");
            Bitmap ret;
            try {
                using (Stream stream = AssetLoader.Open(uri)) {
                    ret = new Bitmap(stream);    
                    PrintDebug(funcid, STATUSUPDATE, "Bitmap downloaded", SWT(funcid));
                }
            } catch (Exception e) {
                Console.WriteLine(e);
                ret = GetBitmap("res\\ph.png");
                PrintDebug(funcid, STATUSUPDATE, "Getting placeholder...", SWT(funcid));
            }
            PrintDebug(funcid, STATUSUPDATE, "Bitmap found", SWE(funcid));
            return ret;
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public static void SaveData(Profile profile) {
        string funcid = "DataIO.SaveData";
        try {
            SWS(funcid);
            if (!Directory.Exists("Profiles")) { Directory.CreateDirectory("Profiles"); }
            if (profile.current != null) {
                profile.current.savetime = DateTime.Now;
            }
            string json = JsonSerializer.Serialize(profile);
            File.WriteAllText($"Profiles/profile-q{profile.profid:00}.json", json);
            PrintDebug(funcid, SUCESS, "json saved", SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public static bool CheckProfileFile(int profid) {
        return File.Exists($"Profiles/profile-q{profid:00}.json");
    }
    public static Profile GetData(int profid) {
        string funcid = "DataIO.GetData";
        try {
            string json = File.ReadAllText($"Profiles/profile-q{profid:00}.json");
            Profile retprof = JsonSerializer.Deserialize<Profile>(json)?? throw new JsonException();
            PrintDebug(funcid, SUCESS, $"json read", SWE(funcid));
            return retprof;
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public static int GetFreeProfid() {
        string funcid = "DataIO.GetFreeProfid";
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
            PrintDebug(funcid, SUCESS, $"freeid found - {freeid:00}", SWE(funcid));
            return freeid;
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public static List<Profile>? GetAllProfiles() {
        string funcid = "DataIO.GetAllProfiles";
        try {
            List<int> profids = GetAllProfids();
            List<Profile> profiles = [];
            foreach (var profidout in profids) { profiles.Add(GetData(profidout)); }
            PrintDebug(funcid, SUCESS, $"{profiles.Count} profiles recieved", SWE(funcid));
            return profiles.Count == 0 ? null : profiles;
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    private static List<int> GetAllProfids() {
        string funcid = "DataIO.GetAllProfids";
        try {
            List<int> profids = [];
            if (!Directory.Exists($"Profiles")) { Directory.CreateDirectory($"Profiles"); }
            IEnumerable<string?> profilefiles = Directory.EnumerateFiles($"Profiles", "profile-q*").Select(Path.GetFileNameWithoutExtension);
            foreach (string? file in profilefiles) {
                if (file == null) {continue;}
                if (int.TryParse(file.Remove(0,9), out int profid)) { profids.Add(profid); } 
            }
            PrintDebug(funcid, SUCESS, $"{string.Join(" | ", profids)} profids recieved", SWE(funcid));
            return profids;
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
}