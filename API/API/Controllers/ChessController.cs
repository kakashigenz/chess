using API.Interface;
using API.Model;
using API.Model.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace API.Controllers
{
    [Route("api/chess")]
    public class ChessController : Controller
    {
        public IChessService _chessService;

        public ChessController(IChessService chessService)
        {
            _chessService = chessService;
        }

        /// <summary>
        /// Nước đi tiếp theo
        /// </summary>
        /// <returns></returns>
        [HttpPost("nextstep")]
        public BaseResponse GetNextStep([FromBody] ChessParam param)
        {
            var response = new BaseResponse();
            try
            {
                var res = _chessService.GetNextStep(param);
                response.OnSuccess(res);
            }catch(Exception ex)
            {
                response.OnException(ex);
            }
            return response;
        }
    }
}
