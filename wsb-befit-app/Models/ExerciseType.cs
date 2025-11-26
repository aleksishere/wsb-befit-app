using System.ComponentModel.DataAnnotations;

namespace BeFit.Models;

public class ExerciseType
{
    public int Id { get; set; }

    [MaxLength(100)]
    [Display(Name = "Nazwa ćwiczenia")]
    public string Name { get; set; }

}