namespace WebAPIN5.Models
{
    public class OperationMessageDTO
    {
        public Guid Id { get; set; }
        public string OperationName { get; set; }

        public override string ToString()
        {
            return $"OperationMessageDTO: {{Id:{Id}, OperationName: {OperationName}}}";
        }

    }
}
