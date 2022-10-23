using System.Collections.Generic;

namespace API.Model
{
    public class ChessParam
    {
        public bool IsMaxmizer { get; set; }
        public List<List<Piece>> Pieces { get; set; }
    }
}
