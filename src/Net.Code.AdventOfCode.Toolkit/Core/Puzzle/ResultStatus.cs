namespace Net.Code.AdventOfCode.Toolkit.Core;

using System.ComponentModel.DataAnnotations;

enum ResultStatus
{
    [Display(Name = "N/A")]
    NotImplemented, // not implemented
    Unknown,        // unknown if correct or not
    Failed,         // failed after verification
    Ok,              // correct after verification
    AnsweredButNotImplemented
}
