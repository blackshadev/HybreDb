var Roadie = require("roadie");
var Hybre = require("./HybreDb.js");

var server = new Roadie.Server({ port: 80, webserviceDir: "webservices/", root: __dirname });
var hybre = new Hybre();

server.addRoutes({
    
    "[GET]/view/{page}/": "View.js",
    "[GET]/view/{page}/{table}/": "View.js",
    "[GET]/view/{page}/{table}/{relation}": "View.js",
    "[GET]/view/": "View.js",
    
    "[POST]/execute/{cmd}/": "HybreDb.js:execute",
    "[GET]/{table}/": "HybreDb.js:list",
    "[POST]/{table}/": "HybreDb.js:createTable",
    "[DELETE]/{table}/": "HybreDb.js:delete",

    "[GET]/static/*": "Static.js"
});
server.hybre = hybre;

hybre.start();
server.start();