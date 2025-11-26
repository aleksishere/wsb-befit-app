using System.ComponentModel.DataAnnotations;

namespace BeFit.Models;

public class TrainingSession
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public virtual Users User { get; set; }
    [Display(Name = "Data i czas rozpoczęcia")]
    public DateTime StartTime { get; set; }

    [Display(Name = "Data i czas zakończenia")]
    public DateTime EndTime { get; set; }
}