namespace MinimalApi.Model
{
    public class TodoModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime Date { get; set; } = DateTime.Now;
        public List<NotesModel> Notes { get; set; }

       
    }
}
