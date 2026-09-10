namespace BlazorIA.Entidades
{
    public class Persona
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Email { get; set; }
        public required decimal Salario { get; set; }
        public required bool Activo { get; set; }

    }
}
