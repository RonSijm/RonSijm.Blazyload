using Microsoft.Extensions.DependencyInjection;
using RonSijm.Syringe;

namespace RonSijm.Blazyload;
public class BlazyFluxorOptions(IServiceCollection services) : SyringeFluxorOptions(services)
{

}