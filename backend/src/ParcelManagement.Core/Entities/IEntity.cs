using System.ComponentModel.DataAnnotations;

namespace ParcelManagement.Core.Entities
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}