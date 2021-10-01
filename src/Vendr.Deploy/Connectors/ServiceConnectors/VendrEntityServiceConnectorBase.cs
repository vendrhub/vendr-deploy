using System;
using System.Collections.Generic;
using System.Linq;
using Vendr.Core.Api;
using Vendr.Core.Models;

#if NETFRAMEWORK
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Deploy.Artifacts;
using Umbraco.Deploy.Connectors.ServiceConnectors;
using Umbraco.Deploy.Exceptions;
#else
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Deploy.Core.Exceptions;
using Umbraco.Deploy.Infrastructure.Artifacts;
using Umbraco.Deploy.Infrastructure.Connectors.ServiceConnectors;
#endif

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    public abstract class VendrEntityServiceConnectorBase<TArtifact, TEntity> : ServiceConnectorBase<TArtifact, GuidUdi, ArtifactDeployState<TArtifact, TEntity>>
        where TArtifact : DeployArtifactBase<GuidUdi>
        where TEntity : EntityBase
    {
        protected readonly IVendrApi _vendrApi;

        public abstract int[] ProcessPasses { get; }

        public abstract string[] ValidOpenSelectors { get; }

        public abstract string AllEntitiesRangeName { get; }

        public abstract string UdiEntityType { get; }

        public VendrEntityServiceConnectorBase(IVendrApi vendrApi)
        {
            _vendrApi = vendrApi;
        }

        public abstract string GetEntityName(TEntity entity);

        public abstract TEntity GetEntity(Guid id);

        public abstract IEnumerable<TEntity> GetEntities();

        public abstract TArtifact GetArtifact(GuidUdi udi, TEntity entity);

        public override TArtifact GetArtifact(object o)
        {
            var entity = o as TEntity;
            if (entity == null)
                throw new InvalidEntityTypeException(string.Format("Unexpected entity type \"{0}\".", o.GetType().FullName));

            return GetArtifact(entity.GetUdi(), entity);
        }

        public override TArtifact GetArtifact(GuidUdi udi)
        {
            EnsureType(udi);

            return GetArtifact(udi, GetEntity(udi.Guid));
        }

        public override NamedUdiRange GetRange(GuidUdi udi, string selector)
        {
            EnsureType(udi);

            if (udi.IsRoot)
            {
                EnsureSelector(selector, ValidOpenSelectors);

                return new NamedUdiRange(udi, AllEntitiesRangeName, selector);
            }

            var entity = GetEntity(udi.Guid);
            if (entity == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(udi));

            return GetRange(entity, selector);
        }

        public override NamedUdiRange GetRange(string entityType, string sid, string selector)
        {
            if (sid == "-1")
            {
                EnsureSelector(selector, ValidOpenSelectors);

                return new NamedUdiRange(Udi.Create(UdiEntityType), AllEntitiesRangeName, selector);
            }

            if (!Guid.TryParse(sid, out Guid result))
                throw new ArgumentException("Invalid identifier.", nameof(sid));

            var entity = GetEntity(result);
            if (entity == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(sid));

            return GetRange(entity, selector);
        }

        private NamedUdiRange GetRange(TEntity e, string selector)
        {
            return new NamedUdiRange(e.GetUdi(), GetEntityName(e), selector);
        }

        public override void Explode(UdiRange range, List<Udi> udis)
        {
            EnsureType(range.Udi);

            if (range.Udi.IsRoot)
            {
                EnsureSelector(range, ValidOpenSelectors);

                udis.AddRange(GetEntities().Select(e => e.GetUdi()));
            }
            else
            {
                var entity = GetEntity(((GuidUdi)range.Udi).Guid);
                if (entity == null)
                    return;

                EnsureSelector(range.Selector, new[] { "this" });

                udis.Add(entity.GetUdi());
            }
        }

        public override ArtifactDeployState<TArtifact, TEntity> ProcessInit(TArtifact art, IDeployContext context)
        {
            EnsureType(art.Udi);

            var entity = GetEntity(art.Udi.Guid);

            return ArtifactDeployState.Create(art, entity, this, ProcessPasses[0]);
        }
    }
}
