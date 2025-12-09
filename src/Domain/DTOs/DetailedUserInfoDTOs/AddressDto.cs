using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs.DetailedUserInfoDTOs;
public class AddressDto
{
    public int Id { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}
