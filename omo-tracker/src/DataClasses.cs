using System;
using Avalonia.Media.Imaging;

namespace omo_tracker;

public enum Status {
    SUCESS,
    FAILURE,
    STATUSUPDATE,
    FUCK
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
    public string prevact;
    public Bitmap waterimg = DataIO.GetBitmap("res\\water\\wmt.bmp");
    public Bitmap nonwatrimg = DataIO.GetBitmap("res\\nonwater\\umt.bmp");
    public bool isholding;
    public TimeSpan holdingtime = TimeSpan.FromSeconds(0);
    public DateTime starttime = new DateTime();
    public DateTime nowtime = new DateTime();

}
public class UiProfileData {
    public Bitmap pfp = DataIO.GetBitmap("res\\water\\wmt.bmp");
    public string nickname = "Choose profile";
}