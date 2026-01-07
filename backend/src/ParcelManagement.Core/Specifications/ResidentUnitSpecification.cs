using System.Linq.Expressions;
using ParcelManagement.Core.Entities;

namespace ParcelManagement.Core.Specifications
{
    public class ResidentUnitByUnitNameSpecification(string unitName) : ISpecification<ResidentUnit>
    {
        List<IncludeExpression<ResidentUnit>> ISpecification<ResidentUnit>.IncludeExpressions => [];

        public int? Page => throw new NotImplementedException();

        public int? Take => throw new NotImplementedException();

        public int? Skip => null;

        List<IncludeExpressionString> ISpecification<ResidentUnit>.IncludeExpressionsString => [];

        public Expression<Func<ResidentUnit, object>> OrderBy => throw new NotImplementedException();

        public Expression<Func<ResidentUnit, object>> OrderByDesc => throw new NotImplementedException();

        public Expression<Func<ResidentUnit, bool>> ToExpression() => ru => ru.UnitName == unitName;
    }

    public class ResidentUnitViewSpecification : ISpecification<ResidentUnit>
    {
        private readonly string? _unitName;
        private readonly ResidentUnitSortableColumn? _column;
        private readonly bool _isAsc;
        private readonly int? _page;
        private readonly int? _take;
        public ResidentUnitViewSpecification(
            string? unitName,
            ResidentUnitSortableColumn? column,
            int? page,
            int? take,
            bool isAsc = true
        )
        {
            _unitName = unitName;
            _column = column;
            _isAsc = isAsc;
            _page = page;
            _take = take;
            IncludeExpressionsString = [
                new IncludeExpressionString("CreatedByUser"),
                new IncludeExpressionString("UpdatedByUser")
            ];
        }
        List<IncludeExpression<ResidentUnit>> ISpecification<ResidentUnit>.IncludeExpressions => [];

        public List<IncludeExpressionString> IncludeExpressionsString { get; }

        public Expression<Func<ResidentUnit, object>>? OrderBy => _isAsc ? GetSortExpression() : null;

        public Expression<Func<ResidentUnit, object>>? OrderByDesc => _isAsc ? null : GetSortExpression();

        public int? Page => _page;

        public int? Take => _take;

        public int? Skip => null;

        public Expression<Func<ResidentUnit, bool>> ToExpression()
        {
            return ru =>
                string.IsNullOrEmpty(_unitName) || ru.UnitName.Contains(_unitName);
        }

        private Expression<Func<ResidentUnit, object>> GetSortExpression()
        {
            return _column switch
            {
                ResidentUnitSortableColumn.UnitName => ru => ru.UnitName,
                ResidentUnitSortableColumn.CreatedAt => ru => ru.CreatedAt,
                _ => ru => ru.Id
            };
        }
    }

    public class ResidentUnitResidentsSpecification : ISpecification<ResidentUnit>
    {
        private readonly Guid _residentUnitId;
        public ResidentUnitResidentsSpecification(Guid residentUnitId)
        {
            _residentUnitId = residentUnitId;
            IncludeExpressionsString = [
                new IncludeExpressionString("UserResidentUnits.User")
            ];
        }

        List<IncludeExpression<ResidentUnit>> ISpecification<ResidentUnit>.IncludeExpressions => throw new NotImplementedException();

        public List<IncludeExpressionString> IncludeExpressionsString { get; }

        public Expression<Func<ResidentUnit, object>>? OrderBy => null;

        public Expression<Func<ResidentUnit, object>>? OrderByDesc => null;

        public int? Page => null;

        public int? Take => null;

        public int? Skip => null;

        public Expression<Func<ResidentUnit, bool>> ToExpression()
        {
            return residentUnit => residentUnit.Id == _residentUnitId;
        }
    }
}