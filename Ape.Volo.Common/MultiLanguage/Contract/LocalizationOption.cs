using System;

namespace Ape.Volo.Common.MultiLanguage.Contract;

public class LocalizationOption
{
    public Type LocalizationType { get; set; }
    public string ResourcesPath { get; set; }
    public string DefaultCulture { get; set; }
    public string[] SupportedCultures { get; set; }
}
