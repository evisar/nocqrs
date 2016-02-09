using sample.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nosqr.api
{

    public abstract class Command<TFeature, TEntity> : ICommand<TFeature, TEntity>
        where TFeature : IFeature<TEntity>
        where TEntity : IEntity
    {
        public TEntity Model { get; private set; }
        public object[] Arguments { get; private set; }

        public Command(TEntity entity, params object[] args)
        {
            this.Model = entity;
            this.Arguments = args;
        }
    }
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
