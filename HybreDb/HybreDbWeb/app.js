var Roadie = require("roadie");
var Hybre = require("./HybreDb.js");

var server = new Roadie.Server({ port: 80, webserviceDir: "webservices/", root: __dirname });
var hybre = new Hybre();

server.addRoutes({
    "[POST]/execute/{cmd}/": "HybreDb.js:execute",
    "[GET]/list/{table}/": "HybreDb.js:list"

});
server.hybre = hybre;

hybre.start();
server.start();