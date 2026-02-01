using api_security.domain.Abstractions;
using System.Threading.Tasks;

namespace api_security.domain.Entities.FoodPlans
{
    public interface IFoodPlanRepository : IRepository<FoodPlan>
    {
    }
}