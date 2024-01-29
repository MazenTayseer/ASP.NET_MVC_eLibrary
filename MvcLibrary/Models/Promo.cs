using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace MvcLibrary.Models;

public class Promo
{
    [Key] public int PromoId { get; set; }

    public string? Code { get; set; }

    [ForeignKey("UserId")] public IdentityUser? User { get; set; }

    public bool Used { get; set; }
}