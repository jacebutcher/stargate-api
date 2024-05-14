using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace StargateAPI.Business.Data
{
    [Table("API_Exceptions")]
    public class API_Exceptions
    {
        public int Logid { get; set; }

        public string ExceptionMsg { get; set; } = string.Empty;

        public string ExceptionType { get; set; } = string.Empty;

        public string ExceptionSource { get; set; } = string.Empty;

        public string ExceptionURL { get; set; } = string.Empty;

        public DateTime Logdate { get; set; }
    }

    public class API_ExceptionsConfiguration : IEntityTypeConfiguration<API_Exceptions>
    {
        public void Configure(EntityTypeBuilder<API_Exceptions> builder)
        {
            builder.HasKey(x => x.Logid);
            builder.Property(x => x.Logid).ValueGeneratedOnAdd();
        }
    }
}
