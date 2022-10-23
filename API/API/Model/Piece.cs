using API.Enum;

namespace API.Model
{
    public class Piece
    {
        /// <summary>
        /// Quân cờ nào
        /// </summary>
        public ChessEnum Chess { get; set; }
        /// <summary>
        /// Giá trị quân cờ
        /// </summary>
        public int Value { get; set; }
        /// <summary>
        /// Đã đi chưa(dùng cho quân tốt, hoặc nhập thành của xe và vua)
        /// </summary>
        public bool IsMoved { get; set; }
    }
}
