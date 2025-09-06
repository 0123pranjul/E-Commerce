namespace JobPortalManagement.Models.DTO
{
    public class FieldMasterVM
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string FieldType { get; set; } = default!;
        public string CssClass { get; set; } = default!;
        public bool IsRequired { get; set; }
        public bool IsVisible { get; set; }
        public int OrderNo { get; set; }
        public string? OptionsJson { get; set; }
    }
    public class FieldMasterListVM
    {
        public List<FieldMasterVM> Fields { get; set; } = new();
    }


    public class RegistrationPageVM
    {
        public List<FieldMasterVM> Fields { get; set; } = new();
        // Fixed fields for registration
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
