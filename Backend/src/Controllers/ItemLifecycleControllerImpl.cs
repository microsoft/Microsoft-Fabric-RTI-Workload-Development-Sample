// <copyright company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;
using Fabric.Rti.workload.Backend.Contracts.FabricAPI.Workload;
using Fabric.Rti.workload.Backend.Services;
using Fabric.Rti.workload.Backend.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fabric.Rti.workload.Backend.Controllers
{
    [ServiceFilter(typeof(RequestLoggingFilter))]
    internal class ItemLifecycleControllerImpl : IItemLifecycleController
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ItemLifecycleControllerImpl> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly IItemFactory _itemFactory;

        public ItemLifecycleControllerImpl(
            IHttpContextAccessor httpContextAccessor,
            ILogger<ItemLifecycleControllerImpl> logger,
            IAuthenticationService authenticationService,
            IItemFactory itemFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _authenticationService = authenticationService;
            _itemFactory = itemFactory;

            _logger.LogTrace("FabricItemsLifecycleHandler: Instance created");
        }

        /// <inheritdoc/>
        public async Task CreateItemAsync(Guid workspaceId, string itemType, Guid itemId, CreateItemRequest createItemRequest)
        {
            var authorizationContext = await _authenticationService.AuthenticateControlPlaneCall(_httpContextAccessor.HttpContext);
            var item = _itemFactory.CreateItem(itemType, authorizationContext);
            await item.Create(workspaceId, itemId, createItemRequest);
        }

        /// <inheritdoc/>
        public async Task UpdateItemAsync(Guid workspaceId, string itemType, Guid itemId, UpdateItemRequest updateItemRequest)
        {
            var authorizationContext = await _authenticationService.AuthenticateControlPlaneCall(_httpContextAccessor.HttpContext);
            var item = _itemFactory.CreateItem(itemType, authorizationContext);
            await item.Load(itemId);
            await item.Update(updateItemRequest);
        }

        /// <inheritdoc/>
        public async Task DeleteItemAsync(Guid workspaceId, string itemType, Guid itemId)
        {
            // Note: The subject token may not always be present during OnDeleteFabricItemAsync.
            var authorizationContext = await _authenticationService.AuthenticateControlPlaneCall(_httpContextAccessor.HttpContext, requireSubjectToken: false);
            if (!authorizationContext.HasSubjectContext)
            {
                _logger.LogWarning($"Subject token not provided for method {nameof(ItemLifecycleControllerImpl)}.{nameof(DeleteItemAsync)}.");
            }

            var item = _itemFactory.CreateItem(itemType, authorizationContext);
            await item.Load(itemId);
            await item.Delete();
        }

        /// <inheritdoc/>
        public async Task<GetItemPayloadResponse> GetItemPayloadAsync(Guid workspaceId, string itemType, Guid itemId)
        {
            var authorizationContext = await _authenticationService.AuthenticateControlPlaneCall(_httpContextAccessor.HttpContext);

            var item = _itemFactory.CreateItem(itemType, authorizationContext);
            await item.Load(itemId);

            var itemPayload = await item.GetItemPayload();

            return new GetItemPayloadResponse
            {
                ItemPayload = itemPayload,
            };
        }
    }
}