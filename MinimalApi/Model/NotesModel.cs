using System.ComponentModel.DataAnnotations;

namespace MinimalApi.Model
{
    public class NotesModel
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; }  = DateTime.Now;
    }
}
