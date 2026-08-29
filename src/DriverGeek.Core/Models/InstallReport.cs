namespace DriverGeek.Core.Models;

/// <summary>How far an install got. The stage is where it stopped, not where it started.</summary>
public enum InstallStage
{
    /// <summary>The gate said no. Nothing on the machine was touched.</summary>
    Refused,

    /// <summary>Stopped while making the System Restore point.</summary>
    RestorePoint,

    /// <summary>Stopped while exporting the driver that is installed now.</summary>
    Export,

    /// <summary>Stopped while downloading the package from Windows Update.</summary>
    Download,

    /// <summary>Stopped while Windows was installing it. The driver may or may not have changed.</summary>
    Install,

    /// <summary>Finished.</summary>
    Done
}

/// <summary>The outcome of one device's install, written in words a person can act on.</summary>
public sealed record InstallReport
{
    public string DeviceName { get; init; } = "";
    public InstallStage Stage { get; init; }
    public bool Succeeded { get; init; }
    public bool RebootRequired { get; init; }

    /// <summary>What happened, in one or two sentences.</summary>
    public string Message { get; init; } = "";

    /// <summary>Where the previous driver was written, when it was written.</summary>
    public string BackupPath { get; init; } = "";

    /// <summary>True when nothing on the machine was changed, so the user can stop worrying.</summary>
    public bool NothingChanged => Stage is InstallStage.Refused or InstallStage.RestorePoint
        or InstallStage.Export or InstallStage.Download;
}
