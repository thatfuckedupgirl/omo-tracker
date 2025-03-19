using System;
using System.Collections.Generic;

namespace omo_tracker;
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
        uiData.starttime = _mainHoldng?.profile_.current?.starttime ?? now;
        uiData.nowtime = DateTime.Now;
        uiData.prevact = _mainHoldng?.profile_.current?.lastact ?? "none";
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
    public void StopHolding(bool wet___ = false) {
        profile_.EndHoldData(wet___);
        DataIO.PrintDebug($"{(profile_.current == null ? "null" : "null not")} OK");
        RequestUiUpdateMainHolding.Invoke(null, EventArgs.Empty);
        RequestHistoryUiMainHolding.Invoke(null, EventArgs.Empty);

    }
}