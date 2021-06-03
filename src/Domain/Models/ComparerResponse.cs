namespace Domain.Models
{
    public class ComparerResponse
    {
        public string Message { get; set; }
        public object Data { get; set; }
    } 
    public enum ResponseType
    {
        OBJECTS_ARE_EQUAL,
        OBJECTS_ARE_NOT_EQUAL,
        OBJECTS_ARE_NOT_OF_SAME_SIZE
    }
}
