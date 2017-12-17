using System.Threading.Tasks;

namespace Quokka.Public.Transformation
{
    public interface IQuokkaProjectTransformation
    {
        Task<TransformationResponse> Transform(TransformationRequest request);
    }
}
