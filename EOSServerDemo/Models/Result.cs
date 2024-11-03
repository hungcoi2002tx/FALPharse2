using System;
using System.Collections.Generic;

namespace EOSServerDemo.Models;

public partial class Result
{
    public int ResultId { get; set; }

    public int SourceId { get; set; }

    public DateTime Time { get; set; }

    public string Status { get; set; } = null!;

    public double Confidence { get; set; }

    public string? Message { get; set; }

    public virtual Source Source { get; set; } = null!;
}
