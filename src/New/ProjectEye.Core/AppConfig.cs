namespace ProjectEye.Core;

public sealed class AppConfig
{
    public int WarnMinutes { get; set; } = 45;
    public int RestSeconds { get; set; } = 20;
    public int TomatoMinutes { get; set; } = 25;
    public bool SoundEnabled { get; set; } = true;
    public string Theme { get; set; } = "Default";
    public string Language { get; set; } = "zh-CN";
}
