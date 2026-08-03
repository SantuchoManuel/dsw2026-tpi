using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities
{
    public abstract class DeleteEntityBase : EntityBase
    {
        public bool Deleted { get; private set; } = false;
        protected DeleteEntityBase(Guid? id = null) : base(id)
        {
        }
        public void MarcarComoEliminado()
        {
            Deleted = true;
        }
    }
}
