var Roadie = require("roadie");
var Hybre = require("./HybreDb.js");

var server = new Roadie.Server({ port: 80, webserviceDir: "webservices/", root: __dirname });
var hybre = new Hybre();

server.addRoutes({
    "[POST]/execute/{cmd}/": "HybreDb.js:execute",
    "[GET]/{table}/": "HybreDb.js:list",
    "[POST]/{table}/": "HybreDb.js:createTable",
    "[DELETE]/{table}/": "HybreDb.js:delete"
});
server.hybre = hybre;

hybre.start();
server.start();