using Cofoundry.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cofoundry.BasicTestSite.Models.BlogPost
{
    public class BlogPostShortDescriptionContainsSpecification : CustomEntitySearchSpecificationBase<BlogPostDataModel>
    {
        private readonly string _contains;
        public BlogPostShortDescriptionContainsSpecification(string contains)
        {
            _contains = contains;
        }

        public override Expression<Func<BlogPostDataModel, bool>> SatisfiedBy => c => c.ShortDescription.Contains(_contains);
    }
}
