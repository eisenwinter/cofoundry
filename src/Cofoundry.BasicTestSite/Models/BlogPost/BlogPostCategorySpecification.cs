using Cofoundry.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Cofoundry.BasicTestSite.Models.BlogPost
{
    public class BlogPostCategorySpecification : CustomEntitySearchSpecificationBase<BlogPostDataModel>
    {
        private readonly int _categoryId;
        public BlogPostCategorySpecification(int categoryId)
        {
            _categoryId = categoryId;
        }

        public override Expression<Func<BlogPostDataModel, bool>> SatisfiedBy => c => c.CategoryId == _categoryId;
    }
}
