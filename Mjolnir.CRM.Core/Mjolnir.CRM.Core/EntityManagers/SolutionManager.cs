﻿using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Mjolnir.CRM.Core.Enums;
using Mjolnir.CRM.Sdk.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mjolnir.CRM.Sdk.Extensions;

namespace Mjolnir.CRM.Core.EntityManagers
{
    //TODO : Do not use Entity as input or output, use real entity type
    //TODO : Convert to async methods

    public class SolutionManager : EntityManagerBase<SolutionEntity>
    {
        internal override string[] DefaultFields => new[] {
            EntityAttributes.PublisherEntityAttributes.FriendlyName,
            EntityAttributes.PublisherEntityAttributes.UniqueName,
            EntityAttributes.PublisherEntityAttributes.CustomizationPrefix,
            EntityAttributes.PublisherEntityAttributes.Description,
            EntityAttributes.PublisherEntityAttributes.SupportingWebsiteUrl,
            EntityAttributes.PublisherEntityAttributes.EMailAddress
        };

        public SolutionManager(CrmContext context)
            : base(context, EntityAttributes.SolutionEntityAttributes.EntityName)
        { }


        public async Task<List<SolutionEntity>> GetAllSolutionsAsync()
        {
            try
            {
                context.TracingService.TraceVerbose("GetAllSolutions started.");

                var retrieveSolutionColumns = new[] {
                    EntityAttributes.SolutionEntityAttributes.FriendlyNameFieldName,
                    EntityAttributes.SolutionEntityAttributes.ParentSolutionIdFieldName,
                    EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName,
                    EntityAttributes.SolutionEntityAttributes.VersionFieldName,
                    EntityAttributes.SolutionEntityAttributes.IsManagedFieldName,
                    EntityAttributes.SolutionEntityAttributes.Description
                };

                var query = new QueryExpression(EntityAttributes.SolutionEntityAttributes.EntityName)
                {
                    ColumnSet = new ColumnSet(retrieveSolutionColumns)
                };

                return (await RetrieveMultipleAsync(query)).ToList<SolutionEntity>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return null;
            }
        }

        public string GetUniqueSolutionName(Entity solution)
        {
            context.TracingService.TraceVerbose("GetUniqueSolutionName started.");

            try
            {
                if (solution.Contains(EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName) &&
                        solution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName) != null)
                {
                    return solution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return string.Empty;
        }

        public bool IsPatchSolution(Entity solution)
        {
            context.TracingService.TraceVerbose("IsPatchSolution started.");

            try
            {
                if (solution.Contains(EntityAttributes.SolutionEntityAttributes.ParentSolutionIdFieldName) &&
                        solution.GetAttributeValue<EntityReference>(EntityAttributes.SolutionEntityAttributes.ParentSolutionIdFieldName) != null)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return false;
        }

        public bool IsManagedSolution(Entity solution)
        {
            context.TracingService.TraceVerbose("IsManagedSolution started.");

            try
            {
                if (solution.Contains(EntityAttributes.SolutionEntityAttributes.IsManagedFieldName) &&
                        solution.GetAttributeValue<bool>(EntityAttributes.SolutionEntityAttributes.IsManagedFieldName))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return false;
        }

        public bool UpdateSolutionVersion(Entity solution, Version newVersion)
        {
            context.TracingService.TraceVerbose("UpdateSolutionVersion started.");

            var result = false;

            try
            {
                if (solution.Contains(EntityAttributes.SolutionEntityAttributes.VersionFieldName))
                    solution[EntityAttributes.SolutionEntityAttributes.VersionFieldName] = newVersion.ToString();

                context.OrganizationService.Update(solution);
                result = true;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return result;
        }

        public bool IsPatchSolutionBySolutionId(Guid solutionId)
        {
            context.TracingService.TraceVerbose("IsPatchSolutionBySolutionId started.");

            var result = false;

            try
            {
                var solution = RetrieveById(solutionId, new ColumnSet(true));
                result = IsPatchSolution(solution);

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return result;
        }

        public EntityReference GetParentSolutionReference(Entity solution)
        {
            context.TracingService.TraceVerbose("GetParentSolutionReference started.");

            try
            {
                if (solution.Contains(EntityAttributes.SolutionEntityAttributes.ParentSolutionIdFieldName) &&
                        solution.GetAttributeValue<EntityReference>(EntityAttributes.SolutionEntityAttributes.ParentSolutionIdFieldName) != null)
                {
                    return solution.GetAttributeValue<EntityReference>(EntityAttributes.SolutionEntityAttributes.ParentSolutionIdFieldName);
                }
                else return null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return null;
            }
        }

        public CloneAsSolutionResponse CloneAsSolution(Entity solution)
        {
            context.TracingService.TraceVerbose("CloneAsSolution started.");

            try
            {
                CloneAsSolutionRequest cloneAsSolutionRequest = new CloneAsSolutionRequest()
                {
                    ParentSolutionUniqueName = solution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName),
                    DisplayName = solution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.FriendlyNameFieldName),
                    VersionNumber = CalculateNewVersion(
                            new Version(solution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.VersionFieldName))).ToString()
                };

                return (CloneAsSolutionResponse)context.OrganizationService.Execute(cloneAsSolutionRequest);
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return null;
            }
        }
        
        public async Task<Guid> GetSolutionIdByUniqueSolutionNameAsync(string uniqueSolutionName)
        {
            context.TracingService.TraceVerbose("GetSolutionIdByUniqueSolutionName started.");

            try
            {
                var retrieveSolutionColumns = new[] {
                    EntityAttributes.SolutionEntityAttributes.IdFieldName,
                    EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName,
                };

                var solutionRecord = await RetrieveFirstByAttributeExactValueAsync(EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName,
                                                        uniqueSolutionName, retrieveSolutionColumns);


                return solutionRecord.Id;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return Guid.Empty;
            }
        }

        public List<SolutionEntity> GetPatchesBySolutionId(Guid solutionId)
        {
            context.TracingService.TraceVerbose("GetPatchesBySolutionId started.");

            try
            {
                var retrieveSolutionColumns = new[] {
                    EntityAttributes.SolutionEntityAttributes.FriendlyNameFieldName,
                    EntityAttributes.SolutionEntityAttributes.ParentSolutionIdFieldName,
                    EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName,
                    EntityAttributes.SolutionEntityAttributes.VersionFieldName,
                    EntityAttributes.SolutionEntityAttributes.IsManagedFieldName
                };

                return RetrieveMultipleByAttributeExactValue(EntityAttributes.SolutionEntityAttributes.ParentSolutionIdFieldName, solutionId, retrieveSolutionColumns)
                            .ToList<SolutionEntity>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return null;
            }
        }

        public SolutionEntity CreateSolution(Guid publisherId, string friendlyName, string description, Version version)
        {
            context.TracingService.TraceVerbose("CreateSolution started.");

            try
            {
                //TODO : Validate

                var solution = new Entity(EntityAttributes.SolutionEntityAttributes.EntityName);

                solution.Attributes.Add(EntityAttributes.SolutionEntityAttributes.FriendlyNameFieldName, friendlyName);
                solution.Attributes.Add(EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName, friendlyName);
                solution.Attributes.Add(EntityAttributes.SolutionEntityAttributes.PublisherId, new EntityReference(EntityAttributes.PublisherEntityAttributes.EntityName, publisherId));
                solution.Attributes.Add(EntityAttributes.SolutionEntityAttributes.Description, description);
                solution.Attributes.Add(EntityAttributes.SolutionEntityAttributes.VersionFieldName, version.ToString());


                var newSolutionId = context.OrganizationService.Create(solution);
                solution.Attributes[EntityAttributes.SolutionEntityAttributes.IdFieldName] = newSolutionId;

                return solution.ToSpecificEntity<SolutionEntity>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return null;
            }
        }

        public List<SolutionComponentEntity> RetrieveSolutionComponents(Entity patch)
        {
            context.TracingService.TraceVerbose("RetrieveSolutionComponents started.");

            try
            {
                var patchSolutionId = patch.Id;

                string[] retrieveSolutionComponentColumns = new string[] {
                    EntityAttributes.SolutionComponentEntityAttributes.SolutionComponentId,
                    EntityAttributes.SolutionComponentEntityAttributes.ComponentType,
                    EntityAttributes.SolutionComponentEntityAttributes.SolutionId,
                    EntityAttributes.SolutionComponentEntityAttributes.ObjectId,
                    EntityAttributes.SolutionComponentEntityAttributes.IsMetadata,
                    EntityAttributes.SolutionComponentEntityAttributes.RootComponentBehavior,
                };

                return RetrieveMultipleByAttributeExactValue(EntityAttributes.SolutionComponentEntityAttributes.SolutionId, new object[] { patchSolutionId }, retrieveSolutionComponentColumns)
                    .ToList<SolutionComponentEntity>();
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return null;
            }
        }

        public void CopySolutionComponents(Entity newSolution, EntityCollection solutionComponentsEntityCollection)
        {
            context.TracingService.TraceVerbose("CopySolutionComponents started.");

            //TODO : May throw error for duplicate, check this scenario

            try
            {
                var solutionUniqueName = newSolution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.UniqueNameFieldName);

                if (solutionComponentsEntityCollection != null && solutionComponentsEntityCollection.Entities.Any())
                {
                    foreach (var component in solutionComponentsEntityCollection.Entities)
                    {
                        var componentType = component.GetAttributeValue<OptionSetValue>(EntityAttributes.SolutionComponentEntityAttributes.ComponentType).Value;
                        var isMetadata = component.GetAttributeValue<bool>(EntityAttributes.SolutionComponentEntityAttributes.IsMetadata);

                        AddSolutionComponentRequest addSolutionComponentRequest = new AddSolutionComponentRequest()
                        {
                            AddRequiredComponents = false,
                            ComponentType = componentType,
                            SolutionUniqueName = solutionUniqueName,
                            ComponentId = component.GetAttributeValue<Guid>(EntityAttributes.SolutionComponentEntityAttributes.ObjectId),
                            RequestId = Guid.NewGuid()
                        };

                        if (component.Contains(EntityAttributes.SolutionComponentEntityAttributes.RootComponentBehavior))
                        {
                            var behaviour = (SolutionComponentRootComponentBehavior)component.GetAttributeValue<OptionSetValue>(EntityAttributes.SolutionComponentEntityAttributes.RootComponentBehavior).Value;
                            if (behaviour == SolutionComponentRootComponentBehavior.Donotincludesubcomponents && isMetadata)
                            {
                                addSolutionComponentRequest.DoNotIncludeSubcomponents = true;
                            }
                        }

                        context.OrganizationService.Execute(addSolutionComponentRequest);
                    }
                }
                else
                {
                    context.TracingService.TraceVerbose("No component found for transferring.");
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        public bool IsPatchInDescription(Entity solution)
        {
            context.TracingService.TraceVerbose("IsPatchInDescription started.");

            try
            {
                if (solution.Contains(EntityAttributes.SolutionEntityAttributes.Description))
                {
                    var description = solution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.Description);

                    //TODO: contastant
                    if (!string.IsNullOrWhiteSpace(description) && description.Contains("is_patch:true"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return false;
        }

        public bool IsUpgradeForSolution(string parentSolutionUniqueName, Entity solution)
        {
            context.TracingService.TraceVerbose("IsUpgradeForSolution started.");

            try
            {
                if (solution.Contains(EntityAttributes.SolutionEntityAttributes.Description))
                {
                    var description = solution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.Description);

                    //TODO: contastant
                    if (!string.IsNullOrWhiteSpace(description) && description.Contains("base_solution_uniquename:" + parentSolutionUniqueName))
                    {
                        return true;
                    }
                    else
                        context.TracingService.TraceVerbose("Solution Name : " + parentSolutionUniqueName);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
            return false;
        }

        public Version GetSolutionVersion(Entity solution)
        {
            return new Version(solution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.VersionFieldName));
        }

        public void RemoveSolutions(List<Guid> solutionIds)
        {
            context.TracingService.TraceVerbose("RemoveSolutions started.");

            foreach (var solutionId in solutionIds)
            {
                try
                {
                    context.OrganizationService.Delete(EntityAttributes.SolutionEntityAttributes.EntityName, solutionId);
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
            }
        }

        public async Task<EntityCollection> GetSolutionUpgradesAync(Entity parentSolution)
        {
            context.TracingService.TraceVerbose("GetSolutionUpgrades started.");

            var upgradeSolutions = new EntityCollection();
            var sortedUpgradeSolutions = new EntityCollection();

            var allSolutions = await GetAllSolutionsAsync();
            var parentSolutionName = GetUniqueSolutionName(parentSolution);

            foreach (var solution in allSolutions)
            {
                //context.TracingService.Trace("\n====\n" + solution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.Description) + "\n====\n");

                if (IsPatchInDescription(solution) && IsUpgradeForSolution(parentSolutionName, solution))
                {
                    context.TracingService.TraceVerbose("Upgrade Solution : " + GetUniqueSolutionName(solution));
                    upgradeSolutions.Entities.Add(solution);
                }
            }

            //Send in ascending order in solution version
            if (upgradeSolutions.Entities.Any())
            {
                sortedUpgradeSolutions = new EntityCollection(upgradeSolutions.Entities.OrderByDescending(key => GetPatchVersionInDescription(key)).ToList());
            }

            return sortedUpgradeSolutions;
        }

        public Guid CreatePatchForBaseSolution(Entity parentSolution, Entity upgradeSolution)
        {
            context.TracingService.TraceVerbose("CreatePatchForBaseSolution started.");

            //Get the version of patch and create a real patch with the same version
            Version patchVersion = GetSolutionVersion(upgradeSolution);

            try
            {
                CloneAsPatchRequest patchRequest = new CloneAsPatchRequest();
                patchRequest.ParentSolutionUniqueName = GetUniqueSolutionName(parentSolution);
                patchRequest.VersionNumber = patchVersion.ToString();
                patchRequest.DisplayName = GetUniqueSolutionName(upgradeSolution);

                var response = (CloneAsPatchResponse)context.OrganizationService.Execute(patchRequest);

                return response.SolutionId;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return Guid.Empty;
            }
        }

        private Version GetPatchVersionInDescription(Entity upgradeSolution)
        {
            context.TracingService.TraceVerbose("GetPatchVersionInDescription started.");

            try
            {
                if (upgradeSolution.Contains(EntityAttributes.SolutionEntityAttributes.Description))
                {
                    var description = upgradeSolution.GetAttributeValue<string>(EntityAttributes.SolutionEntityAttributes.Description);

                    //TODO: contastant
                    if (description.Contains("is_patch:true"))
                    {
                        var lines = description.Split('\n');
                        var versionLine = lines.Where(w => w.Contains("base_solution_verison:")).FirstOrDefault();

                        if (!string.IsNullOrWhiteSpace(versionLine))
                        {
                            string version = versionLine.Substring(("base_solution_verison:").Length).Trim();

                            context.TracingService.TraceVerbose("version : " + version);

                            return new Version(version);
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                return null;
            }
        }


        public Version CalculateNewVersion(Version oldVersion)
        {
            context.TracingService.TraceVerbose("CalculateNewVersion started.");
            return new Version(oldVersion.Major, oldVersion.Minor + 1, 0, 0);
        }
    }
}

