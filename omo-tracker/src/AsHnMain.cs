using System;
using System.Collections.Generic;

namespace omo_tracker;
public static class AsH {
    public static List<Profile>? _allprofiles;
    public static MainHolding? _mainHoldng ;
    public static EventHandler RequestUiUpdateAsH = delegate {};
    public static EventHandler RequestProfileUiUpdateAsh = delegate {};
    public static EventHandler RequestHistoryUiUpdateAsh = delegate {};

    public static bool IsActive() {
        string funcid = "AsH.IsActive";
        try {
            SWS(funcid);
            bool ret = _mainHoldng?.profile_.current?.ison ?? false;
            PrintDebug(funcid, SUCESS, $"isactive = {ret}", SWE(funcid));
            return ret;
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public static UiProfileData GetProfileUiData() {
        string funcid = "AsH.GetProfileUiData";
        try {
            SWS(funcid);
            if (_mainHoldng == null) {
                PrintDebug(funcid, FAILURE, $"mainholdingnull", SWE(funcid));
                return new UiProfileData();
            }
            UiProfileData uiProfileData = new UiProfileData() {
                                                                  nickname = _mainHoldng.profile_.nickname,
                                                                  pfp = DataIO.GetBitmap(_mainHoldng.profile_.pfpsrs)
                                                              };
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
            return uiProfileData;
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public static List<History>? GetHistoryData() {
        string funcid = "AsH.GetHistoryData";
        try {
            SWS(funcid);
            if (_mainHoldng == null) {
                PrintDebug(funcid, FAILURE, $"mainholdingnull", SWE(funcid));
                return null;
            }
            List<History> historylist = [];
            foreach (HoldData holdData in _mainHoldng.profile_.history) {
                History history = new History() {
                                                    nonwater = (int)holdData.nonwater,
                                                    water = (int)holdData.water,
                                                    timeend = holdData.endtime,
                                                    timestart = holdData.starttime,
                                                };
                history.waterimg =
                    DataIO.GetBitmap($"res\\water\\w{(history.water <= 0? "mt" : Math.Clamp((int)(((double)history.water / 1000) * 10), 1, 10)):00}.bmp");
                history.nonwaterimg =
                    DataIO.GetBitmap($"res\\nonwater\\u{(history.nonwater <= 0? "mt" : Math.Clamp((int)(((double)history.nonwater / _mainHoldng?.profile_?.size ?? 1000) * 10), 1, 10)):00}.bmp");
                historylist.Add(history);
            }
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
            return historylist;
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public static UiData GetUiData() {
        string funcid = "AsH.GetUiData";
        try {
            SWS(funcid);
            DateTime now = DateTime.Now;
            UiData uiData = new UiData {
                                           water = (int)(_mainHoldng?.profile_.current?.water ?? 0),
                                           nonwater = (int)(_mainHoldng?.profile_.current?.nonwater ?? 0)
                                       };
            uiData.waterimg = DataIO.GetBitmap($"res\\water\\w{(uiData.water <= 0?
                                                                    "mt" :
                                                                    Math.Clamp((int)(((double)uiData.water / 1000) * 10), 1, 10)):00}.bmp");
            uiData.nonwatrimg = DataIO.GetBitmap($"res\\nonwater\\u{(uiData.nonwater <= 0?
                                                                         "mt" :
                                                                         Math.Clamp((int)(((double)uiData.nonwater / _mainHoldng?.profile_.size ?? 1000) * 10), 1, 10)):00}.bmp");
            uiData.isholding = IsActive();
            uiData.holdingtime = (now - (_mainHoldng?.profile_.current?.starttime ?? now));
            uiData.starttime = _mainHoldng?.profile_.current?.starttime ?? now;
            uiData.nowtime = DateTime.Now;
            uiData.prevact = _mainHoldng?.profile_.current?.lastact ?? "none";
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
            return uiData;
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public static void Setup(bool fromselector = false, int selid = 1) {
        string funcid = "AsH.Setup";
        try {
            SWS(funcid);
            if (AsH._mainHoldng != null) {
                SaveData(AsH._mainHoldng.profile_);    
            }
            if (fromselector && DataIO.CheckProfileFile(selid)) {
                    _mainHoldng = new MainHolding(selid);
                    RequestUiUpdateAsH.Invoke(null, EventArgs.Empty);
                    RequestProfileUiUpdateAsh.Invoke(null, EventArgs.Empty);
                    RequestHistoryUiUpdateAsh.Invoke(null, EventArgs.Empty);
            } else { DataIO.ChangeOrCreateProfile(); }
            PrintDebug(funcid, SUCESS, null , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
}
public class MainHolding {
    public readonly Profile profile_;
    public static EventHandler RequestUiUpdateMainHolding = delegate {};
    public static EventHandler RequestHistoryUiMainHolding = delegate {};
    public MainHolding(int profid) {
        profile_ = DataIO.GetData(profid);
        if (profile_.current != null) {
            profile_.current.SoftStart();
        }
    }
    public void StartHold() {
        string funcid = "AsH.StartHold";
        try {
            SWS(funcid);
            profile_.SetHoldData();
            RequestUiUpdateMainHolding.Invoke(null, EventArgs.Empty);
            RequestHistoryUiMainHolding.Invoke(null, EventArgs.Empty);
            PrintDebug(funcid, SUCESS, $"{(profile_.current != null? profile_.current.holdID.ToString() : "its null. not")} OK" , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
    public void StopHolding(bool wet___ = false) {
        string funcid = "AsH.EndHold";
        try {
            SWS(funcid);
            profile_.EndHoldData(wet___);
            RequestUiUpdateMainHolding.Invoke(null, EventArgs.Empty);
            RequestHistoryUiMainHolding.Invoke(null, EventArgs.Empty);
            PrintDebug(funcid, SUCESS, $"{(profile_.current == null? "null" : "null not")} OK" , SWE(funcid));
        } catch (Exception e) {
            PrintDebug(funcid, FUCK, $"{e.Message}, {e.InnerException?.Message}", SWE(funcid));
            throw;
        }
    }
}