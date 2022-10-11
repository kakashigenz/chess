var config = {
  position: "start",
  draggable: true,
  dropOffBoard: "snapback",
};

var board = Chessboard("myBoard", config);

// board.move('e2-e4') hàm di chuyển với tham số là chuỗii chứa toạ đọ điểm bắt đầu và toạ độ điểm kết thúc
$("#replay-btn").click(function (e) {
  board.start();
});

$("#flip-btn").click(board.flip);
