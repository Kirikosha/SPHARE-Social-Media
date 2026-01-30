using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.ComplaintDTOs;
public class CreateComplaintDto
{
    public required string Reason { get; set; }
    public string? Explanation { get; set; }
    public int TargetId { get; set; }
}
