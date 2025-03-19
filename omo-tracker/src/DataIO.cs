using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace omo_tracker;

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