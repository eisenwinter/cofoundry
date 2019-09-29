using Cofoundry.Core;
using Cofoundry.Domain.CQS;
using Cofoundry.Domain.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Cofoundry.Domain
{
    public class SearchCustomEntitiesQueryHandler<T>
        : IAsyncQueryHandler<SearchCustomEntitiesQuery<T>, PagedQueryResult<CustomEntityRenderSummary>>
        , IPermissionRestrictedQueryHandler<SearchCustomEntitiesQuery<T>, PagedQueryResult<CustomEntityRenderSummary>>
         where T : ICustomEntityDataModel
    {
        private readonly CofoundryDbContext _dbContext;
        private readonly ICustomEntityDefinitionRepository _customEntityDefinitionRepository;
        private readonly ICustomEntityRenderSummaryMapper _customEntityRenderSummaryMapper;

        public SearchCustomEntitiesQueryHandler(
            CofoundryDbContext dbContext,
            ICustomEntityDefinitionRepository customEntityDefinitionRepository,
            ICustomEntityRenderSummaryMapper customEntityRenderSummaryMapper
            )
        {
            _dbContext = dbContext;
            _customEntityDefinitionRepository = customEntityDefinitionRepository;
            _customEntityRenderSummaryMapper = customEntityRenderSummaryMapper;
        }


        public async Task<PagedQueryResult<CustomEntityRenderSummary>> ExecuteAsync(SearchCustomEntitiesQuery<T> query, IExecutionContext executionContext)
        {
            var dbPagedResult = await GetQueryAsync(query, executionContext);

            var results = await _customEntityRenderSummaryMapper.MapAsync(dbPagedResult.Items, executionContext);

            return dbPagedResult.ChangeType(results);
        }

        private async Task<PagedQueryResult<CustomEntityVersion>> GetQueryAsync(SearchCustomEntitiesQuery<T> query, IExecutionContext executionContext)
        {
            var definition = _customEntityDefinitionRepository.GetByCode(query.CustomEntityDefinitionCode);
            EntityNotFoundException.ThrowIfNull(definition, query.CustomEntityDefinitionCode);

            var dbQuery = _dbContext
                .CustomEntityPublishStatusQueries
                .AsNoTracking()
                .FilterByCustomEntityDefinitionCode(query.CustomEntityDefinitionCode)
                .FilterActive()
                .FilterByStatus(query.PublishStatus, executionContext.ExecutionDate);

            // Filter by locale 
            if (query.LocaleId > 0 && definition.HasLocale)
            {
                dbQuery = dbQuery.Where(p => p.CustomEntity.LocaleId == query.LocaleId);
            }
            else
            {
                dbQuery = dbQuery.Where(p => !p.CustomEntity.LocaleId.HasValue);
            }

            JsonValueModifier jsonValueModifier = new JsonValueModifier();
            var translatedQuery = 
                (Expression<Func<CustomEntityPublishStatusQuery,bool>>) 
                jsonValueModifier.Visit(query.SearchExpression);

            dbQuery = dbQuery
                .Where(translatedQuery);

            var dbPagedResult = await dbQuery
                .SortBy(definition, query.SortBy, query.SortDirection)
                .Select(p => p.CustomEntityVersion)
                .Include(e => e.CustomEntity)
                .ToPagedResultAsync(query);

            return dbPagedResult;
        }


        public IEnumerable<IPermissionApplication> GetPermissions(SearchCustomEntitiesQuery<T> query)
        {
            var definition = _customEntityDefinitionRepository.GetByCode(query.CustomEntityDefinitionCode);
            EntityNotFoundException.ThrowIfNull(definition, query.CustomEntityDefinitionCode);

            yield return new CustomEntityReadPermission(definition);
        }

        private class JsonValueModifier : ExpressionVisitor
        {
            //ToDo: implement
            //translate anything in the form a => a.Something == x
            //into p = > SqlServerJsonExtension.JsonValue(p.CustomEntityVersion.SerializedData, "Something") == "x"
            //so basically rebuild the input query to a new one using the json function 
            public override Expression Visit(Expression node)
            {
                throw new NotImplementedException();
               
            }

        }

    }
}
