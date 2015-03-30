var $r = require("roadie");
var db = require("./db.js");

var routes = {
	"[GET]/view/": "view.js:index",
	"[GET]/view/call/": "view.js:call",
	"[GET]/view/table/": "view.js:tables",
	"[GET]/view/table/{table}/": "view.js:table", 
	"[GET]/view/relation/": "view.js:relations",
	"[GET]/view/relation/{rel}": "view.js:relation",

	"[GET]/table/{table}/": "table.js:getTable", 
	"[POST]/table/{table}": "table.js:createTable",
	"[DELETE]/table/{table}": "table.js:deleteTable",


};

var server = new $r.Server({ port: 8080, webserviceDir: "webSvc", root: __dirname });
server.addRoutes([routes]);
server.db = db;

server.start();