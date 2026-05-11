namespace BlazorAI.Entities
{
    public class Person
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Email { get; set; }
        public decimal Salary { get; set; } 
        public bool IsActive { get; set; }
    }
}
