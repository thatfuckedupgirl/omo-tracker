using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace omo_tracker;
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
    public Bitmap GetImage() {
        return DataIO.GetBitmap(pfpsrs);
    }
    public void SetChData(HoldData indata_) {
        string funcid = "Profile.SetChData";
        try {
            SWS(funcid);
            if (current == null) {
                PrintDebug(funcid, FAILURE, "current null" , SWE(funcid));
                return;
            }
            current.SetData(indata_); 
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void SetHoldData() {
        string funcid = "Profile.SetHoldData";
        try {
            SWS(funcid);
            if (current != null) {
                PrintDebug(funcid, FAILURE, "current isnt null" , SWE(funcid));
                return;
            }
            current = new HoldData() {holdID = history.Count() != 0? (history.Last().holdID + 1) : 0};
            current.Start();
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        }catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void EndHoldData(bool wet__ = false) {
        string funcid = "Profile.EndHoldData";
        try {
            SWS(funcid);
            PrintDebug(funcid, STATUSUPDATE, $"current {(current == null ? "null" : "notnull, resetting")}" , SWT(funcid));
            if (current != null) {
                current.Stop(wet__);
                if (((current.endtime - current.starttime)!).Value.TotalMinutes > 1) { history.Add(current); }
            }
            current = null;
            DataIO.SaveData(this);
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void DrinkWater(double ml, bool caffeine) {
        string funcid = "Profile.DrinkWater";
        try {
            SWS(funcid);
            if (current == null) {
                PrintDebug(funcid, FAILURE, "current null" , SWE(funcid));
                return;
            }
            current.Drink(ml, caffeine);
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void Leak(double ml) {
        string funcid = "Profile.Leak";
        try {
            SWS(funcid);
            if (current == null) {
                PrintDebug(funcid, FAILURE, "current null" , SWE(funcid));
                return;
            }
            current.Leak(ml);
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void UndoAct() {
        string funcid = "Profile.UndoAct";
        try {
            SWS(funcid);
            if (current == null) {
                PrintDebug(funcid, FAILURE, "current null" , SWE(funcid));
                return;
            }
            current.DrinkUndo();
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
}



public class HoldData {

    [JsonIgnore] 
    public bool ison;
    [JsonIgnore] 
    private DispatcherTimer? transfer;
    [JsonIgnore] 
    public string lastact;
    [JsonIgnore] 
    private static List<HoldData> drinkhistory = [];
    public static event EventHandler RequestUiUpdateHoldData  = delegate {};
    private void TransferOnTick(object? sender, EventArgs e) {
        string funcid = "HoldData.UndoAct";
        try {
            SWS(funcid);
            if (DataIO.IsDebug) {
                for (int i = 0; i < 99; i++) {
                    water -= (currentrate +(envmod/60));
                    nonwater += currentrate;    
                }
            }
            water -= (currentrate +(envmod/60));
            nonwater += currentrate;
           
            CalculateVars();
            Task.Run(() => {
                         Thread.Sleep(1000 - DateTime.Now.Millisecond);
                         RequestUiUpdateHoldData.Invoke(null, EventArgs.Empty);
                     });
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception ex) {
            PrintDebug(funcid, FUCK, $"{ex.Message}, {ex.InnerException?.Message}");
            throw;
        }
    }
    public void SetData(HoldData indata) {
        string funcid = "HoldData.SetData";
        try {
            SWS(funcid);
            water = indata.water > 0? indata.water : water;
            currentrate = indata.currentrate > 0? indata.currentrate : currentrate;
            drinkmod = indata.drinkmod > 0? indata.drinkmod : drinkmod;
            envmod = indata.envmod > 0? indata.envmod : envmod;
            nonwater = indata.nonwater > 0? indata.nonwater : nonwater;
            starttime = indata.starttime ?? starttime;
            endtime = indata.endtime ?? endtime;
            CalculateVars();
            Save();
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void Start() {
        string funcid = "HoldData.Start";
        try {
            SWS(funcid);
            transfer ??= new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.MaxValue, TransferOnTick);
            ison = true;
            starttime = DateTime.Now;
            transfer.Start();
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void SoftStart() {
        string funcid = "HoldData.SoftStart";
        try {
            SWS(funcid);
            var timeskip = DateTime.Now - savetime;
            TimeSkip(timeskip);
            transfer ??= new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.MaxValue, TransferOnTick);
            ison = true;
            transfer.Start();
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void Stop(bool wet_ = false) {
        string funcid = "HoldData.Stop";
        try {
            SWS(funcid);
            drinkhistory.Clear();
            transfer ??= new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.MaxValue, TransferOnTick);
            ison = false; 
            wet = wet_;
		    endtime = DateTime.Now;
		    transfer.Stop();
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void Drink(double ml, bool caffeine) {
        string funcid = "HoldData.Drink";
        try {
            SWS(funcid);
            Save();
            water+=ml; 
            lastact = "Drink";
            CalculateVars();
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
        
    }
    public void Leak(double ml) {
        string funcid = "HoldData.Drink";
        try {
            SWS(funcid);
            Save();
            nonwater -= ml;
            lastact = "Leak";
            CalculateVars();
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void DrinkUndo() {
        string funcid = "HoldData.DrinkUndo";
        try {
            SWS(funcid);
            if (drinkhistory.Count == 0) {
                PrintDebug(funcid, FAILURE, $"mainholdingnull", SWE(funcid));
                return;
            }
            water = drinkhistory.Last().water;
            nonwater = drinkhistory.Last().nonwater;
            currentrate = drinkhistory.Last().currentrate;
            starttime = drinkhistory.Last().starttime;
            acttime = drinkhistory.Last().acttime;
            CalculateVars();
            TimeSpan? timeskip = DateTime.Now - drinkhistory.Last().acttime;
            TimeSkip(timeskip);
            drinkhistory.Remove(drinkhistory.Last());
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    private void CalculateVars() {
        string funcid = "HoldData.CalculateVars";
        try {
            currentrate = Math.Clamp(((0.009 * Math.Max(water, 0)) + 1) / 60, 0.35, 10);
            RequestUiUpdateHoldData.Invoke(null, EventArgs.Empty);
            PrintDebug(funcid, SUCESS, $"{currentrate:F2}" , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void TimeSkip(TimeSpan? timeskip) {
        string funcid = "HoldData.TimeSkip";
        try {
            SWS(funcid);
            if (timeskip == null) {
                PrintDebug(funcid, FAILURE, $"notimeskipneeded", SWE(funcid));
                return;
            }
            for (int i = 0; i < timeskip.Value.TotalSeconds; i++) { TransferOnTick(null, EventArgs.Empty); }
            PrintDebug(funcid, SUCESS, $"skipped {timeskip.Value.TotalSeconds}s" , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    private void Save() {
        string funcid = "HoldData.TimeSkip";
        try {
            SWS(funcid);
            acttime = DateTime.Now;
            HoldData sv = new HoldData() {
                                             acttime = this.acttime,
                                             water = this.water,
                                             nonwater = this.nonwater,
                                             currentrate = this.currentrate,
                                             starttime = this.starttime
                                         };
            drinkhistory.Add(sv);
            if (drinkhistory.Count > 5) { drinkhistory.RemoveRange(5, drinkhistory.Count - 5); }
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }    }
    [JsonInclude]
    public int holdID{get; set;}
    [JsonInclude]
    public double water {get; set;}
    [JsonInclude]
    public double currentrate{get; set;}
    [JsonInclude]
    public double drinkmod{get; set;} = 0.7;
    [JsonInclude]
    public double envmod{get; set;} = 0.7;
    [JsonInclude]
    public double nonwater{get; set;} 
    [JsonInclude]
    public DateTime? starttime {get; set;}
    [JsonInclude]
    public DateTime? acttime {get; set;}
    [JsonInclude]
    public DateTime? savetime {get; set;}
    [JsonInclude]
    public DateTime? endtime {get; set;}
    [JsonInclude]
    public bool wet {get; set;}

}
