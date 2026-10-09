using System;
using System.Collections.Generic;
using System.Text;

namespace Library.Domain.Entities
{
    public class BookType
    {

        public int Id { get; set; }
        public string? BookTypeName { get; set; }

        public ICollection<BookBookTypeBridge> BookBookTypeBridges { get; set; } = new List<BookBookTypeBridge>();
    }
}
