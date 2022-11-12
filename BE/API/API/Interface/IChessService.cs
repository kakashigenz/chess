using API.Model.Response;
using API.Model;
using System.Collections.Generic;

namespace API.Interface
{
    public interface IChessService
    {
        public NextStepResponse GetNextStep(ChessParam param);
    }
}
