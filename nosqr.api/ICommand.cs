using sample.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api
{
    public interface ICommand<TFeature, TEntity> : ICommand
        where TFeature: IFeature<TEntity>
        where TEntity: IEntity
    {
        TEntity Model { get; }
        object[] Arguments { get; }
    }

    public interface ICommand : IEvent
    {
    }
}
