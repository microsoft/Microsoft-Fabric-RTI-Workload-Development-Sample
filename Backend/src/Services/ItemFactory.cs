// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using Fabric.Rti.workload.Backend.Constants;
using Fabric.Rti.workload.Backend.Contracts;
using Fabric.Rti.workload.Backend.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fabric.Rti.workload.Backend.Services
{
    public class ItemFactory : IItemFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IItemMetadataStore _itemMetadataStore;
        private readonly IAuthenticationService _authenticationService;
        private readonly IFabricApiClient _fabricApiClient;
        private readonly IKustoClientService _kustoClientService;

        public ItemFactory(
            IServiceProvider serviceProvider,
            IItemMetadataStore itemMetadataStore,
            IAuthenticationService authenticationService,
            IFabricApiClient fabricApiClient,
            IKustoClientService kustoClientService)
        {
            _serviceProvider = serviceProvider;
            _itemMetadataStore = itemMetadataStore;
            _authenticationService = authenticationService;
            _fabricApiClient = fabricApiClient;
            _kustoClientService = kustoClientService;
        }

        public IItem CreateItem(string itemType, AuthorizationContext authorizationContext)
        {
            switch (itemType)
            {
                case WorkloadConstants.ItemTypes.Item1:
                    return new Item1(_serviceProvider.GetService<ILogger<Item1>>(), _itemMetadataStore, _authenticationService, _fabricApiClient, _kustoClientService, authorizationContext);

                default:
                    throw new NotSupportedException($"Items of type {itemType} are not supported");
            }
        }
    }
}
