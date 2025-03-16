using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media.Imaging;

namespace omo_tracker.avc;

public class HistoryBoxItem : TemplatedControl {
    public static readonly StyledProperty<Bitmap> _waterimg =
        AvaloniaProperty.Register<HistoryBoxItem, Bitmap>(nameof(WaterImg));
    public Bitmap WaterImg {
        get => GetValue(_waterimg);
        set => SetValue(_waterimg, value);}
    public static readonly StyledProperty<string> _water =
        AvaloniaProperty.Register<HistoryBoxItem, string>(nameof(Water));
    public string Water {
        get => GetValue(_water);
        set => SetValue(_water, value);}
    public static readonly StyledProperty<string> _time =
        AvaloniaProperty.Register<HistoryBoxItem, string>(nameof(Time));
    public string Time {
        get => GetValue(_time);
        set => SetValue(_time, value);}
    public static readonly StyledProperty<string> _nonwater =
        AvaloniaProperty.Register<HistoryBoxItem, string>(nameof(NonWater));
    public string NonWater {
        get => GetValue(_nonwater);
        set => SetValue(_nonwater, value);}
    
    public static readonly StyledProperty<Bitmap> _nonwaterimg =
        AvaloniaProperty.Register<HistoryBoxItem, Bitmap>(nameof(NonWaterImg));
    public Bitmap NonWaterImg {
        get => GetValue(_nonwaterimg);
        set => SetValue(_nonwaterimg, value);}
}