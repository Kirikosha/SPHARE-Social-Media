using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.DetailedUserInfoDTOs;
public class UserProfileDetailsDto
{
    public int Id { get; set; }
    public string? Pronouns { get; set; }
    public string? MainProfileDescription { get; set; }
    public List<string> Interests { get; set; } = [];
    public DateTime? DateOfBirth { get; set; }
}
