using System;
using System.Collections.Generic;

namespace EOSServerDemo.Models;

public partial class Source
{
    public int SourceId { get; set; }

    public string StudentCode { get; set; } = null!;

    public string ImagePath { get; set; } = null!;

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();
}
