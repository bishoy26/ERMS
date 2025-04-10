namespace ERMS.Models
{
    public class Employee
    {
        public int EmployeeID { get; set; }
        // New property to link the Employee record to the Identity user.
        public string IdentityUserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Role { get; set; }
    }
}
