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
        if (current == null) { return; }
        current.SetData(indata_);
    }
    public void SetHoldData() {
        if (current != null) { return; }
        current = new HoldData() {holdID = history.Count() != 0 ? (history.Last().holdID + 1 ): 0};
        current.Start();
    }
    public void EndHoldData(bool wet__ = false) {
        if (current != null) {
            current.Stop(wet__);
            if (((current.endtime - current.starttime)!).Value.TotalMinutes > 1) { history.Add(current); }
        }
        current = null;
        DataIO.SaveData(this);
    }
    public void DrinkWater(double ml, bool caffeine) {
        if (current == null) { return; }
        current.Drink(ml, caffeine);
    }
    public void Leak(double ml) {
        if (current == null) { return; }
        current.Leak(ml);
    }
    public void UndoAct() {
        if (current == null) { return; }
        current.DrinkUndo();
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
        water -= currentrate + envmod;
        nonwater += currentrate;
        CalculateVars();
        Task.Run(() => {
                     Thread.Sleep(1000-DateTime.Now.Millisecond);
                     RequestUiUpdateHoldData.Invoke(null, EventArgs.Empty);
                 });
    }
    public void SetData(HoldData indata) {
        water = indata.water >0 ? indata.water : water;
        currentrate = indata.currentrate >0 ? indata.currentrate : currentrate;
        drinkmod = indata.drinkmod >0 ? indata.drinkmod : drinkmod;
        envmod = indata.envmod >0 ? indata.envmod : envmod;
        nonwater = indata.nonwater >0 ? indata.nonwater : nonwater;
        starttime = indata.starttime ?? starttime;
        endtime = indata.endtime ?? endtime;
        CalculateVars();
        Save();
    }
    public void Start() {
        transfer ??= new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.MaxValue, TransferOnTick);
        ison = true;
        starttime = DateTime.Now;
        transfer.Start();
    }
    public void Stop(bool wet_ = false) {
        drinkhistory.Clear();
        transfer ??= new DispatcherTimer(TimeSpan.FromSeconds(1), DispatcherPriority.MaxValue, TransferOnTick);
        ison = false; 
        wet = wet_;
		endtime = DateTime.Now;
		transfer.Stop();
    }
    public void Drink(double ml, bool caffeine) {
        Save();
        for (int i = 0; i < ml/10; i++) {
            water+=10;   
            if (i%5 == 0) {
                RequestUiUpdateHoldData.Invoke(null, EventArgs.Empty);
            }
        }
        lastact = "Drink";
        CalculateVars();
    }
    public void Leak(double ml) {
        Save();
        for (int i = 0; i < ml/10; i++) {
            nonwater-=10;
            if (i%5 == 0) {
                RequestUiUpdateHoldData.Invoke(null, EventArgs.Empty);
            }
        }
        lastact = "Leak";
        CalculateVars();
    }
    public void DrinkUndo() {
        if (drinkhistory.Count == 0) {
            return;
        }
        water = drinkhistory.Last().water;
        nonwater = drinkhistory.Last().nonwater;
        currentrate = drinkhistory.Last().currentrate;
        starttime = drinkhistory.Last().starttime;
        acttime = drinkhistory.Last().acttime;
        CalculateVars();
        TimeSpan? timeskip = DateTime.Now - drinkhistory.Last().acttime;
        drinkhistory.Remove(drinkhistory.Last());
        if (timeskip != null) {
            for (int i = 0; i < timeskip.Value.TotalSeconds; i++) {
                TransferOnTick(null, EventArgs.Empty);
            }
        }
    }
    private void CalculateVars() {
        currentrate = Math.Clamp(((0.009 * water) +1) / 60, 0.35, 10);
        RequestUiUpdateHoldData.Invoke(null,EventArgs.Empty);
    }
    private void Save() {
        acttime = DateTime.Now;
        HoldData sv = new HoldData() {
                                         acttime = this.acttime,
                                         water = this.water,
                                         nonwater = this.nonwater,
                                         currentrate = this.currentrate,
                                         starttime = this.starttime
                                     };
        drinkhistory.Add(sv);
        if (drinkhistory.Count > 5) {
            drinkhistory.RemoveRange(5, drinkhistory.Count - 5);
        }
    }
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
    public DateTime? endtime {get; set;}
    [JsonInclude]
    public bool wet {get; set;}

}
