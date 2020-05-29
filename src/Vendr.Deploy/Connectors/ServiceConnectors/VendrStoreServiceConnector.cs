using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Deploy;
using Umbraco.Deploy.Connectors.ServiceConnectors;
using Umbraco.Deploy.Exceptions;
using Vendr.Core.Models;
using Vendr.Core.Services;
using Vendr.Deploy.Artifacts;

namespace Vendr.Deploy.Connectors.ServiceConnectors
{
    [UdiDefinition("vendr-store", UdiType.GuidUdi)]
    public class VendrStoreServiceConnector : ServiceConnectorBase<StoreArtifact, GuidUdi, ArtifactDeployState<StoreArtifact, StoreReadOnly>>
    {
        private static readonly string[] ValidOpenSelectors = new []
        {
            "this-and-descendants",
            "descendants"
        };

        private readonly IStoreService _storeService;

        public VendrStoreServiceConnector(IStoreService storeService)
        {
            _storeService = storeService;
        }

        public override StoreArtifact GetArtifact(object o)
        {
            var store = o as StoreReadOnly;
            if (store == null)
                throw new InvalidEntityTypeException(string.Format("Unexpected entity type \"{0}\".", (object)o.GetType().FullName));

            return GetArtifact(store.GetUdi(), store);
        }

        public override StoreArtifact GetArtifact(GuidUdi udi)
        {
            EnsureType(udi);

            return GetArtifact(udi, _storeService.GetStore(udi.Guid));
        }

        private StoreArtifact GetArtifact(GuidUdi udi, StoreReadOnly store)
        {
            if (store == null)
                return null;

            return new StoreArtifact(udi, null)
            {
                Name = store.Name,
                Alias = store.Alias
            };
        }

        public override NamedUdiRange GetRange(GuidUdi udi, string selector)
        {
            EnsureType(udi);

            if (udi.IsRoot)
            {
                EnsureSelector(selector, ValidOpenSelectors);

                return new NamedUdiRange(udi, "ALL VENDR STORES", selector);
            }

            var store = _storeService.GetStore(udi.Guid);
            if (store == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(udi));

            return GetRange(store, selector);
        }

        public override NamedUdiRange GetRange(string entityType, string sid, string selector)
        {
            if (sid == "*")
            {
                EnsureSelector(selector, ValidOpenSelectors);
                return new NamedUdiRange(Udi.Create("vendr-store"), "ALL VENDR STORES", selector);
            }

            if (!Guid.TryParse(sid, out Guid result))
                throw new ArgumentException("Invalid identifier.", nameof(sid));

            
            var store = _storeService.GetStore(result);
            if (store == null)
                throw new ArgumentException("Could not find an entity with the specified identifier.", nameof(sid));

            return GetRange(store, selector);
        }

        private static NamedUdiRange GetRange(StoreReadOnly e, string selector)
        {
            return new NamedUdiRange(e.GetUdi(), e.Name, selector);
        }

        public override void Explode(UdiRange range, List<Udi> udis)
        {
            EnsureType(range.Udi);
            
            if (range.Udi.IsRoot)
            {
                EnsureSelector(range, ValidOpenSelectors);

                udis.AddRange(_storeService.GetStores().Select(e => e.GetUdi()));
            }
            else
            {
                var store = _storeService.GetStore(((GuidUdi)range.Udi).Guid);
                if (store == null)
                    return;

                EnsureSelector(range.Selector, new [] { "this" });

                udis.Add(store.GetUdi());
            }
        }

        public override ArtifactDeployState<StoreArtifact, StoreReadOnly> ProcessInit(StoreArtifact art, IDeployContext context)
        {
            EnsureType(art.Udi);

            var store = _storeService.GetStore(art.Udi.Guid);

            return ArtifactDeployState.Create(art, store, this, 0);
        }

        public override void Process(ArtifactDeployState<StoreArtifact, StoreReadOnly> state, IDeployContext context, int pass)
        {
            if (pass != 0)
                throw new ArgumentOutOfRangeException(nameof(pass));

            state.NextPass = -1;

            // TODO: Use or create a store with a given ID
            // then copy the values over?

            //var store = state.Entity ?? (ILanguage)new Language(state.Artifact.IsoCode);

            //language.CultureName = state.Artifact.Name;
            //this._localizationService.Save(language, 0);
        }

        protected void EnsureType(Udi udi)
        {
            if (udi == null)
                throw new ArgumentNullException(nameof(udi));

            udi.EnsureType(ValidEntityTypes);
        }
    }
}
