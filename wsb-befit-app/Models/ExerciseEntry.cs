using System.ComponentModel.DataAnnotations;

namespace BeFit.Models;

public class ExerciseEntry
{
    public int Id { get; set; }

    [Display(Name = "Typ ćwiczenia")]
    public int ExerciseTypeId { get; set; }
    public virtual ExerciseType ExerciseType { get; set; }

    [Display(Name = "Sesja treningowa")]
    public int TrainingSessionId { get; set; }
    public virtual TrainingSession TrainingSession { get; set; }

    [Display(Name = "Obciążenie (kg)")]
    public float Load { get; set; }

    [Display(Name = "Liczba serii")]
    public int Sets { get; set; }

    [Display(Name = "Powtórzenia w serii")]
    public int RepetitionsPerSet { get; set; }
}