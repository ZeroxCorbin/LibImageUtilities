using System.ComponentModel;

namespace LibImageUtilities.ImageTypes.Jpg;
public enum MarkerTypes : byte
{
    [Description("Start of Frame 0")]
    SOF0 = 0xC0,
    [Description("Start of Frame 1")]
    SOF1 = 0xC1,
    [Description("Start of Frame 2")]
    SOF2 = 0xC2,
    [Description("Start of Frame 3")]
    SOF3 = 0xC3,
    [Description("Define Huffman Table")]
    DHT = 0xC4,
    [Description("Start of Frame 5")]
    SOF5 = 0xC5,
    [Description("Start of Frame 6")]
    SOF6 = 0xC6,
    [Description("Start of Frame 7")]
    SOF7 = 0xC7,

    JPG = 0xC8,

    [Description("Start of Frame 9")]
    SOF9 = 0xC9,
    [Description("Start of Frame 10")]
    SOF10 = 0xCA,
    [Description("Start of Frame 11")]
    SOF11 = 0xCB,
    [Description("")]
    DAC = 0xCC,
    [Description("Start of Frame 13")]
    SOF13 = 0xCD,
    [Description("Start of Frame 14")]
    SOF14 = 0xCE,
    [Description("Start of Frame 15")]
    SOF15 = 0xCF,

    [Description("Restart #")]
    RST = 0xD0,

    [Description("Start of Image")]
    SOI = 0xD8,
    [Description("End of Image")]
    EOI = 0xD9,
    [Description("Start of Scan")]
    SOS = 0xDA,
    [Description("Define Quantization Table")]
    DQT = 0xDB,
    [Description("Define Number of Lines")]
    DNL = 0xDC,
    [Description("Define Restart Interval")]
    DRI = 0xDD,
    [Description("Define Hierarchical Progression")]
    DHP = 0xDE,
    [Description("Expand Reference Component(s)")]
    EXP = 0xDF,

    [Description("Application Specific #")]
    APP0 = 0xE0,

    [Description("Comment")]
    COM = 0xFE
}

public enum ThumbnailFormats : byte
{
    //Thumbnail stored using JPEG encoding
    JPEG = 0x10,
    //Thumbnail stored using one byte per pixel
    Palette = 0x11,
    //Thumbnail stored using three byte per pixel
    RGB = 0x13,
    //Something went wrong
    Unknown = 0x00
}
