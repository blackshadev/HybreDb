var $r = require("roadie");
var db = require("./db.js");

var routes = {
	"[GET]/": "view.js:index",
	"[GET]/call/": "view.js:call",
	"[GET]/table/{table}/": "view.js:table", 
	
	"[POST]/table/{table}": "table.js:createTable",
	"[DELETE]/table/{table}": "table.js:deleteTable",

};

var server = new $r.Server({ port: 8080, webserviceDir: "webSvc", root: __dirname });
server.addRoutes([routes]);
server.db = db;

server.start();