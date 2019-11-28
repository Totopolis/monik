using System;
using Monik.Common;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;

namespace Monik.Service
{
    public sealed class SecureNancyModule : NancyModule
    {
        public SecureNancyModule(ICacheMetric cacheMetric, ICacheSourceInstance sourceInstanceCache, IMonik monik)
        {
            this.RequiresAuthentication();

            Delete("/instances/{id:int}", args =>
            {
                try
                {
                    monik.ApplicationInfo($"Delete /instances/{args.id} by {Context.CurrentUser.Identity.Name}");
                    sourceInstanceCache.RemoveInstance(args.id);
                    return HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method DELETE /instances/id : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Delete("/metrics/{id:int}", args =>
            {
                try
                {
                    monik.ApplicationInfo($"Delete /metrics/{args.id} by {Context.CurrentUser.Identity.Name}");
                    cacheMetric.RemoveMetric(args.id);
                    return HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method DELETE /metrics/id : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Delete("/sources/{id:int}", args =>
            {
                try
                {
                    monik.ApplicationInfo($"Delete /sources/{args.id} by {Context.CurrentUser.Identity.Name}");
                    sourceInstanceCache.RemoveSource((short)args.id);
                    return HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method DELETE /sources/id : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

            Post("/groups", args =>
            {
                try
                {
                    var group = this.Bind<Group_>();
                    monik.ApplicationInfo($"Post /groups {group.Name} by {Context.CurrentUser.Identity.Name}");
                    var result = sourceInstanceCache.CreateGroup(group);
                    return Response.AsJson(result, HttpStatusCode.Created)
                        .WithHeader("Location", $"/groups/{result.ID}");
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method POST /groups : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });
            Delete("/groups/{id:int}", args =>
            {
                try
                {
                    monik.ApplicationInfo($"Delete /groups/{args.id} by {Context.CurrentUser.Identity.Name}");
                    var result = sourceInstanceCache.RemoveGroup((short) args.id);
                    return result ? HttpStatusCode.OK : HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method DELETE /groups/id : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });
            Put("/groups/{groupId:int}/instances/{instanceId:int}", args =>
            {
                try
                {
                    monik.ApplicationInfo($"Put /groups/{args.groupId}/instances/{args.instanceId} by {Context.CurrentUser.Identity.Name}");
                    sourceInstanceCache.AddInstanceToGroup(args.instanceId, (short)args.groupId);
                    return HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method PUT /groups/id/instances/id : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });
            Delete("/groups/{groupId:int}/instances/{instanceId:int}", args =>
            {
                try
                {
                    monik.ApplicationInfo($"Delete /groups/{args.groupId}/instances/{args.instanceId} by {Context.CurrentUser.Identity.Name}");
                    var result = sourceInstanceCache.RemoveInstanceFromGroup(args.instanceId, (short)args.groupId);
                    return result ? HttpStatusCode.OK : HttpStatusCode.NotFound;
                }
                catch (Exception ex)
                {
                    monik.ApplicationError($"Method DELETE /groups/id/instances/id : {ex.Message}");
                    return HttpStatusCode.InternalServerError;
                }
            });

        }
    }
}