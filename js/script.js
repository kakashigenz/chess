import { Chess } from "./chess.js";

var config = {
  position: "start",
  draggable: true,
  dropOffBoard: "snapback",
  onDrop: onDrop,
  onDragStart: onDragStart,
  onSnapEnd: onSnapEnd,
  onMouseoverSquare: onMouseoverSquare,
  onMouseoutSquare: onMouseoutSquare,
  onChange: onChange,
};

var boardElement = $("#myBoard");
var board = Chessboard("myBoard", config);
var game = new Chess();
var whiteSquareGrey = "#a9a9a9";
var blackSquareGrey = "#696969";
var squareClass = "square-55d63";
var table = {
  k: {
    name: 1,
    value: 100,
  },
  q: {
    name: 2,
    value: 9,
  },
  r: {
    name: 3,
    value: 4,
  },
  n: {
    name: 4,
    value: 3,
  },
  b: {
    name: 5,
    value: 3,
  },
  p: {
    name: 6,
    value: 1,
  },
};
var pawnPos = [
  "a7",
  "b7",
  "c7",
  "d7",
  "e7",
  "f7",
  "g7",
  "h7",
  "a2",
  "b2",
  "c2",
  "d2",
  "e2",
  "f2",
  "g2",
  "h2",
];
// mang vi tri bat dau cua vua va xe
var karPos = ["a8", "e8", "h8", "a1", "e1", "h1"];
var checkMove = new Map();
//khai bao su kien
function onDragStart(source, piece, position, orientation) {
  // không được di chuyển quân cờ nếu trò chơi kết thúc
  if (game.game_over()) return false;

  // chỉ được di chuyển quân khi đến lượt
  if (
    (game.turn() === "w" && piece.search(/^b/) !== -1) ||
    (game.turn() === "b" && piece.search(/^w/) !== -1)
  ) {
    return false;
  }
}

function onDrop(source, target) {
  // kiểm tra nước đi hợp lệ
  var res = {
    isMaxmizer: true,
  };

  var move = game.move({
    from: source,
    to: target,
    promotion: "q", // luôn phong cấp quân hậu
  });

  // nước đi không hợp lệ
  if (move === null) return "snapback";

  // danh dau xe hoac vua da di chuyen
  if (
    (move.piece === "r" || move.piece === "k") &&
    karPos.includes(move.from)
  ) {
    checkMove.has(move.from) || checkMove.set(move.from, true);
  }

  var board = game.board().map((item) => {
    return item.map((item) => {
      var move = false;

      // tao phan tu piece
      if (item) {
        //kiem tra quan co da di chua
        if (item.type === "p" && !pawnPos.includes(item.square)) {
          move = true;
        } else if (item.type === "r" || item.type === "k") {
          if (!karPos.includes(item.square)) {
            move = true;
          } else if (checkMove.has(item.square)) {
            move = true;
          }
        }

        return {
          chess: table[item.type].name,
          value:
            item.color === "w"
              ? -table[item.type].value
              : table[item.type].value,
          isMoved: move,
        };
      } else {
        return {
          chess: 0,
          value: 0,
          isMoved: false,
        };
      }
    });
  });

  res.pieces = board;
  console.log(res);

  //ajax call

  // if (game.turn() == "b") {
  //   $.ajax({
  //     url: "https://localhost:5001/api/chess/nextstep",
  //     method: "POST",
  //     crossDomain: true,
  //     headers: { "Access-Control-Allow-Origin": "*" },
  //     data: JSON.stringify(res),
  //     success: function (data) {
  //       console.log(data);
  //     },
  //   });
  // }
  // kiem tra quan co nao vua di
  if (game.turn() == "b") {
    removeHighlights("white");
    boardElement.find(".square-" + source).addClass("highlight-white");
    boardElement.find(".square-" + target).addClass("highlight-white");
  } else {
    removeHighlights("black");
    boardElement.find(".square-" + source).addClass("highlight-black");
    boardElement.find(".square-" + target).addClass("highlight-black");
  }
}

function onChange() {
  updateStatus();
}

// cập nhật lại bàn cờ nếu nhập thành, bắt tốt qua đường, phong cấp
function onSnapEnd() {
  board.position(game.fen());
}

//tô màu các ô có thể di chuyển
function onMouseoverSquare(square, piece) {
  // lấy danh sách các ô có thể di chuyển
  var moves = game.moves({
    square: square,
    verbose: true,
  });

  // thoát nếu không có ô nào để di chuyển
  if (moves.length === 0) return;

  // tô màu ô hiện tại đang trỏ chuột
  greySquare(square);

  // tô màu các ô có thể di chuyển của quân cờ
  for (var i = 0; i < moves.length; i++) {
    greySquare(moves[i].to);
  }
}

//xoá khi di chuyển chuột ra ngoài
function onMouseoutSquare(square, piece) {
  removeGreySquares();
}

//hàm
//ván mới
function handleReset() {
  game.reset();
  board.start();
  removeHighlights("white");
  removeHighlights("black");
}

//hàm thay màu ô vuông
function greySquare(square) {
  var $square = $("#myBoard .square-" + square);

  var background = whiteSquareGrey;
  if ($square.hasClass("black-3c85d")) {
    background = blackSquareGrey;
  }

  $square.css("background", background);
}
//hàm xoá màu các ô di chuyển
function removeGreySquares() {
  $("#myBoard .square-55d63").css("background", "");
}

//hàm xoá màu ô đánh dấu vừa đi
function removeHighlights(color) {
  boardElement.find("." + squareClass).removeClass("highlight-" + color);
}

// hàm cập nhật trạng thái
function updateStatus() {
  var status = "";

  var moveColor = "trắng";
  console.log(game.turn());
  if (game.turn() === "b") {
    moveColor = "đen";
  }

  // chiếu hết
  if (game.in_checkmate()) {
    status = "Trò chơi kết thúc, " + moveColor + " thắng";
  }

  // hoà
  else if (game.in_draw()) {
    status = "Hoà";
  }

  // trò chơi tiếp tục
  else {
    status = "Lượt " + moveColor + " di chuyển";

    // chiếu
    if (game.in_check()) {
      status += ", " + moveColor + " đang chiếu";
    }
  }

  console.log(status);
  $("#status").html(status);
}

// board.move('e2-e4') hàm di chuyển với tham số là chuỗii chứa toạ đọ điểm bắt đầu và toạ độ điểm kết thúc
$("#replay-btn").click(function (e) {
  handleReset();
});
